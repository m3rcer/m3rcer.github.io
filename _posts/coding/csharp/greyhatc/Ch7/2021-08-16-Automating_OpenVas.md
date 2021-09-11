---
title: Automating OpenVAS
date: 2021-08-16 09:50:47 +07:00
categories: greyhatcch7
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Automating OpenVAS.
---

## This Program helps automate aspects of the OpenVAS API to build classes to perform basic vulnerability scans against target hosts on a network.


_[View/Download the Visual Studio Project](https://github.com/m3rcer/C-Sharp-Hax/tree/main/Ch7/vs)_

_Note: 
This program was made for the OpenVAS Scanner v6.0.2. The current version of this writing is 21.4.2. v6.0.2 utilized xml, whereas 21.4.2 utilizes json to interact with the API._


### Code:

**Class Components:**

1. [The OpenVASsession Class](/permalinks/OpenVas/OpenVASSession)

2. [The OpenVASManager Class](/permalinks/OpenVas/OpenVASManager)

3. [The Main Class](/permalinks/OpenVas/Main)


### Output:

```bash
The scan is 1% done.
The scan is 8% done.
The scan is 8% done.
The scan is 46% done.
The scan is 50% done.
The scan is 58% done.
The scan is 72% done.
The scan is 84% done.
The scan is 94% done.
The scan is 98% done.
<get_results_response status="200" status_text="OK">
<result id="57e9d1fa-7ad9-4649-914d-4591321d061a">
<owner>
<name>admin</name>
</owner>
--snip--
</result>
</get_results_response>
```