---
title: Sexy Exam Review
date: 2022-04-05 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Sexy Exam Review 2022
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

-------------------------------------------------------

# My Sexy Journey

![](crto1.png)

## Preface

In my initial days learning to hack I always had a keen interest towards Botnet's and C2's. The power a Botherder beheld being able to control an Army of Computers with the click of a few buttons was everything that peaked my hacker interest. 

I began scourging and playing with every shitty C2 I could find off the Darknet or every open Source github C2. I was quite clueless with everything as there weren't any definitive guides to setting things up, even if they were they were all ancient methods/too good to be true scams on the darknet. I set out trying to understand and study all I could about C2's and Red Team Operations. I Knew Cobalt Strike was it when it came to APT Operations. I remember reading a lot of APT papers from sources like @Mandiant, learning and trying to emulate APT behaviour using a botched up Cracked version of Cobalt Strike 3.x. I finally managed to get my hands on a fully operational Cracked version of Cobalt Strike 4.3 from a user on github called `ORCA666` which was clean from all malware treachery. The project was taken down a few weeks after. If you're interested here's the [link](https://github.com/ORCA666/Cobalt-Wipe).

I watched Raphael Mudge on [youtube](https://www.youtube.com/channel/UCJU2r634VNPeCRug7Y7qdcw), learnt the basic functionality of Cobalt Strike. As an experiment managed to succesfully set up a standard 3 tier architecture with a staging,post-ex and long haul server on seperate VPS's. Setup Redirectors with [Red-Warden](https://github.com/mgeeky/RedWarden), experimented and learnt alot about Cobalt Strike and A/V evasion. Learnt a lot on how to rewrite the Artificat Kit to incorporate direct Syscalls, Adding custom AMSI bypasses and droppers in the Resource Kit etc etc... I also did quite some research and built my own SMTP Server on a VPS with things like DKIM,SPF,DMARC etc setup which could Bypass most spam filters like Gmail,Yahoo etc to accompany my Cobalt Strike Teamserver delivering my phish.

I had been taking a couple other certs/books/courses etc to supplement my overall hacking skillset, played regular CTF's and planned to better my Active Directory skills by starting off and completing the CRTP earlier this January. Even though the whole course is structured around manual Active Directory Enumeration. I did both, did the intended way and after which could'nt resist to not play around using my Cracked Cobalt Strike and Custom Aggressor/Tools finding all or new ways using newer exploits. I learnt soo much more doing this and I highly recommend it. A place where you can detonate practically anything safely are the @PentesterAcademy Labs which is awesome. I even managed to pass the exam pwning all 5 machines using only Cobalt Strike as an added challenge as their are no tool restrictions on the CRTP Exam. 

I had been eyeing the CRTO course for a while as I felt it is the right of passage to becoming able in Red Team Operations using Cobalt Strike. Even though I did have a fair share of experience with Cobalt Strike through my own learning, the course managed to fill in a lot of base knowledge gaps, increase my Active Directory hacking attack skillset and make me overall love and understand Cobalt Strike a 100x more. I'd Recommend doing the CRTP/OSCP as a primer to Active Directory exploitaion before taking this course.


------------------------------------------------------

## Course Review

[RTO (Red Team Ops) by zeropoint security](https://www.zeropointsecurity.co.uk/red-team-ops/overview) in short is a hands on course that teaches you how to operate and perform Core Fundamental Red Team Operations using Cobalt Strike. It is divided into several modules with most modules corresponding to a common MITRE Red Team attack tactic. Some modules are accompanied with videos demonstrating the concept in the practice lab demonstrated by @Rasta_Mouse himself.

### Course Highlights

- Lifetime access to the course material.  
- A search feature that allows you to quickly find the relevant parts of the course.
- Even though @Rasta_Mouse is the only point of support, I did bother him quite a few times and he was almost always instantanious to respond each time.
- Constant updates. During my tenure I found @Rasta_Mouse update the course twice and add some awesome stuff.
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

I scheduled my exam for the 28th of March, 6-8 weeks after enrollment with the idea of the standard 4 days * 12 hours. I felt I was already prepared after a month of practice, but for personal reasons and because @rasta_mouse did add additional modules to the course, I had to posponn the exam. I did enroll in a Pro-Lab from HTB in the meantime called "Cybernetics" 2 weeks prior my exam. Pwning the first domain off Cybernetics I'd say was great practice, I wouldn't say it is mandatory to do so. I feel 6 weeks of good preparation with the course and lab will yield hearty results.


![](crto2.png)

I began my exam at 11.30am and had my first flag in about an hour mainly because I already had my C2 profile ready and only had to work on A/V bypasses initially. The attack path is quite fair and obvious, there are no rabbitholes or any that I could find. I managed to get the first 5 flags in my first day. Knew the path to the 6th but decided to call it a day and took some good rest since I felt this was the trickiest flag of the exam with a little out of the box thinking needed.

The next day I spent the whole day trying to make things work and finally managed to do so by 5 in the evening. I decided to take a massive break after which i found the 7th flag within half an hour of enumeration a 11pm. I just took the rest of the day off as i still had over 28hrs on my exam.

The 3rd day I spent some time debugging as I had managed to break something in the lab. If you do face something similar you can revert all your exam machines by yourself. You dont have to contact @Rasta_Mouse the way I did (Like always he did respond in a few minutes). After an hour of reverting I found the final flag. To some the last 2 flags can seem tricky since they aren't as obvious as the rest. I'd recommend going for the last 2 flags as I feel I've learnt a lot just from completing the exam.

Finally I'm happy to pass with all 8 flags and suprisingly still manage to have over 24 hours of exam lab time. Mainly because I did pause the labs when I took breaks too. I dont have an issue setting things up again if it gives me extra time if say I needed it, thats just my train of thought. The amount of time given is overkill for that thought train and is more than enough to pwn 2 exams. This makes the exam all the more easy,relaxed and enjoyable to pwn all the flags.

### Exam Tips

- Take some time and build the Malleable c2 profile prior the exam in accordance to the threat profile and verify it using `c2lint` from the practice lab instance. I've seen most people waste time in building and setting their initial beacon up on the exam. I managed to get my 1st flag withing an hour preparing this way.
- Understand A/V evasion taught from the course well. Repeat, practice and understand everything taught. Since the practice labs have most machines without harderned defenses, I'd recommend practicing lateral movement,persistence etc with the hardened hosts in the practice labs.
- Personally I feel the amount of time allotted is overkill, so don't stress about time. Hack with tunes, take breaks, smoke some or do anything to keep your mind off when you reach a roadblock. Follow the OSCP mantra to K.I.S.S.
- The attack path will seem clear for some or most flags but be ready to encounter hardened defenses. Unless you understand how to bypass and craft your way through ingenously you might find the exam quite challenging.
- Understand Firewall Rules and implementations in the course. This is really important.
- Read and implement every HINT,TIP and TRICK @Rasta_Mouse has in the course.
- You will not see a majority of the standard tooling discussed in the lab. Practice manual enumeration with all tools in the course well.
- Take structured notes of enumeration using Joplin or so, so that you reproduce steps and have a good track of your enuermation.
- **MAJOR TIP**: **EVERYTHING YOU NEED TO PASS IS IN THE COURSE**. Don't try porting tools or outsmarting the exam. I did it and just wasted time. All the tools and attacks are from the course. Not a copy paste verbatim, you will have to think out of the box and know where to use what.

--------------------------------------------------

## Conclusion

If I could I'd definitely vouch to purchase the licensed version of Cobalt Strike someday mainly to accreditate such a beautiful project by Raphael Mudge. He is an absolute legend. And so is this course - a legal way to get your hands on Cobalt Strike along with training for such a valuable price. I encourage anyone interested with a decent share of Active Directory Exploitation skill to enroll.

Also being taught with assisstance from @Rasta_Mouse himself is a cherry on top. The exam is balanced in terms of difficulty and is a good test in terms of Cobalt Strike Operatibility. The course will someday hopefully be the de-facto for Red Teaming Operations on HR. Can't wait to see what @Rasta_Mouse has in store for CRTO2.





