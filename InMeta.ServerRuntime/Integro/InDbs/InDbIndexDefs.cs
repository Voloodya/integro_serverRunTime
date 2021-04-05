// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbIndexDefs
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;

namespace Integro.InDbs
{
  public class InDbIndexDefs : IEnumerable
  {
    private readonly ArrayList FItems = new ArrayList();
    private readonly InDbDatabase FDb;

    internal InDbIndexDefs(InDbDatabase db) => this.FDb = db;

    internal int Add(InDbIndexDef indexDef) => this.FItems.Add((object) indexDef);

    internal void RemoveAt(int index) => this.FItems.RemoveAt(index);

    internal void Clear() => this.FItems.Clear();

    public InDbIndexDef this[int index] => (InDbIndexDef) this.FItems[index];

    public InDbIndexDef this[string name]
    {
      get
      {
        int index = this.IndexOf(name);
        return index != -1 ? this[index] : throw new InDbException(string.Format("Не найден индекс с именем {0}", (object) name));
      }
    }

    public int IndexOf(string name)
    {
      for (int index = 0; index < this.Count; ++index)
      {
        if (this.FDb.SameIdentifiers(name, this[index].Name))
          return index;
      }
      return -1;
    }

    public int IndexOf(InDbIndexDef indexDef) => this.FItems.IndexOf((object) indexDef);

    public int Count => this.FItems.Count;

    public IEnumerator GetEnumerator() => this.FItems.GetEnumerator();
  }
}
