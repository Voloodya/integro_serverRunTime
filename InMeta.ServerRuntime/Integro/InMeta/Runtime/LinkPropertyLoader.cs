// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.LinkPropertyLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Integro.InMeta.Runtime
{
  internal class LinkPropertyLoader : ObjectPartLoader
  {
    internal readonly MetadataAssociation Association;
    internal readonly ObjectListLoaderByIds[] RefLoaders;
    private readonly MetadataAssociationRefList Refs;

    internal LinkPropertyLoader(MetadataAssociation association)
      : base(association.Property.DataField, association.Selector == null ? (string) null : association.Selector.DataField)
    {
      this.Refs = association.Refs;
      this.RefLoaders = new ObjectListLoaderByIds[this.Refs.Count];
      this.Association = association;
    }

    internal override void Load(
      DataObject obj,
      object value,
      object exValue,
      LoadContext loadContext)
    {
      MetadataProperty property = this.Association.Property;
      if (obj.IsPropertyModified(property))
        return;
      value = DataProperty.EscapeFromDBNull(value);
      exValue = DataProperty.EscapeFromDBNull(exValue);
      obj.LoadPropertyValue(property, value, exValue, loadContext);
      DataId id = new DataId((string) value);
      if (!id.IsEmpty)
      {
        MetadataAssociationRef metadataAssociationRef = this.Association.Selector == null ? this.Refs[0] : this.Refs.FindBySelectorValue(exValue);
        if (metadataAssociationRef != null)
        {
          ObjectListLoaderByIds refLoader = this.RefLoaders[metadataAssociationRef.Index];
          if (refLoader != null && refLoader.ObjectLoader.Count > 0)
            refLoader.Add(id);
        }
      }
    }
  }
}
