---
title: Spoofing and Evasion 
permalink: /permalinks/PhishOPS/PhishNSpoof
categories: RedTeaming
---

<h1 align="center">Spoofing and Evasion with SMTP Builds</h1> 

_________________________________________________________________________________________________

**`NOTE:` All numbering below is in accordance to methods listed previously**

## INDEX

- [Spoofing](#spoofing)
     - [1: A Locally built SMTP server](#spoofing)
     - [2: A Cloud Provided Relay](#spoofing)
     - [3: An Open Relay](#spoofing)
- [Evasion](#evasion)
     - [1: A Locally built SMTP server](#spoofing)
     - [2: A Cloud Provided Relay](#spoofing)
     - [3: An Open Relay](#spoofing)


_________________________________________________________________________________________________

## Spoofing

### 1: A Locally built SMTP server

Let's say we want to impersonate _Facebook/Instagram._ All you have to do once the local SMTP is up an running as discussed [previously](https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/local_smtp.md) is as mentioned below.

_(Optional):_ Create a user account on your vps server in accordance to your spoofed account if you want to add the functionality to recieve emails too. 

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/spoof1.png)

Setup a `New Sending Profile` on GoPhish or a similar MUA (Mail User Agent) of choice as follows:

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/spoof2.png)
  
Fire-Away using the `New Profile`.

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_36.png)

### 2: A Cloud Provided Relay

To spoof all you need to do is register a *username* in accordance with your target . 

For example registering trying a popular MTA like gmail for Facebook@gmail.com would result in the username already taken. Find a cloud provider not as popular as GMail, like Mailjet, Mailgun, Sendgrid and other providers until that target host to spoof is not registered/taken for that provider. In short maybe you could spoof facebook on a less popular SMTP provider like Mailgun if nobody has registered with the username of "Facebook".  The best you could do if your using a popular cloud provider like gmail is make an account that resembles "Facebook" as close as possible like fac3book24x7@gmail.com. That's the closest to Facebook I could find on a popular cloud SMTP provider like gmail. 

So finding a provider in accordance to your campaign and finding the balance in deliverability is hard as not all providers provide A+ delivery to land through the spam folder and not all provide mail names that are available to abuse alongside your chosen campaigns espesicially the popular ones like Facebook.

The header spoofing feature as detailed in the image for the local SMTP is not possible here as we do not completely control the MTA SMTP to do this. 

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/spoof3.png)

### 3: An Open Relay

This basically depends on you [pentesting](https://book.hacktricks.xyz/pentesting/pentesting-SMTP) the open relay, finding some sort of credentials/misconfigurations to take advantage of, enumerating possible usernames and abusing their inbound trust relationships.

__________________________________________________________________________________________________

## Evasion

### 1: A Locally built SMTP server

_Remove sensitive information from email headers with postfix -->_

- To get started, make a small file with regular expressions in `/etc/postfix/header_checks`:
     ```bash
     /^Received:.*with ESMTPSA/              IGNORE
     /^X-Originating-IP:/    IGNORE
     /^X-Mailer:/            IGNORE
     /^Mime-Version:/        IGNORE
     ```
- The “ESMTPSA” match works for me because I only send email via port 465. I don’t allow SASL authentication via port 25. You may need to adjust the regular expression if you accept SASL authentication via SMTP.
- Now, add the following two lines to your `/etc/postfix/main.cf`:
     ```bash
     mime_header_checks = regexp:/etc/postfix/header_checks
     header_checks = regexp:/etc/postfix/header_checks
     ```
- Rebuild the hash table and reload the postfix configuration:
     ```bash
     postmap /etc/postfix/header_checks
     postfix reload
     ```
- Now, send a test email. View the headers and you should see the original received header (with your client IP address) removed, along with details about your mail client.


### 2: A Cloud Provided Relay

_(Optional: If anonymity is priority)_ This is pretty simple. When connecting to any cloud provided Management Interface for your SMTP provided relay always do so through anonymity using a method of choice like a VPN/proxychains/sock5 proxy etc as IP's are logged on most provider sites. Use Crypto support to anonymize payments if they provide such options. 

### 3: An Open Relay

_(Optional: If anonymity is priority)_ Same applies as above. When connecting to an open relay always do so through anonymity using a method of choice like a VPN/proxychains/sock5 or even better using a compromised workstation as a socks proxy.

_________________________________________________________________________________________________



