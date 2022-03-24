---
title: Creating a mutational fuzzer to fuzz POST requests for possible sqli errors.
date: 2021-06-05 15:45:47 +07:00
categories: greyhatcch2
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a mutational fuzzer to fuzz POST requests for possible sqli errors.
---

## This Program is a basic mutational fuzzer as before that will fuzz POST requests to generate errors to validate a possible sql vulnerability.

- We use Burp Suite to intercept and save the POST request(Add Item to cart) to a file to implement in the program. 
- We start by using the `ReadAllLines() method` instead of `ReadAllText()` as it automatically splits at newlines in the file(to split the POST request).
- We use `System.Text.StringBuilder()` method to build strings instead of using the '+=' operator to build strings as the `StringBuilder method` creates only one object in memory resulting in less memory overhead.
- We append "\r\n" to the request else the server will hang.
- We use `System.Net.Sockets.Socket` to build a socket for a already generated http request.
- We create the rhost by passing the ip to `IpEndPoint() method`.
- We use `SocketType.Stream` to tell that it is a streaming socket and `Address.Family` as `InterNetwork` to use `IPV4`.


### Code:

```Csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PostReqFuzzer
{
    class Program
    {
        public static void Main(string[] args)
        {
            string[] requestLines = File.ReadAllLines(args[0]);
            string[] parms = requestLines[requestLines.Length - 1].Split('&');

            string host = string.Empty;
            StringBuilder requestBuilder = new StringBuilder();

            foreach (string ln in requestLines)
            {
                if (ln.StartsWith("Host:"))
                    host = ln.Split(' ')[1].Replace("\r", string.Empty);
                requestBuilder.Append(ln + "\n");
            }

            Console.WriteLine("Host is : " + host + "\r\n");
            string request = requestBuilder.ToString() + "\r\n";
            Console.WriteLine("Final request is: \r\n" + request);

            IPEndPoint rhost = new IPEndPoint(IPAddress.Parse(host), 80);

            foreach(string parm in parms)
            {
                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    sock.Connect(rhost);

                    string val = parm.Split('=')[1];
                    string req = request.Replace("=" + val, "=" + val + "'");

                    byte[] reqBytes = Encoding.ASCII.GetBytes(req);
                    sock.Send(reqBytes);

                    byte[] buf = new byte[sock.ReceiveBufferSize];

                    sock.Receive(buf);
                    string response = Encoding.ASCII.GetString(buf);

                    if (response.Contains("error in your SQL syntax"))
                    {
                        Console.WriteLine("[+] Parameter " + parm + " seems vulnerable to sql injection with value " + val);
                    }
                }
                
            }

        }
    }
}
```

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch2/Post_sql_fuzzer/post_sql_fuzzer.png)


