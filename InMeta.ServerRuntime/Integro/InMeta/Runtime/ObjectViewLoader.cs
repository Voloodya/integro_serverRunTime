// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections;

namespace Integro.InMeta.Runtime
{
  internal class ObjectViewLoader : ObjectPartLoader
  {
    public readonly MetadataObjectView ObjectView;
    private readonly Hashtable FObjectsById = new Hashtable();

    internal ObjectViewLoader(MetadataObjectView objectView)
      : base((string) null, (string) null)
      => this.ObjectView = objectView;

    internal override void Load(
      DataObject obj,
      object value,
      object exValue,
      LoadContext loadContext)
    {
      if (this.FObjectsById.Contains((object) obj.Id))
        return;
      this.FObjectsById.Add((object) obj.Id, (object) obj);
    }

    internal DataObject[] GetObjects()
    {
      DataObject[] dataObjectArray = new DataObject[this.FObjectsById.Count];
      this.FObjectsById.Values.CopyTo((Array) dataObjectArray, 0);
      return dataObjectArray;
    }
  }
}
