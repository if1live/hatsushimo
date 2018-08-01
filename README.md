# hatsushimo

simple io game

[![Build Status](https://travis-ci.org/if1live/hatsushimo.svg?branch=master)](https://travis-ci.org/if1live/hatsushimo)

## projects

* server : .Net Core
* client : Unity 2018.2

* Hatsushimo : shared library
	* Hatsushimo
	* HatsushimoTest
* Mikazuki : game server
	* Mikazuki
	* MikazukiTest
	* MikazukiRunner : executable
* Shigure : bot
	* Shigure
	* ShigureRunner : executable
* Yamakaze : game client
	* Yamakaze


## build and test

```
cd HatsushimoTest
dotnet test
```

```
cd MikazukiTest
dotnet test
```

```
cd MikazukiRunner
dotnet run
```

```
cd Hatsushimo
dotnet build -o ..\Yamakaze\Assets\Hatsushimo\
```

```
cd Shigure
dotnet run
```

