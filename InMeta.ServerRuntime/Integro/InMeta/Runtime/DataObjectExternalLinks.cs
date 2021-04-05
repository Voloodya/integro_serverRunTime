// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataObjectExternalLinks
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataObjectExternalLinks
  {
    public readonly DataObject Object;
    public readonly DataObjectExternalLink[] Links;

    internal DataObjectExternalLinks(DataObject obj, DataObjectExternalLink[] links)
    {
      this.Object = obj;
      this.Links = links;
    }

    public DataObject[] GetObjects()
    {
      int length = 0;
      for (int index = 0; index < this.Links.Length; ++index)
        length += this.Links[index].Objects.Length;
      DataObject[] dataObjectArray = new DataObject[length];
      int index1 = 0;
      for (int index2 = 0; index2 < this.Links.Length; ++index2)
      {
        DataObject[] objects = this.Links[index2].Objects;
        objects.CopyTo((Array) dataObjectArray, index1);
        index1 += objects.Length;
      }
      return dataObjectArray;
    }
  }
}
