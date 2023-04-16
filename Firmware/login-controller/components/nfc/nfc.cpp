
#include <esp_log.h>
#include <inttypes.h>
#include <pn532.h>
#include <NdefMessage.h>

#include "driver/gpio.h"
#include "esp_log.h"

#include "freertos/FreeRTOS.h"
#include "freertos/event_groups.h"
#include "freertos/task.h"
#include "io.h"
#include "base64.h"
#include "hmac_sha256.h"

#define PIN_NUM_NFC_MISO 19
#define PIN_NUM_NFC_MOSI 23
#define PIN_NUM_NFC_CLK 18
#define PIN_NUM_NFC_CS 5
#define PIN_NUM_NFC_IRQ 17

#ifdef __cplusplus
extern "C" {
#endif

static char TAG[] = "nfc";

static pn532_t nfc;

NdefMessage message;
int messageSize;
uint8_t ndefBuf[1024];

char secretKey[50] = {};

void nfc_set_secret_key(char *key)
{
    strcpy(secretKey, key);
}

uint8_t nfc_emulate(uint16_t timeout)
{
    message = NdefMessage();
    char data[50] = {};
    sprintf(data, "%d_%llu_%u", 1, time(NULL), rand());
    message.addTextRecord(data);
    uint8_t tokenHash[32] = {};
    hmac_sha256_encode(secretKey,strlen(secretKey),data,strlen(data),tokenHash,32);
    char base64Token[45] = {};
    asd(base64Token, (const char *)tokenHash, 32);
    message.addTextRecord(base64Token);
    messageSize = message.getEncodedSize();
    if (messageSize > sizeof(ndefBuf)) {
        ESP_LOGE(TAG,"ndefBuf is too small");
        while (1) { }
    }
    
    ESP_LOGI(TAG,"Ndef encoded message size: %d", messageSize);

    message.encode(ndefBuf);
  
    // comment out this command for no ndef message
    pn532_setNdefFile(ndefBuf, messageSize);
    return pn532_emulate(&nfc,0x1E0BB0, timeout);    
}
uint8_t nfc_read_passive(uint16_t timeout){
    uint8_t success;
    uint8_t uid[] = {0, 0, 0, 0, 0, 0, 0}; // Buffer to store the returned UID
    uint8_t uidLength;                     // Length of the UID (4 or 7 bytes depending on ISO14443A card type)

    // Wait for an ISO14443A type cards (Mifare, etc.).  When one is found
    // 'uid' will be populated with the UID, and uidLength will indicate
    // if the uid is 4 bytes (Mifare Classic) or 7 bytes (Mifare Ultralight)
    success = pn532_readPassiveTargetID(&nfc, PN532_MIFARE_ISO14443A, uid, &uidLength, timeout);

    if (success)
    {
        // Display some basic information about the card
        ESP_LOGI(TAG, "Found an ISO14443A card");
        ESP_LOGI(TAG, "UID Length: %d bytes", uidLength);
        ESP_LOGI(TAG, "UID Value:");
        esp_log_buffer_hexdump_internal(TAG, uid, uidLength, ESP_LOG_INFO);
        vTaskDelay(1000 / portTICK_PERIOD_MS);
    }
    return success;
}


void IRAM_ATTR gpio_interrupt_handler(void *args)
{
    int pinNumber = (int)args;
    //xQueueSendFromISR(interputQueue, &pinNumber, NULL);
    ESP_DRAM_LOGI(TAG,"Interrupt from pin %d", pinNumber);
}


void nfc_task(void *pvParameter)
{
    pn532_spi_init(&nfc, PIN_NUM_NFC_CLK, PIN_NUM_NFC_MISO, PIN_NUM_NFC_MOSI, PIN_NUM_NFC_CS, PIN_NUM_NFC_IRQ);
    pn532_begin(&nfc);

    pn532_SAMConfig(&nfc);
    if (!pn532_setPassiveActivationRetries(&nfc, 0xFF))
    {
        ESP_LOGE(TAG, "setPassiveActivationRetries failed");
        while (1)
        {
            vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
    }
    uint32_t versiondata = pn532_getFirmwareVersion(&nfc);
    if (!versiondata)
    {
        ESP_LOGI(TAG, "Didn't find PN53x board");
        while (1)
        {
            vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
    }
    // Got ok data, print it out!
    ESP_LOGI(TAG, "Found chip PN5%x", (unsigned int)((versiondata >> 24) & 0xFF));
    ESP_LOGI(TAG, "Firmware ver. %d.%d", (int)((versiondata >> 16) & 0xFF), (int)((versiondata >> 8) & 0xFF));
    while (1)
    {
        if(nfc_emulate(30000)){
            ESP_LOGI(TAG,"NFC tag read by phone");
        }
        /*if(nfc_read_passive(1000)){
            ESP_LOGI(TAG,"passive NFC tag read succesfully");
        }*/
    }
}

void nfc_init()
{
    if (xTaskCreate(&nfc_task, "nfc_task", 4096, NULL, 4, NULL) != pdTRUE)
    {
        ESP_LOGE(TAG, "Cannot create task nfc_task");
    }
}


#ifdef __cplusplus
}
#endif