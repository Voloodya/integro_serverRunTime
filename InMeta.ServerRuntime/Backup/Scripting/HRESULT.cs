// Decompiled with JetBrains decompiler
// Type: Scripting.HRESULT
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Scripting
{
  internal enum HRESULT
  {
    NotImplemented = -2147467263, // 0x80004001
    NoInterface = -2147467262, // 0x80004002
    Fail = -2147467259, // 0x80004005
    UnknownName = -2147352570, // 0x80020006
    ScriptReported = -2147352319, // 0x80020101
    ElementNotFound = -2147319765, // 0x8002802B
    Ok = 0,
    False = 1,
  }
}
