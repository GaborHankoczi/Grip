idf_component_register(SRCS "nfc.cpp"
                    INCLUDE_DIRS "include"
                    REQUIRES "pn532" "driver" "io" "hmac" "wpa_supplicant")