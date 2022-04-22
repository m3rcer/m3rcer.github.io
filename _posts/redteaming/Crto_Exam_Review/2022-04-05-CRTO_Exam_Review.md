---
title: MRTO Exam Review
date: 2022-04-02 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: MRTO Exam Review 2022
---


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

# My MRTO Journey

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto1.png)

## Preface

In my initial days learning to hack I always had a keen interest towards Botnet's and C2's. The power a Botherder beheld being able to control an Army of Computers with the click of a few buttons was everything that peaked my hacker interest, I think It's quite deplorable that such power is used mostly for cyberfraud. 

I began scourging and playing with every C2 I could find off the Darknet or every open Source github C2. I was quite clueless with everything as there weren't any definitive guides to setting things up, even if they existed, they were all ancient methods/too good to be true well disguised malware/scams. I Knew Cobalt Strike was it when it came to APT Operations. I remember reading a lot of APT papers from sources like [@Mandiant](https://www.mandiant.com/), watching [Raphael Mudge on youtube](https://www.youtube.com/channel/UCJU2r634VNPeCRug7Y7qdcw) trying to emulate APT behaviour using Cracked Versions of Cobalt Strike 3/4.x such as [Cobalt-Wipe]((https://github.com/ORCA666/Cobalt-Wipe) by ORCA666 (Taken down by DMCA. [WayBack Machine](https://archive.org/) is your friend).

As an experiment I managed to succesfully set up a standard 3 tier architecture with a staging,post-ex and long haul server on seperate VPS's. Setup Redirectors with [Red-Warden](https://github.com/mgeeky/RedWarden), experimented and learnt alot about Cobalt Strike and modern A/V evasion. Not just Cobalt Strike I learnt to bypass A/V's using many other C2's such as [SILENTTRINITY](https://github.com/byt3bl33d3r/SILENTTRINITY). Learnt a lot on how to rewrite the Artifact Kit to incorporate direct Syscalls, Adding custom AMSI bypasses and droppers in the Resource Kit etc etc...

