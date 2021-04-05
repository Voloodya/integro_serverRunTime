// Decompiled with JetBrains decompiler
// Type: Scripting.IInternetHostSecurityManager
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scripting
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("3af280b6-cb3f-11d0-891e-00c04fb6bfc4")]
  [ComImport]
  internal interface IInternetHostSecurityManager
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    [return: MarshalAs(UnmanagedType.I4)]
    int GetSecurityId([Out] byte[] pbSecurityId, [In, Out] ref IntPtr pcbSecurityId, IntPtr dwReserved);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    [return: MarshalAs(UnmanagedType.I4)]
    int ProcessUrlAction(
      int dwAction,
      [Out] int[] pPolicy,
      int cbPolicy,
      [Out] byte[] pContext,
      int cbContext,
      int dwFlags,
      int dwReserved);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    [return: MarshalAs(UnmanagedType.I4)]
    int QueryCustomPolicy(
      Guid guidKey,
      out byte[] ppPolicy,
      out int pcbPolicy,
      byte[] pContext,
      int cbContext,
      int dwReserved);
  }
}
