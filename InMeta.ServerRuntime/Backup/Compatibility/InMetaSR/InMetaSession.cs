// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaSR.InMetaSession
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Compatibility.InDBX;
using Integro.InMeta.Runtime;
using System;

namespace Compatibility.InMetaSR
{
  public class InMetaSession : IDisposable
  {
    private DataSession FDataSession;

    internal InMetaSession(DataSession dataSession) => this.FDataSession = dataSession;

    public object DB => (object) new InDbxDb(this.FDataSession.Db);

    void IDisposable.Dispose() => this.Dispose(true);

    ~InMetaSession() => this.Dispose(false);

    private void Dispose(bool disposing)
    {
      if (!disposing || this.FDataSession == null)
        return;
      this.FDataSession.Dispose();
      this.FDataSession = (DataSession) null;
    }
  }
}
