// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ModifiedObjectInfo
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;

namespace Integro.InMeta.Runtime
{
  public class ModifiedObjectInfo
  {
    public readonly string Id;
    public readonly Dictionary<MetadataProperty, object> Properties = new Dictionary<MetadataProperty, object>();

    internal ModifiedObjectInfo(string id) => this.Id = id;

    internal IDictionary ToLogJson(InDbDatabase dbForOriginalValues)
    {
      ListDictionary listDictionary = new ListDictionary();
      if (dbForOriginalValues == null)
      {
        foreach (KeyValuePair<MetadataProperty, object> property in this.Properties)
          listDictionary.Add((object) property.Key.Name, property.Value);
      }
      else
      {
        string originalValuesSelectSql = ModifiedObjectInfo.GetOriginalValuesSelectSql(this.Properties);
        using (InDbCommand command = dbForOriginalValues.CreateCommand(originalValuesSelectSql, DataType.String))
        {
          using (IDataReader dataReader = command.ExecuteReader((object) this.Id))
          {
            bool flag = dataReader.Read();
            foreach (KeyValuePair<MetadataProperty, object> property in this.Properties)
              listDictionary.Add((object) property.Key.Name, (object) new object[2]
              {
                property.Value,
                ModifiedObjectInfo.GetOriginalValue(property.Key, flag ? dataReader : (IDataReader) null)
              });
          }
        }
      }
      return (IDictionary) listDictionary;
    }

    private static string GetOriginalValuesSelectSql(Dictionary<MetadataProperty, object> properties)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str = (string) null;
      foreach (KeyValuePair<MetadataProperty, object> property in properties)
      {
        if (str == null)
        {
          str = string.Format(" FROM [{0}] WHERE [{1}]=?", (object) property.Key.Class.DataTable, (object) property.Key.Class.IDProperty.DataField);
          stringBuilder.AppendFormat("SELECT [{0}]", (object) property.Key.DataField);
        }
        else
          stringBuilder.AppendFormat(",[{0}]", (object) property.Key.DataField);
      }
      stringBuilder.Append(str);
      return stringBuilder.ToString();
    }

    private static object GetOriginalValue(
      MetadataProperty property,
      IDataReader originalValuesReader)
    {
      return originalValuesReader?.GetValue(originalValuesReader.GetOrdinal(property.DataField));
    }
  }
}
