## We use the NexposeManager Class to wrap common API calls and functionality for Nexpose to create and manage various Nexpose scan sites.

* The NexposeManager class implements IDisposable so that we can use NexposeSession to interact with the Nexpose API and log out automatically if necessary.

* The GetSystemInformation() method makes a basic SystemInformation API request to print the Nexpose server information.

* The CreateOrUpdateSite() method helps create/save a scan site with assets.

* The ScanSite() method starts a scan.

* The GetScanStatus() method helps retrieve the scan status.

* The GetPdfSiteReport() method creates a PDF site report.

* The DeleteSite() method deletes the created target scan site.


### CODE:

```csharp
using System;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace Automating_Nexpose
{
    public class NexposeManager : IDisposable
    {
        private readonly NexposeSession _session;
        public NexposeManager(NexposeSession session)
        {
            if (!session.IsAuthenticated)
                throw new ArgumentException("[!] Trying to create manager from "
                    + "unauthenticated session. PLease authenticate.", "session");

            _session = session;
        }

        // Makes a basic SystemInformation API request to print the Nexpose server information
        public XDocument GetSystemInformation()
        {
            XDocument xml = new XDocument(
                new XElement("SystemInformationRequest", 
                    new XAttribute("session-id", _session.SessionID)));

            return (XDocument)_session.ExecuteCommand(xml);
        }

        public void Dispose()
        {
            _session.Logout();
        }



        /* Creating a Nexpose site, scan the site and then download a report of the findings */
        // name: sitename, ips: ip ranges, SiteID: -1 to create new site


        // Create/Saving a scan site with assets
        public XDocument CreateOrUpdateSite(string name, string[] hostnames = null, string[][] ips = null, int SiteID = -1)
        {
            XElement hosts = new XElement("Hosts");
            if (hostnames != null)
            {
                foreach (string host in hostnames)
                    hosts.Add(new XElement("host", host));
            }

            if (ips != null)
            {
                foreach (string[] range in ips)
                {
                    hosts.Add(new XElement("range",
                        new XAttribute("from", range[0]),
                        new XAttribute("to", range[1])));
                }
            }

            XDocument xml = new XDocument(
                new XElement("SiteSaveRequest",
                    new XAttribute("session-id", _session.SessionID),
                    new XElement("Site",
                        new XAttribute("id", SiteID),
                        new XAttribute("name", name),
                        hosts,
                        new XElement("ScanConfig",
                            new XAttribute("name", "Full-audit"),
                            new XAttribute("templateID", "full-audit")))));

            return (XDocument)_session.ExecuteCommand(xml);
        }

        // Starting a Scan
        public XDocument ScanSite(int siteID)
        {
            XDocument xml = new XDocument(
                new XElement("SiteScanRequest",
                    new XAttribute("session-id", _session.SessionID),
                    new XAttribute("site-id", siteID)));
            return (XDocument)_session.ExecuteCommand(xml);
        }


        // Getting scan status
        public XDocument GetScanStatus(int scanID)
        {
            XDocument xml = new XDocument(
                new XElement("ScanStatusRequest",
                    new XAttribute("session-id", _session.SessionID),
                    new XAttribute("scan-id", scanID)));

            return (XDocument)_session.ExecuteCommand(xml);
        }

        /* Create a PDF site Report */
        public byte[] GetPdfSiteReport(int siteID)
        {
            XDocument doc = new XDocument(
                new XElement("ReportAdhocGenerateRequest",
                    new XAttribute("session-id", _session.SessionID),
                    new XElement("AdhocReportConfig",
                        new XAttribute("template-id", "audit-report"),
                        new XAttribute("format", "pdf"),
                        new XElement("Filters",
                            new XAttribute("type", "site"),
                            new XAttribute("id", siteID)))));

            return ((byte[])_session.ExecuteCommand(doc));
        }

        // deleting the Site
        public XDocument DeleteSite(int siteID)
        {
            XDocument xml = new XDocument(
                new XElement("SiteDeleteRequest",
                    new XAttribute("session-id", _session.SessionID),
                    new XAttribute("site-id", siteID)));

            return (XDocument)_session.ExecuteCommand(xml);
        }
    }
}
```