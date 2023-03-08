#include  "stdio.h"
#include "config.h"
#include <list>
#include <stdbool.h>
#include <string.h>
#include <string>
#include "esp_log.h"
#include "cJSON.h"

using namespace std;

rule_t* rules = NULL;
int rules_count = 0;
network_config_t network_config = network_config_t();

#define TAG "Config"


bool file_exists(const char *filename)
{
    FILE *file = fopen(filename, "r");
    if (file != NULL)
    {
        fclose(file);
        return true;
    }
    return false;
}

void handle_wifi_config_line(string line){
    ESP_LOGI("TAG","Parsing line: %s",line.c_str());
    if(line.rfind("ssid=", 0) == 0){
        //ssid line
        strcpy(network_config.wifi_ssid, line.substr(5).c_str());
    }else if(line.rfind("password=", 0) == 0){
        //password line
        strcpy(network_config.wifi_password, line.substr(9).c_str());
    }else if(line.rfind("api-server=", 0) == 0){
        //api-server line
        strcpy(network_config.api_server, line.substr(11).c_str());
    }
}

void config_read_file_line_by_line(char* filename, void (*line_handler)(string)){
    FILE *file = fopen(filename, "r");
    if(file == NULL){
        ESP_LOGE("Config","Error opening file %s",filename);
        return;
    }
    char line[256] = {0};
    int line_char_count = 0;
    int c;
    while((c = fgetc(file)) != EOF){
        if(c == '\n'){ // line read complete                
            //create cpp string from line
            ESP_LOGI("config","read line: %s",line);
            string line_str(line);
            handle_wifi_config_line(line_str);   // parse line             
            memset(line,0,sizeof(line)); //clar line buffer
            line_char_count = 0;
        }else{
            line[line_char_count++] = c;
        }
    }
    //parse last line if not empty
    if(line_char_count > 0){
        // parse line
            string line_str(line);
        line_handler(line_str);
    }
    fclose(file);
}

#define WIFI_CONFIG_FILE "/sdcard/wifi.cfg" // TODO include MOUNTPOINT in path
#define RULES_FILE "/sdcard/rules.txt" // TODO include MOUNTPOINT in path

network_config_t* config_get_network_config(){
    return &network_config;
}

void config_write_default_configs(){
    FILE *file = fopen(WIFI_CONFIG_FILE, "w");
    if(file == NULL){
        ESP_LOGE("Error opening file %s",WIFI_CONFIG_FILE);
        return;
    }
    fprintf(file, "ssid=Legeleszo rackanyaj\n");
    fprintf(file, "password=Pleasewait4sometime\n");
    fprintf(file, "api-server=https://192.168.0.104:44494/api/rule/1\n");
    fclose(file);
}





void config_load_rules(){
    if(file_exists(RULES_FILE)){
        FILE *file = fopen(RULES_FILE, "r");
        fseek(file, 0, SEEK_END);
        int file_size=ftell(file)+1;// filesize + terminating null character
        fseek(file, 0, SEEK_SET);
        char* config = (char*) malloc(file_size);
        if(config == NULL){
            ESP_LOGE(TAG,"Error allocating memory for config file");
            return;
        }
        int read = fread(config,sizeof(char),file_size,file);
        config[read] = '\0';
        fclose(file);
        cJSON* rulesArray = cJSON_Parse(config);
        free(config);
        if(rulesArray == NULL){
            ESP_LOGI(TAG,"Json config file is not valid\r\n\t\t\t%s",config);
            return;
        }
        int rule_count = cJSON_GetArraySize(rulesArray);
        if(rules != NULL) // rules had already been loaded
            free(rules);
        rules = (rule_t*) calloc(sizeof(rule_t),rule_count);
        for(int i = 0;i<rule_count;i++){
            ESP_LOGI(TAG, "rule #%d", i);
            cJSON* rule = cJSON_GetArrayItem(rulesArray, i);
            cJSON* rule_id = cJSON_GetObjectItem(rule, "id");
            cJSON* rule_card_serial_number = cJSON_GetObjectItem(rule, "cardSerialNumber");
            cJSON* rule_fromTime = cJSON_GetObjectItem(rule, "fromTime");
            cJSON* rule_toTime = cJSON_GetObjectItem(rule, "toTime");
            cJSON* rule_days = cJSON_GetObjectItem(rule, "daysOfWeek");
            rules[i].card_id = rule_card_serial_number->valueint;
            rules[i].from_time = rule_fromTime->valueint;
            rules[i].to_time = rule_toTime->valueint;
            rules[i].days_of_week = (char) rule_days->valueint;

            
        }
        rules_count = rule_count;
        cJSON_Delete(rulesArray);
        ESP_LOGI(TAG, "Loaded %d rules", rule_count);
    }
}
void config_store_rule_json(char* json){
    //TODO check if file needs writing
    ESP_LOGI(TAG, "Storing rules to file: %s", json);
    FILE * file = fopen(RULES_FILE, "w");
    fwrite(json, sizeof(char), strlen(json), file);
    fclose(file);
    config_load_rules();
}

void config_load(void)
{
    if(!file_exists(WIFI_CONFIG_FILE))    // if no config is set, write default config
        config_write_default_configs();    
    config_read_file_line_by_line(WIFI_CONFIG_FILE, &handle_wifi_config_line);
}



uint8_t config_validate_request(uint64_t card_serial_number, uint64_t current_time)
{
    for(int i = 0;i<rules_count;i++){
        if(rules[i].card_id == card_serial_number){
            if(rules[i].from_time <= current_time && rules[i].to_time >= current_time){
                // TODO check day of week
                ESP_LOGI(TAG, "Rule %d is valid for card %llu", i, card_serial_number);
                return 1;
            }
        }
    }
    return 0;
}



void config_init(void)
{
    config_write_default_configs();
    config_load();
}
