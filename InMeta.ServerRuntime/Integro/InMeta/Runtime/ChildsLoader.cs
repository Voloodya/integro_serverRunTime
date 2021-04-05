// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ChildsLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Integro.InMeta.Runtime
{
  internal class ChildsLoader : ObjectPartLoader
  {
    public readonly MetadataChildRef ChildRef;
    public readonly ObjectListLoaderByIds RefLoader;

    public ChildsLoader(MetadataChildRef childRef, ObjectListLoaderByIds refLoader)
      : base((string) null, (string) null)
    {
      this.ChildRef = childRef;
      this.RefLoader = refLoader;
    }

    internal override void Load(
      DataObject obj,
      object value,
      object exValue,
      LoadContext loadContext)
    {
      this.RefLoader.Add(obj.Id);
      obj.GetChilds(this.ChildRef).SetCompleted(true);
    }
  }
}
