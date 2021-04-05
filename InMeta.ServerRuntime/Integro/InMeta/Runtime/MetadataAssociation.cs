// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataAssociation
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataAssociation
  {
    private MetadataProperty FSelector;
    public readonly MetadataProperty Property;
    public readonly MetadataAssociationRefList Refs;

    internal MetadataAssociation(MetadataProperty property)
    {
      this.Property = property;
      this.Property.SetAssociation(this);
      this.Refs = new MetadataAssociationRefList();
    }

    internal void LoadFromXml(XmlNode node)
    {
      string attr = XmlUtils.GetAttr(node, "ref-select-case");
      this.FSelector = attr.Length == 0 ? (MetadataProperty) null : this.Property.Class.Properties.Need(attr);
      if (this.FSelector == null)
      {
        MetadataAssociationRef assRef = new MetadataAssociationRef(this, this.Refs.Count);
        assRef.LoadFromXml(node);
        this.Refs.Add(assRef);
      }
      else
      {
        foreach (XmlNode selectNode in node.SelectNodes("case"))
        {
          MetadataAssociationRef assRef = new MetadataAssociationRef(this, this.Refs.Count);
          assRef.LoadFromXml(selectNode);
          if (this.Refs.FindBySelectorValue((object) assRef.SelectorValue) == null)
            ;
          this.Refs.Add(assRef);
        }
        if (this.Refs.Count == 0)
          throw new MetadataException("Не заданы варианты связи.");
        this.FSelector.SetAssociation(this);
      }
    }

    internal void LoadComplete()
    {
    }

    public MetadataClass Class => this.Property.Class;

    public MetadataProperty Selector => this.FSelector;
  }
}
