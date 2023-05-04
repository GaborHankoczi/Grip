#include <stdio.h>
#include "io.h"
#include "driver/gpio.h"

#define DIP_SWITCH_1 36//GPIO_NUM_36
#define DIP_SWITCH_2 39//GPIO_NUM_39
#define DIP_SWITCH_3 34//GPIO_NUM_34
#define DIP_SWITCH_4 35//GPIO_NUM_35
#define DIP_SWITCH_5 16//GPIO_NUM_16
#define DIP_SWITCH_6 4//GPIO_NUM_4
#define DIP_SWITCH_7 21//GPIO_NUM_21
#define DIP_SWITCH_8 2//GPIO_NUM_2


uint8_t io_get_station_number(void){
    return gpio_get_level(DIP_SWITCH_1) << 0 |
    gpio_get_level(DIP_SWITCH_2) << 1 |
    gpio_get_level(DIP_SWITCH_3) << 2 |
    gpio_get_level(DIP_SWITCH_4) << 3 |
    gpio_get_level(DIP_SWITCH_5) << 4 |
    gpio_get_level(DIP_SWITCH_6) << 5 |
    gpio_get_level(DIP_SWITCH_7) << 6 |
    gpio_get_level(DIP_SWITCH_8) << 7;
}


void io_init(){
    gpio_set_direction(DIP_SWITCH_1, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_2, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_3, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_4, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_5, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_6, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_7, GPIO_MODE_INPUT);
    gpio_set_direction(DIP_SWITCH_8, GPIO_MODE_INPUT);
}
