// Decompiled with JetBrains decompiler
// Type: Scripting.IScript
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Scripting
{
  [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
  [Guid("00020400-0000-0000-C000-000000000046")]
  [ComImport]
  internal interface IScript
  {
    object FindProxyForURL(string url, string host);
  }
}
