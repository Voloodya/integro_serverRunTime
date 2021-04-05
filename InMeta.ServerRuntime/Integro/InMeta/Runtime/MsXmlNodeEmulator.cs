// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MsXmlNodeEmulator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class MsXmlNodeEmulator
  {
    private readonly MsXmlDocumentEmulator FDocument;
    internal readonly XmlNode DotNetXmlNode;

    public MsXmlNodeEmulator(XmlNode node, MsXmlDocumentEmulator document)
    {
      this.DotNetXmlNode = node;
      this.FDocument = document;
    }

    public MsXmlNodeEmulator(XmlNode node)
    {
      this.DotNetXmlNode = node;
      this.FDocument = new MsXmlDocumentEmulator(node.OwnerDocument);
    }

    private static MsXmlNodeEmulator Wrap(object node)
    {
      switch (node)
      {
        case MsXmlNodeEmulator msXmlNodeEmulator:
          return msXmlNodeEmulator;
        case XmlNode node1:
          return new MsXmlNodeEmulator(node1);
        default:
          throw new Exception("Ошибка доступа к элементу Xml: ожидается XmlNode или MsXmlNodeEmulator.");
      }
    }

    public MsXmlDocumentEmulator OwnerDocument => this.FDocument;

    public object ParentNode => (object) new MsXmlNodeEmulator(this.DotNetXmlNode.ParentNode, this.FDocument);

    private XmlElement Element => (XmlElement) this.DotNetXmlNode;

    public object AppendChild(object nodeObject)
    {
      MsXmlNodeEmulator msXmlNodeEmulator = MsXmlNodeEmulator.Wrap(nodeObject);
      this.DotNetXmlNode.AppendChild(msXmlNodeEmulator.DotNetXmlNode);
      return (object) msXmlNodeEmulator;
    }

    public object RemoveChild(object nodeObject)
    {
      MsXmlNodeEmulator msXmlNodeEmulator = MsXmlNodeEmulator.Wrap(nodeObject);
      this.DotNetXmlNode.RemoveChild(msXmlNodeEmulator.DotNetXmlNode);
      return (object) msXmlNodeEmulator;
    }

    public string GetAttribute(string name) => this.Element.GetAttribute(name);

    public void SetAttribute(string name, string value) => this.Element.SetAttribute(name, value);

    public object SelectNodes(string query) => (object) new MsXmlNodeListEmulator(this.DotNetXmlNode.SelectNodes(query), this.FDocument);

    public object SelectSingleNode(string query)
    {
      XmlNode node = this.DotNetXmlNode.SelectSingleNode(query);
      return node != null ? (object) new MsXmlNodeEmulator(node, this.FDocument) : (object) (MsXmlNodeEmulator) null;
    }
  }
}
