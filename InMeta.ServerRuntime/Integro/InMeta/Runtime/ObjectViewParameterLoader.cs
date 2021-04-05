// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewParameterLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InMeta.Runtime
{
  internal abstract class ObjectViewParameterLoader
  {
    public abstract void PrepareLoadPlan(LoadPlan plan, DataSession session);

    public abstract object GetValue(DataObject obj);

    public static bool CanCreate(MetadataObjectView view, string name) => ObjectViewIdLoader.CanCreate(view, name) || ObjectViewPropertyLoader.CanCreate(view, name) || ObjectViewVirtualPropertyLoader.CanCreate(view, name);

    public static ObjectViewParameterLoader Create(
      MetadataObjectView view,
      string name)
    {
      ObjectViewParameterLoader viewParameterLoader1 = ObjectViewIdLoader.TryCreate(view, name);
      if (viewParameterLoader1 != null)
        return viewParameterLoader1;
      ObjectViewParameterLoader viewParameterLoader2 = ObjectViewPropertyLoader.TryCreate(view, name);
      if (viewParameterLoader2 != null)
        return viewParameterLoader2;
      return ObjectViewVirtualPropertyLoader.TryCreate(view, name) ?? throw new Exception("Невозможно создать загрузчик значения параметра для \"" + name + "\"");
    }
  }
}
