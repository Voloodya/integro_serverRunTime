// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.PropertyValueGetter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InMeta.Runtime
{
  internal class PropertyValueGetter : MemberValueGetter
  {
    private readonly string FPropertyName;

    public PropertyValueGetter(string propertyName) => this.FPropertyName = propertyName;

    internal static object GetValue(DataObject obj, string propertyName)
    {
      MetadataProperty propertyMetadata = obj.Class.Properties.Need(propertyName);
      if (propertyMetadata.IsId)
        return (object) obj.Id.ToString();
      object untypedValue = obj[propertyMetadata].UntypedValue;
      if (!propertyMetadata.IsLink)
        return untypedValue;
      DataObject dataObject = (DataObject) untypedValue;
      return DataObject.Assigned(dataObject) ? (object) dataObject.Id.ToString() : (object) DBNull.Value;
    }

    internal static void PrepareLoadPlan(LoadPlan plan, DataSession session, string propertyName) => plan.EnsureProperty(plan.Class.Properties.Need(propertyName));

    internal override object GetValue(DataObject obj) => PropertyValueGetter.GetValue(obj, this.FPropertyName);

    internal override void PrepareLoadPlan(LoadPlan plan, DataSession session) => PropertyValueGetter.PrepareLoadPlan(plan, session, this.FPropertyName);
  }
}
