// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.DataType
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InDbs
{
  [Flags]
  public enum DataType
  {
    BaseMask = 65280, // 0x0000FF00
    Unknown = 0,
    Boolean = 256, // 0x00000100
    Integer = 512, // 0x00000200
    Float = Integer | Boolean, // 0x00000300
    Currency = 1024, // 0x00000400
    DateTime = Currency | Boolean, // 0x00000500
    Date = 1281, // 0x00000501
    Time = 1282, // 0x00000502
    String = Currency | Integer, // 0x00000600
    Memo = String | Boolean, // 0x00000700
    Binary = 2048, // 0x00000800
    Picture = 2049, // 0x00000801
    Document = 2050, // 0x00000802
  }
}
