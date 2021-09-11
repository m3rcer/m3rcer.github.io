using System;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Automating_Nexpose
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            using (NexposeSession session = new NexposeSession("jin", "fuckmydick123", "127.0.0.1"))
            {
                using (NexposeManager manager = new NexposeManager(session))
                {
                    string[][] ips =
                    {
                        new string[] {"192.168.0.1", string.Empty }
                    };

                    XDocument site = manager.CreateOrUpdateSite(Guid.NewGuid().ToString(), null, ips);

                    // Storing siteID
                    int siteID = int.Parse(site.Root.Attribute("site-id").Value);

                    // Starting the Scan
                    XDocument scan = manager.ScanSite(siteID);
                    XElement ele = scan.XPathSelectElement("//SiteScanResponse/Scan");

                    int scanID = int.Parse(ele.Attribute("scan-id").Value);
                    XDocument status = manager.GetScanStatus(scanID);

                    while (status.Root.Attribute("status").Value != "finished")
                    {
                        Thread.Sleep(1000);
                        status = manager.GetScanStatus(scanID);
                        Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + status.ToString());
                    }

                    // Generate report and delete site
                    byte[] report = manager.GetPdfSiteReport(siteID);
                    string outdir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string outpath = Path.Combine(outdir, siteID + ".pdf");
                    File.WriteAllBytes(outpath, report);

                    manager.DeleteSite(siteID);
                }
            }
        }
    }
}
