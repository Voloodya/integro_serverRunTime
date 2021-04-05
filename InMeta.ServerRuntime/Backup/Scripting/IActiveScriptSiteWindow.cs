// Decompiled with JetBrains decompiler
// Type: Scripting.IActiveScriptSiteWindow
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Scripting
{
  [Guid("D10F6761-83E9-11CF-8F20-00805F2CD064")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IActiveScriptSiteWindow
  {
    void GetWindow(out IntPtr windowHandle);

    void EnableModeless(bool enable);
  }
}
