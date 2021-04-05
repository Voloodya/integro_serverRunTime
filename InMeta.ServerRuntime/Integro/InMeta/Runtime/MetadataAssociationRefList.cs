// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataAssociationRefList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataAssociationRefList : IEnumerable
  {
    private readonly List<MetadataAssociationRef> FItems;

    public IEnumerator GetEnumerator() => (IEnumerator) this.FItems.GetEnumerator();

    internal MetadataAssociationRefList() => this.FItems = new List<MetadataAssociationRef>();

    internal void Add(MetadataAssociationRef assRef) => this.FItems.Add(assRef);

    public MetadataAssociationRef Find(MetadataClass refClass)
    {
      for (int index = 0; index < this.FItems.Count; ++index)
      {
        MetadataAssociationRef fitem = this.FItems[index];
        if (fitem.RefClass == refClass)
          return fitem;
      }
      return (MetadataAssociationRef) null;
    }

    public MetadataAssociationRef Need(MetadataClass refClass) => this.Find(refClass) ?? throw new MetadataException(string.Format("Не найдено отношение с классом {0}", (object) refClass.Name));

    public MetadataAssociationRef FindBySelectorValue(object value)
    {
      string str = value == null || value == DBNull.Value ? string.Empty : value.ToString();
      for (int index = 0; index < this.FItems.Count; ++index)
      {
        MetadataAssociationRef fitem = this.FItems[index];
        if (fitem.SelectorValue.Equals(str))
          return fitem;
      }
      return (MetadataAssociationRef) null;
    }

    public int Count => this.FItems.Count;

    public MetadataAssociationRef this[int index] => this.FItems[index];
  }
}
