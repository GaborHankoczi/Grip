#include <stdio.h>
#include "rtc.h"
#include "esp_log.h"
#include "time.h"
#include "esp_sntp.h"

void rtc_sync_time(struct timeval *tv)
{
    ESP_LOGI("rtc", "Time synchronized: %lld", tv->tv_sec);
    srand(time(NULL));
}

void rtc_local_init(void)
{
    sntp_set_time_sync_notification_cb(rtc_sync_time);
    sntp_setoperatingmode(SNTP_OPMODE_POLL);
    sntp_setservername(0, "time.google.com");
    sntp_init();
}
