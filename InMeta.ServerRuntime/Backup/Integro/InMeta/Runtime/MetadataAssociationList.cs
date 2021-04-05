// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataAssociationList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataAssociationList : IEnumerable
  {
    private readonly List<MetadataAssociation> FItems = new List<MetadataAssociation>();

    public IEnumerator GetEnumerator() => (IEnumerator) this.FItems.GetEnumerator();

    internal MetadataAssociationList()
    {
    }

    internal void Add(MetadataAssociation ass) => this.FItems.Add(ass);

    public int Count => this.FItems.Count;

    public MetadataAssociation this[int index] => this.FItems[index];

    public MetadataAssociation Find(string name)
    {
      for (int index = 0; index < this.FItems.Count; ++index)
      {
        MetadataAssociation fitem = this.FItems[index];
        if (fitem.Property.Name == name)
          return fitem;
      }
      return (MetadataAssociation) null;
    }

    public MetadataAssociation Need(string name) => this.Find(name) ?? throw new MetadataException(string.Format("Не найдена ассоциация \"{0}\".", (object) name));
  }
}
