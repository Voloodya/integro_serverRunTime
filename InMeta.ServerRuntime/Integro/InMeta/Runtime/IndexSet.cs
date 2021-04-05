// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.IndexSet
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;

namespace Integro.InMeta.Runtime
{
  public class IndexSet
  {
    private const int MaxIndex = 1023;
    private const int BitGroupSize = 32;
    private const int BitGroupCount = 32;
    private readonly int[] FBitGroups = new int[32];

    internal IndexSet()
    {
    }

    internal IndexSet(IndexSet source) => Array.Copy((Array) source.FBitGroups, (Array) this.FBitGroups, 32);

    public bool this[int index]
    {
      get
      {
        if (index < 0 || index > 1023)
          throw new ArgumentOutOfRangeException(nameof (index));
        return (this.FBitGroups[index / 32] & 1 << index % 32) != 0;
      }
      set
      {
        if (index < 0 || index > 1023)
          throw new ArgumentOutOfRangeException(nameof (index));
        if (value)
          this.FBitGroups[index / 32] |= 1 << index % 32;
        else
          this.FBitGroups[index / 32] &= ~(1 << index % 32);
      }
    }

    public bool IsEmpty
    {
      get
      {
        for (int index = 0; index < 32; ++index)
        {
          if (this.FBitGroups[index] != 0)
            return false;
        }
        return true;
      }
    }

    public bool Equals(IndexSet indexSet)
    {
      for (int index = 0; index < 32; ++index)
      {
        if (this.FBitGroups[index] != indexSet.FBitGroups[index])
          return false;
      }
      return true;
    }
  }
}
