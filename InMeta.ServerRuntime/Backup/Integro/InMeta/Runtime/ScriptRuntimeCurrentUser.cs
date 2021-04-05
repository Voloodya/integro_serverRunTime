// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ScriptRuntimeCurrentUser
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class ScriptRuntimeCurrentUser
  {
    private readonly XmlElement FContextElement;

    public string Account { get; private set; }

    public string Caption { get; private set; }

    public string Context { get; private set; }

    public MsXmlNodeEmulator SourceNode { get; private set; }

    public bool Registered { get; private set; }

    public ScriptRuntimeCurrentUser(DataSession session)
    {
      this.Account = session.UserAccount;
      ApplicationUser byAccount = session.Application.Settings.Users.FindByAccount(this.Account);
      this.Registered = byAccount != null;
      if (this.Registered)
      {
        this.Caption = byAccount.Caption;
        this.Context = byAccount.ContextName;
        this.SourceNode = new MsXmlNodeEmulator(byAccount.SourceNode);
        this.FContextElement = (XmlElement) session.ContextNode;
      }
      else
      {
        this.Caption = string.Empty;
        this.Context = string.Empty;
        this.SourceNode = (MsXmlNodeEmulator) null;
        this.FContextElement = (XmlElement) null;
      }
    }

    public string GetContextStr(string xPath, string defaultValue)
    {
      XmlNode xmlNode = this.FContextElement != null ? this.FContextElement.SelectSingleNode(xPath) : (XmlNode) null;
      return xmlNode != null ? xmlNode.InnerText : defaultValue;
    }

    public bool ContextContains(string xPath) => this.FContextElement != null && this.FContextElement.SelectSingleNode(xPath) != null;

    public bool IsPolicyEnabled(string policyName) => this.ContextContains(policyName + "[@type='policy']");
  }
}
