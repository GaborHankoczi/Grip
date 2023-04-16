#include <stdio.h>
#include <stdint.h>
#include <inttypes.h>
#include <string.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "freertos/queue.h"
#include "sdkconfig.h"

#include <esp_log.h>
#include <esp_log_internal.h>

#include "driver/gpio.h"
#include "pn532.h"

#define PN532_DEBUG_EN
//#define MIFARE_DEBUG_EN

#ifdef PN532_DEBUG_EN
#define PN532_DEBUG(fmt, ...) ESP_LOGI("NFC",fmt, ##__VA_ARGS__)
#define PN532_DEBUG_HEX(buffer,len) ESP_LOG_BUFFER_HEXDUMP("NFC",buffer,len,ESP_LOG_INFO)
#define DMSG(fmt, ...) printf(fmt, ##__VA_ARGS__)
#define DMSG_HEX(num) printf(" 0x%02X", num & 0xff);
#else
#define PN532_DEBUG(fmt, ...)
#define PN532_DEBUG_HEX(buffer,len)
#define DMSG(fmt, ...)
#define DMSG_HEX(num)
#endif

#ifdef MIFARE_DEBUG_EN
#define MIFARE_DEBUG(fmt, ...) printf(fmt, ##__VA_ARGS__)
#else
#define MIFARE_DEBUG(fmt, ...)
#endif

#define PN532_PACKBUFFSIZ 64

#ifndef _BV
#define _BV(bit) (1 << (bit))
#endif

#define MAX_TGREAD
#define NDEF_MAX_LENGTH 1024 // theoratically ndef can handle messages ut to 128 kB

typedef enum
{
    NONE,
    CC,
    NDEF
} tag_file; // CC ... Compatibility Container

typedef enum
{
    COMMAND_COMPLETE,
    TAG_NOT_FOUND,
    FUNCTION_NOT_SUPPORTED,
    MEMORY_FAILURE,
    END_OF_FILE_BEFORE_REACHED_LE_BYTES
} responseCommand;

uint8_t compatibility_container[] = {
    0, 0x0F,
    0x20,
    0, 0x54,
    0, 0xFF,
    0x04,                                                        // T
    0x06,                                                        // L
    0xE1, 0x04,                                                  // File identifier
    ((NDEF_MAX_LENGTH & 0xFF00) >> 8), (NDEF_MAX_LENGTH & 0xFF), // maximum NDEF file size
    0x00,                                                        // read access 0x0 = granted
    0x00                                                         // write access 0x0 = granted | 0xFF = deny
};

uint8_t ndef_file[NDEF_MAX_LENGTH];

// Command APDU
#define C_APDU_CLA 0
#define C_APDU_INS 1  // instruction
#define C_APDU_P1 2   // parameter 1
#define C_APDU_P2 3   // parameter 2
#define C_APDU_LC 4   // length command
#define C_APDU_DATA 5 // data

#define C_APDU_P1_SELECT_BY_ID 0x00
#define C_APDU_P1_SELECT_BY_NAME 0x04

// Response APDU
#define R_APDU_SW1_COMMAND_COMPLETE 0x90
#define R_APDU_SW2_COMMAND_COMPLETE 0x00

#define R_APDU_SW1_NDEF_TAG_NOT_FOUND 0x6a
#define R_APDU_SW2_NDEF_TAG_NOT_FOUND 0x82

#define R_APDU_SW1_FUNCTION_NOT_SUPPORTED 0x6A
#define R_APDU_SW2_FUNCTION_NOT_SUPPORTED 0x81

#define R_APDU_SW1_MEMORY_FAILURE 0x65
#define R_APDU_SW2_MEMORY_FAILURE 0x81

#define R_APDU_SW1_END_OF_FILE_BEFORE_REACHED_LE_BYTES 0x62
#define R_APDU_SW2_END_OF_FILE_BEFORE_REACHED_LE_BYTES 0x82

// ISO7816-4 commands
#define ISO7816_SELECT_FILE 0xA4
#define ISO7816_READ_BINARY 0xB0
#define ISO7816_UPDATE_BINARY 0xD6

#define PN532_DELAY(ms) vTaskDelay(ms / portTICK_PERIOD_MS)

static uint8_t pn532ack[] = {0x00, 0x00, 0xFF, 0x00, 0xFF, 0x00};
static uint8_t pn532response_firmwarevers[] = {0x00, 0xFF, 0x06, 0xFA, 0xD5, 0x03};
static uint8_t pn532_packetbuffer[PN532_PACKBUFFSIZ];

static void pn532_readdata(pn532_t *obj, uint8_t *buff, uint8_t n);
static void pn532_writecommand(pn532_t *obj, uint8_t *cmd, uint8_t cmdlen);
static bool pn532_readack(pn532_t *obj);
static bool pn532_isready(pn532_t *obj);
static bool pn532_waitready(pn532_t *obj, uint16_t timeout);
static void pn532_spi_write(pn532_t *obj, uint8_t c);
static uint8_t pn532_spi_read(pn532_t *obj);
void pn532_inRelease(pn532_t *obj);
void pn532_setResponse(responseCommand cmd, uint8_t *buf, uint8_t *sendlen, uint8_t sendlenOffset);

void sendLog(char *data, int datalen)
{
    /*printf("Sending:");
    for(int i = 0;i<datalen;i++){
        printf(" 0x%x", data[i] && 0xFF);
    }
    printf("\n");*/
}
void receiveLog(char *data, int datalen)
{
    /*printf("Receiving:");
    for(int i = 0;i<datalen;i++){
        printf(" 0x%x", data[i] && 0xFF);
    }
    printf("\n");*/
}

void pn532_spi_init(pn532_t *obj, uint8_t clk, uint8_t miso, uint8_t mosi, uint8_t ss, uint8_t irq)
{
    obj->_clk = clk;
    obj->_miso = miso;
    obj->_mosi = mosi;
    obj->_ss = ss;
    obj->_irq = irq;

    gpio_reset_pin(obj->_clk);
    gpio_reset_pin(obj->_miso);
    gpio_reset_pin(obj->_mosi);
    gpio_reset_pin(obj->_ss);
    //gpio_reset_pin(obj->_irq);

    gpio_set_direction(obj->_ss, GPIO_MODE_OUTPUT);
    gpio_set_level(obj->_ss, 1);
    gpio_set_direction(obj->_clk, GPIO_MODE_OUTPUT);
    gpio_set_direction(obj->_mosi, GPIO_MODE_OUTPUT);
    gpio_set_direction(obj->_miso, GPIO_MODE_INPUT);
    gpio_set_direction(obj->_irq, GPIO_MODE_INPUT);

    pn532_isready(obj);
}

/**************************************************************************/
/*!
    @brief  Setups the HW
*/
/**************************************************************************/
void pn532_begin(pn532_t *obj)
{
    gpio_set_level(obj->_ss, 0);

    PN532_DELAY(10);

    // not exactly sure why but we have to send a dummy command to get synced up
    pn532_packetbuffer[0] = PN532_COMMAND_GETFIRMWAREVERSION;
    pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 1, 10);

    // ignore response!

    gpio_set_level(obj->_ss, 1);
}

/**************************************************************************/
/*!
    @brief  Checks the firmware version of the PN5xx chip
    @returns  The chip's firmware version and ID
*/
/**************************************************************************/
uint32_t pn532_getFirmwareVersion(pn532_t *obj)
{
    uint32_t response;

    pn532_packetbuffer[0] = PN532_COMMAND_GETFIRMWAREVERSION;

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 1, 1000))
    {
        return 0;
    }

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 12);

    // check some basic stuff
    if (0 != strncmp((char *)pn532_packetbuffer, (char *)pn532response_firmwarevers, 6))
    {
        PN532_DEBUG("Firmware doesn't match!\n");
        return 0;
    }

    int offset = 7;
    response = pn532_packetbuffer[offset++];
    response <<= 8;
    response |= pn532_packetbuffer[offset++];
    response <<= 8;
    response |= pn532_packetbuffer[offset++];
    response <<= 8;
    response |= pn532_packetbuffer[offset++];

    return response;
}

