// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataObjectView
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataObjectView
  {
    public readonly XmlNode SourceNode;
    public readonly MetadataClass Class;
    public readonly string Name;
    public readonly string Caption;
    public readonly ContentType ContentType;
    public readonly string Script;
    public readonly string ScriptLanguage;
    public readonly MetadataVirtualPropertyList VirtualProperties;
    public readonly int Index;
    public readonly string[] Using;
    private static readonly string[] FContentTypeNames = new string[2]
    {
      "text",
      "html"
    };

    internal MetadataObjectView(MetadataClass cls, XmlNode node, int index)
    {
      this.Index = index;
      this.Class = cls;
      this.Name = XmlUtils.NeedAttr(node, "name");
      if (this.Name.Length == 0)
        this.Name = "default";
      this.VirtualProperties = new MetadataVirtualPropertyList(this);
      this.SourceNode = node;
      this.Caption = XmlUtils.GetAttr(node, "caption", this.Name);
      this.ScriptLanguage = XmlUtils.GetAttr(node, "language", "VBScript");
      this.ContentType = (ContentType) XmlUtils.GetEnumAttr(node, "content-type", MetadataObjectView.FContentTypeNames, 0);
      this.Using = XmlUtils.GetAttr(node, "using").Split(',');
      this.Script = XmlUtils.GetOwnText(node);
    }

    public bool IsDefault => this.Name == "default";

    public override string ToString() => string.Format("Представление объекта: {0}.{1}", (object) this.Class.Name, (object) this.Name);

    internal void LoadVirtualProperties()
    {
      foreach (XmlNode selectNode in this.SourceNode.SelectNodes("virtual-property"))
        this.VirtualProperties.Add((object) new MetadataVirtualProperty(this, selectNode));
    }
  }
}
