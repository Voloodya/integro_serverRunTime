// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectSessionState
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InMeta.Runtime
{
  [Flags]
  public enum ObjectSessionState
  {
    New = 1,
    Existing = 2,
    Error = 4,
    PropertiesModified = 8,
    ChildsModified = 16, // 0x00000010
    Deleted = 32, // 0x00000020
    NullObject = 64, // 0x00000040
    AttachmentsModified = 128, // 0x00000080
  }
}
