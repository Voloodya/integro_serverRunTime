// Decompiled with JetBrains decompiler
// Type: Scripting.IActiveScriptSite
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Scripting
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("DB01A1E3-A42B-11cf-8F20-00805F2CD064")]
  [ComImport]
  internal interface IActiveScriptSite
  {
    void GetLCID(out int lcid);

    void GetItemInfo(string name, ScriptInfo returnMask, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown), Out] object[] item, [MarshalAs(UnmanagedType.LPArray), Out] IntPtr[] typeInfo);

    void GetDocVersionString(out string version);

    void OnScriptTerminate(object result, System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo);

    void OnStateChange(ScriptState scriptState);

    void OnScriptError(IActiveScriptError scriptError);

    void OnEnterScript();

    void OnLeaveScript();
  }
}
