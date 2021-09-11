---
title: Automating Nexpose
date: 2021-08-08 09:50:47 +07:00
categories: greyhatcch6
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Automating Nexpose.
---


## This Program helps automate aspects of the Nexpose API to build classes to perform unauthenticated vulnerability scans against target hosts on a network.

_[View/Download the Visual Studio Project](https://github.com/m3rcer/C-Sharp-Hax/tree/main/Ch6/vs)_

_Note: 
This program was made for the Nexpose API v1.1/v1.2. The current version of this writing is v3. v1.1/v1.2 utilized xml, whereas v3 utilizes json to interact with the API. I couldn't find a depreacted running version of Nexpose running these prior version nonetheless the program works as expected._

- When you start a vulnerability scan in Nexpose, you scan a site which is a collection os hosts/assets on the network. 
Sites are further classified into static and dynamic, we will be focussing on the static site which holds a list of hosts you can only change by reconfiguring the site.

- Notice in the output that Nexpose is returning at least three scan statuses, which are
separate phases of the scan: running , integrating , and finished . Once the scan finishes, our PDF report is written to the user’s Desktop, as expected.

### Code:

**Class Components:**

1. [The NexposeSession Class](/permalinks/Nexpose/NexposeSession)

2. [The NexposeManager Class](/permalinks/Nexpose/NexposeManager)

3. [The Main Class](/permalinks/Nexpose/Main)


### Output:

```bash
C:\Users\example\Documents\ch6\bin\Debug>.\06_automating_nexpose.exe
11:42:24 PM: <ScanStatusResponse success="1" scan-id="4" engine-id="3" status=➊"running" />
–-snip--
11:47:01 PM: <ScanStatusResponse success="1" scan-id="4" engine-id="3" status="running" />
11:47:08 PM: <ScanStatusResponse success="1" scan-id="4" engine-id="3" status=➋"integrating" />
11:47:15 PM: <ScanStatusResponse success="1" scan-id="4" engine-id="3" status=➌"finished" />
C:\Users\example\Documents\ch6\bin\Debug>dir \Users\example\Desktop\*.pdf
Volume in drive C is Acer
Volume Serial Number is 5619-09A2
Directory of C:\Users\example\Desktop
07/30/2017 11:47 PM 103,174 4.pdf ➍
09/09/2015 09:52 PM 17,152,368 Automate the Boring Stuff with Python.pdf
2 File(s) 17,255,542 bytes
0 Dir(s) 362,552,098,816 bytes free
C:\Users\example\Documents\ch6\bin\Debug>
```
