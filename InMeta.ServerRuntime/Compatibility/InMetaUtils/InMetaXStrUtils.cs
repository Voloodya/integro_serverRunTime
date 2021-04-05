// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaUtils.InMetaXStrUtils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System;
using System.Runtime.InteropServices;

namespace Compatibility.InMetaUtils
{
  [ComVisible(true)]
  public class InMetaXStrUtils
  {
    public static readonly InMetaXStrUtils Instance = new InMetaXStrUtils();

    public string XStrNULL => "{{null}}";

    public string VariantToXStr(object value)
    {
      if (value == null)
        return this.XStrNULL;
      switch (Type.GetTypeCode(value.GetType()))
      {
        case TypeCode.Empty:
        case TypeCode.DBNull:
          return this.XStrNULL;
        case TypeCode.Boolean:
          return this.BoolToXStr((object) (bool) value);
        case TypeCode.SByte:
        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return this.LngToXStr((object) Convert.ToInt32(value));
        case TypeCode.Single:
        case TypeCode.Double:
          return this.DblToXStr((object) Convert.ToDouble(value));
        case TypeCode.Decimal:
          return this.CurToXStr((object) Convert.ToDecimal(value));
        case TypeCode.DateTime:
          return this.DateTimeToXStr((object) Convert.ToDateTime(value));
        default:
          return this.StrToXStr((object) value.ToString());
      }
    }

    public object XStrToVariant(string value, string dataType)
    {
      switch (dataType.ToLowerInvariant())
      {
        case "boolean":
          return this.XStrToBool(value);
        case "uint8":
        case "int16":
        case "int32":
          return this.XStrToLng(value);
        case "double":
        case "single":
          return this.XStrToDbl(value);
        case "currency":
          return this.XStrToCur(value);
        case "char":
        case "string":
        case "text":
          return this.XStrToStr(value);
        case "datetime":
          return this.XStrToDateTime(value);
        case "binary":
          throw new InMetaException("Конвертирование типа \"binary\" не реализовано");
        default:
          throw new InMetaException("Невозможно преобразовать строку в формате \"XStr\". Неизвестный тип данных \"" + dataType + "\"");
      }
    }

    public object XStrToVariantDef(string value, string dataType, object defValue)
    {
      try
      {
        return this.XStrToVariant(value, dataType);
      }
      catch
      {
        return defValue;
      }
    }

    private static bool TreatAsNull(object value)
    {
      if (value == null)
        return true;
      TypeCode typeCode = Type.GetTypeCode(value.GetType());
      return typeCode == TypeCode.DBNull || typeCode == TypeCode.Empty;
    }

    public string StrToXStr(object str) => InMetaXStrUtils.TreatAsNull(str) ? this.XStrNULL : str.ToString();

    public object XStrToStr(string xStr) => xStr == this.XStrNULL ? (object) DBNull.Value : (object) xStr;

    public string BoolToXStr(object value) => InMetaXStrUtils.TreatAsNull(value) ? this.XStrNULL : XStrUtils.ToXStr(Convert.ToBoolean(value));

    public object XStrToBool(string xStr) => xStr == this.XStrNULL ? (object) DBNull.Value : (object) XStrUtils.ToBool(xStr, false);

    public string DateTimeToXStr(object value) => InMetaXStrUtils.TreatAsNull(value) ? this.XStrNULL : XStrUtils.ToXStr(Convert.ToDateTime(value));

    public object XStrToDateTime(string xStr) => xStr == this.XStrNULL ? (object) DBNull.Value : (object) XStrUtils.ToDateTime(xStr, DateTime.Now);

    public string LngToXStr(object value) => InMetaXStrUtils.TreatAsNull(value) ? this.XStrNULL : XStrUtils.ToXStr(Convert.ToInt32(value));

    public object XStrToLng(string xStr) => xStr == this.XStrNULL ? (object) DBNull.Value : (object) XStrUtils.ToInt(xStr, 0);

    public string DblToXStr(object value) => InMetaXStrUtils.TreatAsNull(value) ? this.XStrNULL : XStrUtils.ToXStr(Convert.ToDouble(value));

    public object XStrToDbl(string xStr) => xStr == this.XStrNULL ? (object) DBNull.Value : (object) XStrUtils.ToDouble(xStr, 0.0);

    public string CurToXStr(object value) => InMetaXStrUtils.TreatAsNull(value) ? this.XStrNULL : XStrUtils.ToXStr(Convert.ToDecimal(value));

    public object XStrToCur(string xStr) => xStr == this.XStrNULL ? (object) DBNull.Value : (object) XStrUtils.ToCurrency(xStr, 0M);
  }
}
