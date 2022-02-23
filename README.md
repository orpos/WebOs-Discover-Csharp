
# WebOS-Discover-Csharp

This program is a very simple. It's a program to discover lg tvs, based of the python package pywebostv using https://github.com/captainjono/RSSDP

[![NuGet Badge](https://buildstats.info/nuget/webos-discovery)](https://www.nuget.org/packages/webos-discovery/)

## Supported Platforms

Net 5.0

[![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/orpos/WebOs-Discover-Csharp/blob/main/LICENSE) 

## How to use it

It may take a while to load

## To include it use :
```csharp
using webos_discovery;
```
## To get the ips

```csharp
// Get the ips
var ips = new Webos().find_devices();
```
