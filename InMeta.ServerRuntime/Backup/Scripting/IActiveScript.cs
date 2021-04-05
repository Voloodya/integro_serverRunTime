// Decompiled with JetBrains decompiler
// Type: Scripting.IActiveScript
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Scripting
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("BB1A2AE1-A4F9-11cf-8F20-00805F2CD064")]
  [ComImport]
  internal interface IActiveScript
  {
    void SetScriptSite(IActiveScriptSite pass);

    void GetScriptSite(Guid riid, out IntPtr site);

    void SetScriptState(ScriptState state);

    void GetScriptState(out ScriptState scriptState);

    void Close();

    void AddNamedItem(string name, ScriptItem flags);

    void AddTypeLib(Guid typeLib, uint major, uint minor, uint flags);

    void GetScriptDispatch(string itemName, out ScriptControl.IDispatch dispatch);

    void GetCurrentScriptThreadId(out uint thread);

    void GetScriptThreadId(uint win32ThreadId, out uint thread);

    void GetScriptThreadState(uint thread, out ScriptThreadState state);

    void InterruptScriptThread(uint thread, out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo, uint flags);

    void Clone(out IActiveScript script);
  }
}
