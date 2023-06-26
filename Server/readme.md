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

### Nagy HF pontok
OPTIONS ige az erőforrás által támogatott igék lekérdezéséhez 7 ✓
SignalR Core alkalmazása valós idejű, szerver felől érkező push jellegű kommunikációra [7] ✓
teljes szerveroldal hosztolása külső szolgáltatónál 5 ✓
Publikálás docker konténerbe és futtatás konténerből [7] ✓
OpenAPI leíró (swagger) alapú dokumentáció 3 ✓
MS SQL/Azure SQL/LocalDB-től eltérő adatbáziskiszolgáló használata EF Core-ral (egyéb, EF Core v6 támogatott adatbázis) 5 ✓
automatizált (unit vagy integrációs) tesztek készítése 14 ✓
külső komponens használata DTO-k inicializálására [3] ✓
az EF Core működőképességét, az adatbázis elérhetőségét jelző health check végpont publikálása a Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore NuGet csomag használatával [3] ✓
token alapú, ASP.NET Core Identity + Duende Server/IdentityServer5/OpenIddict middleware-rel, interaktív flow egyéb kliens esetén 12 ✓
szerver oldali hozzáférés-szabályozás, az előbbi authentikációra építve szerepkör alapú hozzáférés-szabályozás 2 ✓


7+7+5+7+3+5+14+3+3+12+2
=68