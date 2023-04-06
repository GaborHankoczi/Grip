idf_component_register(SRCS "NdefMessage.cpp" "NdefRecord.cpp" "pn532.c"
                    INCLUDE_DIRS "include"
                    REQUIRES "driver" "esp_rom")