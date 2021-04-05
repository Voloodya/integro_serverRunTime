// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataException
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  [Serializable]
  public class MetadataException : Exception
  {
    public MetadataException()
    {
    }

    public MetadataException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public MetadataException(string message)
      : base(message)
    {
    }

    public MetadataException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
