// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataObjectViewList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataObjectViewList : IEnumerable
  {
    private readonly MetadataClass FClass;
    private readonly List<MetadataObjectView> FItems;

    public IEnumerator GetEnumerator() => (IEnumerator) this.FItems.GetEnumerator();

    internal MetadataObjectViewList(MetadataClass cls)
    {
      this.FClass = cls;
      this.FItems = new List<MetadataObjectView>();
    }

    internal void Add(MetadataObjectView item) => this.FItems.Add(item);

    public MetadataObjectView Find(string name)
    {
      foreach (MetadataObjectView fitem in this.FItems)
      {
        if (fitem.Name == name)
          return fitem;
      }
      return (MetadataObjectView) null;
    }

    public MetadataObjectView Need(string name) => this.Find(name) ?? throw new MetadataException(string.Format("В классе \"{0}\" не найдено представление объекта \"{1}\".", (object) this.FClass.Name, (object) name));

    public bool Contains(MetadataObjectView objectView) => this.FItems.Contains(objectView);

    public bool Contains(string name) => this.Find(name) != null;

    public int Count => this.FItems.Count;

    public MetadataObjectView this[int index] => this.FItems[index];
  }
}
