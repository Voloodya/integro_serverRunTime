// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataPropertyList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataPropertyList : IEnumerable
  {
    private readonly List<MetadataProperty> FItems;

    public IEnumerator GetEnumerator() => (IEnumerator) this.FItems.GetEnumerator();

    public MetadataPropertyList() => this.FItems = new List<MetadataProperty>();

    internal void Add(MetadataProperty item) => this.FItems.Add(item);

    public MetadataProperty Find(string name)
    {
      foreach (MetadataProperty fitem in this.FItems)
      {
        if (fitem.Name == name)
          return fitem;
      }
      return (MetadataProperty) null;
    }

    public bool Contains(MetadataProperty prop) => this.FItems.Contains(prop);

    public MetadataProperty Need(string name) => this.Find(name) ?? throw new MetadataException(string.Format("Не найдено свойство {0}", (object) name));

    public int Count => this.FItems.Count;

    public MetadataProperty this[int index] => this.FItems[index];
  }
}
