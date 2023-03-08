
#include <esp_log.h>
#include <inttypes.h>
#include <rc522.h>

#include "driver/gpio.h"
#include "esp_log.h"


#define PIN_NUM_NFC_MISO  19
#define PIN_NUM_NFC_MOSI  23
#define PIN_NUM_NFC_CLK   18
#define PIN_NUM_NFC_CS 4

static rc522_handle_t scanner;

static char TAG[] = "nfc";

static void rc522_handler(void* arg, esp_event_base_t base, int32_t event_id, void* event_data)
{
    rc522_event_data_t* data = (rc522_event_data_t*) event_data;
    switch(event_id) {
        case RC522_EVENT_TAG_SCANNED: {
                rc522_tag_t* tag = (rc522_tag_t*) data->ptr;
                ESP_LOGI(TAG, "Tag scanned (sn: %" PRIu64 ")", tag->serial_number);
            }
            break;
    }
}


void nfc_init(){
    rc522_config_t config = {
        .spi.host = VSPI_HOST,
        .spi.miso_gpio = PIN_NUM_NFC_MISO,
        .spi.mosi_gpio = PIN_NUM_NFC_MOSI,
        .spi.sck_gpio = PIN_NUM_NFC_CLK,
        .spi.sda_gpio = PIN_NUM_NFC_CS,
        .spi.clock_speed_hz = 400000,
    };

    rc522_create(&config, &scanner);
    rc522_register_events(scanner, RC522_EVENT_ANY, rc522_handler, NULL);
    rc522_start(scanner);
}