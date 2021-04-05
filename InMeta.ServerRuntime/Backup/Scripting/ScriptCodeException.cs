// Decompiled with JetBrains decompiler
// Type: Scripting.ScriptCodeException
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices.ComTypes;

namespace Scripting
{
  public class ScriptCodeException : Exception
  {
    public readonly EXCEPINFO ExcepInfo;

    public ScriptCodeException(EXCEPINFO excepInfo)
      : base(excepInfo.bstrDescription)
      => this.ExcepInfo = excepInfo;
  }
}
