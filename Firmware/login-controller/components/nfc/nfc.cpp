
#include <esp_log.h>
#include <inttypes.h>
#include <pn532.h>
#include <NdefMessage.h>

#include "driver/gpio.h"
#include "esp_log.h"

#include "freertos/FreeRTOS.h"
#include "freertos/event_groups.h"
#include "freertos/task.h"

#define PIN_NUM_NFC_MISO 19
#define PIN_NUM_NFC_MOSI 23
#define PIN_NUM_NFC_CLK 18
#define PIN_NUM_NFC_CS 5
#ifdef __cplusplus
extern "C" {
#endif

static char TAG[] = "nfc";

static pn532_t nfc;

NdefMessage message;
int messageSize;
uint8_t ndefBuf[1024];

uint8_t nfc_emulate()
{

    message = NdefMessage();
    message.addUriRecord("https://grip.sytes.net");
    messageSize = message.getEncodedSize();
    if (messageSize > sizeof(ndefBuf)) {
        ESP_LOGE(TAG,"ndefBuf is too small");
        while (1) { }
    }
    
    ESP_LOGI(TAG,"Ndef encoded message size: %d", messageSize);

    message.encode(ndefBuf);
  
    // comment out this command for no ndef message
    pn532_setNdefFile(ndefBuf, messageSize);
    return pn532_emulate(&nfc,0x563412);    
}



void nfc_task(void *pvParameter)
{
    pn532_spi_init(&nfc, PIN_NUM_NFC_CLK, PIN_NUM_NFC_MISO, PIN_NUM_NFC_MOSI, PIN_NUM_NFC_CS);
    pn532_begin(&nfc);

    pn532_SAMConfig(&nfc);
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
    /*if(pn532_setParameters(&nfc)){
        ESP_LOGI(TAG, "pn532_setParameters success");
    }else{
        ESP_LOGI(TAG, "pn532_setParameters failed");
    }*/
    /*pn532_setPassiveActivationRetries(&nfc, 0xFF);
    pn532_setParameters(&nfc);*/
    while (1)
    {
        nfc_emulate();
    }
    while (1)
    {
        vTaskDelay(1000 / portTICK_PERIOD_MS);
    }
    // configure board to read RFID tags
    if (!pn532_SAMConfig(&nfc))
    {
        ESP_LOGE(TAG, "SAMConfig failed");
        while (1)
        {
            vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
    }

    if (!pn532_setPassiveActivationRetries(&nfc, 0xFF))
    {
        ESP_LOGE(TAG, "setPassiveActivationRetries failed");
        while (1)
        {
            vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
    }

    ESP_LOGI(TAG, "Waiting for an ISO14443A Card ...");

    while (1)
    {
        uint8_t success;
        uint8_t uid[] = {0, 0, 0, 0, 0, 0, 0}; // Buffer to store the returned UID
        uint8_t uidLength;                     // Length of the UID (4 or 7 bytes depending on ISO14443A card type)

        // Wait for an ISO14443A type cards (Mifare, etc.).  When one is found
        // 'uid' will be populated with the UID, and uidLength will indicate
        // if the uid is 4 bytes (Mifare Classic) or 7 bytes (Mifare Ultralight)
        success = pn532_readPassiveTargetID(&nfc, PN532_MIFARE_ISO14443A, uid, &uidLength, 0);

        if (success)
        {
            // Display some basic information about the card
            ESP_LOGI(TAG, "Found an ISO14443A card");
            ESP_LOGI(TAG, "UID Length: %d bytes", uidLength);
            ESP_LOGI(TAG, "UID Value:");
            esp_log_buffer_hexdump_internal(TAG, uid, uidLength, ESP_LOG_INFO);
            vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
        else
        {
            // PN532 probably timed out waiting for a card
            ESP_LOGI(TAG, "Timed out waiting for a card");
        }
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