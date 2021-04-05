// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.AttachmentsSettings
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.IO;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class AttachmentsSettings
  {
    public readonly XmlNode SourceNode;
    public readonly string DefaultFolder;

    internal AttachmentsSettings(XmlNode sourceNode, string applicationRootFolder)
    {
      this.SourceNode = sourceNode;
      string defaultValue = Path.Combine(applicationRootFolder, "Web\\attachments");
      this.DefaultFolder = sourceNode != null ? XmlUtils.GetAttr(sourceNode, nameof (DefaultFolder), defaultValue) : defaultValue;
    }

    public string GetClassFolder(MetadataClass cls) => Path.Combine(this.DefaultFolder, cls.IdentName);

    public string GetObjectFolder(MetadataClass cls, DataId id) => Path.Combine(Path.Combine(this.DefaultFolder, cls.IdentName), id.ToString());
  }
}
