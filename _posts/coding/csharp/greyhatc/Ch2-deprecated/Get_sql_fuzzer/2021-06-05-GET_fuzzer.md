---
title: Creating a mutational fuzzer to fuzz GET requests for possible sqli errors.
date: 2021-06-05 14:45:47 +07:00
categories: greyhatcch2
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a mutational fuzzer to fuzz GET requests for possible sqli errors.
---

## This Program is a basic mutational fuzzer that will fuzz GET requests to generate errors to validate a possible sql/xss vulnerability.

- A fuzzer attempts to find errors in software by sending malformed data. There are 2 general types:
  > Mutational Fuzzer - taint data in a known-good with bad without regard for the protocol or structure of data.
  
  > Generational Fuzzer -  taint data in a known-good with bad taking regard for the nuances of the protocol or structure of data.
_Note: We use BadStore from VulnHub for this Chapter to test our tools against._
- We use the static `Create() method` from the `WebRequest class` to make a `http` type object by passing in the url which is casted back to the `HttpWebRequest` object .
  The method is instantiated to be set to "GET".
- Instantiated objects can be used in the context of a using block to implement a `Dispose() method` at the end of scope. This helps manage scope and prevent resource leaks of objects.
- The `StreamReader class` is used to save the response and the `ReadToEnd() method` to read data till EOL.

### Code Block: 

```csharp
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Web;

namespace fuzz_world
{
    class MainClass
    {
        // Parse all parms from the url
        public static void Main(string[] args)
        {
            string url = args[0];
            int index = url.IndexOf("?");
            string[] parms = url.Remove(0, index + 1).Split('&');
            foreach (string parm in parms)
                Console.WriteLine("\r\n* Parameters found are : " + parm + "\r\n");
               
            // Replacing url with tainted params
            foreach (string parm in parms)
            {
                string xssUrl = url.Replace(parm, parm + "fd<xss>sa");
                string sqlUrl = url.Replace(parm, parm + "fd'sa");
                Console.WriteLine("\r\n* Modding url With tainted parameters for : " + parm);
                Console.WriteLine("XSS tainted url: " + xssUrl);
                Console.WriteLine("Sql tainted url: " + sqlUrl + "\r\n");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sqlUrl);
                request.Method = "GET";

                string sqlresp = string.Empty;
                using (StreamReader rdr = new StreamReader(request.GetResponse().GetResponseStream()))
                    sqlresp = rdr.ReadToEnd();
                    
                // Creating GET Request
                request = (HttpWebRequest)WebRequest.Create(xssUrl);
                request.Method = "GET";

                string xssresp = string.Empty;
                using (StreamReader rdr = new StreamReader(request.GetResponse().GetResponseStream()))
                    xssresp = rdr.ReadToEnd();

                if (xssresp.Contains("<xss>"))
                    Console.WriteLine("[+] Possible XSS point found in parameter --> \r\n" + parm );

                if (sqlresp.Contains("error in your SQL syntax"))
                    Console.WriteLine("[+] Sql injection point found in parameter --> \r\n" + parm);

            }

        }
    }
}
```

### Output:


![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch2/Get_sql_fuzzer/get_sql_fuzzer.png) 