/**************************************************************************/
/*!
    @brief  Sends a command and waits a specified period for the ACK
    @param  cmd       Pointer to the command buffer
    @param  cmdlen    The size of the command in bytes
    @param  timeout   timeout before giving up
    @returns  1 if everything is OK, 0 if timeout occured before an
              ACK was recieved
*/
/**************************************************************************/
// default timeout of one second
bool pn532_sendCommandCheckAck(pn532_t *obj, uint8_t *cmd, uint8_t cmdlen, uint16_t timeout)
{
    // write the command
    pn532_writecommand(obj, cmd, cmdlen);

    // Wait for chip to say its ready!
    if (!pn532_waitready(obj, timeout))
    {
        return false;
    }

    // read acknowledgement
    if (!pn532_readack(obj))
    {
        PN532_DEBUG("No ACK frame received!\n");
        return false;
    }

    // For SPI only wait for the chip to be ready again.
    // This is unnecessary with I2C.
    if (!pn532_waitready(obj, timeout))
    {
        return false;
    }

    return true; // ack'd command
}

/**************************************************************************/
/*!
    Writes an 8-bit value that sets the state of the PN532's GPIO pins
    @warning This function is provided exclusively for board testing and
             is dangerous since it will throw an error if any pin other
             than the ones marked "Can be used as GPIO" are modified!  All
             pins that can not be used as GPIO should ALWAYS be left high
             (value = 1) or the system will become unstable and a HW reset
             will be required to recover the PN532.
             pinState[0]  = P30     Can be used as GPIO
             pinState[1]  = P31     Can be used as GPIO
             pinState[2]  = P32     *** RESERVED (Must be 1!) ***
             pinState[3]  = P33     Can be used as GPIO
             pinState[4]  = P34     *** RESERVED (Must be 1!) ***
             pinState[5]  = P35     Can be used as GPIO
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
bool pn532_writeGPIO(pn532_t *obj, uint8_t pinstate)
{

    // Make sure pinstate does not try to toggle P32 or P34
    pinstate |= (1 << PN532_GPIO_P32) | (1 << PN532_GPIO_P34);

    // Fill command buffer
    pn532_packetbuffer[0] = PN532_COMMAND_WRITEGPIO;
    pn532_packetbuffer[1] = PN532_GPIO_VALIDATIONBIT | pinstate; // P3 Pins
    pn532_packetbuffer[2] = 0x00;                                // P7 GPIO Pins (not used ... taken by SPI)

    PN532_DEBUG("Writing P3 GPIO: %02x\n", pn532_packetbuffer[1]);

    // Send the WRITEGPIO command (0x0E)
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 3, 1000))
        return 0x0;

    // Read response packet (00 FF PLEN PLENCHECKSUM D5 CMD+1(0x0F) DATACHECKSUM 00)
    pn532_readdata(obj, pn532_packetbuffer, 8);

    PN532_DEBUG("Received:");
    for (int i = 0; i < 8; i++)
    {
        PN532_DEBUG(" %02x", pn532_packetbuffer[i]);
    }
    PN532_DEBUG("\n");

    int offset = 5;
    return (pn532_packetbuffer[offset] == 0x0F);
}

/**************************************************************************/
/*!
    Reads the state of the PN532's GPIO pins
    @returns An 8-bit value containing the pin state where:
             pinState[0]  = P30
             pinState[1]  = P31
             pinState[2]  = P32
             pinState[3]  = P33
             pinState[4]  = P34
             pinState[5]  = P35
*/
/**************************************************************************/
uint8_t pn532_readGPIO(pn532_t *obj)
{
    pn532_packetbuffer[0] = PN532_COMMAND_READGPIO;

    // Send the READGPIO command (0x0C)
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 1, 1000))
        return 0x0;

    // Read response packet (00 FF PLEN PLENCHECKSUM D5 CMD+1(0x0D) P3 P7 IO1 DATACHECKSUM 00)
    pn532_readdata(obj, pn532_packetbuffer, 11);

    /* READGPIO response should be in the following format:
    uint8_t            Description
    -------------   ------------------------------------------
    b0..5           Frame header and preamble (with I2C there is an extra 0x00)
    b6              P3 GPIO Pins
    b7              P7 GPIO Pins (not used ... taken by SPI)
    b8              Interface Mode Pins (not used ... bus select pins)
    b9..10          checksum */

    int p3offset = 6;

    PN532_DEBUG("Received:");
    for (int i = 0; i < 11; i++)
    {
        PN532_DEBUG(" %02x", pn532_packetbuffer[i]);
    }
    PN532_DEBUG("\n");

    PN532_DEBUG("P3 GPIO: %02x\n", pn532_packetbuffer[p3offset]);
    PN532_DEBUG("P7 GPIO: %02x\n", pn532_packetbuffer[p3offset + 1]);
    PN532_DEBUG("IO GPIO: %02x\n", pn532_packetbuffer[p3offset + 2]);
    // Note: You can use the IO GPIO value to detect the serial bus being used
    switch (pn532_packetbuffer[p3offset + 2])
    {
    case 0x00: // Using UART
        PN532_DEBUG("Using UART (IO = 0x00)\n");
        break;
    case 0x01: // Using I2C
        PN532_DEBUG("Using I2C (IO = 0x01)\n");
        break;
    case 0x02: // Using SPI
        PN532_DEBUG("Using SPI (IO = 0x02)\n");
        break;
    }

    return pn532_packetbuffer[p3offset];
}

/**************************************************************************/
/*!
    @brief  Configures the SAM (Secure Access Module)
*/
/**************************************************************************/
bool pn532_SAMConfig(pn532_t *obj)
{
    pn532_packetbuffer[0] = PN532_COMMAND_SAMCONFIGURATION;
    pn532_packetbuffer[1] = 0x01; // normal mode;
    pn532_packetbuffer[2] = 0x14; // timeout 50ms * 20 = 1 second
    pn532_packetbuffer[3] = 0x01; // use IRQ pin!

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 4, 1000))
        return false;

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 8);

    int offset = 5;
    return (pn532_packetbuffer[offset] == 0x15);
}

#define PN532_PARAM_FNADUSED (1 << 0)
#define PN532_PARAM_FDIDUSED (1 << 1)
#define PN532_PARAM_FAUTOMATIC_ATR_RES (1 << 2)
#define PN532_PARAM_FAUTOMATICRATS (1 << 4)
#define PN532_PARAM_FISO14443_4_PICC (1 << 5)
#define PN532_PARAM_REMOVE_PREPOSTAMBLE (1 << 6)

bool pn532_setParameters(pn532_t *obj)
{
    uint8_t config = 0;
    config |= PN532_PARAM_FNADUSED;
    config |= PN532_PARAM_FDIDUSED;
    config |= PN532_PARAM_FAUTOMATIC_ATR_RES;
    config |= PN532_PARAM_FAUTOMATICRATS;
    config |= PN532_PARAM_FISO14443_4_PICC;
    // Adding remove pre/postamble will break this library

    uint8_t setParamCmd[] = {
        0x12, // PN532_COMMAND_SET_PARAMETERS (Documentation 7.2.9 page: 85)
        config};

    if (!pn532_sendCommandCheckAck(obj, setParamCmd, 2, 1000))
        return false;

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 8);

    int offset = 5;
    return (pn532_packetbuffer[offset] == 0x13);
}

/**************************************************************************/
/*!
    Sets the MxRtyPassiveActivation uint8_t of the RFConfiguration register
    @param  maxRetries    0xFF to wait forever, 0x00..0xFE to timeout
                          after mxRetries
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
bool pn532_setPassiveActivationRetries(pn532_t *obj, uint8_t maxRetries)
{
    pn532_packetbuffer[0] = PN532_COMMAND_RFCONFIGURATION;
    pn532_packetbuffer[1] = 5;    // Config item 5 (MaxRetries)
    pn532_packetbuffer[2] = 0xFF; // MxRtyATR (default = 0xFF)
    pn532_packetbuffer[3] = 0x01; // MxRtyPSL (default = 0x01)
    pn532_packetbuffer[4] = maxRetries;

    PN532_DEBUG("Setting MxRtyPassiveActivation to %d\n", maxRetries);

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 5, 1000))
        return 0x0; // no ACK

    return 1;
}

/***** ISO14443A Commands ******/

