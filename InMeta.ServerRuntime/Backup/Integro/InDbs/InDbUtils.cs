// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbUtils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Data;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public class InDbUtils
  {
    public static DbType DataTypeToDbType(DataType dt)
    {
      switch (dt)
      {
        case DataType.Boolean:
          return DbType.Boolean;
        case DataType.Integer:
          return DbType.Int32;
        case DataType.Float:
          return DbType.Double;
        case DataType.Currency:
          return DbType.Currency;
        case DataType.DateTime:
          return DbType.DateTime;
        case DataType.Date:
          return DbType.Date;
        case DataType.Time:
          return DbType.Time;
        case DataType.String:
          return DbType.AnsiString;
        case DataType.Memo:
          return DbType.String;
        case DataType.Binary:
          return DbType.Binary;
        case DataType.Picture:
          return DbType.Object;
        case DataType.Document:
          return DbType.Object;
        default:
          throw new InDbException(string.Format("Значение DataType.{0} не может быть преобразовано в DbType.", (object) dt));
      }
    }

    public static bool IsVariableLengthDbType(DbType dbType) => dbType == DbType.Binary | dbType == DbType.AnsiString | dbType == DbType.Object | dbType == DbType.String | dbType == DbType.Xml;

    public static bool IsVariableLengthDataType(DataType dataType) => dataType == DataType.Binary | dataType == DataType.Document | dataType == DataType.Memo | dataType == DataType.Picture | dataType == DataType.String;

    public static TypeCode DataTypeToTypeCode(DataType dt)
    {
      switch (dt)
      {
        case DataType.Boolean:
          return TypeCode.Boolean;
        case DataType.Integer:
          return TypeCode.Int32;
        case DataType.Float:
          return TypeCode.Double;
        case DataType.Currency:
          return TypeCode.Decimal;
        case DataType.DateTime:
          return TypeCode.DateTime;
        case DataType.Date:
          return TypeCode.DateTime;
        case DataType.Time:
          return TypeCode.DateTime;
        case DataType.String:
          return TypeCode.String;
        case DataType.Memo:
          return TypeCode.String;
        case DataType.Binary:
          return TypeCode.Object;
        case DataType.Picture:
          return TypeCode.Object;
        case DataType.Document:
          return TypeCode.Object;
        default:
          throw new InDbException(string.Format("Значение DataType.{0} не может быть преобразовано в TypeCode", (object) dt));
      }
    }

    public static DataType DbTypeToDataType(DbType dt)
    {
      switch (dt)
      {
        case DbType.AnsiString:
          return DataType.String;
        case DbType.Binary:
          return DataType.Binary;
        case DbType.Byte:
          return DataType.Integer;
        case DbType.Boolean:
          return DataType.Boolean;
        case DbType.Currency:
          return DataType.Currency;
        case DbType.Date:
          return DataType.Date;
        case DbType.DateTime:
          return DataType.DateTime;
        case DbType.Decimal:
          return DataType.Float;
        case DbType.Double:
          return DataType.Float;
        case DbType.Int16:
          return DataType.Integer;
        case DbType.Int32:
          return DataType.Integer;
        case DbType.Int64:
          return DataType.Integer;
        case DbType.SByte:
          return DataType.Integer;
        case DbType.Single:
          return DataType.Float;
        case DbType.String:
          return DataType.String;
        case DbType.Time:
          return DataType.Time;
        case DbType.UInt16:
          return DataType.Integer;
        case DbType.UInt32:
          return DataType.Integer;
        case DbType.UInt64:
          return DataType.Integer;
        case DbType.AnsiStringFixedLength:
          return DataType.String;
        case DbType.StringFixedLength:
          return DataType.String;
        default:
          throw new InDbException(string.Format("Значение DbType.{0} не может быть преобразовано в DataType.", (object) dt));
      }
    }

    public static DataType InMetaDataTypeToDataType(string inMetaDt)
    {
      switch (inMetaDt)
      {
        case "boolean":
          return DataType.Boolean;
        case "uint8":
          return DataType.Integer;
        case "int16":
          return DataType.Integer;
        case "int32":
          return DataType.Integer;
        case "single":
          return DataType.Float;
        case "double":
          return DataType.Float;
        case "currency":
          return DataType.Currency;
        case "char":
          return DataType.String;
        case "string":
          return DataType.String;
        case "text":
          return DataType.Memo;
        case "datetime":
          return DataType.DateTime;
        case "binary":
          return DataType.Binary;
        default:
          return DataType.Unknown;
      }
    }

    public static object Convert(object value, DataType targetType)
    {
      if (value == null)
        return (object) null;
      switch (targetType)
      {
        case DataType.Boolean:
          return (object) System.Convert.ToBoolean(value);
        case DataType.Integer:
          return (object) System.Convert.ToInt32(value);
        case DataType.Float:
          return (object) System.Convert.ToDouble(value);
        case DataType.Currency:
          return (object) System.Convert.ToDecimal(value);
        case DataType.DateTime:
        case DataType.Date:
        case DataType.Time:
          return (object) System.Convert.ToDateTime(value);
        case DataType.String:
        case DataType.Memo:
          return (object) System.Convert.ToString(value);
        case DataType.Binary:
        case DataType.Picture:
        case DataType.Document:
          return (object) (byte[]) value;
        default:
          throw new InDbException(string.Format("Значение {0} не может быть преобразовано к типу {1}", value, (object) targetType));
      }
    }

    public static DataType TypeCodeToDataType(TypeCode code)
    {
      switch (code)
      {
        case TypeCode.Empty:
        case TypeCode.Object:
        case TypeCode.DBNull:
        case TypeCode.Char:
        case TypeCode.String:
          return DataType.String;
        case TypeCode.Boolean:
          return DataType.Boolean;
        case TypeCode.SByte:
        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return DataType.Integer;
        case TypeCode.Single:
        case TypeCode.Double:
          return DataType.Float;
        case TypeCode.Decimal:
          return DataType.Currency;
        case TypeCode.DateTime:
          return DataType.DateTime;
        default:
          throw new InDbException(string.Format("Значение TypeCode.{0} не может быть преобразовано в DataType", (object) code));
      }
    }
  }
}
