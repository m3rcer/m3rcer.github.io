---
title: Creating a mutational fuzzer to programatically parse SOAP definitions and fuzz SOAP endpoints for possible sqli errors.
date: 2021-06-29 11:45:47 +07:00
categories: greyhatcch3
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a mutational fuzzer to programatically parse SOAP definitions and fuzz SOAP endpoints for possible sqli errors.
---



## This Program implements XML classes from core libraries programatically parsing WSDL into its respective components and finds endpoints to fuzz via the HTTP/SOAP protocol to find relevant SQL injection vulnerabilities.

_[View/Download the Visual Studio Project](https://github.com/m3rcer/m3rcer.github.io/tree/master/_posts/coding/csharp/greyhatc/Ch3/vs)_

__WSDL Document layout:__

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch3/wsdl_layout.png)

_The program is divided into component classes for easier code management_.

### Program Components:

_(Click to view each code block)_

[The WSDL Class](https://github.com/m3rcer/m3rcer.github.io/blob/master/_posts/coding/csharp/greyhatc/Ch3/wsdl.md) - Encompasses the WSLD document.

[Parsing subclasses](https://github.com/m3rcer/m3rcer.github.io/blob/master/_posts/coding/csharp/greyhatc/Ch3/parse.md) - Parse the WSDL defintions.
   >SoapType subclass
   
>SoapMessage subclass
   
>SoapPortType subclass
   
>SoapBinding sublcass 

[The MAIN class](https://github.com/m3rcer/m3rcer.github.io/blob/master/_posts/coding/csharp/greyhatc/Ch3/main.md) - Fuzz data in WSDL

### OUTPUT:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch3/output.png)

### Inference:

Both the GetUser and DeleteUser operations are potentially vulnerable to SQL injection in the username parameter.
The ListUsers operation reports no potential SQL injections, which makes sense because it has no parameters to begin with.