/**************************************************************************/
/*!
    Waits for an ISO14443A target to enter the field
    @param  cardBaudRate  Baud rate of the card
    @param  uid           Pointer to the array that will be populated
                          with the card's UID (up to 7 bytes)
    @param  uidLength     Pointer to the variable that will hold the
                          length of the card's UID.
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
bool pn532_readPassiveTargetID(pn532_t *obj, uint8_t cardbaudrate, uint8_t *uid, uint8_t *uidLength, uint16_t timeout)
{
    pn532_packetbuffer[0] = PN532_COMMAND_INLISTPASSIVETARGET;
    pn532_packetbuffer[1] = 1; // max 1 cards at once (we can set this to 2 later)
    pn532_packetbuffer[2] = cardbaudrate;

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 3, timeout))
    {
        PN532_DEBUG("No card(s) read\n");
        return 0x0; // no cards read
    }

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 20);
    // check some basic stuff

    /* ISO14443A card response should be in the following format:
    uint8_t            Description
    -------------   ------------------------------------------
    b0..6           Frame header and preamble
    b7              Tags Found
    b8              Tag Number (only one used in this example)
    b9..10          SENS_RES
    b11             SEL_RES
    b12             NFCID Length
    b13..NFCIDLen   NFCID                                      */

    PN532_DEBUG("Found %d tags\n", pn532_packetbuffer[7]);
    if (pn532_packetbuffer[7] != 1)
        return 0;

    uint16_t sens_res = pn532_packetbuffer[9];
    sens_res <<= 8;
    sens_res |= pn532_packetbuffer[10];
    PN532_DEBUG("ATQA: %02x\n", sens_res);
    PN532_DEBUG("SAK: %02x\n", pn532_packetbuffer[11]);

    /* Card appears to be Mifare Classic */
    *uidLength = pn532_packetbuffer[12];

    for (uint8_t i = 0; i < pn532_packetbuffer[12]; i++)
    {
        uid[i] = pn532_packetbuffer[13 + i];
    }

    PN532_DEBUG("UID:");
    for (int i = 0; i < pn532_packetbuffer[12]; i++)
    {
        PN532_DEBUG(" %02x", uid[i]);
    }
    PN532_DEBUG("\n");

    return 1;
}

/**************************************************************************/
/*!
    @brief  Exchanges an APDU with the currently inlisted peer
    @param  send            Pointer to data to send
    @param  sendLength      Length of the data to send
    @param  response        Pointer to response data
    @param  responseLength  Pointer to the response data length
*/
/**************************************************************************/
bool pn532_inDataExchange(pn532_t *obj, uint8_t *send, uint8_t sendLength, uint8_t *response, uint8_t *responseLength)
{
    if (sendLength > PN532_PACKBUFFSIZ - 2)
    {
        PN532_DEBUG("APDU length too long for packet buffer\n");
        return false;
    }
    uint8_t i;

    pn532_packetbuffer[0] = 0x40; // PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = obj->_inListedTag;
    for (i = 0; i < sendLength; ++i)
    {
        pn532_packetbuffer[i + 2] = send[i];
    }

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, sendLength + 2, 1000))
    {
        PN532_DEBUG("Could not send APDU\n");
        return false;
    }

    if (!pn532_waitready(obj, 1000))
    {
        PN532_DEBUG("Response never received for APDU...\n");
        return false;
    }

    pn532_readdata(obj, pn532_packetbuffer, sizeof(pn532_packetbuffer));

    if (pn532_packetbuffer[0] == 0 && pn532_packetbuffer[1] == 0 && pn532_packetbuffer[2] == 0xff)
    {
        uint8_t length = pn532_packetbuffer[3];
        if (pn532_packetbuffer[4] != (uint8_t)(~length + 1))
        {
            PN532_DEBUG("Length check invalid %02x%02x\n", length, (~length) + 1);
            return false;
        }
        if (pn532_packetbuffer[5] == PN532_PN532TOHOST && pn532_packetbuffer[6] == PN532_RESPONSE_INDATAEXCHANGE)
        {
            if ((pn532_packetbuffer[7] & 0x3f) != 0)
            {
                PN532_DEBUG("Status code indicates an error\n");
                return false;
            }

            length -= 3;

            if (length > *responseLength)
            {
                length = *responseLength; // silent truncation...
            }

            for (i = 0; i < length; ++i)
            {
                response[i] = pn532_packetbuffer[8 + i];
            }
            *responseLength = length;

            return true;
        }
        else
        {
            PN532_DEBUG("Don't know how to handle this command: %02x\n", pn532_packetbuffer[6]);
            return false;
        }
    }
    else
    {
        PN532_DEBUG("Preamble missing\n");
        return false;
    }
}

/**************************************************************************/
/*!
    @brief  'InLists' a passive target. PN532 acting as reader/initiator,
            peer acting as card/responder.
*/
/**************************************************************************/
bool pn532_inListPassiveTarget(pn532_t *obj)
{
    pn532_packetbuffer[0] = PN532_COMMAND_INLISTPASSIVETARGET;
    pn532_packetbuffer[1] = 1;
    pn532_packetbuffer[2] = 0;

    PN532_DEBUG("About to inList passive target\n");

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 3, 1000))
    {
        PN532_DEBUG("Could not send inlist message\n");
        return false;
    }

    if (!pn532_waitready(obj, 30000))
    {
        return false;
    }

    pn532_readdata(obj, pn532_packetbuffer, sizeof(pn532_packetbuffer));

    if (pn532_packetbuffer[0] == 0 && pn532_packetbuffer[1] == 0 && pn532_packetbuffer[2] == 0xff)
    {
        uint8_t length = pn532_packetbuffer[3];
        if (pn532_packetbuffer[4] != (uint8_t)(~length + 1))
        {
            PN532_DEBUG("Length check invalid %02x%02x\n", length, (~length) + 1);
            return false;
        }
        if (pn532_packetbuffer[5] == PN532_PN532TOHOST && pn532_packetbuffer[6] == PN532_RESPONSE_INLISTPASSIVETARGET)
        {
            if (pn532_packetbuffer[7] != 1)
            {
                PN532_DEBUG("Unhandled number of targets inlisted\n");
                PN532_DEBUG("Number of tags inlisted: %d\n", pn532_packetbuffer[7]);
                return false;
            }

            obj->_inListedTag = pn532_packetbuffer[8];
            PN532_DEBUG("Tag number: %d\n", obj->_inListedTag);

            return true;
        }
        else
        {
            PN532_DEBUG("Unexpected response to inlist passive host\n");
            return false;
        }
    }
    else
    {
        PN532_DEBUG("Preamble missing\n");
        return false;
    }

    return true;
}

/***** Mifare Classic Functions ******/

/**************************************************************************/
/*!
      Indicates whether the specified block number is the first block
      in the sector (block 0 relative to the current sector)
*/
/**************************************************************************/
bool pn532_mifareclassic_IsFirstBlock(pn532_t *obj, uint32_t uiBlock)
{
    // Test if we are in the small or big sectors
    if (uiBlock < 128)
        return ((uiBlock) % 4 == 0);
    else
        return ((uiBlock) % 16 == 0);
}

/**************************************************************************/
/*!
      Indicates whether the specified block number is the sector trailer
*/
/**************************************************************************/
bool pn532_mifareclassic_IsTrailerBlock(pn532_t *obj, uint32_t uiBlock)
{
    // Test if we are in the small or big sectors
    if (uiBlock < 128)
        return ((uiBlock + 1) % 4 == 0);
    else
        return ((uiBlock + 1) % 16 == 0);
}