I had been taking a couple other certs/books/courses etc to supplement my overall Active Directory skillset, played regular CTF's, built my own labs etc as I felt I understood the toolset and verbatim associated but din't understand know how to hack an Active Directory. I started off by gaining a fair bit off experience from the [PWKv2 labs](https://www.offensive-security.com/pwk-oscp/) after which I completed the [CRTP](https://www.pentesteracademy.com/activedirectorylab) earlier this January. Even though the [CRTP](https://www.pentesteracademy.com/activedirectorylab) is structured around manual Active Directory Enumeration, I did both, the intended way after which could'nt resist to not play around using my Cracked Cobalt Strike instance and Custom Aggressors. I learnt soo much more doing this and I highly recommend "playing outside the box". A place where you can detonate practically any tooling safely are the [@PentesterAcademy Labs](https://www.pentesteracademy.com/redlabs). I even managed to pass the [CRTP](https://www.pentesteracademy.com/activedirectorylab) exam pwning all machines using my Cracked Cobalt Strike instance and wrote a report including Cobalt Strike which was unnecessary but was succesfully evaluated as their are no tool restrictions on the [CRTP](https://www.pentesteracademy.com/activedirectorylab) Exam.  

I had been eyeing the CRTO course for a while as I felt it is the right of passage to becoming able in Red Team Operations using Cobalt Strike and I knew I have to enroll to further hone my Cobalt Strike skillset. Even though I did have a fair share of experience with Cobalt Strike through my own learning, the course managed to fill in a lot of base knowledge gaps, increase my Active Directory hacking attack skillset and make me overall love and understand Cobalt Strike a 100x more. I'd Recommend doing the [CRTP](https://www.pentesteracademy.com/activedirectorylab)/OSCP as a primer to Active Directory exploitaion before taking this course.


------------------------------------------------------

## Course Review

[RTO (Red Team Ops) by zeropoint security](https://www.zeropointsecurity.co.uk/red-team-ops/overview) in short is a hands on course that teaches you how to operate and perform Core Fundamental Red Team Operations using Cobalt Strike. It is divided into several modules with most modules corresponding to a common MITRE Red Team attack tactic. Some modules are accompanied with videos demonstrating the concept in the practice lab demonstrated by [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) himself.

### Course Highlights

- Lifetime access to the course material.  
- A search feature that allows you to quickly find the relevant parts of the course.
- Even though [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) is the only point of support, I did bother him quite a few times and he was almost always instantanious to respond each time.
- Constant updates. During my tenure I found [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) update the course twice and add some awesome stuff.
- Well thought and structured labs to demonstrate each topic.
- An elk Instance to understand the blue side which I'm a big fan off as it teaches better OPSEC behaviour.
- A new Forum where students can discuss has been added.
- Copying and pasting seems fixed.

### Course Drawbacks

- For some since the lab is timed(40 hrs in total) the duration could feel stressful. I personally still have 8 hours remaining in my labs. Even if your lab time does expire lab time could be purchased for $1.25 an hour which is cheap.
- No VPN Access, but I understand it isn't a feasible setup as it causes licensing issues.
- Students aren't allowed to use their own tools on the lab/exam.


------------------------------------------------------

## Exam Review

### Exam Structure

The exam is totally 48 hours, you have 4 days to allocate 48 hours. 4 days * 12 hours is the most viable option to go with. Their is no proctoring and no report submissions which I feel is one of the best parts about the exam. The exam can be scheduled I'd say almost a day before, there are ample slots available. I'd recommend booking the exam a few days prior. Once done, launch the snaplab Exam instance, read the exam instructions and download your threat profile.

### Exam Experience

I scheduled my exam for the 28th of March, 4-6 weeks after enrollment with the idea of the standard 4 days * 12 hours. I felt I was already prepared after a month of practice, but for personal reasons and because [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) did add additional modules to the course, I had to posponn the exam. I did enroll in a Pro-Lab from HTB in the meantime called [Cybernetics](https://www.hackthebox.com/newsroom/prolab-cybernetics) 2 weeks prior my exam. Pwning the first domain off [Cybernetics](https://www.hackthebox.com/newsroom/prolab-cybernetics) I'd say was great practice, I wouldn't say it is mandatory to do so. I did it because I was saturated with the RTO Course and thought extra practice could never hurt. I feel 4-6 weeks of good preparation with the course and lab will yield hearty results.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto2.png)

I began my exam at 11.30am and had my first flag in about an hour mainly because I already had my C2 profile ready prior the exam and only had to work on A/V bypasses initially. The attack path is quite fair and obvious, there are no rabbitholes or any that I could find. I managed to get the first 5 flags in my first day. Knew the path to the 6th but decided to call it a day and took some good rest since I felt this was the trickiest flag of the exam with a little out of the box thinking needed.

The next day I spent the whole day trying to make things work and finally managed to do so by 5 in the evening. I decided to take a massive break after which i found the 7th flag within half an hour of enumeration around 11pm. I just took the rest of the day off as i still had over 28hrs lab time on my exam.

The 3rd day I spent some time debugging as I had managed to break something in the lab. If you do face something similar you can revert all your exam machines by yourself. You dont have to contact [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) the way I did (Like always he did respond in a few minutes). After an hour of reverting I found the final flag. To some the last 2 flags can seem tricky since they aren't as obvious as the rest. I'd recommend going for the last 2 flags as I feel I've learnt a lot just from completing the exam.

Finally I'm happy to pass with all 8 flags and suprisingly still manage to have over 24 hours of exam lab time. Mainly because I did pause the labs when I took breaks too. I dont have an issue setting things up again if it gives me extra time if say I needed it, thats just my train of thought. The amount of time given is overkill for that thought train and is more than enough to pwn 2 exams. This makes the exam all the more easy,relaxed and enjoyable to pwn all the flags.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Crto_Exam_Review/crto3.png)

### Exam Tips

- Take some time and build the Malleable C2 profile prior the exam in accordance to the threat profile and verify it using `c2lint` from the practice lab instance if you dont own Cobalt Strike. I've seen most people waste time in building and setting their initial beacon up on the exam. I managed to get my 1st flag withing an hour preparing this way.
- Understand A/V evasion taught from the course well. Repeat, practice and understand everything taught. Since the practice labs have most machines without harderned defenses, I'd recommend practicing lateral movement,persistence etc with the hardened hosts in the practice labs.
- Personally I feel the amount of time allotted is overkill, so don't stress about time. Hack with tunes, take breaks, smoke some or do anything to keep your mind off when you reach a roadblock. Follow the OSCP mantra to K.I.S.S.
- The attack path will seem clear for some or most flags but be ready to encounter hardened defenses. Unless you understand how to bypass and craft your way through ingenously you might find the exam quite challenging.
- Understand Firewall Rules and implementations in the course. This is really important.
- Read and implement every HINT,TIP and TRICK [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) has nudged for in the course.
- You will not see a majority of the standard tooling discussed in the lab. Practice manual enumeration with all tools in the course well.
- Take structured notes of enumeration using Joplin or so, so that you reproduce steps and have a good track of your enuermation.
- **MAJOR TIP**: **EVERYTHING YOU NEED TO PASS IS IN THE COURSE**. Don't try porting tools or outsmarting the exam. I did it and just wasted time. All the tools and attacks are from the course. Not a copy paste verbatim, you will have to think out of the box and know where to use what.

--------------------------------------------------

## Conclusion

If I could I'd definitely vouch to purchase the licensed version of Cobalt Strike someday mainly to accreditate such a beautiful project by Raphael Mudge and my love for Cobalt Strike. He is an absolute legend. And so is this course - a legal way to get your hands on Cobalt Strike along with training for such a valuable price. I encourage anyone interested with a decent share of Active Directory Exploitation skill to enroll.

Also being taught with assisstance from [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) himself is an honor. The exam is balanced in terms of difficulty and is a good test in terms of Cobalt Strike operatibility. The course will someday hopefully be the de-facto for Red Teaming Operations on HR. Can't wait to see what [@Rasta_Mouse](https://twitter.com/_rastamouse?lang=en) has in store for CRTO2 and his other upcoming courses!


----------------------------------------------------

## What's Next

I plan to continue and complete [Cybernetics](https://www.hackthebox.com/newsroom/prolab-cybernetics) from HackTheBox and I've enrolled into [PACES](https://www.pentesteracademy.com/gcb) alongside where I still plan to use my Cracked Cobalt Strike Instance and further better my skillset with and without a C2. I am currently writting an Aggressor Script too for learning purposes. After [PACES](https://www.pentesteracademy.com/gcb) I plan to finally apply for Open Red Team Positions.


