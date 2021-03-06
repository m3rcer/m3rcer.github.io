---
title: PhishOPS
date: 2022-04-09 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
#layout: post
description: Red Team SMTP Builds and Operations for Phishing
---

# Red Team SMTP Builds and Operations for Phishing

This is a brief guide detailing my research setting up various **SMTP server implementations** for carrying out **Red Team Phishing Operations**.

  ``` python


                                                                        _______
                                                                       |.-----.|
                                                                       ||x . x||
                                                                       ||_.-._||
 /~~~/                                                                 `--)-(--`
(҂`_´)                                                                __[=== o]___
<,︻╦╤─ 📧   📧   📧   📧   📧   📧   📧   📧   📧   📧   📧    |:::::::::::|
                                                                     `-=========-`
  ```

_________________________________________________________________________________________________

## Index 

- [Overview](#overview)
- [Introduction](#introduction)
- [Methods](#methods)
- [Spoofing and Evasion](#spoofing-and-evasion)
- [Impact](#impact)
- [Issues](#issues)
- [Screenshots](#screenshots)
- [References](#references)

_________________________________________________________________________________________________

## Overview

* Understand various methods to implement a SMTP server to use alongside your phishing Operations.
* Achieve scores as below to bypass modern MTA spam filters.
  - Surpass `9/10` in overall deliverability on [mail tester](https://mail-tester.com)
  - Score a spam score of `<0` on [SpamAssasin's Json api](https://spamcheck.postmarkapp.com/)
* Understand a Red Team mindset while engaging various SMTP relays.
* Spoofing and Evasion Tradecraft to reduce MTA Endpoint Detection.

_________________________________________________________________________________________________

## Introduction


The default ports used are 25, 465 (Exchange) and 587 that are meant to be used for submissions from your e-mail client to the e-mail server and higher ports are used for relaying between SMTP-server.

- Some verbatim assocaited with SMTP:
  - `Mail User Agent (MUA)`: This is a (part of a) program connecting to a SMTP-server in order to send an email. Most likely this is your Outlook, Thunderbird, whatever.
  - `Mail Transfer Agent (MTA)`: The transport service part of a program. They receive and transfer the emails. This might be an Exchange server, an internet facing gateway and so on.

A workflow of an email´s travel from one user to another would look like so: `MUA → MSA → MTA → internet → MTA → MDA → MUA`

A "relay" SMTP system receives mail from an SMTP client and transmits it, without modification to the message data other than adding trace information, to another SMTP server for further relaying or for delivery.

_________________________________________________________________________________________________

## Methods

Refer this [section here](/permalinks/PhishOPS/StartingPoint) to gain a brief understanding of MTA filter bypasses before you delve into any listed method.

**Below are detailed hyperlinks to each stated method**

1. __⭐[Build your Local SMTP Server on a VPS](/permalinks/PhishOPS/localsmtp)⭐__
2. __[Abuse an External Cloud Provided SMTP Server](/permalinks/PhishOPS/Cloudsmtp)__
3. __[Pentesting Exposed SMTP Relays & Exchange Servers](/permalinks/PhishOPS/directsmtp)__

**`NOTE:` All numbering below is in accordance to each specified method listed here**

_________________________________________________________________________________________________

## Spoofing and Evasion

- [Spoofing/Evasion Tradecraft to reduce MTA Endpoint Detection](/permalinks/PhishOPS/PhishNSpoof)

_________________________________________________________________________________________________

## Impact

Each method listed should be used accordingly when oppurtunity presents itself.
  1. The 1st method is less used and termed dated by the community in my opinion because of how cumbersome a process it is to build a fully functional SMTP server with all the checks to bypass modern MTA filters. 
  **I would mainly like to emphasize this method of building your own SMTP as it still does yield hearty results from what i've researched. It's a lengthy process but if done right can lead to great scores and can bypass most modern day spam filters improving its deliverability as time passes. Also spoofing would be possible and how good would depend on your creativity, this would work still without changing its deliverability/spam score.** 
  2. The fastest to setup and no brainer would be to use a cloud provided relay. This was fairly easy to setup and abuse for the past years as many providers offered free mail relays and the popular ones did offer good deliverability so you would be assured it would land through the primary/secondary inbox and not spam. But there are increased checks with the reputable cloud providers and the ones that accept easy sign up's have weakly setup SMTP relays making deliverability/spam score not up to the mark.
  3. When you find a vulnerable mail server/poorly configured open relay, blindly abuse the 3rd method to connect directly and engage the target SMTP as an open relay and abuse its trust internally by using the server to mail internal clients if such an oppurtunity where to arise. Pull up some [Hacktricks](https://book.hacktricks.xyz/pentesting/pentesting-smtp) and get on SMTP pentesting.
  Easy, but I feel it is a rarity to find such an implementation in today's day. If you do, you know what to do and you most likely would bypass the spam filter too and land in straight through the primary inbox abusing the interal MTA's trust and deliverability.

_________________________________________________________________________________________________

## Issues

1. Quite challenging to build and setup. 
2. Only the popular and reputable providers bypass most filters well and as stated have increased checks at play now to establish an infrastructure. Also they hinder with what you can and not do(spoofing, content, background checks).
3. This would be the best outcome that is if your target has an open relay or poor authentication blindly go for it. Sadly the number of open relays/exchange servers seems to decline in recent times.

_________________________________________________________________________________________________

## Screenshots

**SPF and DKIM Check**

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_33.png)

**Email Score and Placement**

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_34.png)

**Primary Inbox Check: Gmail**

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_36.png)
  
**Primary Inbox Check: Yahoo**

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_37.png)

**SpamAssasin API**

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_35.png)


_________________________________________________________________________________________________

## References

- [Alexander Færøy's hosting providers repo](https://gitlab.torproject.org/ahf)
- [h4ms1k - Attacking Exchange Servers](https://h4ms1k.github.io/Red_Team_exchange/)
- [linuxbabe/setup-basic-postfix-mail-sever-ubuntu](https://www.linuxbabe.com/mail-server/setup-basic-postfix-mail-sever-ubuntu)
- [luemmelsec/Pentest-Everything-SMTP](https://luemmelsec.github.io/Pentest-Everything-SMTP/)
- [Hacktricks - Pentesting SMTP](https://book.hacktricks.xyz/pentesting/pentesting-smtp)
- Some useful automated scripts:
  - [n0pe-sled/Postfix-Server-Setup](https://github.com/n0pe-sled/Postfix-Server-Setup/blob/master/ServerSetup.sh) 
  - [jamesm0rr1s/Mail-Server-Setup-for-Phishing](https://github.com/jamesm0rr1s/Mail-Server-Setup-for-Phishing)