/**************************************************************************/
/*!
    Tries to authenticate a block of memory on a MIFARE card using the
    INDATAEXCHANGE command.  See section 7.3.8 of the PN532 User Manual
    for more information on sending MIFARE and other commands.
    @param  uid           Pointer to a uint8_t array containing the card UID
    @param  uidLen        The length (in bytes) of the card's UID (Should
                          be 4 for MIFARE Classic)
    @param  blockNumber   The block number to authenticate.  (0..63 for
                          1KB cards, and 0..255 for 4KB cards).
    @param  keyNumber     Which key type to use during authentication
                          (0 = MIFARE_CMD_AUTH_A, 1 = MIFARE_CMD_AUTH_B)
    @param  keyData       Pointer to a uint8_t array containing the 6 uint8_t
                          key value
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_mifareclassic_AuthenticateBlock(pn532_t *obj, uint8_t *uid, uint8_t uidLen, uint32_t blockNumber, uint8_t keyNumber, uint8_t *keyData)
{
    uint8_t i;

    // Hang on to the key and uid data
    memcpy(obj->_key, keyData, 6);
    memcpy(obj->_uid, uid, uidLen);
    obj->_uidLen = uidLen;

    MIFARE_DEBUG("Trying to authenticate card\n");
    for (int i = 0; i < obj->_uidLen; i++)
    {
        MIFARE_DEBUG(" %02x", obj->_uid[i]);
    }
    MIFARE_DEBUG("\n");
    MIFARE_DEBUG("Using authentication KEY %c\n", keyNumber ? 'B' : 'A');
    for (int i = 0; i < 6; i++)
    {
        MIFARE_DEBUG(" %02x", obj->_key[i]);
    }
    MIFARE_DEBUG("\n");

    // Prepare the authentication command //
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE; /* Data Exchange Header */
    pn532_packetbuffer[1] = 1;                            /* Max card numbers */
    pn532_packetbuffer[2] = (keyNumber) ? MIFARE_CMD_AUTH_B : MIFARE_CMD_AUTH_A;
    pn532_packetbuffer[3] = blockNumber; /* Block Number (1K = 0..63, 4K = 0..255 */
    memcpy(pn532_packetbuffer + 4, obj->_key, 6);
    for (i = 0; i < obj->_uidLen; i++)
    {
        pn532_packetbuffer[10 + i] = obj->_uid[i]; /* 4 uint8_t card ID */
    }

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 10 + obj->_uidLen, 1000))
        return 0;

    // Read the response packet
    pn532_readdata(obj, pn532_packetbuffer, 12);

    // check if the response is valid and we are authenticated???
    // for an auth success it should be bytes 5-7: 0xD5 0x41 0x00
    // Mifare auth error is technically uint8_t 7: 0x14 but anything other and 0x00 is not good
    if (pn532_packetbuffer[7] != 0x00)
    {
        MIFARE_DEBUG("Authentification failed\n");
        for (int i = 0; i < 12; i++)
        {
            MIFARE_DEBUG(" %02x", pn532_packetbuffer[i]);
        }
        MIFARE_DEBUG("\n");
        return 0;
    }

    return 1;
}

/**************************************************************************/
/*!
    Tries to read an entire 16-uint8_t data block at the specified block
    address.
    @param  blockNumber   The block number to authenticate.  (0..63 for
                          1KB cards, and 0..255 for 4KB cards).
    @param  data          Pointer to the uint8_t array that will hold the
                          retrieved data (if any)
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_mifareclassic_ReadDataBlock(pn532_t *obj, uint8_t blockNumber, uint8_t *data)
{
    MIFARE_DEBUG("Trying to read 16 bytes from block %d\n", blockNumber);

    /* Prepare the command */
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = 1;               /* Card number */
    pn532_packetbuffer[2] = MIFARE_CMD_READ; /* Mifare Read command = 0x30 */
    pn532_packetbuffer[3] = blockNumber;     /* Block Number (0..63 for 1K, 0..255 for 4K) */

    /* Send the command */
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 4, 1000))
    {
        MIFARE_DEBUG("Failed to receive ACK for read command\n");
        return 0;
    }

    /* Read the response packet */
    pn532_readdata(obj, pn532_packetbuffer, 26);

    /* If uint8_t 8 isn't 0x00 we probably have an error */
    if (pn532_packetbuffer[7] != 0x00)
    {
        MIFARE_DEBUG("Unexpected response:");
        for (int i = 0; i < 26; i++)
        {
            MIFARE_DEBUG(" %02x", pn532_packetbuffer[i]);
        }
        MIFARE_DEBUG("\n");
        return 0;
    }

    /* Copy the 16 data bytes to the output buffer        */
    /* Block content starts at uint8_t 9 of a valid response */
    memcpy(data, pn532_packetbuffer + 8, 16);

    /* Display data for debug if requested */
    MIFARE_DEBUG("Block %d\n", blockNumber);
    for (int i = 0; i < 16; i++)
    {
        MIFARE_DEBUG(" %02x", data[i]);
    }
    MIFARE_DEBUG("\n");

    return 1;
}

/**************************************************************************/
/*!
    Tries to write an entire 16-uint8_t data block at the specified block
    address.
    @param  blockNumber   The block number to authenticate.  (0..63 for
                          1KB cards, and 0..255 for 4KB cards).
    @param  data          The uint8_t array that contains the data to write.
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_mifareclassic_WriteDataBlock(pn532_t *obj, uint8_t blockNumber, uint8_t *data)
{
    MIFARE_DEBUG("Trying to write 16 bytes to block %d\n", blockNumber);

    /* Prepare the first command */
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = 1;                /* Card number */
    pn532_packetbuffer[2] = MIFARE_CMD_WRITE; /* Mifare Write command = 0xA0 */
    pn532_packetbuffer[3] = blockNumber;      /* Block Number (0..63 for 1K, 0..255 for 4K) */
    memcpy(pn532_packetbuffer + 4, data, 16); /* Data Payload */

    /* Send the command */
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 20, 1000))
    {
        MIFARE_DEBUG("Failed to receive ACK for write command\n");
        return 0;
    }
    PN532_DELAY(10);

    /* Read the response packet */
    pn532_readdata(obj, pn532_packetbuffer, 26);

    return 1;
}

/**************************************************************************/
/*!
    Formats a Mifare Classic card to store NDEF Records
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_mifareclassic_FormatNDEF(pn532_t *obj)
{
    uint8_t sectorbuffer1[16] = {0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1};
    uint8_t sectorbuffer2[16] = {0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1};
    uint8_t sectorbuffer3[16] = {0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0x78, 0x77, 0x88, 0xC1, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};

    // Note 0xA0 0xA1 0xA2 0xA3 0xA4 0xA5 must be used for key A
    // for the MAD sector in NDEF records (sector 0)

    // Write block 1 and 2 to the card
    if (!(pn532_mifareclassic_WriteDataBlock(obj, 1, sectorbuffer1)))
        return 0;
    if (!(pn532_mifareclassic_WriteDataBlock(obj, 2, sectorbuffer2)))
        return 0;
    // Write key A and access rights card
    if (!(pn532_mifareclassic_WriteDataBlock(obj, 3, sectorbuffer3)))
        return 0;

    // Seems that everything was OK (?!)
    return 1;
}

/**************************************************************************/
/*!
    Writes an NDEF URI Record to the specified sector (1..15)
    Note that this function assumes that the Mifare Classic card is
    already formatted to work as an "NFC Forum Tag" and uses a MAD1
    file system.  You can use the NXP TagWriter app on Android to
    properly format cards for this.
    @param  sectorNumber  The sector that the URI record should be written
                          to (can be 1..15 for a 1K card)
    @param  uriIdentifier The uri identifier code (0 = none, 0x01 =
                          "http://www.\n", etc.)
    @param  url           The uri text to write (max 38 characters).
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_mifareclassic_WriteNDEFURI(pn532_t *obj, uint8_t sectorNumber, uint8_t uriIdentifier, const char *url)
{
    // Figure out how long the string is
    uint8_t len = strlen(url);

    // Make sure we're within a 1K limit for the sector number
    if ((sectorNumber < 1) || (sectorNumber > 15))
        return 0;

    // Make sure the URI payload is between 1 and 38 chars
    if ((len < 1) || (len > 38))
        return 0;

    // Note 0xD3 0xF7 0xD3 0xF7 0xD3 0xF7 must be used for key A
    // in NDEF records

    // Setup the sector buffer (w/pre-formatted TLV wrapper and NDEF message)
    uint8_t sectorbuffer1[16] = {0x00, 0x00, 0x03, len + 5, 0xD1, 0x01, len + 1, 0x55, uriIdentifier, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
    uint8_t sectorbuffer2[16] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
    uint8_t sectorbuffer3[16] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
    uint8_t sectorbuffer4[16] = {0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7, 0x7F, 0x07, 0x88, 0x40, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
    if (len <= 6)
    {
        // Unlikely we'll get a url this short, but why not ...
        memcpy(sectorbuffer1 + 9, url, len);
        sectorbuffer1[len + 9] = 0xFE;
    }
    else if (len == 7)
    {
        // 0xFE needs to be wrapped around to next block
        memcpy(sectorbuffer1 + 9, url, len);
        sectorbuffer2[0] = 0xFE;
    }
    else if ((len > 7) && (len <= 22))
    {
        // Url fits in two blocks
        memcpy(sectorbuffer1 + 9, url, 7);
        memcpy(sectorbuffer2, url + 7, len - 7);
        sectorbuffer2[len - 7] = 0xFE;
    }
    else if (len == 23)
    {
        // 0xFE needs to be wrapped around to final block
        memcpy(sectorbuffer1 + 9, url, 7);
        memcpy(sectorbuffer2, url + 7, len - 7);
        sectorbuffer3[0] = 0xFE;
    }
    else
    {
        // Url fits in three blocks
        memcpy(sectorbuffer1 + 9, url, 7);
        memcpy(sectorbuffer2, url + 7, 16);
        memcpy(sectorbuffer3, url + 23, len - 24);
        sectorbuffer3[len - 22] = 0xFE;
    }

    // Now write all three blocks back to the card
    if (!(pn532_mifareclassic_WriteDataBlock(obj, sectorNumber * 4, sectorbuffer1)))
        return 0;
    if (!(pn532_mifareclassic_WriteDataBlock(obj, (sectorNumber * 4) + 1, sectorbuffer2)))
        return 0;
    if (!(pn532_mifareclassic_WriteDataBlock(obj, (sectorNumber * 4) + 2, sectorbuffer3)))
        return 0;
    if (!(pn532_mifareclassic_WriteDataBlock(obj, (sectorNumber * 4) + 3, sectorbuffer4)))
        return 0;

    // Seems that everything was OK (?!)
    return 1;
}

/***** Mifare Ultralight Functions ******/

