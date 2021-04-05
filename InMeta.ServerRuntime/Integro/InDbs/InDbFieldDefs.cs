// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbFieldDefs
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;

namespace Integro.InDbs
{
  public class InDbFieldDefs : IEnumerable
  {
    private readonly ArrayList FItems = new ArrayList();
    private readonly InDbDatabase FDb;

    internal int Add(InDbFieldDef fieldDef) => this.FItems.Add((object) fieldDef);

    internal void RemoveAt(int index) => this.FItems.RemoveAt(index);

    internal InDbFieldDefs(InDbDatabase db) => this.FDb = db;

    internal void Clear() => this.FItems.Clear();

    public int IndexOf(string name)
    {
      for (int index = 0; index < this.Count; ++index)
      {
        if (this.FDb.SameIdentifiers(name, this[index].Name))
          return index;
      }
      return -1;
    }

    public InDbFieldDef this[int index] => (InDbFieldDef) this.FItems[index];

    public InDbFieldDef this[string name]
    {
      get
      {
        int index = this.IndexOf(name);
        return index != -1 ? this[index] : throw new InDbException(string.Format("Не найдено поле с именем {0}", (object) name));
      }
    }

    public string[] GetFieldNames()
    {
      string[] strArray = new string[this.Count];
      for (int index = 0; index < this.Count; ++index)
        strArray[index] = this[index].Name;
      return strArray;
    }

    public int Count => this.FItems.Count;

    public IEnumerator GetEnumerator() => this.FItems.GetEnumerator();
  }
}
