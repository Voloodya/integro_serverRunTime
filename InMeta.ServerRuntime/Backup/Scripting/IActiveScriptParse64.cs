// Decompiled with JetBrains decompiler
// Type: Scripting.IActiveScriptParse64
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scripting
{
  [Guid("C7EF7658-E1EE-480E-97EA-D52CB4D76D17")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IActiveScriptParse64
  {
    void InitNew();

    void AddScriptlet(
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
      out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    HRESULT ParseScriptText(
      string code,
      string itemName,
      [MarshalAs(UnmanagedType.IUnknown)] object context,
      string delimiter,
      IntPtr sourceContextCookie,
      uint startingLineNumber,
      ScriptText flags,
      [MarshalAs(UnmanagedType.Struct)] out object result,
      out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo);
  }
}
