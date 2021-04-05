// Decompiled with JetBrains decompiler
// Type: Scripting.ScriptItem
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Scripting
{
  [Flags]
  internal enum ScriptItem : uint
  {
    None = 0,
    IsVisible = 2,
    IsSource = 4,
    GlobalMembers = 8,
    IsPersistent = 64, // 0x00000040
    CodeOnly = 512, // 0x00000200
    NoCode = 1024, // 0x00000400
  }
}
