// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataPropertyLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Integro.InMeta.Runtime
{
  internal class DataPropertyLoader : ObjectPartLoader
  {
    private readonly MetadataProperty PropertyMetadata;

    internal DataPropertyLoader(MetadataProperty propertyMetadata)
      : base(propertyMetadata.DataField, (string) null)
      => this.PropertyMetadata = propertyMetadata;

    internal override void Load(
      DataObject obj,
      object value,
      object exValue,
      LoadContext loadContext)
    {
      obj.LoadPropertyValue(this.PropertyMetadata, value, exValue, loadContext);
    }
  }
}
