// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.ScriptRuntimeDataObjectChildList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using System.Collections;

namespace InMeta.ServerRuntime
{
  public class ScriptRuntimeDataObjectChildList : IEnumerable
  {
    private readonly DataObjectChildList FData;

    public ScriptRuntimeDataObjectChildList(DataObjectChildList data) => this.FData = data;

    public int Count => this.FData.Count;

    public ScriptRuntimeDataObject this[int index] => new ScriptRuntimeDataObject(this.FData[index]);

    public ScriptRuntimeDataObject AddNew() => new ScriptRuntimeDataObject(this.FData.AddNew());

    public void DeleteAll() => this.FData.DeleteAll();

    public void Sort(string propertyNames) => this.FData.Sort(propertyNames);

    public ScriptRuntimeDataObject First => new ScriptRuntimeDataObject(this.FData.First);

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new ScriptRuntimeDataObjectList.Enumerator(this.FData.GetEnumerator());
  }
}
