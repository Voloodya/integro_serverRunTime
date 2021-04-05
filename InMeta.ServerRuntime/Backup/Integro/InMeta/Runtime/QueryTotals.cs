// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.QueryTotals
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Collections;
using System.Data;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal class QueryTotals
  {
    private DataStorage FStorage;
    private StringBuilder FSql = new StringBuilder();
    private DataType[] FSqlParamTypes;
    private object[] FSqlParamValues;

    public QueryTotals(
      DataStorage storage,
      string columns,
      string condition,
      object[] args,
      string groupBy)
    {
      this.FStorage = storage;
      SqlUtils.Navigation navigation = new SqlUtils.Navigation(this.FStorage.Class);
      this.FSql.Append("SELECT ");
      navigation.ReplaceCompoundIdentifiers(columns, this.FSql);
      StringBuilder dest1 = new StringBuilder();
      navigation.ReplaceCompoundIdentifiers(condition, dest1);
      StringBuilder dest2 = new StringBuilder();
      navigation.ReplaceCompoundIdentifiers(groupBy, dest2);
      this.FSql.Append(' ');
      navigation.AppendFromSection(this.FSql);
      if (dest1.Length > 0)
      {
        this.FSql.Append(" WHERE ");
        this.FSql.Append(dest1.ToString());
      }
      if (dest2.Length > 0)
      {
        this.FSql.Append(" GROUP BY ");
        this.FSql.Append(dest2.ToString());
      }
      SqlUtils.ConvertObjectsToInDbParams(args, out this.FSqlParamTypes, out this.FSqlParamValues);
    }

    public Totals Execute()
    {
      ArrayList arrayList = new ArrayList();
      InDbCommand command = this.FStorage.Session.Db.CreateCommand(this.FSql.ToString(), this.FSqlParamTypes);
      IDataReader dataReader = command.ExecuteReader(this.FSqlParamValues);
      try
      {
        while (dataReader.Read())
        {
          object[] values = new object[dataReader.FieldCount];
          dataReader.GetValues(values);
          arrayList.Add((object) new Totals.Row(values));
        }
      }
      finally
      {
        dataReader.Dispose();
        command.Dispose();
      }
      return new Totals((Totals.Row[]) arrayList.ToArray(typeof (Totals.Row)));
    }
  }
}
