// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataScriptLibrary
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataScriptLibrary
  {
    public readonly XmlNode SourceNode;
    public readonly string Name;
    public readonly string[] Using;
    public readonly string Language;
    public readonly string Text;

    public MetadataScriptLibrary(XmlNode sourceNode)
    {
      this.SourceNode = sourceNode;
      this.Name = XmlUtils.GetAttr(sourceNode, "name", "Default");
      this.Using = XmlUtils.GetAttr(sourceNode, "using").Split(',');
      this.Language = XmlUtils.GetAttr(sourceNode, "language", "VBScript");
      this.Text = XmlUtils.GetOwnText(sourceNode);
    }
  }
}
