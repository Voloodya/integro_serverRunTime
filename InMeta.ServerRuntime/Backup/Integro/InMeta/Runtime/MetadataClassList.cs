// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataClassList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataClassList : ICollection, IEnumerable
  {
    private readonly ArrayList FItems;
    private readonly Hashtable FItemsByName;

    internal MetadataClassList()
    {
      this.FItems = new ArrayList();
      this.FItemsByName = new Hashtable();
    }

    public IEnumerator GetEnumerator() => this.FItems.GetEnumerator();

    internal void Add(MetadataClass item)
    {
      this.FItems.Add((object) item);
      this.FItemsByName.Add((object) item.Name, (object) item);
    }

    internal void Clear()
    {
      this.FItems.Clear();
      this.FItemsByName.Clear();
    }

    public MetadataClass Find(string name) => (MetadataClass) this.FItemsByName[(object) name];

    public MetadataClass Need(string name) => this.Find(name) ?? throw new MetadataException(string.Format("Не найден класс {0}", (object) name));

    public bool Contains(string name) => this.FItemsByName.ContainsKey((object) name);

    public void CopyTo(Array array, int index) => this.FItems.CopyTo(array, index);

    public int Count => this.FItems.Count;

    public object SyncRoot => this.FItems.SyncRoot;

    public bool IsSynchronized => this.FItems.IsSynchronized;

    public MetadataClass this[int index] => (MetadataClass) this.FItems[index];

    public MetadataClass FindByIdentName(string identName)
    {
      for (int index = 0; index < this.FItems.Count; ++index)
      {
        MetadataClass fitem = (MetadataClass) this.FItems[index];
        if (fitem.IdentName == identName)
          return fitem;
      }
      return (MetadataClass) null;
    }
  }
}
