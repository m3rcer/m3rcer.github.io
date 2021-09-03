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
    public class SoapType
    {
        public SoapType(XmlNode type)
        {
            // Retrive the passed nodes "name" attribute
            this.Name = type.Attributes["name"].Value;
            this.Parameters = new List<SoapTypeParameter>();
            //Checking for child/sub-child nodes 
            if (type.HasChildNodes && type.FirstChild.HasChildNodes)
            {
                foreach (XmlNode node in type.FirstChild.FirstChild.ChildNodes)
                    this.Parameters.Add(new SoapTypeParameter(node));
            }
        }
        public string Name { get; set; }
        public List<SoapTypeParameter> Parameters { get; set; }
    }

    public class SoapTypeParameter
    {
        public SoapTypeParameter(XmlNode node)
        {
            if (node.Attributes["maxOccurs"].Value == "Unbounded")
                this.MaximumOccurence = int.MaxValue;
            else
                this.MaximumOccurence = int.Parse(node.Attributes["maxOccurs"].Value);

            this.MinimumOccurence = int.Parse(node.Attributes["minOccurs"].Value);
            this.Name = node.Attributes["name"].Value;
            this.Type = node.Attributes["type"].Value;
        }

        public int MinimumOccurence { get; set; }
        public int MaximumOccurence { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