/**************************************************************************/
/*!
    Tries to read an entire 4-uint8_t page at the specified address.
    @param  page        The page number (0..63 in most cases)
    @param  buffer      Pointer to the uint8_t array that will hold the
                        retrieved data (if any)
*/
/**************************************************************************/
uint8_t pn532_mifareultralight_ReadPage(pn532_t *obj, uint8_t page, uint8_t *buffer)
{
    if (page >= 64)
    {
        MIFARE_DEBUG("Page value out of range\n");
        return 0;
    }

    MIFARE_DEBUG("Reading page %d\n", page);

    /* Prepare the command */
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = 1;               /* Card number */
    pn532_packetbuffer[2] = MIFARE_CMD_READ; /* Mifare Read command = 0x30 */
    pn532_packetbuffer[3] = page;            /* Page Number (0..63 in most cases) */

    /* Send the command */
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 4, 1000))
    {
        MIFARE_DEBUG("Failed to receive ACK for write command\n");
        return 0;
    }

    /* Read the response packet */
    pn532_readdata(obj, pn532_packetbuffer, 26);
    MIFARE_DEBUG("Received:");
    for (int i = 0; i < 26; i++)
    {
        MIFARE_DEBUG(" %02x", pn532_packetbuffer[i]);
    }
    MIFARE_DEBUG("\n");

    /* If uint8_t 8 isn't 0x00 we probably have an error */
    if (pn532_packetbuffer[7] == 0x00)
    {
        /* Copy the 4 data bytes to the output buffer         */
        /* Block content starts at uint8_t 9 of a valid response */
        /* Note that the command actually reads 16 uint8_t or 4  */
        /* pages at a time ... we simply discard the last 12  */
        /* bytes                                              */
        memcpy(buffer, pn532_packetbuffer + 8, 4);
    }
    else
    {
        MIFARE_DEBUG("Unexpected response reading block:");
        for (int i = 0; i < 26; i++)
        {
            MIFARE_DEBUG(" %02x", pn532_packetbuffer[i]);
        }
        MIFARE_DEBUG("\n");
        return 0;
    }

    /* Display data for debug if requested */
    MIFARE_DEBUG("Page %d:", page);
    for (int i = 0; i < 4; i++)
    {
        MIFARE_DEBUG(" %02x", buffer[i]);
    }
    MIFARE_DEBUG("\n");

    // Return OK signal
    return 1;
}

/**************************************************************************/
/*!
    Tries to write an entire 4-uint8_t page at the specified block
    address.
    @param  page          The page number to write.  (0..63 for most cases)
    @param  data          The uint8_t array that contains the data to write.
                          Should be exactly 4 bytes long.
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_mifareultralight_WritePage(pn532_t *obj, uint8_t page, uint8_t *data)
{

    if (page >= 64)
    {
        MIFARE_DEBUG("Page value out of range\n");
        // Return Failed Signal
        return 0;
    }

    MIFARE_DEBUG("Trying to write 4 uint8_t page %d\n", page);

    /* Prepare the first command */
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = 1;                           /* Card number */
    pn532_packetbuffer[2] = MIFARE_ULTRALIGHT_CMD_WRITE; /* Mifare Ultralight Write command = 0xA2 */
    pn532_packetbuffer[3] = page;                        /* Page Number (0..63 for most cases) */
    memcpy(pn532_packetbuffer + 4, data, 4);             /* Data Payload */

    /* Send the command */
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 8, 1000))
    {
        MIFARE_DEBUG("Failed to receive ACK for write command\n");
        // Return Failed Signal
        return 0;
    }
    PN532_DELAY(10);

    /* Read the response packet */
    pn532_readdata(obj, pn532_packetbuffer, 26);

    // Return OK Signal
    return 1;
}

/***** NTAG2xx Functions ******/

/**************************************************************************/
/*!
    Tries to read an entire 4-uint8_t page at the specified address.
    @param  page        The page number (0..63 in most cases)
    @param  buffer      Pointer to the uint8_t array that will hold the
                        retrieved data (if any)
*/
/**************************************************************************/
uint8_t pn532_ntag2xx_ReadPage(pn532_t *obj, uint8_t page, uint8_t *buffer)
{
    // TAG Type       PAGES   USER START    USER STOP
    // --------       -----   ----------    ---------
    // NTAG 203       42      4             39
    // NTAG 213       45      4             39
    // NTAG 215       135     4             129
    // NTAG 216       231     4             225

    if (page >= 231)
    {
        MIFARE_DEBUG("Page value out of range\n");
        return 0;
    }

    MIFARE_DEBUG("Reading page %d\n", page);

    /* Prepare the command */
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = 1;               /* Card number */
    pn532_packetbuffer[2] = MIFARE_CMD_READ; /* Mifare Read command = 0x30 */
    pn532_packetbuffer[3] = page;            /* Page Number (0..63 in most cases) */

    /* Send the command */
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 4, 1000))
    {
        MIFARE_DEBUG("Failed to receive ACK for write command\n");
        return 0;
    }

    /* Read the response packet */
    pn532_readdata(obj, pn532_packetbuffer, 26);
    MIFARE_DEBUG("Received:");
    for (int i = 0; i < 26; i++)
    {
        MIFARE_DEBUG(" %02x", pn532_packetbuffer[i]);
    }
    MIFARE_DEBUG("\n");

    /* If uint8_t 8 isn't 0x00 we probably have an error */
    if (pn532_packetbuffer[7] == 0x00)
    {
        /* Copy the 4 data bytes to the output buffer         */
        /* Block content starts at uint8_t 9 of a valid response */
        /* Note that the command actually reads 16 uint8_t or 4  */
        /* pages at a time ... we simply discard the last 12  */
        /* bytes                                              */
        memcpy(buffer, pn532_packetbuffer + 8, 4);
    }
    else
    {
        MIFARE_DEBUG("Unexpected response reading block:");
        for (int i = 0; i < 26; i++)
        {
            MIFARE_DEBUG(" %02x", pn532_packetbuffer[i]);
        }
        MIFARE_DEBUG("\n");
        return 0;
    }

    /* Display data for debug if requested */
    MIFARE_DEBUG("Page %d:", page);
    for (int i = 0; i < 4; i++)
    {
        MIFARE_DEBUG(" %02x", buffer[i]);
    }
    MIFARE_DEBUG("\n");

    // Return OK signal
    return 1;
}

