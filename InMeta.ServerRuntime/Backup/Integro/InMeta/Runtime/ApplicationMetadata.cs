// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ApplicationMetadata
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class ApplicationMetadata
  {
    public readonly XmlNode SourceNode;
    public readonly MetadataClassList Classes;
    public readonly MetadataScriptLibraries ScriptLibraries;

    public ApplicationMetadata(XmlNode sourceNode)
    {
      this.SourceNode = sourceNode;
      this.Classes = new MetadataClassList();
      this.ScriptLibraries = new MetadataScriptLibraries(sourceNode);
      foreach (XmlNode selectNode in this.SourceNode.SelectNodes("class"))
      {
        string name = XmlUtils.NeedAttr(selectNode, "name");
        MetadataClass metadataClass = this.Classes.Find(name);
        if (metadataClass == null)
        {
          metadataClass = new MetadataClass(this, name, this.Classes.Count);
          this.Classes.Add(metadataClass);
        }
        metadataClass.LoadFromXml(selectNode);
      }
      foreach (MetadataClass metadataClass in this.Classes)
        metadataClass.LoadAssociations();
      foreach (MetadataClass metadataClass in this.Classes)
        metadataClass.LoadObjectViews();
      foreach (MetadataClass metadataClass in this.Classes)
        metadataClass.LoadVirtualProperties();
    }
  }
}
