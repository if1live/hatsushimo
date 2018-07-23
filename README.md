# hatsushimo

simple io game

[![Build Status](https://travis-ci.org/if1live/hatsushimo.svg?branch=master)](https://travis-ci.org/if1live/hatsushimo)

## projects

* server : .Net Core
* client : Unity 2018.2

* HatsushimoShared : c# shared library
* HatsushimoServer : game server library
* HatsushimoServerTest : unit test for HatsushimoShared, HatsushimoServer
* HatsushimoServerMain : game server binary
* HatsushimoClient : game client

## build and test

```
cd HatsushimoServerTest
dotnet test

# watch
dotnet watch test
```

```
cd HatsushimoServerMain
dotnet run

# watch
dotnet watch run
```

```
cd HatsushimoShared
dotnet build -o ..\HatsushimoClient\Assets\HatsushimoShared\

# watch
dotnet watch -- build -o ..\HatsushimoClient\Assets\HatsushimoShared\
```
