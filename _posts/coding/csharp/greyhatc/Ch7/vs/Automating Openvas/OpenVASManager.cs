using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;

namespace Automating_Openvas
{
    public class OpenVASManager : IDisposable
    {
        private OpenVASSession _sesssion;

        // Constructor returning session
        public OpenVASManager(OpenVASSession session)
        {
            if (session != null)
                _sesssion = session;
            else
                throw new ArgumentNullException("session");
        }

        public XDocument GetVersion()
        {
            return _sesssion.ExecuteCommand(XDocument.Parse("<get_version />"));
        }

        private void Dispose()
        {
            _sesssion.Dispose();
        }

        // Getting Scan configs
        public XDocument GetScanConfigurations()
        {
            return _sesssion.ExecuteCommand(XDocument.Parse("<get_configs />"));
        }


        // Create Scan target
        public XDocument CreateSimpleTarget(string cidrRange, string targetName)
        {
            XDocument createTargetXML = new XDocument(
                new XElement("create_target",
                    new XElement("name", targetName),
                    new XElement("hosts", cidrRange)));

            return _sesssion.ExecuteCommand(createTargetXML);
        }


        // Creating and starting Tasks
        public XDocument CreateSimpleTask(string name, string comment, Guid configID, Guid targetID)
        {
            XDocument createTaskXML = new XDocument(
                new XElement("create_task",
                    new XElement("name", name),
                    new XElement("comment", comment),
                    new XElement("config",
                        new XAttribute("id", configID.ToString())),
                        new XElement("target",
                            new XAttribute("id", targetID.ToString()))));

            return _sesssion.ExecuteCommand(createTaskXML);
        }

        // Watching Scan and getting results
        public XDocument StartTask(Guid taskID)
        {
            XDocument startTaskXML = new XDocument(
                new XElement("start_task",
                    new XAttribute("task_id", taskID.ToString())));

            return _sesssion.ExecuteCommand(startTaskXML);
        }


        public XDocument GetTasks(Guid? taskID = null)
        {
            if (taskID != null)
                return _sesssion.ExecuteCommand(new XDocument(
                new XElement("get_tasks",
                new XAttribute("task_id", taskID.ToString()))));
            return _sesssion.ExecuteCommand(XDocument.Parse("<get_tasks />"));
        }
        public XDocument GetTaskResults(Guid taskID)
        {

            XDocument getTaskResultsXML = new XDocument(
            new XElement("get_results",
            new XAttribute("task_id", taskID.ToString())));
            return _sesssion.ExecuteCommand(getTaskResultsXML);
        }

    }
}
