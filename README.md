
# WebOS-Discover-Csharp

This program is a very simple program to discover lg tvs, based of the python package pywebostv



## How to use it

It may take a while to load

##To include it use :
```csharp
using webos_discovery;
```
## To get the ips

```csharp
// Get the ips
var ips = new webos_discovery.Client().get_ips();
```

If you are using the 1.0.1 or above you can use the Log option
like this:
```csharp
var ips = new webos_discovery.Client(true).get_ips();
```
