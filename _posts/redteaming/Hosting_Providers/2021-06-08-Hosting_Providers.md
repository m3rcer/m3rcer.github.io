---
title: Hosting Providers for your phishing/SMTP builds
date: 2021-06-08 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Dark/Clear Web Hosting Providers for your SMTP builds!
---

Finding a good hosting provider that supports all your needs to build and configure a smtp server with relays is hard.

Most hosting providers do not allow open relays and do not allow outbound 25/587.

Finding Providers who support tor relays and at the right price is even harder.

**Here is a link that provides a list of all the available hosting providers that fit the criteria explained below . Choose accordingly.** 

-----------------------------------------------------------------------------------------------------

[Link to list of good / Bad ISP's](https://gitlab.torproject.org/legacy/trac/-/wikis/doc/GoodBadISPs)


__All credits go to [Alexander Færøy](https://gitlab.torproject.org/ahf) for this wonderful link__.

-----------------------------------------------------------------------------------------------------

**Follows are some base criteria for selection of ISP's :**

- Can you create PTR record to improve email deliverability. _This is very important for high delivery rate_ by main MTA's like gmail.

- Relays : Relays are also referred to as "routers" or "nodes." They receive traffic on a network and pass it along.
Do they accept entry and exit relays ? Tor relays for added anonymity?

*DO they allow inbound and outbound relays on port 25/587?* --> This is most crucial else your SMTP server wont be allowed to send outbound emails rendering your money and setup useless.

- Do they have high reputation IP addresses? You definitely don’t want to be listed on the dreaded Microsoft Outlook IP blacklist or the SpamRats blacklist. Some blacklists block an entire IP range and you have no way to delist your IP address from this kind of blacklist.

- Accountability : Is the account holder responsible for traffic that flows through or is the ISP responsible? The first is preferred , say if there was an abuse complaint the ISP wouldn't handle it and it would become the responsibility of the account holder to . This remdiates unwanted domain suspensions and puts the power of resolution of conflict in your own hands.  

- ASN : Network operators need Autonomous System Numbers (ASNs) to control routing within their networks and to exchange routing information with other Internet Service Providers (ISPs).

- Bridges : Tor bridges, also called Tor bridge relays, are alternative entry points to the Tor network that are not all listed publicly. Using a bridge makes it harder, but not impossible, for your Internet Service Provider to know that you are using Tor. *This is a safer way to use Tor!*

- Cryptocurrency Support: If you want your payment terms to be anonymous. 
_Payment in Monero is truly anonymous and powerfull when operating over a cli_.

**Some vps providers i recommend and have experience with:** FLokinet, Cockbox, njalla , vpsbg.eu, nicevps.net.

_Host Safe!_  