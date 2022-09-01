---
title: Certified Enterprise Security Specialist (PACES) Review - Hacking "Global Central Bank (GCB)"
date: 2022-08-30 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
#description: PACES Review 2022
header-img: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

<p align="center">
     <img src="https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/forest-map.png">
</p>

-------------------------------------------------------

- [Preface](#preface)
- [Lab Review](#lab-review)
- [Exam Review](#exam-review)
- [What's Next](#whats-next)

-------------------------------------------------------

## Preface

Ever since I've begun learning Active Directory Hacking, I've always aspired to someday complete the PACES certification mainly because of the "challenge" it poses and after completing the CRTP and CRTO I felt it was the perfect time to enrol into the PACES certification to continue on learning advanced enterprise technologies and implementations and this certification did not disappoint in that regard. Review's posted by cybersecurity professional's like [Cas Van Cooten](https://casvancooten.com/posts/2021/10/so-you-wanna-hack-a-bank-global-central-bank-paces-certification-review/) and [thehackerish](https://www.youtube.com/watch?v=XnvWijxOu1A) did inspire me and give good insight about the certification before I began.

The course deals with enterprise implementations like LAPS, JEA, WSL, RBCD, WDAC, ASR, AWL, Credential Guard, CLM, WSUS, virtualization and User simulation. An elk instance is provided alongside to analyse the logs and understand better OPSEC behaviour. A lot of techniques/technologies are bought into play which does require extensive research beyond the course contents.

I'd recommend having a strong Active Directory Hacking skillset along with knowledge of common abuse misconfigurations to begin with this journey.

-------------------------------------------------------

## Lab Review

![](https://github.com/m3rcer/m3rcer.github.io/raw/master/_posts/redteaming/Paces_Review/lab.png)

This by far has to be the interesting and most hardened Active Directory Lab I've engaged. The labs also known as GCB (Global Central Bank) simulates an Enterprise Cyber Range that mimics a financial institution's network. The lab is truly vast with around 7 forest's in total with unique attack paths for each host. What I really liked was that the lab wasn't CTF like rather required an overall methodological approach which was great. The course comes along with 3 hours of video course material which deals with only with explanations regarding attack vectors taught beyond the CRTE/CRTP course. Everything before is expected to be known or researched. 
This lab doesn’t have flags and is objective based. There is no walkthrough for the labs other than Hint Diagrams which give hints about the approach to consider. If you ever get stuck the support is amazing and offers immediate support and remediation on the topic. I did find my self asking for help quite a bit during the labs and I'm glad I did because I just couldn't figure some parts on my own. 
For this course the biggest takeaway I can say is the extensive use of the `ADModule` and `Credential Objects`. If you ever think the lab is broke, I'd say most times your enumeration is. There are lots of pivots, double inceptive pivots and interesting paths to access each target.

I started my lab the starting of July and enrolled for a month which was enough time for me, I found myself spending around 6-8 hrs a day on the lab and had completed the lab by the 3rd week. I did revise and go through all attack paths the final week. When I began my labs, I planned to use `sliver` to get more competent with it, I wasted a lot of time getting `sliver` to work in this environment since `sliver` required internet access to generate live payloads. I found a smart circumvent to generate offline payloads with `sliver` by generating payloads on my windows host with internet access, copying the `.sliver`, `.sliver-client` folders from `C:\Users\<username>` onto the lab foothold vm's `C:\Users\<username>` location. After which, copying the `sliver` server binary onto the lab vm grants you a smart way to be able to use the `regenerate` command to regenerate the previously generated payloads without the internet, this way I managed to generate `sliver` payloads offline but alas I found myself ditching `sliver` a lot throughout the labs because in most places `smb`/`tcp` pivots weren't implacable for lateral movement.

I managed to complete the labs in around 3 weeks. The last week I revised the labs and tried various new attacks/tools. Some portions I found annoying was scanning a `/16` subnet range which took me a whole day and engaging the interactive phishing client which broke on multiple instances. Also I would've loved it if the lab kept up to date with modern AV's. Since this course doesn’t concentrate much on AV evasion and rather on pure Active Directory Hacking this is okay, but say if AV signatures were updated this lab would be a fun nightmare to accomplish just like any soul's game.

-------------------------------------------------------

## Exam Review

![](https://github.com/m3rcer/m3rcer.github.io/raw/master/_posts/redteaming/Paces_Review/lab2.png)

I began my exam on `Aug 5th`, after a 5-day break from the labs and the objective was to get command execution (not necessarily with administrative privileges) on 5 target server's other than the foothold after which secure all found vulnerabilities and setup defence’s/configurations as per said client requirements (provided with the exam objective mail). I managed to pwn 4/5 servers on my first day after which I gave up because I had exhausted my enumeration/out of the box thinking for the day. I woke up just 1.5 hrs after feeling time paranoid and redid all my enumeration steps to check if I missed any step and there it was in plain sight hiding in a step I managed to miss during my enumeration phase. After which cleartext passwords are provided on a target DC to go about securing the environment as per said requirements. The course doesn’t prepare you a lot in terms of defences but I spent a good time understanding and researching these vulnerabilities in my own home lab so I had completed the defence part in about 2 hrs after. I found both parts very amusing and definitely learnt a lot. 

Some key take away’s to pass the exam are:
- Do not treat the course/exam lab as a CTF, treat it with a realistic overall methodological approach.
- Spend time researching these attacks in your homelab or prepare through widely available research for the defence portion since the course doesn’t do much in this regard.
- Learn to be really good and competent using the `ADModule` for enumeration and also leveraging `credential objects` when things seem broken. 
- Enumerate a forest quickly using `bloodhound` and be competent with manual enumeration too, understand all attack vectors taught during the labs and you will definitely pass this exam. 

Overall I'd say the exam labs is a fair challenge, not as hard as the course labs but definitely does teach a lot and is a great addition to my Active Directory Exploitation experience. I'm glad to have this certificate to my name and am grateful for the experience.

![](https://github.com/m3rcer/m3rcer.github.io/raw/master/_posts/redteaming/Paces_Review/cert.png)

-------------------------------------------------------

## What's Next

I will be delving further into Malware Development, AV Evasion via research/courses from `sektor7`. Also, I plan to research Azure Active Directory and various other cloud Active Directory Services in the near future.

-------------------------------------------------------
