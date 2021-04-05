// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MsXmlDocumentEmulator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class MsXmlDocumentEmulator
  {
    internal readonly XmlDocument FDocument;

    public MsXmlDocumentEmulator(XmlDocument document) => this.FDocument = document;

    public string xml => this.FDocument.OuterXml;

    public bool async
    {
      get => false;
      set
      {
      }
    }

    public void Load(string url) => this.FDocument.Load(url);

    public MsXmlNodeEmulator documentElement => new MsXmlNodeEmulator((XmlNode) this.FDocument.DocumentElement, this);

    public MsXmlNodeEmulator createElement(string name) => new MsXmlNodeEmulator((XmlNode) this.FDocument.CreateElement(name), this);

    public MsXmlNodeEmulator createCDATASection(string data) => new MsXmlNodeEmulator((XmlNode) this.FDocument.CreateCDataSection(data), this);
  }
}
