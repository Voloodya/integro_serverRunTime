// Decompiled with JetBrains decompiler
// Type: Scripting.ActiveScriptParseWrapper
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Scripting
{
  internal class ActiveScriptParseWrapper
  {
    private readonly IActiveScriptParse32 FAsp32;
    private readonly IActiveScriptParse64 FAsp64;

    internal ActiveScriptParseWrapper(object comObject)
    {
      if (IntPtr.Size == 4)
        this.FAsp32 = (IActiveScriptParse32) comObject;
      else
        this.FAsp64 = (IActiveScriptParse64) comObject;
    }

    internal void InitNew()
    {
      if (this.FAsp32 != null)
        this.FAsp32.InitNew();
      else
        this.FAsp64.InitNew();
    }

    internal void AddScriptlet(
      string defaultName,
      string code,
      string itemName,
      string subItemName,
      string eventName,
      string delimiter,
      IntPtr sourceContextCookie,
      uint startingLineNumber,
      ScriptText flags,
      out string name,
      out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo)
    {
      if (this.FAsp32 != null)
        this.FAsp32.AddScriptlet(defaultName, code, itemName, subItemName, eventName, delimiter, sourceContextCookie, startingLineNumber, flags, out name, out exceptionInfo);
      else
        this.FAsp64.AddScriptlet(defaultName, code, itemName, subItemName, eventName, delimiter, sourceContextCookie, startingLineNumber, flags, out name, out exceptionInfo);
    }

    internal HRESULT ParseScriptText(
      string code,
      string itemName,
      [MarshalAs(UnmanagedType.IUnknown)] object context,
      string delimiter,
      IntPtr sourceContextCookie,
      uint startingLineNumber,
      ScriptText flags,
      [MarshalAs(UnmanagedType.Struct)] out object result,
      out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo)
    {
      return this.FAsp32 != null ? this.FAsp32.ParseScriptText(code, itemName, context, delimiter, sourceContextCookie, startingLineNumber, flags, out result, out exceptionInfo) : this.FAsp64.ParseScriptText(code, itemName, context, delimiter, sourceContextCookie, startingLineNumber, flags, out result, out exceptionInfo);
    }

    public void Dispose()
    {
      if (this.FAsp32 != null)
        Marshal.ReleaseComObject((object) this.FAsp32);
      if (this.FAsp64 == null)
        return;
      Marshal.ReleaseComObject((object) this.FAsp64);
    }
  }
}
