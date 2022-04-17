---
title: Writing a basic hello world program in C#.
date: 2021-05-29 10:45:47 +07:00
categories: greyhatcch1
#modified: 20-08-29 09:24:47 +07:00
#tags: [blog, netlify, jekyll, github]
description: Writing a basic hello world program in C#.
---



## This is a basic program printing hello world and the time using core built-in classes.

- The System namespace allows us to access libraries in a program.
- Namespaces declare where our classes live in.
- DateTime is a core C# class for dealing with dates.
- Compile the source code on linux using the Mono compiler (mcs).


### Code:

```csharp
using System;

namespace hello_world
{
        class MainClass
        {
                public static void Main(string[] args)
                {
                        string hello = "Hello World!"; //simple string definition
                        DateTime now = DateTime.Now; //Build In core class usage
                        Console.Write(hello); //Single line w no newline at end
                        Console.WriteLine("The date is " + now.ToLongDateString()); //Adds newline to the end
                }
        }
}

```

### Output:

![Image](https://raw.githubusercontent.com/m3rcer/m3rcer.github.io/master/_posts/coding/csharp/greyhatc/IntroScripts/hello/hello.png)
