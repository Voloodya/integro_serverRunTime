// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataChildRefList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataChildRefList : IEnumerable
  {
    private readonly List<MetadataChildRef> FItems;
    public readonly MetadataClass Class;

    public IEnumerator GetEnumerator() => (IEnumerator) this.FItems.GetEnumerator();

    internal MetadataChildRefList(MetadataClass cls)
    {
      this.Class = cls;
      this.FItems = new List<MetadataChildRef>();
    }

    internal MetadataChildRef Ensure(MetadataAssociationRef ownerRef)
    {
      MetadataChildRef metadataChildRef = this.Find(ownerRef.Association.Class);
      if (metadataChildRef == null)
      {
        metadataChildRef = new MetadataChildRef(ownerRef, this.FItems.Count);
        this.FItems.Add(metadataChildRef);
      }
      return metadataChildRef;
    }

    public MetadataChildRef Find(MetadataClass childClass)
    {
      for (int index = 0; index < this.FItems.Count; ++index)
      {
        MetadataChildRef metadataChildRef = this[index];
        if (metadataChildRef.ChildClass == childClass)
          return metadataChildRef;
      }
      return (MetadataChildRef) null;
    }

    public MetadataChildRef Need(MetadataClass childClass) => this.Find(childClass) ?? throw new MetadataException(string.Format("Не найдено отношение с дочерним классом {0}", (object) childClass.Name));

    public MetadataChildRef Need(string className) => this.Need(this.Class.Metadata.Classes.Need(className));

    public int Count => this.FItems.Count;

    public MetadataChildRef this[int index] => this.FItems[index];
  }
}
