---
title: Automating Nessus
date: 2021-07-28 09:50:47 +07:00
categories: greyhatcch5
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Automating Nessus.
---


## This Program helps automate aspects of the Nessus API to build classes to perform unauthenticated vulnerability scans against target hosts on a network.

_[View/Download the Visual Studio Project](https://github.com/m3rcer/C-Sharp-Hax/tree/main/Ch5/vs)_

* REST (representational state transfer) is a way of accessing and interacting with resources (such as user accounts or vulnerability scans) on the server, usually over HTTP, using a variety of HTTP methods(GET, POST, DELETE, and PUT).

_Note:_ 
_The REST API to automate scans was removed from Nesssus 7.0 and above. If you need to launch scans in an automated way, you would have to upgrade to Tenable.io or Tenable.sc which have full API integrations. 
I tried looking for Nessus versions < 7.0 as only versions prior 7.0 would work with this program but failed to find a working version. Except the "scan automation" part everything else works with the API(Refer Main.md)_.



### Code:

**Class Components:**

1. [The NessusSession Class.](/permalinks/Nessus/NessusSession)

2. [The NessusManager Class](/permalinks/Nessus/NessusManager)

3. [The Main Class.](/permalinks/Nessus/Main)


### Output:

```bash
$ mono ch5_automating_nessus.exe
Scan status: running
Scan status: running
Scan status: running
--snip--
{
"count": 1,
"plugin_name": "SSL Version 2 and 3 Protocol Detection",
"vuln_index": 62,
"severity": 2,
"plugin_id": 20007,
"severity_index": 30,
"plugin_family": "Service detection"
}
{
"count": 1,
"plugin_name": "SSL Self-Signed Certificate",
"vuln_index": 61,
"severity": 2,
"plugin_id": 57582,
"severity_index": 31,
"plugin_family": "General"
}
{
"count": 1,
"plugin_name": "SSL Certificate Cannot Be Trusted",
"vuln_index": 56,
"severity": 2,
"plugin_id": 51192,
"severity_index": 32,
"plugin_family": "General"
}
```

