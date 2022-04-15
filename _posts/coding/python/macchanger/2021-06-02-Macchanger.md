---
title: A MAC Address Changer
date: 2021-06-02 10:45:47 +07:00
categories: python_101
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: A Python MAC-Changer.
---

<p align="center">
 <img src="https://pics.me.me/thumb_airport-wifi-expires-me-changes-my-mac-address-airport-wifi-56785626.png">
</p>


## This is a basic python script for changing the Mac Address on a unix/linux system implementing core concepts of python used at large by the community.  

 
- Implementing the `subprocess` module to interact and use os commands in a secure manner.
- Implementing the `optparse` module to initialize options with ease and parse them according to functions. 
- Implementing the `re` module along wiht pythex to build basic regex to find patterns like the mac_address in this case.
- Cross compatibility for python2/3.
- Basic error handling and added functionality.


### Code:


```python
import subprocess
import optparse
import re

#WORKS ON PYTHON2/3

def get_arguments():
    parser = optparse.OptionParser()
    #initializing options
    parser.add_option("-i", "--interface", dest="interface", help="Interface to change the Mac Address")
    parser.add_option("-b", "--mac", dest="new_mac", help="New MAC address to use")
    (options, arguments) = parser.parse_args()
    if not options.interface:
        #code to handle error
        parser.error("[-] Please specify an interface, use --help for info.")
    elif not options.new_mac:
        #code to handle error
        parser.error("[-] Please specify a mac address, use --help for info.")
        #returns only options not arguments
    return options



def change_mac(interface, new_mac):

    # os cmds to change mac
    subprocess.call(["ip", "link", "set", "dev", interface, "down"])
    subprocess.call(["ip", "link", "set", "dev", interface, "address", new_mac])
    subprocess.call(["ip", "link", "set", "dev", interface, "up"])
    print("[+] Chaning Mac Address for : " + interface + " to " + new_mac)


def get_current_mac(interface):
    # Function returning current mac
    ifconfig_result = subprocess.check_output(["ip", "link", "show", interface]).decode("utf-8")
    mac_address_search_result = re.search(r"\w\w:\w\w:\w\w:\w\w:\w\w:\w\w", ifconfig_result)
    if mac_address_search_result:
        return mac_address_search_result.group(0)
    else:
        print("Please enter an interface with a valid mac address.")
        exit(0)


print("""
   __  __                _                                 
 |  \/  | __ _  ___ ___| |__   __ _ _ __   __ _  ___ _ __ 
 | |\/| |/ _` |/ __/ __| '_ \ / _` | '_ \ / _` |/ _ \ '__|
 | |  | | (_| | (_| (__| | | | (_| | | | | (_| |  __/ |   
 |_|  |_|\__,_|\___\___|_| |_|\__,_|_| |_|\__, |\___|_|   
                                          |___/           
        """)


options = get_arguments()

current_mac = get_current_mac(options.interface)
print("\nCurrent MAC is : " + str(current_mac))

change_mac(options.interface, options.new_mac)

current_mac = get_current_mac(options.interface)
if current_mac == options.new_mac:
    print("[+] MAC Address was succesfully changed to " + current_mac)
else:
    print("[-] MAC Address wasn't successfully changed.")

```

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/python/macchanger/macchanger.png)
