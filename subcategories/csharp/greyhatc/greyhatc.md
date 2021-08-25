---
title: Grey Hat C#
permalink: /subcategories/csharp/greyhatc/greyhatc
layout: page
excerpt: All post.
comments: false
---




<p align="center">
     <img src="https://libribook.com/Images/gray-hat-c-pdf.jpg">
</p>

* [View/Download(*I do not own this*)](https://drive.google.com/file/d/0B4hhbFaItiPxY0FNbG4ycFNxcXM/view?resourcekey=0-IyY1FRkKjxsaz8tZQpxLuw)

* Support the Authors(Brandon Perry, Matt Graeber) and grab a copy yourself if u can. 

* For hacker's with a mild understanding of programming like python / C++ and an urge to develop C# skills for Red Teaming and creating the latest offensive hacking tradecraft.

\*_Not complete yet, under active dev_*


## Contents:

### 1. Introductory scripts.

{%- for post in site.categories.grayhatc_ch1 -%}
	  <article class="post-item">
	    <h3 class="post-item-title">
	      <a href="{{ post.url }}">{{ post.title | escape }}</a>
	    </h3> 
	  </article>
{%- endfor -%}

- [Writing a basic hello world program in C#.](IntroScripts/hello/README.md)

- [Implementing an Abstract class with the override method and an Interface.](IntroScripts/abstract/README.md)

- [Implementing a class using a Delegate and referencing through Anonymous methods.](IntroScripts/anonymous/README.md)

- [Using OS commands using the P/Invoke Method.](IntroScripts/pinvoke/README.md)


### 2. Fuzzing and exploiting sql/xss.

- [Creating a mutational fuzzer to fuzz GET requests for possible sqli errors.](Ch2/Get_sql_fuzzer/README.md)

- [Creating a mutational fuzzer to fuzz POST requests for possible sqli errors.](Ch2/Post_sql_fuzzer/README.md)

- [Creating a mutational fuzzer to fuzz JSON requests for possible sqli errors.](Ch2/Json_sql_fuzzer/README.md)

- [Creating a UNION based mutational fuzzer to dump the database.](Ch2/UNION_sql_fuzzer/README.md)

- [Creating a Blind/Boolean based mutational fuzzer to dump the database.](Ch2/Boolean_blind_sql_fuzzer/README.md)


### 3. Fuzzing SOAP endpoints.

- [Creating a mutational fuzzer to programatically parse SOAP definitions and fuzz SOAP endpoints for possible sqli errors.](Ch3/README.md)

### 4. Writing Connect-Back, Binding and Metasploit payloads.


- [Writing a program which creates a listener on the target host and allows an attacker to connect to it remotely.](Ch4/Binding-payload/README.md)

- [Implementing a program which creates a listener on the target host and allows an attacker to connect to it remotely.](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch4/Binding%20payload/README.md)


- [Writing  a program which allows an attacker to listen for a connection back from a victim host.](Ch4/Connect-back-payload/README.md)


- [Running x86 and x86-x64 Metasploit payloads from C#.]()

     > [Writing a program to execute native windows payloads as unmanaged code.](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch4/Native%20windows%20payload/README.md)

     > [Writing an architecture independent program to execute both native windows/linux payloads as unmanaged code.](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch4/Native%20linux%20payload/README.md)


- [Implementing a program to create an UDP payload and a listener to implement an UDP remote connection as an alternate channel of communication.](https://github.com/me4cer98/C-Sharp-Hax/blob/main/Ch4/UDP%20payload/README.md)

- [Implementing/Running x86 and x86-x64 Metasploit payloads from C#.]()

     > [Writing a program to execute native windows payloads as unmanaged code.](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch4/Native%20windows%20payload/README.md)

     > [Writing an architecture,OS independent program to execute both native windows/linux payloads as unmanaged code.](https://github.com/me4cer98/C-Sharp-Hax/blob/main/Ch4/Native%20linux%20payload/README.md)


### 5. Automating Nessus 

- [Nessus API Automation](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch5/README.md)

### 6. Automating Nexpose 

- [Nexpose API Automation](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch6/README.md)

### 7. Automating OpenVAS 

- [Nexpose API Automation](https://github.com/m3rcer/C-Sharp-Hax/blob/main/Ch7/README.md)