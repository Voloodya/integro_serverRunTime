// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.Totals
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class Totals
  {
    private Totals.Row[] FRows;

    internal Totals(Totals.Row[] rows) => this.FRows = rows;

    public int Count => this.FRows.Length;

    public Totals.Row this[int index] => this.FRows[index];

    public class Row
    {
      private object[] FValues;

      internal Row(object[] values) => this.FValues = values;

      public int Count => this.FValues.Length;

      public object this[int index] => this.FValues[index];

      public bool IsNull(int index)
      {
        object fvalue = this.FValues[index];
        return fvalue == null || fvalue == DBNull.Value;
      }

      public string AsString(int index, string defaultValue) => !this.IsNull(index) ? Convert.ToString(this.FValues[index]) : defaultValue;

      public string AsString(int index) => this.AsString(index, string.Empty);

      public int AsInteger(int index, int defaultValue) => !this.IsNull(index) ? Convert.ToInt32(this.FValues[index]) : defaultValue;

      public int AsInteger(int index) => this.AsInteger(index, 0);

      public double AsDouble(int index, double defaultValue) => !this.IsNull(index) ? Convert.ToDouble(this.FValues[index]) : defaultValue;

      public double AsDouble(int index) => this.AsDouble(index, 0.0);

      public DateTime AsDateTime(int index, DateTime defaultValue) => !this.IsNull(index) ? Convert.ToDateTime(this.FValues[index]) : defaultValue;

      public DateTime AsDateTime(int index) => this.AsDateTime(index, DateTime.MinValue);

      public Decimal AsDecimal(int index, Decimal defaultValue) => !this.IsNull(index) ? Convert.ToDecimal(this.FValues[index]) : defaultValue;

      public Decimal AsDecimal(int index) => this.AsDecimal(index, 0M);

      public bool AsBoolean(int index, bool defaultValue) => !this.IsNull(index) ? Convert.ToBoolean(this.FValues[index]) : defaultValue;

      public bool AsBoolean(int index) => this.AsBoolean(index, false);
    }
  }
}