/**************************************************************************/
/*!
    Tries to write an entire 4-uint8_t page at the specified block
    address.
    @param  page          The page number to write.  (0..63 for most cases)
    @param  data          The uint8_t array that contains the data to write.
                          Should be exactly 4 bytes long.
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_ntag2xx_WritePage(pn532_t *obj, uint8_t page, uint8_t *data)
{
    // TAG Type       PAGES   USER START    USER STOP
    // --------       -----   ----------    ---------
    // NTAG 203       42      4             39
    // NTAG 213       45      4             39
    // NTAG 215       135     4             129
    // NTAG 216       231     4             225

    if ((page < 4) || (page > 225))
    {
        MIFARE_DEBUG("Page value out of range\n");
        // Return Failed Signal
        return 0;
    }

    MIFARE_DEBUG("Trying to write 4 uint8_t page %d\n", page);

    /* Prepare the first command */
    pn532_packetbuffer[0] = PN532_COMMAND_INDATAEXCHANGE;
    pn532_packetbuffer[1] = 1;                           /* Card number */
    pn532_packetbuffer[2] = MIFARE_ULTRALIGHT_CMD_WRITE; /* Mifare Ultralight Write command = 0xA2 */
    pn532_packetbuffer[3] = page;                        /* Page Number (0..63 for most cases) */
    memcpy(pn532_packetbuffer + 4, data, 4);             /* Data Payload */

    /* Send the command */
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 8, 1000))
    {
        MIFARE_DEBUG("Failed to receive ACK for write command\n");

        // Return Failed Signal
        return 0;
    }
    PN532_DELAY(10);

    /* Read the response packet */
    pn532_readdata(obj, pn532_packetbuffer, 26);

    // Return OK Signal
    return 1;
}

/**************************************************************************/
/*!
    Writes an NDEF URI Record starting at the specified page (4..nn)
    Note that this function assumes that the NTAG2xx card is
    already formatted to work as an "NFC Forum Tag".
    @param  uriIdentifier The uri identifier code (0 = none, 0x01 =
                          "http://www.\n", etc.)
    @param  url           The uri text to write (null-terminated string).
    @param  dataLen       The size of the data area for overflow checks.
    @returns 1 if everything executed properly, 0 for an error
*/
/**************************************************************************/
uint8_t pn532_ntag2xx_WriteNDEFURI(pn532_t *obj, uint8_t uriIdentifier, char *url, uint8_t dataLen)
{
    uint8_t pageBuffer[4] = {0, 0, 0, 0};

    // Remove NDEF record overhead from the URI data (pageHeader below)
    uint8_t wrapperSize = 12;

    // Figure out how long the string is
    uint8_t len = strlen(url);

    // Make sure the URI payload will fit in dataLen (include 0xFE trailer)
    if ((len < 1) || (len + 1 > (dataLen - wrapperSize)))
        return 0;

    // Setup the record header
    // See NFCForum-TS-Type-2-Tag_1.1.pdf for details
    uint8_t pageHeader[12] =
        {
            /* NDEF Lock Control TLV (must be first and always present) */
            0x01, /* Tag Field (0x01 = Lock Control TLV) */
            0x03, /* Payload Length (always 3) */
            0xA0, /* The position inside the tag of the lock bytes (upper 4 = page address, lower 4 = uint8_t offset) */
            0x10, /* Size in bits of the lock area */
            0x44, /* Size in bytes of a page and the number of bytes each lock bit can lock (4 bit + 4 bits) */
            /* NDEF Message TLV - URI Record */
            0x03,         /* Tag Field (0x03 = NDEF Message) */
            len + 5,      /* Payload Length (not including 0xFE trailer) */
            0xD1,         /* NDEF Record Header (TNF=0x1:Well known record + SR + ME + MB) */
            0x01,         /* Type Length for the record type indicator */
            len + 1,      /* Payload len */
            0x55,         /* Record Type Indicator (0x55 or 'U' = URI Record) */
            uriIdentifier /* URI Prefix (ex. 0x01 = "http://www.\n") */
        };

    // Write 12 uint8_t header (three pages of data starting at page 4)
    memcpy(pageBuffer, pageHeader, 4);
    if (!(pn532_ntag2xx_WritePage(obj, 4, pageBuffer)))
        return 0;
    memcpy(pageBuffer, pageHeader + 4, 4);
    if (!(pn532_ntag2xx_WritePage(obj, 5, pageBuffer)))
        return 0;
    memcpy(pageBuffer, pageHeader + 8, 4);
    if (!(pn532_ntag2xx_WritePage(obj, 6, pageBuffer)))
        return 0;

    // Write URI (starting at page 7)
    uint8_t currentPage = 7;
    char *urlcopy = url;
    while (len)
    {
        if (len < 4)
        {
            memset(pageBuffer, 0, 4);
            memcpy(pageBuffer, urlcopy, len);
            pageBuffer[len] = 0xFE; // NDEF record footer
            if (!(pn532_ntag2xx_WritePage(obj, currentPage, pageBuffer)))
                return 0;
            // DONE!
            return 1;
        }
        else if (len == 4)
        {
            memcpy(pageBuffer, urlcopy, len);
            if (!(pn532_ntag2xx_WritePage(obj, currentPage, pageBuffer)))
                return 0;
            memset(pageBuffer, 0, 4);
            pageBuffer[0] = 0xFE; // NDEF record footer
            currentPage++;
            if (!(pn532_ntag2xx_WritePage(obj, currentPage, pageBuffer)))
                return 0;
            // DONE!
            return 1;
        }
        else
        {
            // More than one page of data left
            memcpy(pageBuffer, urlcopy, 4);
            if (!(pn532_ntag2xx_WritePage(obj, currentPage, pageBuffer)))
                return 0;
            currentPage++;
            urlcopy += 4;
            len -= 4;
        }
    }

    // Seems that everything was OK (?!)
    return 1;
}

/************** high level communication functions (handles both I2C and SPI) */

/**************************************************************************/
/*!
    @brief  Tries to read the SPI or I2C ACK signal
*/
/**************************************************************************/
bool pn532_readack(pn532_t *obj)
{
    uint8_t ackbuff[6];

    pn532_readdata(obj, ackbuff, 6);

    return (0 == strncmp((char *)ackbuff, (char *)pn532ack, 6));
}

/**************************************************************************/
/*!
    @brief  Return true if the PN532 is ready with a response.
*/
/**************************************************************************/
bool pn532_isready(pn532_t *obj)
{
    int res = !gpio_get_level(obj->_irq);
    //PN532_DEBUG("ready check: %s",res==0?"false":"true");
    return res;
    gpio_set_level(obj->_ss, 0);
    PN532_DELAY(10);
    char i = PN532_SPI_STATREAD;
    sendLog(&i, 1);
    pn532_spi_write(obj, PN532_SPI_STATREAD);
    // read uint8_t
    uint8_t x = pn532_spi_read(obj);
    receiveLog((char *)&x, 1);
    gpio_set_level(obj->_ss, 1);

    // Check if status is ready.
    return x == PN532_SPI_READY;
}

/**************************************************************************/
/*!
    @brief  Waits until the PN532 is ready.
    @param  timeout   Timeout before giving up
*/
/**************************************************************************/
bool pn532_waitready(pn532_t *obj, uint16_t timeout)
{
    uint8_t step = 10;
    uint16_t timer = 0;
    while (!pn532_isready(obj))
    {
        if (timeout != 0)
        {
            timer += step;
            if (timer > timeout)
            {
                PN532_DEBUG("TIMEOUT!");
                return false;
            }
        }
        PN532_DELAY(step);
    }
    return true;
}

uint8_t ackPacket[] = {0x00, 0x00, 0xFF, 0x00, 0xFF, 0x00};

/**************************************************************************/
/*!
    @brief  Reads n bytes of data from the PN532 via SPI or I2C.
    @param  buff      Pointer to the buffer where data will be written
    @param  n         Number of bytes to be read
*/

