// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaManager.CMeta
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Compatibility.InMetaSR;
using Compatibility.InMetaUtils;
using Integro.InMeta.Runtime;
using System;
using System.Xml;

namespace Compatibility.InMetaManager
{
  public class CMeta
  {
    private DataApplication FDataApplication;

    public string AppId { get; private set; }

    public void Load(string applicationId)
    {
      this.AppId = applicationId;
      this.FDataApplication = new DataApplication(applicationId, (string) null);
      this.Application = (object) new InMetaApplication(this.FDataApplication);
    }

    public object Application { get; private set; }

    public object Root => (object) new MsXmlNodeEmulator(this.FDataApplication.Metadata.SourceNode);

    public string MetaVersionAsString => ((XmlElement) this.FDataApplication.Metadata.SourceNode).GetAttribute("Version");

    public string MetaVersion => this.MetaVersionAsString;

    public string AppPath => this.FDataApplication.Settings.Url;

    public bool DevelopmentMode => this.FDataApplication.Settings.DevelopmentMode;

    public string GetContextNameByUser(string account) => this.FDataApplication.Settings.Users.NeedByAccount(account).ContextName;

    public string GetEntryPointByUser(string account) => ((XmlElement) this.FDataApplication.Settings.SourceNode.SelectSingleNode("context[@name='" + this.FDataApplication.Settings.Users.NeedByAccount(account).ContextName + "']"))?.GetAttribute("entry-point");

    public object NeedClass(string className) => (object) new MsXmlNodeEmulator(this.FDataApplication.Metadata.SourceNode.SelectSingleNode("class[@name='" + className + "']") ?? throw new Exception(string.Format("Не найден класс \"{0}\".", (object) className)));

    public object NeedView(object classNode, string viewName)
    {
      if (string.IsNullOrEmpty(viewName))
        viewName = "default";
      return InMetaXmlUtils.SelectSingleNode(classNode, "object-view[@name='" + viewName + "']") ?? throw new Exception(string.Format("Не найдено представление объекта \"{0}\".", (object) viewName));
    }
  }
}
