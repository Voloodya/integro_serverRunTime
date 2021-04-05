// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectPartLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Integro.InMeta.Runtime
{
  internal abstract class ObjectPartLoader
  {
    protected internal readonly string FieldName;
    protected internal readonly string ExFieldName;

    protected ObjectPartLoader(string fieldName, string exFieldName)
    {
      this.FieldName = fieldName;
      this.ExFieldName = exFieldName;
    }

    internal abstract void Load(
      DataObject obj,
      object value,
      object exValue,
      LoadContext loadContext);
  }
}
