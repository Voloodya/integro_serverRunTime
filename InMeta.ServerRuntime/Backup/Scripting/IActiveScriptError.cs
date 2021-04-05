// Decompiled with JetBrains decompiler
// Type: Scripting.IActiveScriptError
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Scripting
{
  [Guid("EAE1BA61-A4ED-11cf-8F20-00805F2CD064")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [ComImport]
  internal interface IActiveScriptError
  {
    void GetExceptionInfo(out System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo);

    void GetSourcePosition(out uint sourceContext, out uint lineNumber, out int characterPosition);

    void GetSourceLineText(out string sourceLine);
  }
}
