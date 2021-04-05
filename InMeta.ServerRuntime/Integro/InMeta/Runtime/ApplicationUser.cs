// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ApplicationUser
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class ApplicationUser
  {
    public readonly XmlNode SourceNode;
    public readonly string Account;
    public readonly string Caption;
    public readonly string ContextName;

    internal ApplicationUser(XmlNode sourceNode)
    {
      this.SourceNode = sourceNode;
      this.Account = XmlUtils.GetAttr(sourceNode, "account");
      this.Caption = XmlUtils.GetAttr(sourceNode, "caption");
      if (this.Caption == string.Empty)
        this.Caption = this.Account;
      this.ContextName = XmlUtils.GetAttr(sourceNode, "context");
    }
  }
}
