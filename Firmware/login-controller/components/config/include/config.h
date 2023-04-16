#ifdef __cplusplus
extern "C" {
#endif

void config_init(void);

typedef struct {
    char wifi_ssid[50];
    char wifi_password[50];
    char api_server[50];
    char api_key[50];
} network_config_t;

typedef struct {
    uint64_t card_id;
    uint64_t from_time;
    uint64_t to_time;
    char days_of_week;
} rule_t;

network_config_t* config_get_network_config();
void config_store_rule_json(char* json);

#ifdef __cplusplus
}
#endif