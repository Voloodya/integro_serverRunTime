// Decompiled with JetBrains decompiler
// Type: Scripting.ScriptText
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Scripting
{
  [Flags]
  internal enum ScriptText : uint
  {
    None = 0,
    DelayExecution = 1,
    IsVisible = 2,
    IsExpression = 32, // 0x00000020
    IsPersistent = 64, // 0x00000040
    HostManageSource = 128, // 0x00000080
  }
}
