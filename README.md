# T-Rex miner Readonly API
Allows T-Rex miner to be safely monitored via HTTP without permissions for "config" and "trex" commands. 
T-rex miner has no option to separate access rules for "summary"(stats reading) and "config"/"trex"(full access to parameters like pool, wallets, gpu configs, etc.) requests. So it cannot be safely monitored from unsafe environment. 
My program fixes this drawback by creating Http server with single supported request "summary" that can safely share localhost miner's stats(Server polls localhost miner every 1 sec for "summary" request response). 
## Examples
To simply create globally(from LAN/whole network depending on NAT settings, white IP etc) accesible ApiServer just start a program without any arguments:
```
T-RexReadOnlyAPI.exe
```
To select a readonly ApiServer port(1234 for example) start a program with 1 argument:
```
T-RexReadOnlyAPI.exe 1234
```
To select both IP address and port for readonly ApiServer start a program with 2 arguments:
```
T-RexReadOnlyAPI.exe 192.168.1.2 1234
```
or 
```
T-RexReadOnlyAPI.exe localhost 1234
```
To run program on other OS use mono:
```
mono T-RexReadOnlyAPI.exe 192.168.1.2 1234
```
## Drawbacks of current version
Almost no input error checking, don't write broken data or program will crash.
