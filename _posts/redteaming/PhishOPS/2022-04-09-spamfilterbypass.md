---
title: Building A Local SMTP Server To Bypass Modern Spam Filters
date: 2022-06-09 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
#layout: post
description: Building a local SMTP server to bypass spam filters
---

**This Blog details setting up a local SMTP phishing server on a VPS from scratch to implement SPF,DKIM,DMARC and other modern checks to bypass modern spam filters like gmail, yahoo, outlook etc and to send/recieve unlimited mails for free as long as the hosting provider permits it.**

Refer to this [Starting Point Section](/permalinks/PhishOPS/StartingPoint) to understand the theory and techniques to bypass modern spam filters. 

## Introduction

The default ports used by SMTP are 25, 465 (Exchange) and 587 that are meant to be used for submissions from your e-mail client to the e-mail server and higher ports are used for relaying between SMTP-server.

- Some verbatim assocaited with SMTP:
  - `Mail User Agent (MUA)`: This is a (part of a) program connecting to a SMTP-server in order to send an email. Most likely this is your Outlook, Thunderbird, whatever.
  - `Mail Transfer Agent (MTA)`: The transport service part of a program. They receive and transfer the emails. This might be an Exchange server, an internet facing gateway and so on.

A workflow of an email´s travel from one user to another would look like so: `MUA → MSA → MTA → internet → MTA → MDA → MUA`

A "relay" SMTP system receives mail from an SMTP client and transmits it, without modification to the message data other than adding trace information, to another SMTP server for further relaying or for delivery.

-------------------------------------------------------------------------

## Screenshots

### Primary Inbox Check

#### Gmail

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_36.png)

#### Yahoo

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_37.png)

### SPF and DKIM Check

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_33.png)

### Email Score and Placement

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_34.png)

----------------------------------- 

## Index

***I will be breaking build this into 3 broad stages***

