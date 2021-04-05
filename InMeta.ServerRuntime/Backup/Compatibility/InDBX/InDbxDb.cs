// Decompiled with JetBrains decompiler
// Type: Compatibility.InDBX.InDbxDb
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System;

namespace Compatibility.InDBX
{
  public class InDbxDb
  {
    private readonly InDbDatabase FDb;

    internal InDbxDb(InDbDatabase db) => this.FDb = db;

    public object Execute2(string query, object paramTypes, object paramValues)
    {
      object[] paramTypes1 = (object[]) paramTypes;
      object[] objArray = (object[]) paramValues;
      using (InDbCommand command = this.FDb.CreateCommand(query, InDbxDb.ParamTypeArrayToDataTypeArray(paramTypes1)))
        return (object) new InDbxCursor(command.ExecuteReader(objArray));
    }

    private static DataType[] ParamTypeArrayToDataTypeArray(object[] paramTypes)
    {
      if (paramTypes == null)
        return (DataType[]) null;
      DataType[] dataTypeArray = new DataType[paramTypes.Length];
      for (int index = 0; index < paramTypes.Length; ++index)
        dataTypeArray[index] = InDbxDb.ParamTypeToDataType(paramTypes[index]);
      return dataTypeArray;
    }

    private static DataType ParamTypeToDataType(object paramType)
    {
      switch ((InDbxDb.ParamType) (Convert.ToInt32(paramType) & 65280))
      {
        case InDbxDb.ParamType.Boolean:
          return DataType.Boolean;
        case InDbxDb.ParamType.Integer:
          return DataType.Integer;
        case InDbxDb.ParamType.Float:
          return DataType.Float;
        case InDbxDb.ParamType.Currency:
          return DataType.Currency;
        case InDbxDb.ParamType.DateTime:
          return DataType.DateTime;
        case InDbxDb.ParamType.String:
          return DataType.String;
        case InDbxDb.ParamType.Memo:
          return DataType.Memo;
        case InDbxDb.ParamType.Binary:
          return DataType.Binary;
        default:
          throw new Exception("Некорректный тип параметра: " + paramType);
      }
    }

    private enum ParamType
    {
      Boolean = 256, // 0x00000100
      Integer = 512, // 0x00000200
      Float = 768, // 0x00000300
      Currency = 1024, // 0x00000400
      DateTime = 1280, // 0x00000500
      String = 1536, // 0x00000600
      Memo = 1792, // 0x00000700
      Binary = 2048, // 0x00000800
      BaseMask = 65280, // 0x0000FF00
    }
  }
}
