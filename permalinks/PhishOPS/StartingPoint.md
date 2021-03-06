---
title: Requirements and good practices needed to bypass modern MTA spam filters
permalink: /permalinks/PhishOPS/StartingPoint
categories: RedTeaming
---

<h1 align="center">Starting Point</h1>

<h3 align="center">This section gives a short brief to understand various requirements/techniques to achieve spam filter bypasses and improve overall mail deliverability. This is an overall gist of good practices.</h3> 

## Find a hosting provider/ISP that allows an SMTP build

Finding a good hosting provider that supports all your needs to build an already configured SMTP server with relays is hard. Basically the ISP could support relays, asn, bridges and tor support, crypto depending on your needs. 
Do they have high reputation IP addresses? You definitely don’t want to be listed on the dreaded Microsoft Outlook IP blacklist or the SpamRats blacklist. Outbound port `25/587` is mandatory for allowing your SMTP server to send outbound and receive inbound mails. So be careful to select an ISP that provides a VPS that meets the criteria.

**[Here is the link](https://gitlab.torproject.org/legacy/trac/-/wikis/doc/GoodBadISPs)** that provides a list of all the available VPS's that fit the criteria explained above. Choose accordingly. All credits go to [Alexander Færøy](https://gitlab.torproject.org/ahf) for this wonderful Repository.

## Domain Name age

Domain age is one of the first factors checked by anti-spam filters. The general rule is **the older the domain, the more trustworthy it gets**, provided it has been used with care and following good practices of email deliverability. Keep in mind that your domain reputation equals your sender reputation.

## Permanently disable ipv6 and uninstall unnecessary services like exim

Ipv6 is tricky to configure along with ipv4 and just adds a weighted overhead. For example, you'd have to create a separate reverse DNS entry for ipv6 along w the ipv4 else Gmail mail servers are bound to reject you. 
Exim or any other mail services that come by default packaged with some distributions like debian 8. They'd hinder the installation of another mail service. So, uninstall any unwanted mail service of the kind also if they exist on your distro pre-packaged.

## Setting the hostname for your VPS

Some MTAs even query DNS to see if FQDN in the `smtpd` banner resolves to the IP of your mail server.

## Setting up rDNS/PTR record

A pointer record, or PTR record, maps an IP address to an FQDN. It’s the counterpart to the A record and is used for reverse DNS (rDNS) lookup.
Reverse resolution of IP address with PTR record can help with blocking spammers. Many MTAs accept email only if the server is really responsible for a certain domain. 
In short this is one of the most important factors that provides non repudiation to the domain as it proves that the domain belongs to that IP/VPS.

__This is one of the main factors that Gmail and other major MTA's consider, so the rdns setup is a must.__ Find a service provider that allows rDNS change support (Refer to above to find service providers of the sort). Some have an option to configure the record like DNS, others require you to contact support and they'd do it for you.

## Enable TLS encryption

TLS encryption is a good practice to enable as they ensure the mails originate and are transported in a secure way. Basically, a TLS certificate needs to be installed to secure the traffic. Grab a certificate for free using `letsencrypt` from `certbot`.  Certbot is quite straightforward with its installation instructions.

## Implementing an IMAP server/Desktop client

Install an IMAP server to be able to easily setup a desktop client to send and receive emails on the go remotely. This would work the same way you'd use a Desktop client to connect to a provider’s SMTP relay.  You can use a remote desktop client like Thunderbird and use your SMTP server as a secure relay.   __Dovecot__ is my personal choice and is quite easy to setup and allows mailboxes and their indexes to be modified by multiple computers at the same time, while still performing well. Also ensure that you have set the A record on your Domain Name to the IP address of the Server before you do so. 

## Adding Aliases

Once the server is up  we need to tell it where to send mail to and from . Aliases improve efficiency and improve handling of a lot of emails to default accounts re-routing it to the account of choice. Aliases let you specify an alternate name to any mail account. 

## Setting up SPF, DKIM, DMARC records for added delivery rate

Along with setting up the rDNS record which is mandatory these records add and improve delivery rate. I've seen my spam score rise significantly after adding these records  
- _SPF:_ A Sender Policy Framework (SPF) record is a DNS record that identifies specific mail servers that are allowed to send email on behalf of your domain.
- _DKIM:_ DKIM (DomainKeys Identified Mail) is an email security standard designed to make sure messages aren't altered in transit between the sending and recipient servers. . Once the signature is verified with the public key by the recipient server, the message passes DKIM and is considered authentic.
- _DMARC:_ DMARC, is a technical standard that helps protect email senders and recipients from spam, spoofing, and phishing. Specifically, DMARC establishes a method for a domain owner to: Publish its email authentication practices.

## Publish records and add DNS entries

Along with the successful setup of postfix and dovecot publishing the records/adding these relevant entries correctly can be tricky.

Make sure to have published the following records:
- `A record` - Maps the VPS IP to mail.domain.com .
- `PTR/ rDNS record` - A reverse lookup of mail.domain.com results in yielding your VPS's IP. 
- `MX record` - specifies the mail server responsible for accepting email messages on behalf of a domain name.
- `SPF record` - a txt record specific to mail servers that are allowed to send email on behalf of your domain.
- `DKIM record` - a txt record of your generated pubkey to verify non-repudiation.
- `DMARC record` - a txt record that helps protect email senders and recipients from spoofing. 

Now that you have a base understanding of the requirements to bypass Spam filter MTA's, [Let's build your local SMTP here.](https://m3rcer.github.io/redteaming/spamfilterbypass/)

_________________________________________________________________________________________________