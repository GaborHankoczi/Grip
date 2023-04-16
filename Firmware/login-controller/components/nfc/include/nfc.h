#ifndef NFC_H
#define NFC_H

#ifdef __cplusplus
extern "C" {
#endif

void nfc_init();
void nfc_set_secret_key(char *key);

#ifdef __cplusplus
}
#endif

#endif