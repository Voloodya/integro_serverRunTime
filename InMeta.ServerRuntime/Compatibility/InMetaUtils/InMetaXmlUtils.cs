// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaUtils.InMetaXmlUtils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Compatibility.InMetaUtils
{
  [ComVisible(true)]
  public class InMetaXmlUtils
  {
    public static readonly InMetaXmlUtils Instance = new InMetaXmlUtils();

    public MsXmlDocumentEmulator CreateDoc(string rootElementName = "")
    {
      XmlDocument document = new XmlDocument();
      if (!string.IsNullOrEmpty(rootElementName))
        document.AppendChild((XmlNode) document.CreateElement(rootElementName));
      return new MsXmlDocumentEmulator(document);
    }

    public string VersionPI => "<?xml version=\"1.0\"?>";

    public string DefaultXmlPI => "<?xml version=\"1.0\" encoding=\"windows-1251\"?>";

    public void AddDefaultPI(object doc) => this.AddPI(doc, "version=\"1.0\" encoding=\"windows-1251\"");

    public void AddPI(object doc, string data)
    {
      XmlDocument xmlDocument = InMetaXmlUtils.TryGetXmlDocument(doc);
      if (xmlDocument != null)
      {
        XmlNode firstChild = xmlDocument.FirstChild;
        if (firstChild != null && (firstChild.NodeType == XmlNodeType.ProcessingInstruction && firstChild.Name == "xml"))
          xmlDocument.RemoveChild(firstChild);
        XmlProcessingInstruction processingInstruction = xmlDocument.CreateProcessingInstruction("xml", data);
        xmlDocument.InsertBefore((XmlNode) processingInstruction, xmlDocument.FirstChild);
      }
      else
      {
        object firstChild = InMetaXmlUtils.InteropGetFirstChild(doc);
        if (firstChild != null && (InMetaXmlUtils.InteropGetNodeType(firstChild) == 7 && InMetaXmlUtils.InteropGetNodeName(firstChild) == "xml"))
          InMetaXmlUtils.InteropRemoveChild(doc, firstChild);
        object processingInstruction = InMetaXmlUtils.InteropCreateProcessingInstruction(doc, "xml", data);
        InMetaXmlUtils.InteropInsertBefore(doc, processingInstruction, InMetaXmlUtils.InteropGetFirstChild(doc));
      }
    }

    private static XmlDocument TryGetXmlDocument(object doc)
    {
      switch (doc)
      {
        case MsXmlDocumentEmulator documentEmulator:
          return documentEmulator.FDocument;
        case XmlDocument xmlDocument:
          return xmlDocument;
        default:
          return (XmlDocument) null;
      }
    }

    public object LoadFile(string url)
    {
      XmlDocument document = new XmlDocument();
      document.Load(url);
      return (object) new MsXmlDocumentEmulator(document);
    }

    public bool HasAttr(object element, string attributeName)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(element);
      return xmlNode != null ? ((XmlElement) xmlNode).HasAttribute(attributeName) : InMetaXmlUtils.InteropGetAttribute(element, attributeName) != null;
    }

    public string GetAttr(object element, string attributeName, string attributeDefaultValue = "")
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(element);
      return xmlNode != null ? XmlUtils.GetAttr(xmlNode, attributeName, attributeDefaultValue) : InMetaXmlUtils.InteropGetAttribute(element, attributeName) ?? attributeDefaultValue;
    }

    public string NeedAttr(object element, string attributeName)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(element);
      if (xmlNode != null)
        return ((XmlElement) xmlNode).GetAttribute(attributeName);
      return InMetaXmlUtils.InteropGetAttribute(element, attributeName) ?? throw new Exception("Элемент не содержит обязательный атрибут \"" + attributeName + "\".");
    }

    public string NeedId(object element) => this.NeedAttr(element, "id");

    public void SetAttr(object element, string attributeName, string attributeValue)
    {
      ((XmlElement) InMetaXmlUtils.TryGetXmlNode(element))?.SetAttribute(attributeName, attributeValue);
      InMetaXmlUtils.InteropSetAttribute(element, attributeName, (object) attributeValue);
    }

    public string GetNodeText(object element)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(element);
      if (xmlNode != null)
        return XmlUtils.GetOwnText(xmlNode);
      object nodes = InMetaXmlUtils.InteropSelectNodes(element, "text()");
      StringBuilder stringBuilder = new StringBuilder();
      int length = InMetaXmlUtils.InteropGetLength(nodes);
      for (int index = 0; index < length; ++index)
      {
        object node = InMetaXmlUtils.InteropGetItem(nodes, index);
        stringBuilder.Append(InMetaXmlUtils.InteropGetNodeValue(node));
      }
      stringBuilder.Replace("\r\n", "\r");
      stringBuilder.Replace('\n', '\r');
      stringBuilder.Replace("\r", "\r\n");
      return stringBuilder.ToString();
    }

    public void SetNodeText(object element, string text, bool textAsCDataSection = false)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(element);
      if (xmlNode != null)
      {
        XmlUtils.ReplaceOwnText(xmlNode, text);
      }
      else
      {
        object nodes = InMetaXmlUtils.InteropSelectNodes(element, "text()");
        int length = InMetaXmlUtils.InteropGetLength(nodes);
        for (int index = 0; index < length; ++index)
          InMetaXmlUtils.InteropRemoveChild(element, InMetaXmlUtils.InteropGetItem(nodes, index));
        if (string.IsNullOrEmpty(text))
          return;
        InMetaXmlUtils.InteropAppendChild(element, InMetaXmlUtils.InteropCreateTextNode(InMetaXmlUtils.InteropGetOwnerDocument(element), text));
      }
    }

    public object GetSubNode(object parentElement, string childElementName)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(parentElement);
      if (xmlNode != null)
        return (object) new MsXmlNodeEmulator(xmlNode.SelectSingleNode(childElementName) ?? throw new Exception("Не найден обязательный дочерний элемент \"" + childElementName + "\"."));
      return InMetaXmlUtils.InteropSelectSingleNode(parentElement, childElementName) ?? throw new Exception("Не найден обязательный дочерний элемент \"" + childElementName + "\".");
    }

    public string GetSubNodeText(object parentElement, string childElementName) => this.GetNodeText(this.GetSubNode(parentElement, childElementName));

    public string GetChildText(object parentElement, string childElementName)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(parentElement);
      if (xmlNode != null)
      {
        XmlNode node = xmlNode.SelectSingleNode(childElementName);
        return node != null ? XmlUtils.GetOwnText(node) : (string) null;
      }
      object element = InMetaXmlUtils.InteropSelectSingleNode(parentElement, childElementName);
      return element != null ? this.GetNodeText(element) : (string) null;
    }

    public object AddElement(
      object parentElement,
      string childElementName,
      string childElementText = "",
      bool childElementTextAsCDataSection = false)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(parentElement);
      if (xmlNode != null)
        return (object) new MsXmlNodeEmulator((XmlNode) XmlUtils.AppendElement(xmlNode, childElementName, childElementText));
      object ownerDocument = InMetaXmlUtils.InteropGetOwnerDocument(parentElement);
      object element = InMetaXmlUtils.InteropCreateElement(ownerDocument, childElementName);
      InMetaXmlUtils.InteropAppendChild(parentElement, element);
      if (!string.IsNullOrEmpty(childElementText))
        InMetaXmlUtils.InteropAppendChild(element, InMetaXmlUtils.InteropCreateTextNode(ownerDocument, childElementText));
      return element;
    }

    public string DecodeText(string text) => text.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");

    public string EncodeText(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");

    public string EncodeXSLPatternString(string pattern) => pattern.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'");

    public object CallWebService(string url, string nameSpace, string action, object request) => throw new NotImplementedException();

    public object LoadHttp(string url, object request) => throw new NotImplementedException();

    public object SendHttpRequest(string url, object request) => throw new NotImplementedException();

    private static XmlNode TryGetXmlNode(object node)
    {
      if (node is MsXmlNodeEmulator msXmlNodeEmulator)
        return msXmlNodeEmulator.DotNetXmlNode;
      XmlNode xmlNode = node as XmlNode;
      return node != null ? xmlNode : (XmlNode) null;
    }

    private static object InteropCreateElement(object document, string name) => InteropUtility.Invoke(document, "createElement", (object) name);

    private static object InteropCreateTextNode(object document, string text) => InteropUtility.Invoke(document, "createTextNode", (object) text);

    private static object InteropSelectNodes(object node, string pattern) => InteropUtility.Invoke(node, "selectNodes", (object) pattern);

    private static object InteropSelectSingleNode(object node, string pattern) => InteropUtility.Invoke(node, "selectSingleNode", (object) pattern);

    private static string InteropGetAttribute(object node, string name)
    {
      object obj = InteropUtility.Invoke(node, "getAttribute", (object) name);
      return obj == null || obj == DBNull.Value ? (string) null : obj.ToString();
    }

    private static void InteropSetAttribute(object node, string name, object value) => InteropUtility.Invoke(node, "setAttribute", (object) name, value);

    private static string InteropGetNodeName(object node) => (string) InteropUtility.PropertyGet(node, "nodeName");

    private static object InteropGetNodeValue(object node) => InteropUtility.PropertyGet(node, "nodeValue");

    private static object InteropGetOwnerDocument(object node) => InteropUtility.PropertyGet(node, "ownerDocument");

    private static object InteropCreateProcessingInstruction(object doc, string name, string data) => InteropUtility.Invoke(doc, "createProcessingInstruction", (object) name, (object) data);

    private static int InteropGetNodeType(object node) => (int) InteropUtility.PropertyGet(node, "nodeType");

    private static object InteropGetFirstChild(object node) => InteropUtility.PropertyGet(node, "firstChild");

    private static void InteropInsertBefore(object parent, object newChild, object refNode) => InteropUtility.Invoke(parent, "insertBefore", newChild, refNode);

    private static void InteropAppendChild(object parent, object child) => InteropUtility.Invoke(parent, "appendChild", child);

    private static void InteropRemoveChild(object parent, object child) => InteropUtility.Invoke(parent, "removeChild", child);

    private static int InteropGetLength(object nodes) => (int) InteropUtility.PropertyGet(nodes, "length");

    private static object InteropGetItem(object nodes, int index) => InteropUtility.Invoke(nodes, "item", (object) index);

    internal static object SelectSingleNode(object node, string pattern)
    {
      XmlNode xmlNode = InMetaXmlUtils.TryGetXmlNode(node);
      if (xmlNode == null)
        return InMetaXmlUtils.InteropSelectSingleNode(node, pattern);
      XmlNode node1 = xmlNode.SelectSingleNode(pattern);
      return node1 != null ? (object) new MsXmlNodeEmulator(node1) : (object) (MsXmlNodeEmulator) null;
    }
  }
}
