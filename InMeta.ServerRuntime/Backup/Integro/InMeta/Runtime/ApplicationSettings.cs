// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ApplicationSettings
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class ApplicationSettings
  {
    public readonly XmlNode SourceNode;
    public readonly string Url;
    public readonly bool DevelopmentMode;
    public readonly bool CheckRefIntegrity;
    public readonly string RootFolder;
    public readonly string Name;
    public readonly ApplicationConfig Config;
    public readonly ApplicationDbConfig DbConfig;
    public readonly ApplicationUserList Users;
    public readonly AttachmentsSettings Attachments;
    private readonly bool FLogEnabled;
    private readonly string FLogFileName;
    private readonly Encoding FLogEncoding;
    private readonly UpdateLogMode FUpdateLogMode;

    internal ApplicationSettings(DataApplicationState app, XmlNode sourceNode)
    {
      this.SourceNode = sourceNode;
      this.Config = new ApplicationConfig(sourceNode.SelectSingleNode("config"));
      this.DbConfig = new ApplicationDbConfig(sourceNode.SelectSingleNode("driver"));
      this.Users = new ApplicationUserList(sourceNode.SelectNodes("user"));
      this.Url = XmlUtils.GetChildText(this.Config.SourceNode, "app-path", app.Id);
      this.DevelopmentMode = XmlUtils.GetBoolAttr(this.Config.SourceNode, "development-mode", false);
      this.CheckRefIntegrity = XmlUtils.GetBoolAttr(this.Config.SourceNode, "check-ref-integrity", true);
      this.RootFolder = XmlUtils.GetAttr(sourceNode, "AppRootFolder");
      this.Name = XmlUtils.GetAttr(sourceNode, "name");
      this.Attachments = new AttachmentsSettings(this.Config.SourceNode.SelectSingleNode(nameof (Attachments)), this.RootFolder);
      XmlNode node1 = this.Config.SourceNode.SelectSingleNode("log");
      if (node1 != null)
      {
        this.FLogEnabled = XmlUtils.GetBoolAttr(node1, "enabled", false);
        this.FLogFileName = XmlUtils.GetAttr(node1, "file-name", "InMeta.Log");
        this.FLogEncoding = Encoding.GetEncoding(XmlUtils.GetAttr(node1, "encoding", "windows-1251"));
      }
      XmlNode node2 = this.Config.SourceNode.SelectSingleNode("UpdateLog");
      if (node2 == null)
        return;
      bool boolAttr1 = XmlUtils.GetBoolAttr(node2, "Enabled", false);
      bool boolAttr2 = XmlUtils.GetBoolAttr(node2, "IncludeOriginalValues", false);
      if (boolAttr1)
        this.FUpdateLogMode = boolAttr2 ? UpdateLogMode.Changes : UpdateLogMode.ChangesWithOriginalValues;
    }

    public string GetLocalSettings(string path, string def)
    {
      XmlNode xmlNode = this.Config.SourceNode.SelectSingleNode("local-settings/" + path);
      return xmlNode != null ? xmlNode.InnerText : def;
    }

    public bool LogEnabled => this.FLogEnabled;

    public UpdateLogMode UpdateLogMode => this.FUpdateLogMode;

    public string LogFileName => this.FLogFileName;

    public Encoding LogEncoding => this.FLogEncoding;
  }
}
