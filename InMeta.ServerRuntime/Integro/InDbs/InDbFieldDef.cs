// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbFieldDef
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public class InDbFieldDef
  {
    internal string FOriginalName = string.Empty;
    internal DataType FOriginalDataType = DataType.Unknown;
    internal int FOriginalSize = 0;
    public string Name;
    public DataType DataType;
    public int Size;

    internal string OriginalName => this.FOriginalName;

    internal DataType OriginalDataType => this.FOriginalDataType;

    internal int OriginalSize => this.FOriginalSize;

    internal InDbFieldDef(string name, DataType dataType, int size, bool isNew)
    {
      this.Name = name;
      this.DataType = dataType;
      this.Size = size;
      if (isNew)
        return;
      this.FOriginalName = name;
      this.FOriginalDataType = dataType;
      this.FOriginalSize = size;
    }
  }
}
