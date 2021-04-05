// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataAssociationRef
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataAssociationRef
  {
    private MetadataProperty FRefProperty;
    private string FSelectorValue;
    internal MetadataChildRef OwnerChildRef;
    private string FAggregationRoleMemberName;
    private bool FAggregationBuiltIn;
    private AggregationType FAggregationType;
    private string FAggregationObjectView;
    private string FAssociationObjectView;
    public readonly MetadataAssociation Association;
    internal readonly int Index;
    private static readonly string[] FAggregationTypeNames = new string[2]
    {
      "attribute",
      "part"
    };

    public bool AggregationBuiltIn => this.FAggregationBuiltIn;

    public AggregationType AggregationType => this.FAggregationType;

    public string AggregationObjectView => this.FAggregationObjectView;

    public string AssociationObjectView => this.FAssociationObjectView;

    internal MetadataAssociationRef(MetadataAssociation ass, int index)
    {
      this.Association = ass;
      this.Index = index;
    }

    internal void LoadFromXml(XmlNode node)
    {
      MetadataClass metadataClass1 = this.Association.Class;
      try
      {
        MetadataClass metadataClass2 = metadataClass1.Metadata.Classes.Need(XmlUtils.NeedAttr(node, "ref-class"));
        this.FRefProperty = metadataClass2.Properties.Find(XmlUtils.GetAttr(node, "ref-property")) ?? metadataClass2.IDProperty;
        this.FSelectorValue = node.Name == "case" ? XmlUtils.NeedAttr(node, "value") : string.Empty;
        this.FAssociationObjectView = XmlUtils.GetAttr(node, "association-object-view", "default");
        if (this.Association.Property.IsAggregation)
        {
          this.FAggregationRoleMemberName = XmlUtils.GetAttr(node, "role-prog-id", this.Association.Class.IdentName);
          this.FAggregationBuiltIn = XmlUtils.GetBoolAttr(node, "is-built-in");
          this.FAggregationType = (AggregationType) XmlUtils.GetEnumAttr(node, "aggregation-type", MetadataAssociationRef.FAggregationTypeNames, 0);
          this.FAggregationObjectView = XmlUtils.GetAttr(node, "aggregation-object-view", "default");
          metadataClass2.Childs.Ensure(this);
        }
        metadataClass2.ExternalRefs.Add(this);
      }
      catch (Exception ex)
      {
        throw new MetadataException(string.Format("Ошибка загрузки свойства {0}", (object) this.Association.Property.Name), ex);
      }
    }

    public MetadataClass RefClass => this.FRefProperty.Class;

    public string SelectorValue => this.FSelectorValue;

    public string AggregationRoleMemberName => this.FAggregationRoleMemberName;
  }
}
