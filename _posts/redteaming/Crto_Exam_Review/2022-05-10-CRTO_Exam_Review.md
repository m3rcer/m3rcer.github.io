---
title: CRTO (Red Team Ops) Review - A Cobalt Strike Battle Ground
date: 2022-05-10 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: CRTO 2022
---

<p align="center">
     <img src="https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto6.png">
</p>


-------------------------------------------------------

Index:
- [Preface](#preface)
- [Course Review](#Bcourse-review)
	- [Course Highlights](#course-highlights)
	- [Course Drawbacks](#course-drawbacks)
- [Exam Review](#exam-review)
	- [Exam Structure](#exam-structure)
	- [Exam Experience](#exam-experience)
	- [Exam Tips](#exam-tips)
- [Conclusion](#conclusion)
- [What's Next](#whats-next)

-------------------------------------------------------

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto1.png)

-------------------------------------------------------

## Preface

In my initial days learning to hack, I remember always having a keen interest towards C2's & Botnet operations. The power a Botherder beheld being able to control an Army of Computers with the click of a few buttons was everything that peaked my hacker interest at that moment, I think it’s quite deplorable that such power is primarily used for cybercrime, fraud, DDOS and the like. On the contrary stories like the [Carna Botnet](https://darknetdiaries.com/episode/13/) which are quite the opposite, with reasons such as "scanning the internet to create a map of where all public facing computers are in the world to understand the internet better" or [Ghost Exodus's](https://darknetdiaries.com/episode/70/) hilarious tutorial on [how to manually make a botnet](https://www.youtube.com/watch?v=2UKeHbrsF94&ab_channel=WIRED) 😅 were some hacker stories that were inspirational through my journey.

I played with most C2's I could find that were mostly open source or free and was interested in Cobalt Strike as it is one of the most stable C2's to perform advanced red team C2 operations and provides the user a lot of flexibility in terms of customizability and adding plugins via aggressor scripts. I remember enjoying reading a lot of papers from sources like [@Mandiant](https://www.mandiant.com/), watching [Raphael Mudge on youtube](https://www.youtube.com/channel/UCJU2r634VNPeCRug7Y7qdcw) and trying to emulate said "APT" behaviour using Cracked Versions of Cobalt Strike 3/4.x with projects such as [Cobalt-Wipe](https://github.com/ORCA666/Cobalt-Wipe) by [ORCA666](https://twitter.com/ORCA10K) (Now taken down by DMCA.).

As an experiment I managed to successfully set up a standard 3 tier architecture with a staging, post-exploitation and long haul server on separate VPS's. Setup Redirectors with [Red-Warden](https://github.com/mgeeky/RedWarden) and during the process learnt many things like how to rewrite the Artifact Kit to fully incorporate direct Syscalls, incorporating custom AMSI bypasses and droppers in the Resource Kit, found safe ways to execute my .NET tooling using Aggressors like [InlineExecute-Assembly](https://github.com/anthemtotheego/InlineExecute-Assembly) or [reflection methods](https://redteamer.tips/a-tale-of-net-assemblies-cobalt-strike-size-constraints-and-reflection/), Creating undetectable [Malleable C2 profiles](https://github.com/threatexpress/malleable-c2/blob/master/jquery-c2.4.0.profile), implementing [Cross platform beacons on Cobalt Strike](https://github.com/gloxec/CrossC2) etc.

I had also been taking a couple other certs/books/courses to supplement my overall Hacking/Active Directory skillset, played regular CTF's, built my own labs and the like. I started off by gaining a fair bit off Active Directory Hacking experience from the [OSCP labs](https://www.offensive-security.com/pwk-oscp/) after which I completed the [CRTP](https://www.pentesteracademy.com/activedirectorylab) earlier this January which managed to give me a strong base. Even though the [CRTP](https://www.pentesteracademy.com/activedirectorylab) is structured around manual Active Directory Enumeration, I could'nt resist to not play around using my Cobalt Strike cracked instance and Custom Aggressors after completing the course exercises as intended. I learnt soo much doing extra, and I highly recommend a similar mindset of "playing outside the box" and always trying new things. I even managed to pass the [CRTP](https://www.pentesteracademy.com/activedirectorylab) exam pwning all machines and wrote a report detailing my exploitation using Cobalt Strike which was succesfully evaluated as their are no tool restrictions on the [CRTP](https://www.pentesteracademy.com/activedirectorylab) exam which I thought was great.

I had been eyeing the [CRTO course by Zeropoint Security](https://www.zeropointsecurity.co.uk/red-team-ops/overview) for a while during this tenure as I felt it is the right of passage to becoming able in Red Team Operations using Cobalt Strike and was a good test to my experience, so I knew I had to enroll. Even though I did have a fair share of experience with Cobalt Strike and other C2's through my own learning, the course still managed to fill in a lot of knowledge gaps, give a strong structured mindset around Active Directory hacking with a C2, make me overall competent in enumeration and love Cobalt Strike a 100x more. I'd recommend doing the [CRTP](https://www.pentesteracademy.com/activedirectorylab)/[OSCP]((https://www.offensive-security.com/pwk-oscp/)) as a primer to Active Directory exploitaion before taking this course and it isn't mandatory to have experience with Cobalt Strike prior like me, if you do so that is a plus.


------------------------------------------------------

## Course Review

[RTO (Red Team Ops) by Zeropoint Security](https://www.zeropointsecurity.co.uk/red-team-ops/overview) in short is a hands on course that teaches you how to operate and perform core fundamental Red Team Operations using Cobalt Strike. It is divided into several modules with most modules corresponding to a common MITRE Red Team attack tactic. Some modules are accompanied with videos demonstrating the concept in the practice lab demonstrated by [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) himself.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto4.png)

### Course Highlights

- Lifetime access to the course material.  
- A search feature that allows you to quickly find the relevant parts of the course.
- Even though [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) is the only point of support, I did bother him quite a few times and he was almost always instantaneous to respond each time.
- Constant updates. During my tenure I found [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) update the course twice and add some awesome stuff.
- Well thought and structured labs to demonstrate each topic.
- An elk Instance to understand the blue side which I'm a big fan off as it teaches better OPSEC behaviour.
- A new Forum where students can discuss has been added.
- Copying and pasting seems fixed.

### Course Drawbacks

- For some since the lab is timed (40 hrs in total) the duration could feel stressful. I personally still have 8 hours remaining in my labs. Even if your lab time does expire, lab time could be purchased for $1.25 an hour which is cheap.
- No VPN Access, but I understand it isn't a feasible setup as it causes licensing issues.
- Students aren't allowed to use their own tools on the lab/exam.


------------------------------------------------------

## Exam Review

### Exam Structure

The exam is totally 48 hours, you have 4 days to allocate 48 hours. 4 days * 12 hours/day is the most viable option to go with. There is no proctoring or report submission. I feel this is one of the best parts about the exam. The exam can be scheduled I'd say almost a day before, there are ample slots available. I'd recommend booking the exam a few days prior and once done, read the exam instructions and download your threat profile.

### Exam Experience

I scheduled my exam for the 28th of March, 4-6 weeks after enrollment with the idea of the standard 4 days * 12 hours/day. I felt I was already prepared after a month of practice, but for personal reasons and because [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) did add additional modules to the course, I had to posponn the exam. I did enroll in a Pro-Lab from HTB in the meantime called [Cybernetics](https://www.hackthebox.com/newsroom/prolab-cybernetics) two weeks prior my exam. Pwning the first domain off [Cybernetics](https://www.hackthebox.com/newsroom/prolab-cybernetics) I'd say was great practice, I wouldn't say it is mandatory to do so. I did it because I was saturated with the RTO Course and thought extra practice could never hurt. I feel 4-6 weeks of good preparation with the course and lab will yield hearty results.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto2.png)

I began my exam at 11.30am and had my first flag in about an hour mainly because I already had my C2 profile ready prior the exam and only had to work on A/V bypasses initially. The attack path is quite fair and obvious, there are no rabbitholes or any that I could find. I managed to get the first 5 flags in my first day. Knew the path to the 6th but decided to call it a day and took some good rest since I felt this was the trickiest flag of the exam with a little out of the box thinking needed.

The next day I spent the whole day trying to make things work and finally managed to do so by 5pm in the evening. I decided to take a massive break after which i found the 7th flag within half an hour of enumeration around 11pm. I just took the rest of the day off as i still had over 28hrs lab time on my exam and 2 more days.

The 3rd day I spent some time debugging as I had managed to break something in the lab. If you do face something similar you can revert all your exam machines yourself. You dont have to contact [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) the way I did (Like always he did respond in a few minutes).

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto5.png)

 After an hour of reverting I found the final flag. To some the last 2 flags can seem tricky since they aren't as obvious as the rest. I'd recommend going for the last 2 flags as I feel I've learnt a lot just from completing the exam. I spent the rest of the 3rd and 4rth day just gaming and relaxing awaiting my RTO badge. 

Finally, I'm happy to pass with all 8 flags and surprisingly still managed to have over 24 hours of exam lab time. Mainly because I did pause the labs when I took breaks too. I don't have an issue setting things up again if it gives me extra time if say I needed it, that’s just my train of thought. The amount of time given is overkill for that thought train and is more than enough to pwn 2 exams. This makes the exam all the more easy, relaxed and enjoyable to try to pwn all the flags.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto3.png)

**"Issued on Apr 1, 2022"** - A perfect day for the RTO badge to arrive. 

### Exam Tips

- Take some time and build the Malleable C2 profile prior the exam in accordance to the threat profile and verify it using `c2lint` from the practice lab instance if you don’t own Cobalt Strike. I've seen most people waste time in building and setting their initial beacon up on the exam. I managed to get my 1st flag within an hour preparing this way.
- Understand A/V evasion taught from the course well. Repeat, practice and understand everything taught. Since the practice labs have most machines without hardened defences, I'd recommend practicing lateral movement, persistence etc with the hardened hosts in the practice labs.
- Personally, I feel the amount of time allotted is overkill, so don't stress about time. Hack with tunes, take breaks and do anything to keep your mind off when you reach a roadblock. Follow the OSCP mantra to K.I.S.S.
- The attack path will seem clear for some or most flags but be ready to encounter hardened defences. Unless you understand how to bypass and craft your way through ingeniously you might find the exam quite challenging.
- Understand Firewall Rules and implementations in the course. This is really important.
- Read and implement every HINT, TIP and TRICK [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) has nudged for in the course.
- You will not see a majority of the standard tooling discussed in the lab. Practice manual enumeration with all tools in the course well.
- Take structured notes of enumeration using Joplin or so, so that you reproduce steps and have a good track of your enumeration.
- **MAJOR TIP**: **EVERYTHING YOU NEED TO PASS IS IN THE COURSE**. Don't try porting tools or outsmarting the exam. I did it and just wasted time. All the tools and attacks are from the course. Not a copy paste verbatim, you will have to think out of the box and know where to use what.

--------------------------------------------------

## Conclusion

Cobalt Strike is a beautiful project by Raphael Mudge and so is this course, a legal way to get your hands on Cobalt Strike training for such a valuable price. I encourage anyone interested with a decent share of Active Directory Exploitation skill to enrol.

Also learning a course from [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) is an absolute privilege. The exam is balanced in terms of difficulty and is a good test in terms of Cobalt Strike operability and I do agree with the mindset of limited tooling for the exam as it does force you to cultivate better discipline in terms of enumeration. The course will someday hopefully be the de-facto for Red Teaming Operations on HR. Can't wait to see what [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) has in store for CRTO2 and his other upcoming courses!


----------------------------------------------------

## What's Next

I plan to continue and complete [Cybernetics](https://www.hackthebox.com/newsroom/prolab-cybernetics) from HackTheBox and I've recently enrolled into [PACES](https://www.pentesteracademy.com/gcb) alongside where I plan to understand advanced active directory implementations and counter enterprise defence mechanisms such as LAPS, PAM Trusts, JEA, WSL, RBCD, WDAC, ASR, AWL, Credential Guard, CLM, virtualization and more. I am also currently writing an Aggressor Script for learning purposes and the community which I'm due to release quite soon.
 
