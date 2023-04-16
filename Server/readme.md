# Grip server documentation

## Token format expected from user devices

### Message format
```
StationNumber_Epochtime_Salt
```
Where StationNumber is the number set on the station, Epochtime is the current time according to the station, and salt is a randomly generated character sequence for hash security purposes.

Example:
```
12_1681236890_1878442754
```

### Token format
Token is the HMAC-SHA256 hash of the message in base64 format (https://en.wikipedia.org/wiki/HMAC)