/**************************************************************************/
void pn532_readdata(pn532_t *obj, uint8_t *buff, uint8_t n)
{
    gpio_set_level(obj->_ss, 0);
    PN532_DELAY(10);
    char i = PN532_SPI_DATAREAD;
    sendLog(&i, 1);
    pn532_spi_write(obj, PN532_SPI_DATAREAD);

    for (uint8_t i = 0; i < n; i++)
    {
        PN532_DELAY(1);
        buff[i] = pn532_spi_read(obj);
    }
    if (memcmp(buff, ackPacket, 6) == 0)
    {
        PN532_DEBUG("ACK received");
    }
    else
    {
        uint8_t buffer_offset = buff[0] == 0x00 && buff[1] == 0xFF ? 5 : 6; // Preampble not allways present
        PN532_DEBUG("Reading with length %d: ", buff[3 + (buffer_offset - 6)] - 1);
        /*for (int i = buffer_offset; i < buffer_offset - 1 + buff[3 + (buffer_offset - 6)]; i++)
        {
            PN532_DEBUG(" 0x%02x,", buff[i]);
        }*/
        PN532_DEBUG_HEX(buff + buffer_offset, buff[3 + (buffer_offset - 6)] - 1);
    }

    gpio_set_level(obj->_ss, 1);
}

uint8_t target[] = {
        0x8C,             // INIT AS TARGET
        0x05,             // MODE -> BITFIELD
        0x04, 0x00,       // SENS_RES - MIFARE PARAMS
        0x00, 0x00, 0x00, // NFCID1T
        0x20,             // SEL_RES
        // FELICA PARAMS
        0x01, 0xFE, // NFCID2t (8 bytes) https://github.com/adafruit/Adafruit-PN532/blob/master/Adafruit_PN532.cpp FeliCa NEEDS TO BEGIN WITH 0x01 0xFE!
        0x05, 0x01, 0x86,
        0x04, 0x02, 0x02,
        0x03, 0x00, // PAD (8 bytes)
        0x4B, 0x02, 0x4F,
        0x49, 0x8A, 0x00,
        0xFF, 0xFF, // System code (2 bytes)

        0x01, 0x01, 0x66, // NFCID3t (10 bytes)
        0x6D, 0x01, 0x01, 0x10,
        0x02, 0x00, 0x00,

        0x00, // length of general bytes
        0x00  // length of historical bytes
    };

/**************************************************************************/
/*!
    @brief  set the PN532 as iso14443a Target behaving as a SmartCard
    @param  None
    #author Salvador Mendoza(salmg.net) new functions:
    -AsTarget
    -getDataTarget
    -setDataTarget
*/
/**************************************************************************/
uint8_t pn532_AsTarget(pn532_t *obj,uint32_t uid,uint16_t timeout)
{
    if(uid!=0){
        memcpy(target+4,&uid,3);
    }
    if (!pn532_sendCommandCheckAck(obj, target, sizeof(target), (uint16_t)timeout))
        return false;

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 64);
    // Response shold be something like: 0x00 0x00 0xFF 0x03 0xFD 0xD5 0x87 0x29 0x7B 0x00
    // 87 is the response code
    // 29 means released by initiator
    int offset = 6;
    return (pn532_packetbuffer[offset] == 0x8D /*&& pn532_packetbuffer[offset+1] == 0x29*/);
}

void pn532_setNdefFile(const uint8_t *ndef, const int16_t ndefLength)
{
    if (ndefLength > (NDEF_MAX_LENGTH - 2))
    {
        DMSG("ndef file too large (> NDEF_MAX_LENGHT -2) - aborting");
        return;
    }

    ndef_file[0] = ndefLength >> 8;
    ndef_file[1] = ndefLength & 0xFF;
    memcpy(ndef_file + 2, ndef, ndefLength);
}

bool pn532_emulate(pn532_t *obj, uint32_t uid, uint16_t timeout)
{
    bool processingCommands = true;
    uint8_t rwbuf[128];
    uint8_t sendlen;
    tag_file currentFile = NONE;
    if (pn532_AsTarget(obj,uid,timeout))
    {
        // ESP_LOGI(TAG, "pn532_AsTarget success");

        while (processingCommands)
        {
            // TODO check for communication ending 0x29
            uint8_t readLength = 0;
            if (!pn532_getDataTarget(obj, rwbuf, &readLength))
            {
                DMSG("pn532_getDataTarget failed\n");
                pn532_inRelease(obj);
                return false;
            }
            uint8_t p1 = rwbuf[C_APDU_P1];
            uint8_t p2 = rwbuf[C_APDU_P2];
            uint8_t lc = rwbuf[C_APDU_LC];
            uint16_t p1p2_length = ((int16_t)p1 << 8) + p2;

            switch (rwbuf[C_APDU_INS]) // switch instruction command
            {
            case ISO7816_SELECT_FILE:
                switch (p1)
                {
                case C_APDU_P1_SELECT_BY_ID:
                    if (p2 != 0x0c)
                    {
                        DMSG("C_APDU_P2 != 0x0c\n");
                        pn532_setResponse(COMMAND_COMPLETE, rwbuf, &sendlen, 0);
                    }
                    else if (lc == 2 && rwbuf[C_APDU_DATA] == 0xE1 && (rwbuf[C_APDU_DATA + 1] == 0x03 || rwbuf[C_APDU_DATA + 1] == 0x04))
                    {
                        pn532_setResponse(COMMAND_COMPLETE, rwbuf, &sendlen, 0);
                        if (rwbuf[C_APDU_DATA + 1] == 0x03)
                        {
                            currentFile = CC;
                        }
                        else if (rwbuf[C_APDU_DATA + 1] == 0x04)
                        {
                            currentFile = NDEF;
                        }
                    }
                    else
                    {
                        pn532_setResponse(TAG_NOT_FOUND, rwbuf, &sendlen, 0);
                    }
                    break;
                case C_APDU_P1_SELECT_BY_NAME:
                    const uint8_t ndef_tag_application_name_v2[] = {0, 0x7, 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x01};
                    if (0 == memcmp(ndef_tag_application_name_v2, rwbuf + C_APDU_P2, sizeof(ndef_tag_application_name_v2)))
                    {
                        pn532_setResponse(COMMAND_COMPLETE, rwbuf, &sendlen, 0);
                    }
                    else
                    {
                        DMSG("function not supported\n");
                        pn532_setResponse(FUNCTION_NOT_SUPPORTED, rwbuf, &sendlen, 0);
                    }
                    break;
                }
                break;
            case ISO7816_READ_BINARY:
                switch (currentFile)
                {
                case NONE:
                    pn532_setResponse(TAG_NOT_FOUND, rwbuf, &sendlen, 0);
                    break;
                case CC:
                    if (p1p2_length > NDEF_MAX_LENGTH)
                    {
                        pn532_setResponse(END_OF_FILE_BEFORE_REACHED_LE_BYTES, rwbuf, &sendlen, 0);
                    }
                    else
                    {
                        memcpy(rwbuf, compatibility_container + p1p2_length, lc);
                        pn532_setResponse(COMMAND_COMPLETE, rwbuf + lc, &sendlen, lc);
                    }
                    break;
                case NDEF:
                    if (p1p2_length > NDEF_MAX_LENGTH)
                    {
                        pn532_setResponse(END_OF_FILE_BEFORE_REACHED_LE_BYTES, rwbuf, &sendlen, 0);
                    }
                    else
                    {
                        memcpy(rwbuf, ndef_file + p1p2_length, lc);
                        pn532_setResponse(COMMAND_COMPLETE, rwbuf + lc, &sendlen, lc);
                    }
                    break;
                }
                break;
            case ISO7816_UPDATE_BINARY:
                if (false) // Tag writeable?
                {
                    pn532_setResponse(FUNCTION_NOT_SUPPORTED, rwbuf, &sendlen, 0);
                }
                else
                {
                    if (p1p2_length > NDEF_MAX_LENGTH)
                    {
                        pn532_setResponse(MEMORY_FAILURE, rwbuf, &sendlen, 0);
                    }
                    else
                    {
                        // Probably shouldn't update the original message
                        char written[100] = {};
                        memcpy(written, rwbuf + C_APDU_DATA, lc);
                        written[lc]=0;
                        //memcpy(ndef_file + p1p2_length, rwbuf + C_APDU_DATA, lc);
                        pn532_setResponse(COMMAND_COMPLETE, rwbuf, &sendlen, 0);
                        DMSG("Tag was written with new NDEF message: %s",written);
                        //uint16_t ndef_length = (ndef_file[0] << 8) + ndef_file[1];
                        //if ((ndef_length > 0) /*&& (updateNdefCallback != 0)*/)
                        //{
                            
                            // updateNdefCallback(ndef_file + 2, ndef_length);
                        //}
                    }
                }
                break;
            default:
                DMSG("Command not supported!");
                DMSG_HEX(rwbuf[C_APDU_INS]);
                DMSG("\n");
                pn532_setResponse(FUNCTION_NOT_SUPPORTED, rwbuf, &sendlen, 0);
            }

            if (!pn532_setDataTarget(obj, rwbuf, sendlen))
            {
                DMSG("tgSetData failed\n!");
                pn532_inRelease(obj);
                return false;
            }
        }
        return true;
    }
    else
    {
        DMSG("pn532_AsTarget failed");
        return false;
    }
}

