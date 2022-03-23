---
title: ARP-Poison-Detector
date: 2021-08-14 09:45:47 +07:00
categories: python_101
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: A python script that detects ARP Cache Poisoning.
---

<p align="left">
 <img src="https://www.memecreator.org/static/images/memes/3007699.jpg">
</p>

## This is a python script that detects ARP Cache Poisoning.

- This script sniffs for ARP responses and compares the `hwsrc` attribute of the response to the original MAC address (found using the previously defined `get_mac()` module), and if any changes were detected would confirm ARP Cache Poisoning.


### Code:

```python
#!/usr/bin/env python

import scapy.all as scapy

def get_mac(ip):
    arp_request = scapy.ARP(pdst=ip)
    broadcast = scapy.Ether(dst="ff:ff:ff:ff:ff:ff")
    arp_request_broadcast = broadcast/arp_request
    answered_list = scapy.srp(arp_request_broadcast, timeout=0, verbose=False)[0]
    return answered_list[0][1].hwsrc

def sniff(interface):
    # store --> storing packets in memory
    # prn --> callback function
    # filter --> filter according to BPF like TCP/port 80/ICMP
    scapy.sniff(iface=interface, store=False, prn=process_sniffed_packet)


def process_sniffed_packet(packet):
    if packet.haslayer(scapy.ARP) and packet[scapy.ARP].op == 2:
        try:
            real_mac = get_mac(packet[scapy.ARP].psrc)
            response_mac = packet[scapy.ARP].hwsrc
            print(real_mac)
            print(response_mac)
            if real_mac != response_mac:
                print("[!] Under Attack - ARP Cache Poisoning Detected!")
        except IndexError:
            pass

            
print("""
        ___    ____  ____     ____        _                     ____       __            __            
   /   |  / __ \/ __ \   / __ \____  (_)________  ____     / __ \___  / /____  _____/ /_____  _____
  / /| | / /_/ / /_/ /  / /_/ / __ \/ / ___/ __ \/ __ \   / / / / _ \/ __/ _ \/ ___/ __/ __ \/ ___/
 / ___ |/ _, _/ ____/  / ____/ /_/ / (__  ) /_/ / / / /  / /_/ /  __/ /_/  __/ /__/ /_/ /_/ / /    
/_/  |_/_/ |_/_/      /_/    \____/_/____/\____/_/ /_/  /_____/\___/\__/\___/\___/\__/\____/_/     
           """)


sniff("eth0")
```

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/ARPDetector/detect1.png)