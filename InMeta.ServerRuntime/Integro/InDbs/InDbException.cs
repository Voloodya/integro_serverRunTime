// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbException
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Integro.InDbs
{
  [ComVisible(false)]
  [Serializable]
  public class InDbException : Exception
  {
    public InDbException()
    {
    }

    public InDbException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public InDbException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public InDbException(string message)
      : base(message)
    {
    }
  }
}
