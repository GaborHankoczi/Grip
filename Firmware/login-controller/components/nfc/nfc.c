
#include <esp_log.h>
#include <inttypes.h>
#include <pn532.h>

#include "driver/gpio.h"
#include "esp_log.h"

#include "freertos/FreeRTOS.h"
#include "freertos/event_groups.h"
#include "freertos/task.h"


#define PIN_NUM_NFC_MISO  19
#define PIN_NUM_NFC_MOSI  23
#define PIN_NUM_NFC_CLK   18
#define PIN_NUM_NFC_CS 5


static char TAG[] = "nfc";

static pn532_t nfc;

void nfc_task(void *pvParameter)
{
    pn532_spi_init(&nfc, PIN_NUM_NFC_CLK, PIN_NUM_NFC_MISO, PIN_NUM_NFC_MOSI, PIN_NUM_NFC_CS);
    pn532_begin(&nfc);

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
    ESP_LOGI(TAG, "Found chip PN5 %x", (unsigned int)((versiondata >> 24) & 0xFF));
    ESP_LOGI(TAG, "Firmware ver. %d.%d", (int)((versiondata >> 16) & 0xFF), (int)((versiondata >> 8) & 0xFF));

    // configure board to read RFID tags
    if(!pn532_SAMConfig(&nfc)){
        ESP_LOGW(TAG, "SAMConfig failed");
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

void nfc_init(){
    if(xTaskCreate(&nfc_task, "nfc_task", 4096, NULL, 4, NULL)!=pdTRUE){
        ESP_LOGE(TAG, "Cannot create task nfc_task");
    }


}