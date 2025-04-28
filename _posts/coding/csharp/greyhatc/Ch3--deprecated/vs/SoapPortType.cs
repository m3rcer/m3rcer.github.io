using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace WSDL_Fuzzer
{
    public class SoapPortType
    {
        public SoapPortType(XmlNode node)
        {
            this.Name = node.Attributes["name"].Value;
            this.Operations = new List<SoapOperation>();
            foreach (XmlNode op in node.ChildNodes)
                this.Operations.Add(new SoapOperation(op));
        }
        public string Name { get; set; }

        public List<SoapOperation> Operations { get; set; }
    }

    public class SoapOperation
    {
        public SoapOperation(XmlNode op)
        {
            this.Name = op.Attributes["name"].Value;
            foreach (XmlNode message in op.ChildNodes)
            {
                if (message.Name.EndsWith("input"))
                    this.Input = message.Attributes["message"].Value;
                else if (message.Name.EndsWith("output"))
                    this.Output = message.Attributes["message"].Value;
            }
        }
        public string Name { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
    }

}
