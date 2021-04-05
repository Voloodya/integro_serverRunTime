// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewIdLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InMeta.Runtime
{
  internal class ObjectViewIdLoader : ObjectViewParameterLoader
  {
    public new static bool CanCreate(MetadataObjectView view, string name) => string.Equals(name, "id", StringComparison.InvariantCultureIgnoreCase);

    public static ObjectViewParameterLoader TryCreate(
      MetadataObjectView view,
      string name)
    {
      return ObjectViewIdLoader.CanCreate(view, name) ? (ObjectViewParameterLoader) new ObjectViewIdLoader() : (ObjectViewParameterLoader) null;
    }

    public override void PrepareLoadPlan(LoadPlan plan, DataSession session)
    {
    }

    public override object GetValue(DataObject obj) => (object) obj.Id.ToString();
  }
}
