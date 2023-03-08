#pragma once
#include <stdbool.h>

void wifi_init(void);

typedef struct {
    ulong timestamp;
    uint64_t card_serial_number;
    uint8_t door_id;
    bool allowed;
}log_transfer_t;