// Decompiled with JetBrains decompiler
// Type: Compatibility.IISUtils.IISUtilsObject
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Compatibility.IISUtils
{
  public class IISUtilsObject
  {
    public object ReadBinary(object source, int count, int bufSize)
    {
      int destinationIndex = 0;
      bufSize = Math.Max(bufSize, 1);
      byte[] numArray = new byte[count];
      IStream stream = (IStream) source;
      IntPtr num = Marshal.AllocCoTaskMem(8);
      for (; count > 0; count -= bufSize)
      {
        int cb = Math.Min(bufSize, count);
        byte[] pv = new byte[cb];
        stream.Read(pv, cb, num);
        int length = Marshal.ReadInt32(num);
        if (length != cb)
          throw new Exception(string.Format("Ошибка чтения данных\r\nсчитано: {0}\r\nзапрошено: {1}", (object) length, (object) cb));
        Array.Copy((Array) pv, 0, (Array) numArray, destinationIndex, length);
        destinationIndex += length;
      }
      Marshal.FreeCoTaskMem(num);
      return (object) numArray;
    }
  }
}
