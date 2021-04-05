// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.ScriptRuntimeDataObjectList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using System.Collections;

namespace InMeta.ServerRuntime
{
  public class ScriptRuntimeDataObjectList : IEnumerable
  {
    private readonly DataObjectList FItems;

    public ScriptRuntimeDataObjectList(DataObjectList items) => this.FItems = items;

    public int Count => this.FItems.Count;

    public ScriptRuntimeDataObject this[int index] => new ScriptRuntimeDataObject(this.FItems[index]);

    public void Sort(string propertyNames) => this.FItems.Sort(propertyNames);

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new ScriptRuntimeDataObjectList.Enumerator(this.FItems.GetEnumerator());

    internal class Enumerator : IEnumerator
    {
      private readonly IEnumerator FDataEnumerator;

      public Enumerator(IEnumerator dataEnumerator) => this.FDataEnumerator = dataEnumerator;

      public bool MoveNext() => this.FDataEnumerator.MoveNext();

      public void Reset() => this.FDataEnumerator.Reset();

      public object Current => (object) new ScriptRuntimeDataObject((DataObject) this.FDataEnumerator.Current);
    }
  }
}
