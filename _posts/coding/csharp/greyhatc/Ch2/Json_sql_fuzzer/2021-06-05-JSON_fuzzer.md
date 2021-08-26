---
title: Creating a mutational fuzzer to fuzz JSON requests for possible sqli errors.
date: 2021-06-05 16:45:47 +07:00
categories: greyhatcch2
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a mutational fuzzer to fuzz JSON requests for possible sqli errors.
---

## This Program is a basic mutational fuzzer that will now fuzz JSON requests to generate errors to validate a possible sql vulnerability.

- We open our request file using the File.OpenRead() method and pass the file stream returned to the StreamReader constructor which after being instantiated reads all data using the ReadToEnd() method .

- We create a JObject to programatically iterate and parse the json using the Json.Net library.

- We use the DeepClone() method that will get a seperate object to be operated on that is identical to the first as we can't alter the objects we iterate over.

- Weakly developed and a majority of developed apps don't care if the value type changes. We use this functionality to convert integers to strings and fuzz them after as we would a normal string var.

- We build our http request using the HttpWebRequest method as before and send it using the Stream.Write() method. 

- We then use the try/catch block to catch any exceptions and match them to return true if so validating the sql error.


### Code:

```Csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSON_Fuzzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = args[0];
            string requestFile = args[1];
            string[] request = null;
            
            using (StreamReader rdr = new StreamReader(File.OpenRead(requestFile)))
                request = rdr.ReadToEnd().Split('\n');
            
            // Last element of request array
            string json = request[request.Length - 1];
            JObject obj = JObject.Parse(json);

            Console.WriteLine("[+] Fuzzing POST requests to URL : " + url);
            IterateAndFuzz(url, obj);            
        }

        private static void IterateAndFuzz(string url, JObject obj)
        {
            foreach (var pair in (JObject)obj.DeepClone())
            {
                // Checking for str, int type 
                if (pair.Value.Type == JTokenType.String || pair.Value.Type == JTokenType.Integer)
                {
                    Console.WriteLine("\n\rFuzzing Key : " + pair.Key);

                    if (pair.Value.Type == JTokenType.Integer)
                        Console.WriteLine("Converting Int type to a String in order to fuzz.");

                    JToken oldVal = pair.Value;
                    obj[pair.Key] = pair.Value.ToString() + "'";

                    if (Fuzz(url, obj.Root))
                        Console.WriteLine("--> SQL Injection vector found at: " + pair.Key);
                    else
                        Console.WriteLine(pair.Key + " does not seem vulnerable to this SQL injection vector.");

                    obj[pair.Key] = oldVal;
                }
            }                 
        }

        private static bool Fuzz(string url, JToken obj)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(obj.ToString());

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentLength = data.Length;
            req.ContentType = "application/javascript";

            using (Stream stream = req.GetRequestStream())
                stream.Write(data, 0, data.Length);

            try
            {
                req.GetResponse();
            }
            catch (WebException e)
            {
                string resp = string.Empty;
                using (StreamReader r = new StreamReader(e.Response.GetResponseStream()))
                    resp = r.ReadToEnd();

                return (resp.Contains("syntax error") || resp.Contains("unterminated"));
            }

            return false;
        }
    }
}
```

## Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch2/Json_sql_fuzzer/json_sql_fuzer.PNG)
