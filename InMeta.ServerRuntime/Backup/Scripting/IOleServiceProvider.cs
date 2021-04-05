// Decompiled with JetBrains decompiler
// Type: Scripting.IOleServiceProvider
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scripting
{
  [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IOleServiceProvider
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    int QueryService([In] ref Guid guidService, [In] ref Guid riid, out IntPtr ppvObject);
  }
}