void pn532_setResponse(responseCommand cmd, uint8_t *buf, uint8_t *sendlen, uint8_t sendlenOffset)
{
    switch (cmd)
    {
    case COMMAND_COMPLETE:
        buf[0] = R_APDU_SW1_COMMAND_COMPLETE;
        buf[1] = R_APDU_SW2_COMMAND_COMPLETE;
        *sendlen = 2 + sendlenOffset;
        break;
    case TAG_NOT_FOUND:
        buf[0] = R_APDU_SW1_NDEF_TAG_NOT_FOUND;
        buf[1] = R_APDU_SW2_NDEF_TAG_NOT_FOUND;
        *sendlen = 2;
        break;
    case FUNCTION_NOT_SUPPORTED:
        buf[0] = R_APDU_SW1_FUNCTION_NOT_SUPPORTED;
        buf[1] = R_APDU_SW2_FUNCTION_NOT_SUPPORTED;
        *sendlen = 2;
        break;
    case MEMORY_FAILURE:
        buf[0] = R_APDU_SW1_MEMORY_FAILURE;
        buf[1] = R_APDU_SW2_MEMORY_FAILURE;
        *sendlen = 2;
        break;
    case END_OF_FILE_BEFORE_REACHED_LE_BYTES:
        buf[0] = R_APDU_SW1_END_OF_FILE_BEFORE_REACHED_LE_BYTES;
        buf[1] = R_APDU_SW2_END_OF_FILE_BEFORE_REACHED_LE_BYTES;
        *sendlen = 2;
        break;
    }
}

/**************************************************************************/
/*!
    @brief  retrieve response from the emulation mode
    @param  cmd    = data
    @param  cmdlen = data length
*/
/**************************************************************************/
uint8_t pn532_getDataTarget(pn532_t *obj, uint8_t *cmd, uint8_t *cmdlen)
{
    uint8_t length;
    pn532_packetbuffer[0] = 0x86;
    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 1, 1000))
    {
        PN532_DEBUG("Error en ack\n");
        return false;
    }

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 64);
    length = pn532_packetbuffer[3] - 3;
    uint8_t status_offset = pn532_packetbuffer[0] == 0x00 && pn532_packetbuffer[1] == 0xFF ? 0 : 1; // Preamble not allways present
    if(pn532_packetbuffer[6+1] != 0x00) // Released by initiator or not in proper state
    {
        return false;
    }

    // if (length > *responseLength) {// Bug, should avoid it in the reading target data
    //   length = *responseLength; // silent truncation...
    // }

    for (int i = 0; i < length; ++i)
    {
        cmd[i] = pn532_packetbuffer[8 + i];
    }
    *cmdlen = length;
    return true;
}

void pn532_inRelease(pn532_t *obj)
{
    pn532_packetbuffer[0] = 0x52;
    pn532_packetbuffer[1] = 0x00;
    pn532_sendCommandCheckAck(obj, pn532_packetbuffer, 2, 1000);
    pn532_readdata(obj, pn532_packetbuffer, 64);
}

/**************************************************************************/
/*!
    @brief  set data in PN532 in the emulation mode
    @param  cmd    = data
    @param  cmdlen = data length
*/
/**************************************************************************/
uint8_t pn532_setDataTarget(pn532_t *obj, uint8_t *cmd, uint8_t cmdlen)
{
    uint8_t length;
    // cmd1[0] = 0x8E; Must!
    pn532_packetbuffer[0] = 0x8E;
    memcpy(pn532_packetbuffer + 1, cmd, cmdlen);

    if (!pn532_sendCommandCheckAck(obj, pn532_packetbuffer, cmdlen+1, 1000))
        return false;

    // read data packet
    pn532_readdata(obj, pn532_packetbuffer, 8);
    length = pn532_packetbuffer[3] - 3;
    for (int i = 0; i < length; ++i)
    {
        cmd[i] = pn532_packetbuffer[8 + i];
    }
    // cmdl = 0
    cmdlen = length;

    int offset = 7;
    return (pn532_packetbuffer[offset] == 0x00);//0x15);
}

/**************************************************************************/
/*!
    @brief  Writes a command to the PN532, automatically inserting the
            preamble and required frame details (checksum, len, etc.)
    @param  cmd       Pointer to the command buffer
    @param  cmdlen    Command length in bytes
*/
/**************************************************************************/
void pn532_writecommand(pn532_t *obj, uint8_t *cmd, uint8_t cmdlen)
{
    uint8_t checksum;

    cmdlen++;

    PN532_DEBUG("Sending with length %d:", cmdlen - 1);

    gpio_set_level(obj->_ss, 0);
    PN532_DELAY(10); // or whatever the PN532_DELAY is for waking up the board

    pn532_spi_write(obj, PN532_SPI_DATAWRITE);

    checksum = PN532_PREAMBLE + PN532_PREAMBLE + PN532_STARTCODE2;
    pn532_spi_write(obj, PN532_PREAMBLE);
    pn532_spi_write(obj, PN532_PREAMBLE);
    pn532_spi_write(obj, PN532_STARTCODE2);

    pn532_spi_write(obj, cmdlen);
    pn532_spi_write(obj, ~cmdlen + 1);

    pn532_spi_write(obj, PN532_HOSTTOPN532);
    checksum += PN532_HOSTTOPN532;

    // PN532_DEBUG(" 0x%02x, 0x%02x, 0x%02x, 0x%02x, 0x%02x, 0x%02x,", (uint8_t)PN532_PREAMBLE, (uint8_t)PN532_PREAMBLE, (uint8_t)PN532_STARTCODE2, (uint8_t)cmdlen, (uint8_t)(~cmdlen + 1), (uint8_t)PN532_HOSTTOPN532);

    for (uint8_t i = 0; i < cmdlen - 1; i++)
    {
        pn532_spi_write(obj, cmd[i]);
        checksum += cmd[i];
        //PN532_DEBUG(" 0x%02x,", cmd[i]);
    }
    PN532_DEBUG_HEX(cmd, cmdlen - 1);


    //PN532_DEBUG("\n");
    pn532_spi_write(obj, ~checksum);
    pn532_spi_write(obj, PN532_POSTAMBLE);
    gpio_set_level(obj->_ss, 1);

    // PN532_DEBUG(" 0x%02x, 0x%02x\n", (uint8_t)~checksum, (uint8_t)PN532_POSTAMBLE);
}
/************** low level SPI */

/**************************************************************************/
/*!
    @brief  Low-level SPI write wrapper
    @param  c       8-bit command to write to the SPI bus
*/
/**************************************************************************/
void pn532_spi_write(pn532_t *obj, uint8_t c)
{

    int8_t i;
    gpio_set_level(obj->_clk, 1);

    for (i = 0; i < 8; i++)
    {
        gpio_set_level(obj->_clk, 0);
        if (c & _BV(i))
        {
            gpio_set_level(obj->_mosi, 1);
        }
        else
        {
            gpio_set_level(obj->_mosi, 0);
        }
        gpio_set_level(obj->_clk, 1);
    }
}

/**************************************************************************/
/*!
    @brief  Low-level SPI read wrapper
    @returns The 8-bit value that was read from the SPI bus
*/
/**************************************************************************/
uint8_t pn532_spi_read(pn532_t *obj)
{
    int8_t i, x;
    x = 0;

    gpio_set_level(obj->_clk, 1);

    for (i = 0; i < 8; i++)
    {
        if (gpio_get_level(obj->_miso))
        {
            x |= _BV(i);
        }
        gpio_set_level(obj->_clk, 0);
        gpio_set_level(obj->_clk, 1);
    }

    return x;
}