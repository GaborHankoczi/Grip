#pragma once
void sd_init(void);
unsigned char* sd_get_config_value(const char* key);

#define CONFIG_WIFI_SSID "WIFI_SSID"
#define CONFIG_WIFI_PASSWORD "WIFI_PASSWORD"
#define CONFIG_SERVER_HOST "SERVER_HOST"