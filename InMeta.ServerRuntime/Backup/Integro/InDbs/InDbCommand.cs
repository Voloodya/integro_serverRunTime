// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbCommand
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Data;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public abstract class InDbCommand : IDisposable
  {
    public abstract IDataReader ExecuteReader(params object[] paramValues);

    public abstract int Execute(params object[] paramValues);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~InDbCommand() => this.Dispose(false);
  }
}
