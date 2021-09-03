---
title: Parsing subclasses
permalink: /permalinks/SOAP_fuzzer/parse.md
date: 2021-06-29 11:45:47 +07:00
categories: greyhatcch3
---



## These defined subclasses parse the WSDL defintions and return back the parsed values to the WSDL class and its respective definitions.

- We use the Attributes property on the nodes passed into the constructor to retrieve the node’s specific attributes.

- We use the FirstChild property and the ChildNodes property to determine and enumerate the list of child nodes available.

### 1. SoapType Parser class:

_Example of SoapType definition:_

```
<xs:element name="AddUser">
  <xs:complexType>
      <xs:sequence>
            <xs:element minOccurs="0" maxOccurs="1" name="username" type="xs:string"/>
            <xs:element minOccurs="0" maxOccurs="1" name="password" type="xs:string"/>
      </xs:sequence>
</xs:complexType></xs:element>
```

### CODE:

```csharp
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
```


### 2. SoapMessage Parser class:

_Example of SoapType definition:_

```
<message name="AddUserHttpGetIn">
  <part name="username" type="s:string"/>
  <part name="password" type="s:string"/>
</message>
```

### CODE:

```csharp
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
    // Defines set of data that server expects/responds to using "parts"
    public class SoapMessage
    {
        public SoapMessage(XmlNode node)
        {
            this.Name = node.Attributes["name"].Value;
            this.Parts = new List<SoapMessagePart>();

            if (node.HasChildNodes)
            {
                foreach (XmlNode part in node.ChildNodes)
                    this.Parts.Add(new SoapMessagePart(part));
            }
        }

        public string Name { get; set; }

        public List<SoapMessagePart> Parts { get; set; }
    }




    public class SoapMessagePart
    {
        public SoapMessagePart(XmlNode part)
        {
            this.Name = part.Attributes["name"].Value;

            if (part.Attributes["element"] != null)
                this.Element = part.Attributes["element"].Value;
            else if (part.Attributes["type"] != null)
                this.Type = part.Attributes["type"].Value;
            else
                throw new ArgumentException("Neither element nor type attribute exist", nameof(part));
        }

        public string Name { get; set; }
        public string Element { get; set; }
        public string Type { get; set; }
    }
}
```

### 3. SoapPortType Parser class:

_Example of SoapPortType definition:_

```
<portType name="VulnerableServiceSoap">
  <operation name="AddUser">
      <input message="s0:AddUserSoapIn"/>
      <output message="s0:AddUserSoapOut"/>
  </operation>
  <operation name="ListUsers">
      <input message="s0:ListUsersSoapIn"/>
      <output message="s0:ListUsersSoapOut"/>
  </operation>
    <operation name="GetUser">
        <input message="s0:GetUserSoapIn"/>
        <output message="s0:GetUserSoapOut"/>
    </operation>
      <operation name="DeleteUser">
          <input message="s0:DeleteUserSoapIn"/>
          <output message="s0:DeleteUserSoapOut"/>
      </operation></portType>
```

### CODE:

```csharp
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
```

### 4. SoapBinding Parser class:

_Example of SoapBinding definition:_

```
<binding name="VulnerableServiceSoap" type="s0:VulnerableServiceSoap">
  <soap:binding transport="http://schemas.xmlsoap.org/soap/http"/>
  <operation name="AddUser">
      <soap:operation soapAction="http://tempuri.org/AddUser" style="document"/>
      <input>
            <soap:body use="literal"/>
      </input>
      <output>
            <soap:body use="literal"/>
      </output>
   </operation>
</binding>
```

### CODE: 

```csharp
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
	public class SoapBinding
	{
		public SoapBinding(XmlNode node)
		{
			this.Name = node.Attributes["name"].Value;
			this.Type = node.Attributes["type"].Value;
			this.IsHTTP = false;
			this.Operations = new List<SoapBindingOperation>();
			foreach (XmlNode op in node.ChildNodes)
			{
				if (op.Name.EndsWith("operation"))
				{
					this.Operations.Add(new SoapBindingOperation(op));
				}
				else if (op.Name == "http:binding")
				{
					this.Verb = op.Attributes["verb"].Value;
					this.IsHTTP = true;
				}
			}
		}

		public string Name { get; set; }
		public List<SoapBindingOperation> Operations { get; set; }
		public bool IsHTTP { get; set; }
		public string Verb { get; set; }
		public string Type { get; set; }
	}

	public class SoapBindingOperation
	{
		public SoapBindingOperation(XmlNode op)
		{
			this.Name = op.Attributes["name"].Value;

			foreach (XmlNode node in op.ChildNodes)
			{
				if (node.Name == "http:operation")
					this.Location = node.Attributes["location"].Value;
				if (node.Name == "soap:operation" || node.Name == "soap12:operation")
					this.SoapAction = node.Attributes["soapAction"].Value;
			}
		}

		public string Name { get; set; }
		public string Location { get; set; }
		public string SoapAction { get; set; }
	}
}
```
