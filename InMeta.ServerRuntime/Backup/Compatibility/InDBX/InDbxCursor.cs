// Decompiled with JetBrains decompiler
// Type: Compatibility.InDBX.InDbxCursor
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Data;

namespace Compatibility.InDBX
{
  public class InDbxCursor : IDisposable
  {
    private IDataReader FReader;

    internal InDbxCursor(IDataReader reader)
    {
      this.FReader = reader;
      this.EOF = !this.FReader.Read();
    }

    public bool EOF { get; private set; }

    public object this[object name] => this.FReader.GetValue(this.FReader.GetOrdinal(name.ToString()));

    void IDisposable.Dispose() => this.Dispose(true);

    ~InDbxCursor() => this.Dispose(false);

    private void Dispose(bool disposing)
    {
      if (disposing && this.FReader != null)
        this.FReader.Dispose();
      this.FReader = (IDataReader) null;
    }

    public void Next() => this.EOF = !this.FReader.Read();
  }
}
