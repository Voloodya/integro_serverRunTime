// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.CustomIdGenerator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InMeta.Runtime.CentralServer
{
  [Serializable]
  public class CustomIdGenerator
  {
    public readonly string Name;
    public readonly int LastNumber;

    public CustomIdGenerator(string name, int lastNumber)
    {
      this.Name = name;
      this.LastNumber = lastNumber;
    }
  }
}
