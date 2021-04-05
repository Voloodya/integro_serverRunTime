// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.IDataObjectList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public interface IDataObjectList : IEnumerable
  {
    int Count { get; }

    DataObject this[int index] { get; }
  }
}
