
# WebOS-Discover-Csharp

This program is a very simple program to discover lg tvs, based of the python package pywebostv



## How to use it

```csharp
// Get the ips
var ips = new webos_discovery.Client().get_ips();
```

To include it use :
```
using webos_discovery
```
