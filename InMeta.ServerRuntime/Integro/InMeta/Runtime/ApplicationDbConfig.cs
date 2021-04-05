// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ApplicationDbConfig
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class ApplicationDbConfig
  {
    public readonly XmlNode SourceNode;
    public readonly string DriverName;
    public readonly Hashtable Parameters;

    internal ApplicationDbConfig(XmlNode sourceNode)
    {
      this.SourceNode = sourceNode;
      this.DriverName = XmlUtils.GetAttr(sourceNode, "name");
      this.Parameters = new Hashtable();
      XmlNodeList xmlNodeList = sourceNode.SelectNodes("param");
      for (int i = 0; i < xmlNodeList.Count; ++i)
      {
        XmlElement xmlElement = (XmlElement) xmlNodeList[i];
        string str = XmlUtils.NeedAttr((XmlNode) xmlElement, "name");
        string encrypted = xmlElement.InnerText;
        if (str.ToLower() == "login-password")
          encrypted = Utility.Decrypt(encrypted, xmlElement.GetAttribute("encryption"));
        this.Parameters[(object) str] = (object) encrypted;
      }
    }
  }
}
