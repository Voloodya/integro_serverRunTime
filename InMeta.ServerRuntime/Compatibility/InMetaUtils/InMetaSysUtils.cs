// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaUtils.InMetaSysUtils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.IO;
using System.Runtime.InteropServices;

namespace Compatibility.InMetaUtils
{
  [ComVisible(true)]
  public class InMetaSysUtils
  {
    public static readonly InMetaSysUtils Instance = new InMetaSysUtils();

    public object LoadBinarySlice(string fileName, int pos, int size)
    {
      using (FileStream fileStream = File.OpenRead(fileName))
      {
        fileStream.Position = (long) pos;
        byte[] buffer = new byte[size];
        fileStream.Read(buffer, 0, size);
        return (object) buffer;
      }
    }

    public void SaveBinarySlice(string fileName, int pos, object bytes)
    {
      using (FileStream fileStream = File.OpenWrite(fileName))
      {
        fileStream.Position = (long) pos;
        byte[] buffer = (byte[]) bytes;
        fileStream.Write(buffer, 0, buffer.Length);
      }
    }
  }
}
