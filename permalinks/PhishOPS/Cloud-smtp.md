---
title: External Cloud Relays
permalink: /permalinks/PhishOPS/Cloud-smtp
categories: RedTeaming
---


<h1 align="center">Use an external cloud provided SMTP server</h1>

These days abusing/ using SMTP servers provided by cloud hostings for phishing/spam has become more strict due to the recent constant abuse.

_I will be detailing `Gmail SMTP` as the cloud SMTP example here, it's on you to find another provider for better delivery/ your needs._


_________________________________________________________________________________________________


## Setting up GMails external SMTP:

- GMails SMTP details are as follows:

 > Gmail SMTP server address: SMTP.gmail.com.

 > Gmail SMTP name: Your full name.
 
 > Gmail SMTP username: Your full Gmail address (e.g. you@gmail.com)
 
 > Gmail SMTP password: The password that you use to log in to Gmail.

 > Gmail SMTP port (TLS): 587.

 > Gmail SMTP port (SSL): 465


- Visit https://myaccount.google.com/lesssecureapps and enable "Less secure app access" to your gmail account.

- Setting up GMails SMTP with GoPhish:

 > Login to your GoPhish server --> Sending Profiles --> New Profile.

   - Setup the config as detailed in the start.

   ![Image](https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/images/cloud-smtp-1.png)

   - Test the email. 

![Image](https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/images/cloud-smtp-2.png)

![Image](https://github.com/m3rcer/Red-Team-SMTP-Spam-Filter-Bypass/blob/main/images/cloud-smtp-3.png)

_________________________________________________________________________________________________

## Inference:

The mails should land through the spam into the primary like cheese with delivery depending on the cloud provider. In this case it would be 100% since GMails delivery would be so.

GMail and most other cloud providers offer roughly around 300 mails or so on a daily basis for free. Plans with payment can be used to upgrade and improve the number of mails sent per day.

_________________________________________________________________________________________________









