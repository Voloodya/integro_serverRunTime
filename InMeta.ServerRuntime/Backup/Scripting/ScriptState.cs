// Decompiled with JetBrains decompiler
// Type: Scripting.ScriptState
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Scripting
{
  internal enum ScriptState : uint
  {
    Uninitialized,
    Started,
    Connected,
    Disconnected,
    Closed,
    Initialized,
  }
}
