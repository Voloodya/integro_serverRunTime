// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.InMetaException
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Scripting;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  [Serializable]
  public class InMetaException : Exception
  {
    public readonly string DetailsHtml;

    protected InMetaException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public InMetaException(string message)
      : base(message)
    {
    }

    public InMetaException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public InMetaException(string message, string detailsHtml)
      : base(message)
      => this.DetailsHtml = detailsHtml;

    public InMetaException(string message, string detailsHtml, Exception innerException)
      : base(message, innerException)
      => this.DetailsHtml = detailsHtml;

    public InMetaException(
      ScriptControl scriptControl,
      ScriptErrorOperation errorOperation,
      string sourceFormat,
      params object[] sourceFormatParameters)
      : this(scriptControl.Error.Description, Utility.GetScriptErrorDetailsHtml(scriptControl.Error, errorOperation, string.Format(sourceFormat, sourceFormatParameters)))
    {
    }
  }
}