- [Setup Prerequisites](#setup-prerequisites)
- [STAGE 1](#stage-1)
  - [Setting up a Message Transport System (MTS) aka SMTP server (Postfix)](#setting-up-a-message-transport-system-mts-aka-smtp-server-postfix)
    - [Set Hostname and DNS records](#set-hostname-and-dns-records)
    - [Permanently disable ipv6 and uninstall unecessary services like exim](#permanently-disable-ipv6-and-uninstall-unecessary-services-like-exim)
    - [PTR record (rDNS)](#ptr-record-rdns)
    - [Installing Postfix](#installing-postfix)
- [STAGE 2](#stage-2)
  - [Install an IMAP SERVER (Dovecot), enable TLS encryption and setup a Desktop client](#install-an-imap-server-dovecot-enable-tls-encryption-and-setup-a-desktop-client)
    - [Getting TLS encryption and a certificate the easy way](#getting-tls-encryption-and-a-certificate-the-easy-way)
    - [Installing/configuring the IMAP Server - Dovecot](#installingconfiguring-the-imap-server---dovecot)
    - [Configuring the Authentication Mechanism](#configuring-the-authentication-mechanism)
    - [Setting up SASL Authentication](#setting-up-sasl-authentication)
    - [Setting up the Desktop Email Client](#setting-up-the-desktop-email-client)
- [STAGE 3](#stage-3)
  - [Setting up SPF and DKIM with Postfix](#setting-up-spf-and-dkim-with-postfix)
    - [Setting and configuring SPF](#setting-and-configuring-spf)
    - [Setting up DKIM](#setting-up-dkim)
    - [Generate Private and Public Keypairs](#generate-private-and-public-keypairs)
    - [Publish Your Public Key in the DNS Records](#publish-your-public-key-in-the-dns-records)
    - [Connect Postfix to OpenDKIM](#connect-postfix-to-opendkim)
- [Spoofing](#spoofing)
- [Evasion](#evasion)
- [Validation and checks](#validation-and-checks)

_________________________________________________________________________________________________

## Setup Prerequisites

__Note: For this example I've used the following and would recommend a similar setup -->__

  * Ubuntu 20.04LTS as my distro.
  * Gmail as the testing mail service.
  * Namecheap as my domain hosting provider.
  * Thunderbird as my desktop client for testing.
  * GoPhish/CobaltStrike as my phish client.
  * Disabled any firewall rules against ports 25,587,80,443,465,143,993,110,995.
  * A reputed domain name with no blacklists aged over 3 months.              

_________________________________________________________________________________________________

## __STAGE 1__ 

## Setting up a Message Transport System (MTS) aka SMTP server (Postfix)

Postfix is a light, easy to use MTS which serves 2 primary purposes:
- Transporting email messages from a mail client/mail user agent (MUA) to a remote SMTP server.
- Accepts emails from other SMTP servers. 

We will configure postfix for a single domain in this tutorial. Before we install postfix note to do the following before. 

### Set Hostname and DNS records

Postfix uses the server's hostname to identify itself when communicating with other MTAs. A hostname could be a single word or a FQDN.

_Note: We will use `example.com` as our registered domain as an example here_.

Make sure your hostnames set to a FQDN such as `mail.example.com` by using the command: `sudo hostnamectl set-hostname mail.example.com`

Gracefully reboot your server using `init 6` after.

**Set up DNS records**

MX records tell other MTA's that your mail server `mail.example.com` is responsible for email delivery for your domain name.

```
MX record    @           mail.example.com
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/mx_record.png)

An A record maps your FQDN to your IP address.

```
mail.example.com        <ip-addr>
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/a_record.png)


### Permanently disable ipv6 and uninstall unecessary services like exim

Ipv6 is tricky to configure along w ipv4 and just adds a weighted overhead . For example , you'd have to create a seperate reverse dns entry for ipv6 along w the ipv4 else gmail mail servers are bound to reject you. 

Exim or any other mail services that come by default packaged with some distributions like debian 8 . They'd hinder the installation of another mail service . So uninstall any unwanted mail service of the kind if they exist on your distro prepackaged.

__Permanently disable ipv6:__

Edit the **/etc/sysctl.conf** configuration file by adding the following lines: `vi /etc/sysctl.conf`

```bash
net.ipv6.conf.all.disable_ipv6=1
net.ipv6.conf.default.disable_ipv6=1
```

This works on ubuntu 20.04, If it dosen't find an equivalent to disable ipv6 for your specific distro. A recommened method would be using grub too. Check this [article](https://itsfoss.com/disable-ipv6-ubuntu-linux/) for more details .

### PTR record (rDNS)

Your PTR record does the inverse, ie maps your IP address back to your FQDN. This is as crucial as it gets as MTA's like gmail and most out there will only accept mails through into the primary inbox if this is set right.

_This could be an option your hosting provider allows you to setup like how you did your domain records (`cockbox.org` uses this method ) or you'd have to probably contact support and they'd do it for you (`flokinet.is` works this way). Either case find a hosting provider that supports this._

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/ptr_record.png)

### Installing Postfix

Run this in a terminal:

```bash
sudo apt-get update

sudo apt-get install postfix -y
```

While installation you will be asked to select a type for mail configuration. Select `Internet Site`. This option allows Postfix to send emails to other MTAs and receive emails from other MTAs.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_1.png)

Next enter your domain name when prompted for the system mail name as your domain name without __"mail"__ ie just `example.com`. 

This ensures that your mail address naming convention would be in the form of -

[-] `name@example.com` and not,

[x] `name@mail.example.com`. 

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_2.png)

Use a valid subdomain replacement if you would need to implement one, it will work. 

Once installation is complete a `/etc/postfix/main.cf` config file would be automatically generated along with postfix starting up.

Check your current Postfix version using `postconf mail_version`.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_4.png)

Use `Socket Statistics` - `ss` utility to check if postfix is running on port 25 succesfully: `sudo ss -lnpt | grep master`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_3.png)

If you'd like to view the various binaries shipped along with postfix check them out with `dpkg -L postfix | grep /usr/sbin/` 

`Sendmail` is a binary place at `/usr/sbin/sendmail` which is compatible with postfix.

Send out your first testmail to your test email account using : `echo "test email" | sendmail your-test-account@gmail.com`

Or you could install mailutils using `sudo apt-get install mailutils` . Just type `mail` and follow along the prompts entering the required fields and hitting `Ctrl+D` once done to send the mail.

*Note:* The email might land through into your primary right away but could be potentially flagged by other stronger MTA's and their spam filters. We will be comparing the spam score at each stage to see the overall improvement in deliverability.

Incase your hosting provider has blocked outbound port 25, verify it using: `telnet gmail-SMTP-in.l.google.com 25`

If you see a status showing `"Connected" --> outbound 25 works succesfully`. Use `quit` to quit the command.

Head on over to your gmail inbox and open up the mail. Click on the drop down below the `Printer icon` to the right as shown in the screenshot --> next click on `show original`. --> next click on the `Copy to clipboard` button to copy all contents.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_5.png)

Head on over to `https://spamcheck.postmarkapp.com/` and paste your contents in and check your spam score.

_Note the score over each stage ._

-------------------------------------------------------------------------------------------------

## __STAGE 2__

## Install an IMAP SERVER (Dovecot), enable TLS encryption and setup a Desktop client

### Getting TLS encryption and a certificate the easy way

TLS encryption is mandatory and ensures secured delivery. `LetsEncrypt` offers a free certificate with assisstance from their client - `certbot`.

Head on over to `https://certbot.eff.org/`. Click on  `Get Certbot instructions`.

Select your server as the Software and which distro your running on system. In my case as I said before im using Apache2 and Ubuntu20.04LTS.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/certbot1.png)

Follow along the instructions to succesfully install certbot and when you reach an instruction such as `sudo certbot --apache` you will be prompted for the domains and subdomains to enable TLS on along with an administrative mail contact. Fill them as your hosting needs. 

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/certbot-setup2.png)

You will then find your certificates in `/etc/letsencrypt/live/example.com/`.

_Note: Use `fullchain.pem` as the supplied certificate and `privkey.pem` as the key . `Fullchain.pem` is a concatenation of `cert.pem` and `chain.pem` in one file._

All your TLS certificates will now be live and the config automatically replaced in your respective web servers config. Renew or set a cronjob to renew your certificates periodically as listed by certbot.

### Enable Submission Service in Postfix

To send emails from a desktop email client, we need to enable the submission service of Postfix so that the email client can submit emails to Postfix SMTP server. 

Edit the `master.cf` file using your favorite text editor as follows. Im using `vim` as my editor: `sudo vi /etc/postfix/master.cf`

In the `submission` section, uncomment the `submission...` line and  add the following lines (the 2nd line on) as stated here below it. 
This method ensures no bad tabs/spaces causing the config to error out. (Be careful editting this)

```bash
submission     inet     n    -    y    -    -    SMTPd
  -o syslog_name=postfix/submission
  -o SMTPd_tls_security_level=encrypt
  -o SMTPd_tls_wrappermode=no
  -o SMTPd_sasl_auth_enable=yes
  -o SMTPd_relay_restrictions=permit_sasl_authenticated,reject
  -o SMTPd_recipient_restrictions=permit_mynetworks,permit_sasl_authenticated,reject
  -o SMTPd_sasl_type=dovecot
  -o SMTPd_sasl_path=private/auth
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/tls1.png)

This configuration enables the submission daemon of Postfix and requires TLS encryption so that we can later connect using a desktop client. This listens on port:587 by default. 

To use Microsoft Outlook as a desktop client listening over port:465. Then you need to do the same and enable the submission daemon over port 465.

Uncomment the `SMTPs..` line as before and paste the follows below it:

```bash
SMTPs     inet  n       -       y       -       -       SMTPd
  -o syslog_name=postfix/SMTPs
  -o SMTPd_tls_wrappermode=yes
  -o SMTPd_sasl_auth_enable=yes
  -o SMTPd_relay_restrictions=permit_sasl_authenticated,reject
  -o SMTPd_recipient_restrictions=permit_mynetworks,permit_sasl_authenticated,reject
  -o SMTPd_sasl_type=dovecot
  -o SMTPd_sasl_path=private/auth
```

Save and close the file.

Next, we need to specify the location of the previously before generated TLS certificate and private key in the Postfix config file. To do this we need to edit the `main.cf` conf file: `sudo vi /etc/postfix/main.cf`

Delete/Comment-out any previous TLS parameters and edit the TLS parameters as follows. Add the TLS param code block from before and replace `SMTPd_tls_cert_file` with the full path to your `fullchain.pem`. Or just replace `example.com` with your domain name if you're using Ubuntu like me.

```bash
#Enable TLS Encryption when Postfix receives incoming emails
SMTPd_tls_cert_file=/etc/letsencrypt/live/example.com/fullchain.pem
SMTPd_tls_key_file=/etc/letsencrypt/live/example.com/privkey.pem
SMTPd_tls_security_level=may 
SMTPd_tls_loglevel = 1
SMTPd_tls_session_cache_database = btree:${data_directory}/SMTPd_scache

#Enable TLS Encryption when Postfix sends outgoing emails
SMTP_tls_security_level = may
SMTP_tls_loglevel = 1
SMTP_tls_session_cache_database = btree:${data_directory}/SMTP_scache

#Enforce TLSv1.3 or TLSv1.2
SMTPd_tls_mandatory_protocols = !SSLv2, !SSLv3, !TLSv1, !TLSv1.1
SMTPd_tls_protocols = !SSLv2, !SSLv3, !TLSv1, !TLSv1.1
SMTP_tls_mandatory_protocols = !SSLv2, !SSLv3, !TLSv1, !TLSv1.1
SMTP_tls_protocols = !SSLv2, !SSLv3, !TLSv1, !TLSv1.1
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_6.png)

Save and close the file and restart Postfix. Now run the following command to verify if Postfix is listening on port 587 (port 465 if you've configured outlook too).

`sudo systemctl restart postfix`

`sudo ss -lnpt | grep master`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_7.png)


### Installing/configuring the IMAP Server - Dovecot

Enter the following command to install Dovecot's core packages and the IMAP daemon package on your Ubuntu/custom server: `sudo apt install dovecot-core dovecot-imapd`

To setup `POP3` to fetch emails, install the `dovecot-pop3d` package as follows next: `sudo apt install dovecot-pop3d`

Check the version of Dovecot: `dovecot --version`

**Enabling IMAP/POP3/LMTP Protocol**

You can enable and use any protocol depending on your setup and the way you'd like to recieve and manage the mail system. Enabling atleast one is mandatory.

`IMAP/POP3`:

Edit the main dovecot config file using: `sudo vi /etc/dovecot/dovecot.conf`

Add/append the following line to enable both the IMAP and POP3 protocol: `protocols = imap pop3`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_8.png)

**Configuring the Mailbox Location**

By default, Postfix and Dovecot use the `mbox format` to store emails. By default each user’s emails are stored in a single file in `/var/mail/username`. To change it to use the `Maildir format` where email messages will be stored under the Maildir directory under each respective user’s home directory for easy management follow along: `sudo vi /etc/dovecot/conf.d/10-mail.conf`

Find and change the `mail_location` to the value as follows: `mail_location = maildir:~/Maildir`

Also append the following line to the file. If you're on Ubuntu 18.04+ this line is automatically added so you dont have to enter it: `mail_privileged_group = mail`

Save and close the file. Now create/add dovecot to the mail group so that Dovecot can read theINBOX using: `sudo adduser dovecot mail`

Although we configured Dovecot to store emails in the `Maildir format`, by default Postfix uses its built-in local delivery agent (LDA) to move inbound emails to the message store and it will be saved in the `mbox format`.

To avoid this we also configure Postfix to pass incoming emails to Dovecot using the LMTP protocol. This is a simplified version of SMTP where incoming emails will be saved in the required "Maildir format" we've setup to use.

Now install the Dovecot LMTP server using: `sudo apt install dovecot-lmtpd`

Lets Edit the Dovecot main configuration file to set this up: `sudo vi /etc/dovecot/dovecot.conf`

Add `lmtp` to the supported protocols as before: `protocols = imap pop3 lmtp`

(I've set all to run in this example)

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_9.png)

Save and close the file.

Its now time to edit the `Dovecot 10-master.conf` file: `sudo vi /etc/dovecot/conf.d/10-master.conf`

Find and replace/comment out the `lmtp` service definition to the following:

```bash
service lmtp {
 unix_listener /var/spool/postfix/private/dovecot-lmtp {
   mode = 0600
   user = postfix
   group = postfix
  }
}
``` 

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_10.png)

Now, edit the Postfix main configuration file: `sudo vi /etc/postfix/main.cf`

Append the following lines to the end of the file to deliver incoming emails to the local message store via the Dovecot LMTP server and disable `SMTPUTF8`.

```bash
mailbox_transport = lmtp:unix:private/dovecot-lmtp
SMTPutf8_enable = no
```
Save and close the file.


### Configuring the Authentication Mechanism

Lets start by editing the authentication config file: `sudo vi /etc/dovecot/conf.d/10-auth.conf`

Uncomment/add the following lines: `disable_plaintext_auth = yes`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_11.png)

This will disable plaintext authentication when there’s no SSL/TLS encryption for added security and no fallback to vulnerable versions.

`#auth_username_format = %Lu` and change its value to --> `auth_username_format = %n`. This is required as we setup canonical mailbox users.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_12.png)

`auth_mechanisms = plain` and change its value to --> `auth_mechanisms = plain login`. This only enables the PLAIN authentication mechanism.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_13.png)

__Configuring SSL/TLS Encryption__

Edit the SSL/TLS config file as follows: `sudo vi /etc/dovecot/conf.d/10-ssl.conf`

Find and change the value of `ssl = yes` to `ssl = required`

Find and change the value of `#ssl_prefer_server_ciphers = no` to `ssl_prefer_server_ciphers = yes` 

Disable outdated and inscure SSLv3, TLSv1 and TLSv1.1 by adding the following line to the end of the file: `ssl_protocols = !SSLv3 !TLSv1 !TLSv1.1`

Next find the following lines:

```bash
ssl_cert = </etc/dovecot/private/dovecot.pem
ssl_key = </etc/dovecot/private/dovecot.key
```

Replace them with the perviously generated location of your Let’s Encrypt TLS certificate and private key.

It would be as follows:

```bash
ssl_cert = </etc/letsencrypt/live/example.com/fullchain.pem
ssl_key = </etc/letsencrypt/live/example.com/privkey.pem
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_14.png)

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_15.png)


### Setting up SASL Authentication

Edit the `10-master.conf` file as before: `sudo vi /etc/dovecot/conf.d/10-master.conf`

Change `service auth` section to the following so that Postfix can find the appropriate Dovecot authentication server.

```bash
service auth {
    unix_listener /var/spool/postfix/private/auth {
      mode = 0660
      user = postfix
      group = postfix
    }
}
```
![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_16.png)

Save and close the file.

__Auto-create Sent and Trash Folder__

Edit the following config file: `sudo vi /etc/dovecot/conf.d/15-mailboxes.conf`

Now to `auto-create` a specific section just append the following inside each respective code block: `auto = create`

Example: To auto-create the Trash folder in your client-->

```bash
 mailbox Trash {
    auto = create
    special_use = \Trash
 }
```

By default its good practice to enable common folders such as - "Drafts, Junk, Sent, Trash" for better usage and tracking of the mails sent and recieved.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_17.png)

Save the file and restart Postfix and Dovecot: `sudo systemctl restart postfix dovecot`

**We are almost done with stage 2!**

Dovecot will be listening on port 143 (IMAP) and 993 (IMAPS).

`sudo ss -lnpt | grep dovecot`

`systemctl status dovecot`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_18.png)


### Setting up the Desktop Email Client

I've setup Thunderbird as my Desktop client and would recommend so.

Install it using:
- On windows : [Go here](https://www.thunderbird.net/en-US/)
- On NIX: `sudo apt install thunderbird`

Run Thunderbird. You'd most likely see a popup stating to setup your mail account if not go to `Edit -> Account Settings -> Account Actions -> Add Mail Account` to add a mail account. Click on Configure manually and setup as follows:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_19.png)

Select the IMAP protocol; Enter `mail.example.com` as the server name; Choose port 143 and `STARTTLS`; Choose `normal password` as the authentication method.

_Note: You can also use port 993 with SSL/TLS encryption for IMAP, and use port 465 with SSL/TLS encryption for SMTP if you've set this up with Microsoft Outlook._

You will now be able to connect to your setup mail server and finally send and receive emails with any external desktop email client using your mail server as a secure encrypted relay!

Send a test mail and enter your credentials to ensure your setups up and working fine.

You can now Create various Users on your VPS mail sevrer and create various associated mail accounts for sending/recieving capability.

`sudo adduser -m Yahoo` --> Add user with home directory

You can list all available mailbox users with: `sudo doveadm user '*'`

It's advisable to restart Dovecot each time you add users.

And STAGE 2 is complete!

TroubleShooting tips:
- If you get a Relay access denied error it's most likely that our VPS hosting provider dosen't allow relay over these ports.
- If you use the Cloudflare DNS service, you should not enable the CDN (proxy) feature when creating DNS an A record and an AAAA record for the hostname of your mail server as Cloudflare dosen't support SMTP or IMAP proxy.

Let's check our spam score:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_38.png)

Let's improve on this.

-------------------------------------------------------------------------------------------------

# Stage 3

## Setting up SPF and DKIM with Postfix

We finally have a working Postfix SMTP server and Dovecot IMAP server with which we can send and receive email using any external email client like a desktop client (thunderbird).

Although we have correctly set up our DNS MX, A and PTR records our emails are still flagged as spam by strong and popular email services such as Gmail and Outlook mail.

As we all know most of our targets would be using such mail services so to succesfully bypass most strong spam filters its mandatory to set up a SPF and DKIM record as explained before.

And we begin,

### Setting and configuring SPF

Get back to your respective domain management interface for DNS and create a new TXT record as follows: `TXT  @   v=spf1 mx ~all`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_20.png)
> v=spf1: indicates that this is an SPF record and the SPF record version we are using is SPF1.

> mx: means all hosts listed in the MX records are allowed to send emails for your domain and any other hosts are disallowed.

> \~all: indicates that emails from your domain should only come from hosts specified in the SPF record.
Use the following command to verify you've succesfully added the record: `dig example.com txt +short`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_21.png)

**Configuring SPF Policy Agent**

We now need to tell Postfix to check for SPF records of incoming emails. This doesn’t help ensure outgoing email delivery but helps with detecting forged incoming emails.

Install the required packages: `sudo apt install postfix-policyd-spf-python`

Next, edit the Postfix master process configuration file: `sudo vi /etc/postfix/master.cf`

Now append the following to the end of the file:

```bash
policyd-spf  unix  -       n       n       -       0       spawn
    user=policyd-spf argv=/usr/bin/policyd-spf
```
![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_22.png)

Save and close the file. Next, edit the Postfix main configuration file: `sudo vi /etc/postfix/main.cf`

Append the following lines at the end of the file as before:

```bash
policyd-spf_time_limit = 3600
SMTPd_recipient_restrictions =
   permit_mynetworks,
   permit_sasl_authenticated,
   reject_unauth_destination,
   check_policy_service unix:private/policyd-spf
```
This will impose a restriction on incoming emails by rejecting unauthorized email and checking SPF record.

Save and close the file and restart Postfix.

`sudo systemctl restart postfix`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_23.png)

When you receive an email from a domain that has an SPF record the next time, you can see the SPF check results in the raw email header. It would be as follows: `Received-SPF: Pass (sender SPF authorized).`

### Setting up DKIM

Install OpenDKIM which is an open-source implementation of the DKIM sender authentication system using: `sudo apt install opendkim opendkim-tools`

Next add postfix user to the opendkim group: `sudo gpasswd -a postfix opendkim`

Edit the OpenDKIM main configuration file as follows: `sudo vi /etc/opendkim.conf`

Uncomment the following lines and replace simple with `relaxed/simple`:

```bash
Canonicalization   simple
Mode               sv
SubDomains         no
```
Next, add the following lines below `#ADSPAction` continue line. If your file doesn’t have `#ADSPAction` continue line, then just add them below `SubDomains  no`.

```bash
AutoRestart         yes
AutoRestartRate     10/1M
Background          yes
DNSTimeout          5
SignatureAlgorithm  rsa-sha256
```
![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_24.png)

Add the following lines at the end of this file if you're on a different distro. (Note that On Ubuntu 20.04, this is already set)

```bash
#OpenDKIM user
# Remember to add user postfix to group opendkim
UserID             opendkim
```

Finally append this too to the end of the file, Save and close it.

```bash
# Map domains in From addresses to keys used to sign messages
KeyTable           refile:/etc/opendkim/key.table
SigningTable       refile:/etc/opendkim/signing.table

# Hosts to ignore when verifying signatures
ExternalIgnoreList  /etc/opendkim/trusted.hosts

# A set of internal hosts whose mail should be signed
InternalHosts       /etc/opendkim/trusted.hosts
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_25.png)

__Create Signing Table, Key Table and Trusted Hosts File__

Create a directory structure for OpenDKIM as follows:

`sudo mkdir /etc/opendkim`

`sudo mkdir /etc/opendkim/keys`

Let's change the owner from root to opendkim and make sure only the opendkim user can read and write to the keys directory:

```bash
sudo chown -R opendkim:opendkim /etc/opendkim

sudo chmod go-rw /etc/opendkim/keys
```

Now, create the signing table: `sudo vi /etc/opendkim/signing.table`

Append this line. This tells OpenDKIM that if a sender on your server is using a `@example.com` address, then it should be signed with the private key identified by `default._domainkey.example.com`. Replace `example.com` with your domain: `*@example.com    default._domainkey.example.com`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_26.png)

Save and close the file. Next create the key table: `sudo vi /etc/opendkim/key.table`

Append the following: `default._domainkey.example.com     example.com:default:/etc/opendkim/keys/example.com/default.private`

This tells the location of the private key.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_27.png)

Save and close the file. 

Now, create the trusted hosts file: `sudo vi /etc/opendkim/trusted.hosts`

Append the following lines to the newly created file. This tells OpenDKIM that if an email is coming from localhost or from the same domain, then OpenDKIM should not perform DKIM verification on the email.

```bash
127.0.0.1
localhost

*.example.com
```
Save and close the file.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_28.png)

### Generate Private and Public Keypairs

Since DKIM is used to sign outgoing messages and verify incoming messages, we need to generate a private key for signing and a public key for remote verification. 

The Public key will be published in DNS.

Let's begin by creating a separate folder for the domain as follows: `sudo mkdir /etc/opendkim/keys/example.com`

Now generate keys using opendkim-genkey tool: `sudo opendkim-genkey -b 2048 -d example.com -D /etc/opendkim/keys/example.com -s default -v`

Make opendkim as the owner of the private key: `sudo chown opendkim:opendkim /etc/opendkim/keys/example.com/default.private`

### Publish Your Public Key in the DNS Records

Grab the public key using: `sudo cat /etc/opendkim/keys/example.com/default.txt`

_Note: The string after the `p parameter` is the public key._

Now copy everything in the between the parentheses and paste it creating a new DNS record in your domain DNS config as follows:

_Note: Delete all double quotes and white spaces in the value field if any using some sed magic._

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_29.png)

Finally, Lets test the DKIM Key: `sudo opendkim-testkey -d example.com -s default -vvv`

 You will see `Key OK` in the command output if all goes well until here.

```bash
opendkim-testkey: using default configfile /etc/opendkim.conf
opendkim-testkey: checking key 'default._domainkey.your-domain.com'
opendkim-testkey: key secure
opendkim-testkey: key OK
```

It may take time for your DKIM record to propagate over the Internet depending on your domain provider.

_Note: If you happen to see `Key not secure` in the command output, this is because DNSSEC isn’t enabled on your domain name. DNSSEC is a security standard for secure DNS query. Most domain names haven’t enabled DNSSEC by default. There’s no need change this for now._ 

### Connect Postfix to OpenDKIM

Postfix can talk to OpenDKIM via a Unix socket file. The default socket file used by OpenDKIM runs in a chroot jail. So we need to change the OpenDKIM Unix socket file.

Create a directory to hold the OpenDKIM socket file and allow only the opendkim user and the postfix group to access it:

```bash
sudo mkdir /var/spool/postfix/opendkim

sudo chown opendkim:postfix /var/spool/postfix/opendkim
```

Then edit the OpenDKIM main configuration file: `sudo vi /etc/opendkim.conf`

Find the following line (Ubuntu 20.04): `Socket    local:/run/opendkim/opendkim.sock` or `Socket    local:/var/run/opendkim/opendkim.sock` (for Ubuntu 18.04)

Replace it with the following line (If you can’t find the above line, then add the following line.): `Socket    local:/var/spool/postfix/opendkim/opendkim.sock`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_30.png)

Similarly, find the following line in the `/etc/default/opendkim` file: `sudo vi /etc/default/opendkim`

`SOCKET="local:/var/run/opendkim/opendkim.sock"` or `SOCKET=local:$RUNDIR/opendkim.sock`

Change it to:

`SOCKET="local:/var/spool/postfix/opendkim/opendkim.sock"`

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_31.png)

Save and close the file.

Alas, we need to edit the Postfix main configuration file: `sudo vi /etc/postfix/main.cf`

Append the following lines to the end of this file.  Postfix will now be able to call OpenDKIM via the milter protocol.

```bash
# Milter configuration
milter_default_action = accept
milter_protocol = 6
SMTPd_milters = local:opendkim/opendkim.sock
non_SMTPd_milters = $SMTPd_milters
```

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_32.png)

Save and close the file. Then restart Opendkim and the Postfix service: `sudo systemctl restart opendkim postfix`

**AND FINALLY, WE ARE DONE!.**

_________________________________________________________________________________________________

## Spoofing

Let's say we want to impersonate _Facebook/Instagram._ Make sure to buy domain names such as `support-services.com` that go in accordance with your campaign so that the spoofed sender will be in the form of `facebook@support-services.com` for this example.

- _(Optional):_ Create a user account on your vps server in accordance to your spoofed account if you want to add the functionality to recieve emails too. 

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/spoof1.png)

- Setup a `New Sending Profile` on GoPhish or a similar MUA (Mail User Agent) of choice as follows:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/spoof2.png)
  
- Fire-Away using the `New Profile`.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_36.png)


_________________________________________________________________________________________________


## Evasion

__Remove sensitive information from email headers with postfix -->__

- To get started, make a small file with regular expressions in `/etc/postfix/header_checks`:
     ```bash
     /^Received:.*with ESMTPSA/              IGNORE
     /^X-Originating-IP:/    IGNORE
     /^X-Mailer:/            IGNORE
     /^Mime-Version:/        IGNORE
     ```
- The `ESMTPSA` match works for me because I only send email via port 465. I don’t allow `SASL authentication` via port 25. You may need to adjust the regular expression if you accept SASL authentication via SMTP.
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

_________________________________________________________________________________________________

## Validation and checks

#### Primary Inbox Check

#### Gmail

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_36.png)

#### Yahoo

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_37.png)


### SPF and DKIM Check

Send a test email from thunderbird/gophish or locally to your test Gmail Account and click on the drop down as before --> __show original__. 

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_33.png)


### Email Score and Placement

Go to https://www.mail-tester.com. You will see a unique email address. Send an email from your domain to this address and then check your score.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_34.png)

### SpamAssasin API

Go back to `https://spamcheck.postmarkapp.com/` as before. Go to `show original` as before and click on the `copy to clipboard` button to call the whole message and paste it in the Check score field on the site

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/PhishOPS/images/postfix_install_35.png)


_________________________________________________________________________________________________

































