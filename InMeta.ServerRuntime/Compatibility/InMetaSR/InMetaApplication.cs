// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaSR.InMetaApplication
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;

namespace Compatibility.InMetaSR
{
  public class InMetaApplication
  {
    private readonly DataApplication FDataApplication;

    internal InMetaApplication(DataApplication dataApplication) => this.FDataApplication = dataApplication;

    public object CreateSession() => (object) new InMetaSession(this.FDataApplication.CreateSession());
  }
}
