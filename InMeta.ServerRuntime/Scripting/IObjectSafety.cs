// Decompiled with JetBrains decompiler
// Type: Scripting.IObjectSafety
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scripting
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("CB5BDC81-93C1-11cf-8F20-00805F2CD064")]
  [ComImport]
  internal interface IObjectSafety
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    [return: MarshalAs(UnmanagedType.I4)]
    int GetInterfaceSafetyOptions(
      ref Guid riid,
      out int pdwSupportedOptions,
      out int pdwEnabledOptions);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    [return: MarshalAs(UnmanagedType.I4)]
    int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions);
  }
}
