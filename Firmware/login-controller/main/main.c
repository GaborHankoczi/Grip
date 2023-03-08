

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#include "esp_log.h"
#include "esp_system.h"

#include "nfc.h"
#include "sd.h"
#include "wifi.h"
#include "config.h"
#include "time.h"

void app_main(void)
{

    char TAG[] = "system";
    ESP_LOGI("main","Starting");

    sd_init();
    config_init();
    nfc_init();
    wifi_init();

    ESP_LOGI("main","Setup complete");
    char free_heap[16];
    while(1){
        ESP_LOGI(TAG, "current time: %lld", time(NULL));
        itoa(esp_get_free_heap_size(), free_heap, 10);
        ESP_LOGI(TAG, "free heap: %s bytes",free_heap);
        vTaskDelay(1000 / portTICK_PERIOD_MS);
    }
    return;
}
