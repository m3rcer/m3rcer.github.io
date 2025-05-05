---
title: Bypassing ETW, Sysmon, EventLog and Command-Line-Argument Spoofing
date: 2023-01-06 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Bypassing ETW, Sysmon, EventLog and Command-Line-Argument Spoofing
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

<!-- TOC start -->

- [Bypassing ETW](#bypassing-etw)
   * [User-Mode Attacks](#user-mode-attacks)
      + [Block Logging Events using EtwEventWrite ret patch](#block-logging-events-using-etweventwrite-ret-patch)
         - [Demo within current process](#demo-within-current-process)
         - [Demo with a remote process](#demo-with-a-remote-process)
      + [Disable .NET ETW providers using COMPlus_ETWEnabled envar](#disable-net-etw-providers-using-complus_etwenabled-envar)
         - [Demo within current process](#demo-within-current-process-1)
         - [Other variations](#other-variations)
      + [Unregister and disable ETW providers](#unregister-and-disable-etw-providers)
         - [Demo with Remote Process](#demo-with-remote-process)
      + [Obfuscating .NET assemblies](#obfuscating-net-assemblies)
   * [Kernel-Mode Attacks](#kernel-mode-attacks)
      + [ETW Session Hijacking](#etw-session-hijacking)
         - [Demo hijacking PROCMON TRACE ETW Session](#demo-hijacking-procmon-trace-etw-session)
      + [Using Custom Sycalls ](#using-custom-sycalls)
         - [Demo With ScareCrow](#demo-with-scarecrow)
- [Bypassing Sysmon](#bypassing-sysmon)
   * [User-Mode Attacks](#user-mode-attacks-1)
      + [Bypassing Sysmon Configs](#bypassing-sysmon-configs)
      + [Bypass Sysmon ETW DNS Tracing](#bypass-sysmon-etw-dns-tracing)
         - [Demo to patch the DNS ETW provider.](#demo-to-patch-the-dns-etw-provider)
   * [Kernel-Mode Attacks](#kernel-mode-attacks-1)
      + [Unloading the Sysmon Driver](#unloading-the-sysmon-driver)
         - [Demo using Shhmon](#demo-using-shhmon)
      + [Disabling Sysmon functionality with Co-Existing Altitude Numbers](#disabling-sysmon-functionality-with-co-existing-altitude-numbers)
         - [Demo showcasing Sysmon Altitude Collision Attack](#demo-showcasing-sysmon-altitude-collision-attack)
      + [Patching Sysmon EtwEventWrite events: Sysmon-Gag](#patching-sysmon-etweventwrite-events-sysmon-gag)
         - [Demo with Sysmon-Gag](#demo-with-sysmon-gag)
      + [Injecting code into Sysmon to redirect execution flow: SymonEnte](#injecting-code-into-sysmon-to-redirect-execution-flow-symonente)
      + [Bypassing Sysmon Configs](#bypassing-sysmon-configs-1)
- [Universally Bypassing ETW and Sysmon](#universally-bypassing-etw-and-sysmon)
   * [Kernel-Mode Attacks](#kernel-mode-attacks-2)
      + [Leveraging InfinityHook to hook NtTraceEvent](#leveraging-infinityhook-to-hook-nttraceevent)
         - [Demo to perform a ETW/Sysmon universal bypass](#demo-to-perform-a-etwsysmon-universal-bypass)
- [Bypassing EventLog](#bypassing-eventlog)
   * [Kernel-Mode Attacks](#kernel-mode-attacks-3)
      + [Killing Event Log Threads using Phant0m](#killing-event-log-threads-using-phant0m)
         - [Demo universally disabling all Event Log Events](#demo-universally-disabling-all-event-log-events)
      + [Block Specifc Yara Events via hooking EtwEventCallback](#block-specifc-yara-events-via-hooking-etweventcallback)
         - [Demo with SharpEvtMute](#demo-with-sharpevtmute)
      + [Event Log Manipulation using EventCleaner and Eventlogedit-evtx--Evolution](#event-log-manipulation-using-eventcleaner-and-eventlogedit-evtx-evolution)
         - [Demo universally disabling Event Log and tampering a specific event using EventCleaner](#demo-universally-disabling-event-log-and-tampering-a-specific-event-using-eventcleaner)
         - [Demo universally disabling Event Log and tampering a specific event using Eventlogedit-evtx-Evolution](#demo-universally-disabling-event-log-and-tampering-a-specific-event-using-eventlogedit-evtx-evolution)
- [Command-line Argument Spoofing](#command-line-argument-spoofing)
         - [Demo with DNS-ETW Bypass POC](#demo-with-dns-etw-bypass-poc)

<!-- TOC end -->



<!-- TOC --><a name="bypassing-etw"></a>
# Bypassing ETW


<!-- TOC --><a name="user-mode-attacks"></a>
## User-Mode Attacks

<!-- TOC --><a name="block-logging-events-using-etweventwrite-ret-patch"></a>
### Block Logging Events using EtwEventWrite ret patch

ETW providers are loaded in process memory as dlls which log events using the ***EtwEventWrite*** API function call.

The ***EtwEventWrite*** function defintion is as follows for 32bit and 64bit applications:

- 32bit:

![](Images/Pasted%20image%2020221026193644.png)

- 64bit:

![](Images/Pasted%20image%2020221026194001.png)


This technique originally [researched by [Adam Chester](https://twitter.com/_xpn_) from MDSec](https://blog.xpnsec.com/hiding-your-dotnet-etw/) is one of the most popular techniques for ETW bypasses. It can be used for the current process or also a target remote process accessible by our current user context. In short this works by using the **ret** opcode seen at the **EtwEventWrite** function end to patch the beginning of the subroutine to avoid its intended execution flow executing the **ret** opcode initially to return without writing any events. This way **all ETW events** can be stopped from being logged for the **current process**.

[Adam Chester](https://twitter.com/_xpn_) originally showcased the technique for 32bit using the following patch:

- 32bit: 
```
\xc2\x14\x00\x00 --> ret 14h
```


Minor variations of this patch are publically available. For example [Sektor7](https://institute.sektor7.net/rto-maldev-intermediate) showcases this technique with the following patch to zero out the registers before returning using the **ret** opcode:

- 32bit: 
```
\x33\xc0\xc2\x14\x00 --> xor eax, eax; ret 14
```

- 64bit: 
```
\x48\x33\xc0\xc3 --> xor rax, rax; ret
```

The control flow of **EtwEventWrite** can be modified using:
• Function patching with RET
• Import Address Table (IAT) Hooking
• Inline hooking

For instance, [TamperETW by Outflank](https://github.com/outflanknl/TamperETW) is a slight modification of this technique which allows using native system calls to hook the **EtwEventWrite** to redirect flow to a custom **MyEtwEventWrite** function and selectively forward .NET ETW events.

Another reimplementation called [BypassETW_CSharp](https://github.com/Kara-4search/BypassETW_CSharp) uses **RtlInitializeResource** API to patch the target **EtwEventWrite** function the same way to stop ETW events being logged for the current process.

To use this technique in a remote process we can use [injectEtwBypass by boku7](https://github.com/boku7/injectEtwBypass) which patches the  **EtwEventWrite** function using a **ret** opcode in a target remote process accessible by our current user context.

<!-- TOC --><a name="demo-within-current-process"></a>
#### Demo within current process

We can detect Seatbelt's execution in memory using the **Microsoft CLR Runtime ETW Provider** with a GUID **{e13c0d23-ccbc-4e12-931b-d9cc2eee27e4}**. Using SilkETW and a simple yara rule we can detect SeatBelt's **MethodNameSpace: (Seatbelt.Seatbelt)** to view the ETW .NET trace events. Process hacker can also used to analyze events from the **.Net Runtime ETW Provider**. 

Yara rule:

```powershell
rule Seatbelt_MethodNameSpace
{
    strings:
        $s1 = "MethodNamespace=Seatbelt.Seatbelt" ascii wide nocase

    condition:
        all of ($s*)
}
```

Cobalt Strike and the **inlineExecute-Assembly** aggressor are used to execute Seatbelt within the current beacon process context.

![](Images/Pasted%20image%2020221027150734.png)

Viewing events generated by SilkETW:

![](Images/Pasted%20image%2020221027152201.png)

Viewing the loaded .NET assemblies using Process Hacker:

![](Images/Pasted%20image%2020221027153248.png)

Now using the **--etw** flag within **inlineExecute-Assembly** it is possible to bypass ETW using the the **EtwEventWrite** patch to find no ETW .NET runtime events generated by SilkETW for the beacon process.

![](Images/Pasted%20image%2020221027160951.png)

![](Images/Pasted%20image%2020221027151140.png)

![](Images/Pasted%20image%2020221027152258.png)

No ETW events are forwarded. Viewing for .NET assemblies in the beacon process using Process Hacker:

![](Images/Pasted%20image%2020221027151653.png)

To specifically bypass yara rules such as for method/namespace names as above and blend into the ETW .NET trace events we can implement custom __AppDomain Names__ using the **--appdomain** argument and unique __named pipe names__ using the **--pipe** argument within **inlineExecute-Assembly**.

<!-- TOC --><a name="demo-with-a-remote-process"></a>
#### Demo with a remote process

We target a powershell process spawned by our current user with a **PID:3472** to inject our ETW bypass into.

![](Images/Pasted%20image%2020221027160202.png)

We use the **injectEtwBypass** aggressor which implements **Sycalls via Halos|HellsGate** for lesser visibility avoiding commonly abused Win32 APIs to inject the same **EtwEventWrite** patch as follows.

![](Images/Pasted%20image%2020221027160644.png)

![](Images/Pasted%20image%2020221027160316.png)

![](Images/Pasted%20image%2020221027160300.png)


<!-- TOC --><a name="disable-net-etw-providers-using-complus_etwenabled-envar"></a>
### Disable .NET ETW providers using COMPlus_ETWEnabled envar

A quick way to only disable the Microsoft CLR Runtime Provider referencing the GUID **{e13c0d23-ccbc-4e12-931b-d9cc2eee27e4}** yet again researched by [Adam Chester](https://twitter.com/_xpn_) is by setting the environment variable  **COMPlus_ETWEnabled** to **0**.

From [Adam Chester's Blog](https://blog.xpnsec.com/hiding-your-dotnet-complus-etwenabled/) it is stated that **COMPlus_\*** settings provide developers via environment variables or registry value a number of configuration options which can be set at runtime with various levels of impact on the CLR, from loading alternative JITters, to tweaking performance and even dumping the IL of a method. 

If **COMPlus_ETWEnabled=0** is enabled, the CLR will jump past the block of ETW registrations to the .NET ETW providers, this way avoid logging any .NET assemblies within the current process context.

<!-- TOC --><a name="demo-within-current-process-1"></a>
#### Demo within current process

[Adam Chester](https://twitter.com/_xpn_) has developed a [POC](https://gist.github.com/xpn/64e5b6f7ad370c343e3ab7e9f9e22503) to showcase this along with his developed argument spoofing technique to hide the **COMPlus_ETWEnabled=0** variable from **CreateProcess**. We will be using this POC to disable **.NET assembly ETW trace events**.

Compiling the source using **x86_64-w64-mingw32-g++** and executing the POC spawns a powershell prompt with added Command line Argument spoofing over the **COMPlus_ETWEnabled=0** bypass hence disabling all .NET assembly ETW trace events for the spawned powershell process.

![](Images/Pasted%20image%2020221027200248.png)

<!-- TOC --><a name="other-variations"></a>
#### Other variations

Since the discovery of the attack on the envar **COMPlus_ETWEnabled**, variations have emerged as showcased in [this blog](http://redplait.blogspot.com/2020/07/whats-wrong-with-etw.html)

- Disable tracing for services.exe by disabling the registry key: **TracingDisabled** located at **HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Tracing\\SCM\\Regular**. (Requires admin privileges)

![](Images/Pasted%20image%2020221031115307.png)

-  Similarly **rpcrt4.dll** can be disabled to avoid RPC generated events using the registry key **ExtErrorInformation** at: **HKLM\\Software\\Policies\\Microsoft\\Windows NT\\Rpc**. Here's a [POC](https://github.com/redplait/armpatched/blob/master/ldr/rpcrt4_hack.cpp) to find ETW handles in **rpcrt4.dll**.

<!-- TOC --><a name="unregister-and-disable-etw-providers"></a>
### Unregister and disable ETW providers

This method was researched by [modexp](https://twitter.com/modexpblog) and details how to register and disable a specific ETW provider for a target process. Here's the [blog](https://modexp.wordpress.com/2020/04/08/red-teams-etw/#disable) detailing how **modexp** managed to do this. Also he has released a POC which can be found [here](https://github.com/odzhan/injection/tree/master/etw)

In short, ETW providers register using the **advapi32!EventRegister** API, this is forwarded to **ntdll!EtwEventRegister**. This API validates arguments and forwards them to **ntdll!EtwNotificationRegister**. The caller provides a unique GUID that represents a provider on the system, an optional callback function and an optional callback context. This Callback function for a provider is invoked in request by the kernel to enable or disable tracing. This way Registration handles can later be used with **EventUnregister** to disable tracing.

[ETW Dump](https://github.com/odzhan/injection/tree/master/etw) is the POC released showcasing this and can also display information about each ETW providers in the registration table of one or more processes as follows.


![](Images/Pasted%20image%2020221031160226.png)

<!-- TOC --><a name="demo-with-remote-process"></a>
#### Demo with Remote Process

In this case we target **powershell.exe**. The Callback function as stated earlier for a provider is invoked in request by the kernel to enable or disable tracing. For the CLR, the relevant function is **clr!McGenControlCallbackV2**. Code redirection is achieved by replacing the callback address with the address of a new callback (shellcode) which must use the same prototype which can be done via **NtTraceControl**.

[modexp](https://twitter.com/modexpblog) unregisters a specifc provider in his POC by passing the registration handle to **ntdll!EtwEventUnregister**. In this instance he disables the .NET Runtime provider for a target powershell process running under our current user privileges. Examining with SilkETW after disabling the provider we dont find any new .NET ETW events for the spawned powershell process.  

![](Images/Pasted%20image%2020221031161247.png)


<!-- TOC --><a name="obfuscating-net-assemblies"></a>
### Obfuscating .NET assemblies

It is possible to obfuscate .NET assemblies using tools like [ConfuserEx](https://yck1509.github.io/ConfuserEx/) or an [LLVM-Obfuscator](https://github.com/obfuscator-llvm/obfuscator) to break yara .NET ETW signatures and bypass ETW selective .NET events whose detection rely on specific yara signatures. 


<!-- TOC --><a name="kernel-mode-attacks"></a>
## Kernel-Mode Attacks

<!-- TOC --><a name="etw-session-hijacking"></a>
### ETW Session Hijacking

This attack vector was originally researched by [Binarly.io](https://binarly.io/) and details the research in this [blog](https://www.binarly.io/posts/Design_issues_of_modern_EDR%27s_bypassing_ETW-based_solutions/index.html). An ETW session is a global object identified by a unique name that allows multiple ETW consumers to subscribe and receive events from different ETW providers. The default number of simultaneously running sessions is 64. The NT kernel supports a maximum of 8 System Logger Sessions with hardcoded unique names.

Process Monitor is a tool for malware analysis which uses the same technology as many EDRs.
[Process Monitor version 3.85]() uses a session called **PROCMON TRACE** to record network events. 
An attacker can stop a target ETW session using Session names like the **PROCMON TRACE** session and start fake ones instead to stop the consumer, in this case Process Monitor 3.85 from recieving any events. Relaunching Process Monitor too does not fix this attack.

To hijack a Secure ETW session like Windows Defender ETW Sessions different principles apply since we cannot stop the Defender ETW sessions and hijack it as for Process Monitor.
Defender uses the two ETW sessions: *DefenderApiLogger* and *DefenderAuditLogger*. They are run on startup as Autologger Sessions and hence can be disabled by: 

1. Modifying the Startup registry setting: **reg add "HKLM\\System\\CurrentControlSet\\Control\\WMI\\Autologger\\DefenderApiLogger" /v "Start" /t REG_DWORD /d "0" /f**
2. Each Defender ETW session has a security descriptor in registry. By patching the values in registry it is possible to modify the Defender ETW session security descriptors. 
3. It is possible to load a malware driver to patch the **WMI_LOGGER_CONTEXT** structure while using the **QueryAllTracesW() / StopTraceW()** APIs to stop or query a Secure ETW Defender session (not stoppable and running with high privs by default).


<!-- TOC --><a name="demo-hijacking-procmon-trace-etw-session"></a>
#### Demo hijacking PROCMON TRACE ETW Session

[ORCx41](https://github.com/ORCx41) has released a [POC](https://github.com/ORCx41/EtwSessionHijacking) demonstrating this attack. This works by stopping the target ETW session and starting one under the same name. It then checks for the status of the newly started malware session and if terminated it can repeat the process from the beginning. As long as this malware created session is active network events aren't detected.

We start up ProcMon and analyze all network events after visiting a site on chrome as follows:

![](Images/Pasted%20image%2020221104184624.png)

After running the ETW Procmon Session Hijacker and revisiting a site on chrome no Network events are further generated.

![](Images/Pasted%20image%2020221104184805.png)


<!-- TOC --><a name="using-custom-sycalls"></a>
### Using Custom Sycalls 

Attacker can call custom Syscalls which dosen't include ETW instrumentation and information about the call to avoid logging.

ETW utilizes built-in Syscalls to generate telemetry. Since ETW is a native feature built into Windows, security products do not need to "hook" the ETW syscalls to gain this information. As a result, to prevent ETW, [ScareCrow](https://github.com/optiv/ScareCrow) patches numerous ETW syscalls, flushing out the registers and returning the execution flow to the next instruction. This is enabled by default and to disable this feature use the **-noetw** argument.

<!-- TOC --><a name="demo-with-scarecrow"></a>
#### Demo With ScareCrow

Compile Cobalt strike Shellcode using ScareCrow into an executable with ETW patched as follows:

![](Images/Pasted%20image%2020221110200832.png)

Examining the ScareCrow beacon process after .NET execution within the process using Process Hacker we find no .NET assemblies tab loaded.

![](Images/Pasted%20image%2020221110201557.png)


Examining with SilkETW and a basic yara rule to detect Outlook.exe process name via .NET generated events, we find no .NET logs generated from the loader Outlook.exe process.

- Yara rule:

```powershell
rule Outlook_ScareCrowProcessNameDetection
{
    strings:
        $s1 = "ProcessName=Outlook" ascii wide nocase

    condition:
        all of ($s*)
}
```

![](Images/Pasted%20image%2020221110202318.png)



<!-- TOC --><a name="bypassing-sysmon"></a>
# Bypassing Sysmon

<!-- TOC --><a name="user-mode-attacks-1"></a>
## User-Mode Attacks

<!-- TOC --><a name="bypassing-sysmon-configs"></a>
### Bypassing Sysmon Configs

Sysmon Configuration files are detection rules written as XML templates and it is easy to find blindspots in these configuration files for unaccounted execution beyond its scope.

It is possible to retrieve Sysmon Configuration files in a number of ways, via registry / [Get-SysmonConfiguration.ps1](https://www.powershellgallery.com/packages/Posh-Sysmon/1.0/Content/Functions%5CGet-SysmonConfiguration.ps1) (admin privileges needed). If lucky it is possible to find the sysmon configuration file with misconfigured ACLs accessible with user privileges. 
It is possible to use a tool such as [sysmon-config-bypass-finder](https://github.com/mkorman90/sysmon-config-bypass-finder) /  [SysmonRuleParser.ps1](https://github.com/mattifestation/PSSysmonTools/blob/master/PSSysmonTools/Code/SysmonRuleParser.ps1) to find loopholes within the configuration file to perform malicious execution without leaving any Sysmon Logs. 

There are 2 more methods to tamper with Sysmon Configs which is mentioned in the next section.

<!-- TOC --><a name="bypass-sysmon-etw-dns-tracing"></a>
### Bypass Sysmon ETW DNS Tracing

[Adam Chester](https://twitter.com/_xpn_) researched and detailed how DNS ETW events can be patched similar to his first bypass and  showcased this in his [blog](https://blog.xpnsec.com/evading-sysmon-dns-monitoring/). 

In short, DNS events generated are sent to the **Microsoft-Windows-DNS-Client** from within **DnsApi.dll**. Since this dll is loaded within the current process we can control its execution flow. It is possible to patch the **DNSAPI!McTemplateU0zqxqz** routine using a simple **ret** opcode during runtime to return execution without sending an event via **EtwEventWriteTransfer**.

Here is the [POC](https://gist.github.com/xpn/59bbde64b965b4374a9f390d4ae44288#file-dns_sysmon_patch-c) released by [Adam Chester](https://twitter.com/_xpn_). This POC can be coupled with other Sysmon ETW bypass techniques like driver unloading/Altitude number collision where DNS entries persist because of the unloading of Microsoft DNS ETW providers/kernel minidrivers. 

<!-- TOC --><a name="demo-to-patch-the-dns-etw-provider"></a>
#### Demo to patch the DNS ETW provider.

We use the above POC to patch the DNS ETW provider to disable forwarding any DNS ETW events and once done ping google.com using the **DnsQuery_A** WINAPI to test this as follows. As a result we see only a **PROCESSS_CREATION** Event with the latest [SwiftOnSecurity sysmon-config](https://github.com/SwiftOnSecurity/sysmon-config).

![](Images/Pasted%20image%2020221109145620.png)


<!-- TOC --><a name="kernel-mode-attacks-1"></a>
## Kernel-Mode Attacks

<!-- TOC --><a name="unloading-the-sysmon-driver"></a>
### Unloading the Sysmon Driver

It is possible to unload the Sysmon Kernel minidriver from memory which effectively kills only the kernel portion of sysmon using **fltmc**: **fltmc unload \<driver name\>**. This requires **SE_LOAD_DRIVER** privileges.

![](Images/Pasted%20image%2020221101164137.png)

Implications: 

- Once the driver is unloaded, an error event with an ID of **DriverCommunication** will be generated.

![](Images/Pasted%20image%2020221101164421.png)

- **Error log** stating couldn't recieve events and some logging functionality persists like **DNS queries** because of the use of seperate ETW providers (Sysmon uses other Microsoft ETW providers too for logging)

![](Images/Pasted%20image%2020221101164023.png)

- Command line logging would be captured by Sysmon before the driver is unloaded.

![](Images/Pasted%20image%2020221101163955.png)

- Since this technique requires the **SeLoadDriverPrivileges** an audit is logged for its usecase.

![](Images/Pasted%20image%2020221101164607.png)

- **FilterManager** reports that a File System Driver was unloaded succesfully:

![](Images/Pasted%20image%2020221101164817.png)

A reboot would fix and load back the Sysmon Driver.

<!-- TOC --><a name="demo-using-shhmon"></a>
#### Demo using Shhmon

Another way to perform this attack is by using [Shhmon](https://github.com/matterpreter/Shhmon) explained in this [Blog](https://posts.specterops.io/shhmon-silencing-sysmon-via-driver-unload-682b5be57650) which uses WIN APIs to perform the same unloading process. One implication it helps evade is the initial command line logging which would be captured by Sysmon before the driver is unloaded.

![](Images/Pasted%20image%2020221101165802.png)


<!-- TOC --><a name="disabling-sysmon-functionality-with-co-existing-altitude-numbers"></a>
### Disabling Sysmon functionality with Co-Existing Altitude Numbers

[Sektor7](https://institute.sektor7.net/rto-maldev-intermediate) showcased that by changing the altitude number of the Sysmon Kernel Minidriver to that of an already existing minidriver it is possible to break Sysmons functionality and disrupt the Sysmon Minidriver from processing Sysmon events.

Query Sysmon Minidriver Altitude using **fltmc** or registry: **reg query "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Sysmondrv\\Instances\\Sysmon Instance"**

![](Images/Pasted%20image%2020221107151622.png)

Most of the times the Symon Altitude Number would remain constant but it is possible to change this.

2 minor drawbacks of this method is that it leaves and error log for loading the kernel driver and the second that it requires a reboot to make it effective. 


<!-- TOC --><a name="demo-showcasing-sysmon-altitude-collision-attack"></a>
#### Demo showcasing Sysmon Altitude Collision Attack

Let us target the **WDFilter - \\Device\\Mup** Minidriver and change Sysmon's Altitude to its Altitude Number.

![](Images/Pasted%20image%2020221107151850.png)

Change Sysmon Altitude using registry: **reg add "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Sysmondrv\\Instances\\Sysmon Instance" /v Altitude /t REG_SZ /d 328010 /f**

![](Images/Pasted%20image%2020221107152119.png)

To make this effective reboot the machine. Verify Sysmons existence using **fltmc instances** to find no Sysmon Minidriver:

![](Images/Pasted%20image%2020221107152330.png)

Checking EventViewer after simulating a Sysmon Event using **SysmonSimulator** we find an error log loading the Kernel Minidriver hence effectively killing the kernel portion of Sysmon. However DNS queries could still be logged since it depends on a seperate DNS ETW provider. 

![](Images/Pasted%20image%2020221107153223.png)

![](Images/Pasted%20image%2020221107152727.png)

<!-- TOC --><a name="patching-sysmon-etweventwrite-events-sysmon-gag"></a>
### Patching Sysmon EtwEventWrite events: Sysmon-Gag

As before, it is possible to get a handle to the **SysmonDrv** process and patch (**ret**) **EtwEventWrite** to disable writing Sysmon ETW events as a whole. This was showcased by [Sektor7]() and termed it **Sysmon-Gag**.

Performing this technique avoids all Sysmon events, including DNS log entries and Error Log messages.

Sysmon functionality can be restored with a simple system or service restart.

An RDLL POC implementation of this technique can be found on github: [SysmonQuiet](https://github.com/ScriptIdiot/SysmonQuiet)

<!-- TOC --><a name="demo-with-sysmon-gag"></a>
#### Demo with Sysmon-Gag

Executing **Sysmon-Gag** and note to see that the Sysmon Kernel minidriver portion is untouched:

![](Images/Pasted%20image%2020221107163932.png)

Testing Process Creating and DNS events using Sysmon Simulator we find no events generated:

![](Images/Pasted%20image%2020221107164415.png)

<!-- TOC --><a name="injecting-code-into-sysmon-to-redirect-execution-flow-symonente"></a>
### Injecting code into Sysmon to redirect execution flow: SymonEnte

This technique researched by [codewhitesec](https://github.com/codewhitesec) involves injecting code into Sysmon which redirects the execution flow in such a way that events can be manipulated before being forwarded to the SIEM. Here is the [POC](https://github.com/codewhitesec/SysmonEnte) detailing the research.

In short as per the [blog](https://codewhitesec.blogspot.com/2022/09/attacks-on-sysmon-revisited-sysmonente.html) it works as follows:
-   Suspend all threads of Sysmon.
-   Create a limited handle to Sysmon and elevate it by duplication.
-   Clone the pseudo handle of Sysmon to itself in order to bypass SACL as proposed by [James Forshaw](https://www.tiraniddo.dev/2017/10/bypassing-sacl-auditing-on-lsass.html).
-   Inject a hook manipulating all events (in particular ProcessAccess events on Sysmon).
-   Resume all threads.

Additionally, **SysmonEnte** uses [indirect syscalls](https://github.com/thefLink/RecycledGate) to bypass possible userland hooks.

The Repo provides two ways to use **SysmonEnte**, one via inject shellcode - **SysmonEnte.bin** and the second via using a loader - **EntenLoader.exe \<PID Sysmon\>**

<!-- TOC --><a name="bypassing-sysmon-configs-1"></a>
### Bypassing Sysmon Configs

The second way to circumvent and operate in the configuration blindspots is to delete the Sysmon Configuration file. We can clear these configs in the registry. Sysmon will see the registry being changed and it will automatically reload the configuration and since no rules are present it will be blinded temporarily allowing to operate in a small time frame without detection depending on how the configuration is maintained. One major giveaway of this technique is that process creation and process termination events will have a blank rule name in them. 

Lastly, it is possible to mess with Sysmon Configs by replacing the original with one of the attackers chosing. There is a technique showcased by [Sinaei](https://twitter.com/Intel80x86) on [twitter](https://twitter.com/intel80x86/status/1186009977158287360?lang=en). Basically, whenever Sysmon detects a configuration change, it calls **DeviceIoControl** with the **0x83400008 IOCTL** in order to tell **SysmonDrv.sys** to update its rules and then Sysmon sends a **Event ID:16: Sysmon config state change** to Event Logs. It is possible to create and configure a very permissable Sysmon Config by modifying the registry: **HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\services\\SysmonDrv\\Parameters** and send a **0x83400008 IOCTL** directly after without sending an update changed notification to Event logs. This [POC](https://github.com/SinaKarvandi/Process-Magics/tree/master/Bypass%20Sysmon%20With%20Updating%20Rules) was tested for Sysmon version 9.21 and prior. 

<!-- TOC --><a name="universally-bypassing-etw-and-sysmon"></a>
# Universally Bypassing ETW and Sysmon

<!-- TOC --><a name="kernel-mode-attacks-2"></a>
## Kernel-Mode Attacks

<!-- TOC --><a name="leveraging-infinityhook-to-hook-nttraceevent"></a>
### Leveraging InfinityHook to hook NtTraceEvent

This technique was researched by [batsec](https://github.com/bats3c). Here is the [blog](https://web.archive.org/web/20211128164539/https://blog.dylan.codes/evading-sysmon-and-windows-event-logging/) and [POC](https://github.com/bats3c/Ghost-In-The-Logs) detailing the research.

As per the blog, **Sysmon64.exe** uses the **ReportEventW** Windows API call to report events. Going deeper down the call chain it is found that **ReportEventW** in **ADVAPI32.dll** is essentially a wrapper around **EtwEventWriteTransfer** which is defined in **NTDLL.dll**. By examining **EtwEventWriteTransfer** it is found that it calls the kernel function **NtTraceEvent** which is defined inside **ntoskrnl.exe**.

![](Images/Pasted%20image%2020221101145205.png)

It is possible to patch the **nt!NtTraceEvent** function with a **ret** instruction to avoid returning any events. Since this is a kernel function we need to have our hooking code running inside the kernel space as well and bypass protections like **Kernel driver signing enforcement** and **PatchGuard**. The author leverages [InfinityHook](https://web.archive.org/web/20211128164539/https://github.com/everdox/InfinityHook) to bypass these protections and hook the **nt!NtTraceEvent** to our **DetourNtTraceEvent** which if returns any non zero value return **STATUS_SUCCESS** signifying that the event was reported successfully, which in reality wasn't.

Since this is a universal bypass, it should stop ETW and Sysmon events for all processes.

<!-- TOC --><a name="demo-to-perform-a-etwsysmon-universal-bypass"></a>
#### Demo to perform a ETW/Sysmon universal bypass

We can begin testing if Sysmon is universally disabled for all processes. A simple test would be to spawn a powershell process after using the universal bypass to check if Sysmon reports the **PROCESS_CREATE** event.

Load the driver and set the hook:

![](Images/Pasted%20image%2020221101150727.png)

Enable the hook:

![](Images/Pasted%20image%2020221101151145.png)

Spawning a new powershell prompt we find there are no logs logged by Symon.

![](Images/Pasted%20image%2020221101151124.png)

It is also possible to check this using **SysmonSimulator** to test Process Creation to find no new events:

![](Images/Pasted%20image%2020221101162214.png)

Starting SilkETW and noting for .NET ETW events after spawning a Powershell Prompt we find no events reported:

![](Images/Pasted%20image%2020221101151759.png)

Analysing the spawned PowerShell Prompt using ProcessHacker we find no .NET assemblies loaded:

![](Images/Pasted%20image%2020221101151846.png)

Disable the hook, enabling all logging:

![](Images/Pasted%20image%2020221101152053.png)

![](Images/Pasted%20image%2020221101162355.png)




<!-- TOC --><a name="bypassing-eventlog"></a>
# Bypassing EventLog

<!-- TOC --><a name="kernel-mode-attacks-3"></a>
## Kernel-Mode Attacks

<!-- TOC --><a name="killing-event-log-threads-using-phant0m"></a>
### Killing Event Log Threads using Phant0m

It is possible to target the Event Log detect and kill the threads responsible for the Event Log service. Doing so would result in the Event Log service appear to be running but intrun the system does not collect logs since the threads required for log collection were terminated. 

This concept was originally introduced by the **Shadow Brokers leak of NSA’s eventlogedit**. 

[Phant0m](https://github.com/hlldz/Phant0m) is a good POC for this technique of disabling Event Logs from being logged via Event Log Thread termination. It leverages two techniques to perform this. 

The first technique leverages the TEB block (thread environment block) to find the numeric tag of the target Event Log Service and kill its corresponding threads responsible for the Event Logging functionality. The second method leverages dlls associated with the Event Log threads to validate if these threads are using the specific dll and if the thread is using that DLL, it is the Windows Event Log Service’s thread and then Phant0m kills the thread.

<!-- TOC --><a name="demo-universally-disabling-all-event-log-events"></a>
#### Demo universally disabling all Event Log Events

A custom view is created to log all events in the Event Log Service for demo purposes.

![](Images/Pasted%20image%2020221103163350.png)

Analysing threads of the EventLog Service process using Process Hacker:

![](Images/Pasted%20image%2020221103165211.png)

Execute **phant0m** to terminate EventLog threads:

![](Images/Pasted%20image%2020221103164349.png)

Analysis of EventLog Service threads after phant0m execution:

![](Images/Pasted%20image%2020221103165327.png)

Making a small change to **Local Security Policy** we find no new logs generated in our Custom View

![](Images/Pasted%20image%2020221103165700.png)

![](Images/Pasted%20image%2020221103165611.png)

<!-- TOC --><a name="block-specifc-yara-events-via-hooking-etweventcallback"></a>
### Block Specifc Yara Events via hooking EtwEventCallback

This technique was researched by [Jumpsec Labs](https://labs.jumpsec.com/). Here is the [blog](https://labs.jumpsec.com/pwning-windows-event-logging-with-yara-rules/) detailing the research. 

Basically **wevtsvc.dll** which is the Event Log service uses **OpenTraceW** to open a tracing session. **OpenTraceW** takes the **EVENT_TRACE_LOGFILEW** structure as an argument. This structure has the value **EventRecordCallback** which points to the callback function that will be given the event. The callback function is just a bit of assembly that will call **EventCallback**. This callback receives the event in the **EVENT_RECORD** structure which contains info about the event including the ETW provider. It is possible to hook this callback to our own yara callback and patch it to return no events and also restore it dynamically so that events that aren't targetted can be delivered. 

**EvtMuteHook.dll** contains the core functionality, (via hooking **EtwEventCallback**) once it is injected it will apply a temporary filter which will allow all/selective events to be reported, this filter can be dynamically updated without having to reinject the dll. **SharpEvtMute.exe** is its C# .NET assembly implementation compatible with **execute-assembly/inlineExecuteAssembly**.

<!-- TOC --><a name="demo-with-sharpevtmute"></a>
#### Demo with SharpEvtMute

We use the same Yara rule along with SilkETW and execute Seatbelt using **inlineExecute-assembly** from Cobalt Strike to detect Seatbelt execution based on Methodspace Names.

![](Images/Pasted%20image%2020221031190907.png)

Start by injecting the hook into the event service. Next, once the hook is placed add the following filter to drop all yara events by EventViewer. 

*Note: It is possible to use other complex yara filters to filter specific criteria.*

![](Images/Pasted%20image%2020221031200557.png)

As noted from the output, after executing **Seatbelt** using **inlineExecute-Assembly** from Cobalt Strike, SilkETW crashes after the first rule detection and fails to write to the EventLog.

<!-- TOC --><a name="event-log-manipulation-using-eventcleaner-and-eventlogedit-evtx-evolution"></a>
### Event Log Manipulation using EventCleaner and Eventlogedit-evtx--Evolution

[EventCleaner](https://github.com/QAX-A-Team/EventCleaner) and [Eventlogedit-evtx--Evolution](https://github.com/3gstudent/Eventlogedit-evtx--Evolution) combines the method of **Event Viewer Thread Tampering (showcased in previous section), Duplicating Handles** and **Event Record Manipulation** to disrupt the Event Log Service and manipulate/delete induvidual logs. The only difference between the two is that **EventCleaner** disrupts the Event Log service and deletes the target malicious log leaving out a gap between sequential record id's, while **Eventlogedit-evtx--Evolution** does the same but after deleting the target malicious log restores it in a way with sequential record ids.

<!-- TOC --><a name="demo-universally-disabling-event-log-and-tampering-a-specific-event-using-eventcleaner"></a>
#### Demo universally disabling Event Log and tampering a specific event using EventCleaner

As an example we modify the **Password Policy** in **Local System Policy** which generates the following event with a **Event Record ID:244**. 

![](Images/Pasted%20image%2020221103193249.png)

![](Images/Pasted%20image%2020221103193228.png)

We will be targetting to clear this specific event using **EventCleaner**. Begin by suspending the Event Log threads:

![](Images/Pasted%20image%2020221103193553.png)

Duplicate the handle on the **security.evtx** file and close the current handle:

![](Images/Pasted%20image%2020221103193619.png)

It is noted that the Event Log service no longer has a file handle for **security.evtx**:

![](Images/Pasted%20image%2020221103191705.png)

Delete the **EventRecordID:244**, fix the checksums, and restore the file handle.

![](Images/Pasted%20image%2020221103193721.png)

Resume the threads of the service:

![](Images/Pasted%20image%2020221103193751.png)

Inspecitng **security.evtx** it is observed that the **EventRecordID** skips from 243 to 245, hence the target **EventRecordID:244** was succesfully deleted. However this attack can be detected due to non-sequential EventRecordIDs.

![](Images/Pasted%20image%2020221103193952.png)

<!-- TOC --><a name="demo-universally-disabling-event-log-and-tampering-a-specific-event-using-eventlogedit-evtx-evolution"></a>
#### Demo universally disabling Event Log and tampering a specific event using Eventlogedit-evtx-Evolution

We can improve on the previous demo to avoid non-sequential EventRecordIDs using **Eventlogedit-evtx-Evolution**. We use the same example as before changing the **Password Policy** using **Local System Policy** to generate a Security Event with a **EventRecordID: 4739**

![](Images/Pasted%20image%2020221103194522.png)

![](Images/Pasted%20image%2020221103194555.png)

As with the previous attempt, our first step is to suspend the threads of the EventLog service as follows:

![](Images/Pasted%20image%2020221103194712.png)

We next use the **EvtExportLog WinAPI** to copy the current **Security.evtx** log, excluding a specific **EventRecordID** as follows:

![](Images/Pasted%20image%2020221103194851.png)

Use **DeleteRecordbyGetHandleEx.exe**, to move **temp.evtx**to replace **Security.evtx** by duplicating the file handle on the file as follows:

![](Images/Pasted%20image%2020221103195110.png)

Finally resume the threads with **SuspendorResumeTid.exe** as follows:

![](Images/Pasted%20image%2020221103195204.png)

We’ve now successfully replaced the log without evidence of EventRecordID inconsistencies.

<!-- TOC --><a name="command-line-argument-spoofing"></a>
# Command-line Argument Spoofing

Cobalt Strike 3.13 introduced the **argue** aggressor command showcasing argument spoofing intially. [Adam Chester](https://twitter.com/_xpn_) improved on this and wrote a good [blog](https://blog.xpnsec.com/how-to-argue-like-cobalt-strike/) describing his add reimplementations with a good [POC](https://gist.github.com/xpn/1c51c2bfe19d33c169fe0431770f3020#file-argument_spoofing-cpp).

In short tools like Process Hacker use APIs such as **NtQueryInformationProcess** to retrieve the **PEB** (Process Environment Block) structure from a target process and then enumerate process arguments from **ProcessParameters** located within the **PEB** structure.  It is possible to update **ProcessParameters** structure to hide arguments passed to the process.

It is possible to perform this by first spawning our target process in a suspended state using the **CREATE_SUSPENDED** flag within the **CreateProcessA** API. Next parsing and grabbing the **PEB** structure address using the **ReadProcessMemory** API. We then parse the  **ProcessParameters** structure field within the read **PEB** Structure to read the process arguments which is of type **struct UNICODE_STRING**.  It is possible now to update the arguments by writing to the **UNICODE_STRING.Buffer** address within **struct UNICODE_STRING** using **WriteProcessMemory** after which resume execution with **ResumeExecution** API.  

This is how basically the **argue** aggressor worked but tools like Process Hacker actually retrieve a copy of the **PEB** each time the process is inspected, meaning that spoofed arguments will be revealed. [Adam Chester](https://twitter.com/_xpn_) improved on this technique with a slight modifications by creating a corrupted **UNICODE_STRING** which did hide arguments from tools like Process Hacker and next tried setting the **Length** parameter to be less than the size of the string set within the **Buffer** to make the remainder of the command line hidden (Unicode length of process name). This worked efficiently for spoofing arguments with Sysmon detection and tools like Process Hacker could no longer read command line arguments.

*NOTE: Make sure to keep the length of the spoofed arguments longer than the actual arguments.*

There are many reimplementations of this POC such as: [CmdLineSpoofer](https://github.com/plackyhacker/CmdLineSpoofer) and [PEB-PPIDspoofing_Csharp](https://github.com/Kara-4search/PEB-PPIDspoofing_Csharp)

<!-- TOC --><a name="demo-with-dns-etw-bypass-poc"></a>
#### Demo with DNS-ETW Bypass POC

Executing [Adam Chesters POC](https://gist.github.com/xpn/1c51c2bfe19d33c169fe0431770f3020#file-argument_spoofing-cpp) we note that Process Hacker dosen't include any command line arguments:

![](Images/Pasted%20image%2020221110184820.png)

Examining with Sysmon we find the spoofed command line arguments:

![](Images/Pasted%20image%2020221110184858.png)

Actual command line arguments executed from POC source are:

![](Images/Pasted%20image%2020221110184949.png)