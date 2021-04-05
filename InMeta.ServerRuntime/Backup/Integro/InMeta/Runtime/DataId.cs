// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataId
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataId : IComparable
  {
    private const string HexChars = "0123456789ABCDEF";
    private readonly byte[] FValue;
    private static readonly char[] FToStringBuf = new char[12];
    public static readonly DataId Empty = new DataId();

    private DataId()
    {
    }

    public DataId(string s)
    {
      if (string.IsNullOrEmpty(s))
      {
        this.FValue = (byte[]) null;
      }
      else
      {
        this.FValue = new byte[12];
        bool flag = true;
        for (int index = 0; index < 12; ++index)
        {
          byte num = index < s.Length ? (byte) s[index] : (byte) 32;
          if (num != (byte) 32)
            flag = false;
          this.FValue[index] = num;
        }
        if (flag)
          this.FValue = (byte[]) null;
      }
    }

    public DataId(DataId id) => this.FValue = id.FValue;

    public DataId(int groupLocalId, int number)
    {
      long num = ((long) groupLocalId << 32) + (long) (uint) number;
      if (num == 0L)
        return;
      this.FValue = new byte[12];
      this.FValue[0] = (byte) "0123456789ABCDEF"[(int) (num >> 44 & 15L)];
      this.FValue[1] = (byte) "0123456789ABCDEF"[(int) (num >> 40 & 15L)];
      this.FValue[2] = (byte) "0123456789ABCDEF"[(int) (num >> 36 & 15L)];
      this.FValue[3] = (byte) "0123456789ABCDEF"[(int) (num >> 32 & 15L)];
      this.FValue[4] = (byte) "0123456789ABCDEF"[(int) (num >> 28 & 15L)];
      this.FValue[5] = (byte) "0123456789ABCDEF"[(int) (num >> 24 & 15L)];
      this.FValue[6] = (byte) "0123456789ABCDEF"[(int) (num >> 20 & 15L)];
      this.FValue[7] = (byte) "0123456789ABCDEF"[(int) (num >> 16 & 15L)];
      this.FValue[8] = (byte) "0123456789ABCDEF"[(int) (num >> 12 & 15L)];
      this.FValue[9] = (byte) "0123456789ABCDEF"[(int) (num >> 8 & 15L)];
      this.FValue[10] = (byte) "0123456789ABCDEF"[(int) (num >> 4 & 15L)];
      this.FValue[11] = (byte) "0123456789ABCDEF"[(int) (num & 15L)];
    }

    public override int GetHashCode()
    {
      if (this.FValue == null)
        return 0;
      int num = (int) this.FValue[0];
      for (int index = 1; index < this.FValue.Length; ++index)
        num += num << 4 ^ (int) this.FValue[index];
      return num;
    }

    public override string ToString()
    {
      if (this.FValue == null)
        return string.Empty;
      lock (DataId.FToStringBuf)
      {
        for (int index = 0; index < 12; ++index)
          DataId.FToStringBuf[index] = (char) this.FValue[index];
        return new string(DataId.FToStringBuf);
      }
    }

    public bool IsEmpty => this.FValue == null;

    public int CompareTo(DataId id) => DataId.Compare(this, id);

    public override bool Equals(object obj) => this.CompareTo(obj) == 0;

    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      DataId id2 = obj as DataId;
      return (object) id2 != null ? DataId.Compare(this, id2) : throw new ArgumentException(string.Format("Сравниваемый объект {0} не является идентификатором.", (object) obj.GetType()));
    }

    public static DataId[] Split(string list, char separator)
    {
      if (list == null || list.Trim().Length == 0)
        return new DataId[0];
      string[] strArray = list.Split(separator);
      DataId[] dataIdArray = new DataId[strArray.Length];
      for (int index = 0; index < strArray.Length; ++index)
        dataIdArray[index] = new DataId(strArray[index].Trim());
      return dataIdArray;
    }

    public static string Join(string separator, DataId[] ids)
    {
      if (ids.Length == 0)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder(ids[0].ToString());
      for (int index = 1; index < ids.Length; ++index)
        stringBuilder.Append(separator).Append(ids[index].ToString());
      return stringBuilder.ToString();
    }

    public static string Join(string separator, IDataObjectList objs)
    {
      if (objs.Count == 0)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder(objs[0].Id.ToString());
      for (int index = 1; index < objs.Count; ++index)
        stringBuilder.Append(separator).Append(objs[index].Id.ToString());
      return stringBuilder.ToString();
    }

    public static int Compare(DataId id1, DataId id2)
    {
      if ((object) id1 == null)
        return (object) id2 == null ? 0 : -1;
      if ((object) id2 == null)
        return 1;
      byte[] fvalue1 = id1.FValue;
      byte[] fvalue2 = id2.FValue;
      if (fvalue1 == fvalue2)
        return 0;
      if (fvalue1 == null)
        return fvalue2 == null ? 0 : -1;
      if (fvalue2 == null)
        return 1;
      for (int index = 0; index < 12; ++index)
      {
        byte num1 = fvalue1[index];
        byte num2 = fvalue2[index];
        if ((int) num1 < (int) num2)
          return -1;
        if ((int) num1 > (int) num2)
          return 1;
      }
      return 0;
    }

    public static bool operator ==(DataId id1, DataId id2) => DataId.Compare(id1, id2) == 0;

    public static bool operator !=(DataId id1, DataId id2) => DataId.Compare(id1, id2) != 0;

    public bool TryExtractParts(out int localGroupId, out int number)
    {
      number = 0;
      return this.TryExtractPart(0, 4, out localGroupId) && this.TryExtractPart(4, 8, out number);
    }

    private bool TryExtractPart(int partStart, int partSize, out int partValue)
    {
      partValue = 0;
      if (this.FValue == null)
        return false;
      long num1 = 0;
      while (partSize-- > 0)
      {
        long num2 = num1 << 4;
        char ch = (char) this.FValue[partStart++];
        if (ch >= '0' && ch <= '9')
          num1 = num2 | (long) ch - 48L;
        else if (ch >= 'a' && ch <= 'f')
        {
          num1 = num2 | (long) ch - 97L + 10L;
        }
        else
        {
          if (ch < 'A' || ch > 'F')
            return false;
          num1 = num2 | (long) ch - 65L + 10L;
        }
      }
      partValue = (int) num1;
      return true;
    }
  }
}
