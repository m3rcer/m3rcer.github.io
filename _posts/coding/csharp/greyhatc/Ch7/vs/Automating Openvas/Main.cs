using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net.Security;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;


namespace Automating_Openvas
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            using (OpenVASSession session = new OpenVASSession("admin", "admin", "192.168.0.128"))
            {
                using (OpenVASManager manager = new OpenVASManager(session))
                {
                    // Get version
                    XDocument version = manager.GetVersion();
                    Console.WriteLine(version);

                    // Create scan
                    XDocument target = manager.CreateSimpleTarget("192.168.0.1", Guid.NewGuid().ToString());
                    string targetID = target.Root.Attribute("id").Value;
                    XDocument configs = manager.GetScanConfigurations();
                    string disoveryConfigID = string.Empty;

                    foreach (XElement node in configs.Descendants("name"))
                    {
                        if (node.Value == "Discovery")
                        {
                            disoveryConfigID = node.Parent.Attribute("id").Value;
                            break;
                        }
                    }
                    Console.WriteLine("[+] Creating scan of target " + targetID + " with scan config " + disoveryConfigID);

                    // Adding Start and Create task
                    XDocument task = manager.CreateSimpleTask(Guid.NewGuid().ToString(), string.Empty, new Guid(disoveryConfigID), new Guid(targetID));

                    Guid taskID = new Guid(task.Root.Attribute("id").Value);

                    manager.StartTask(taskID);

                    // Wrapping up the automation
                    XDocument status = manager.GetTasks(taskID);
                    while (status.Descendants("status").First().Value != "Done")
{
                        Thread.Sleep(5000);
                        Console.Clear();
                        string percentComplete = status.Descendants("progress").First().Nodes()
                        .OfType<XText>().First().Value;
                        Console.WriteLine("The scan is " + percentComplete + "% done.");
                        status = manager.GetTasks(taskID);
                    }
                    XDocument results = manager.GetTaskResults(taskID);
                    Console.WriteLine(results.ToString());
                }
            }
        }
    }
}
