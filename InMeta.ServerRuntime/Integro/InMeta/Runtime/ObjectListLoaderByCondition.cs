// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectListLoaderByCondition
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal class ObjectListLoaderByCondition : ObjectListLoader
  {
    private readonly string Condition;
    private readonly object[] FParamValues;
    private readonly DataType[] FParamTypes;

    internal ObjectListLoaderByCondition(
      ObjectLoader objectLoader,
      string condition,
      params object[] paramArray)
      : base(objectLoader)
    {
      this.Condition = condition ?? string.Empty;
      SqlUtils.ConvertObjectsToInDbParams(paramArray, out this.FParamTypes, out this.FParamValues);
    }

    internal override void Load(InDbDatabase db, DataObjectList dstObjs)
    {
      StringBuilder sql = new StringBuilder();
      bool onePass = true;
      if (this.Condition.Length == 0)
        sql.Append(this.SelectSql);
      else
        this.GenerateLoadSql(sql, ref onePass);
      if (onePass || this.ObjectLoader.Count == 0)
      {
        using (InDbCommand command = db.CreateCommand(sql.ToString(), this.FParamTypes))
        {
          using (IDataReader reader = command.ExecuteReader(this.FParamValues))
          {
            LoadContext loadContext = this.Condition == string.Empty ? LoadContext.FetchAllObjects : (LoadContext) 0;
            this.Load(reader, dstObjs, loadContext);
          }
        }
      }
      else
      {
        List<DataId> dataIdList = new List<DataId>();
        using (InDbCommand command = db.CreateCommand(sql.ToString(), this.FParamTypes))
        {
          using (IDataReader dataReader = command.ExecuteReader(this.FParamValues))
          {
            while (dataReader.Read())
              dataIdList.Add(new DataId(dataReader.GetString(0)));
          }
        }
        this.FStorage.Session.LoadData(this.ObjectLoader.BasePlan, dstObjs, dataIdList.ToArray(), (string) null);
      }
    }

    private void GenerateLoadSql(StringBuilder sql, ref bool onePass)
    {
      SqlUtils.Navigation navigation = new SqlUtils.Navigation(this.Class);
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(" WHERE ");
      int startIndex = 0;
      int nameStart = 0;
      for (int nameEnd = 0; SqlUtils.FindInMetaCompoundIdentifierInSQL(this.Condition, startIndex, ref nameStart, ref nameEnd); startIndex = nameEnd)
      {
        if (nameStart > startIndex)
          stringBuilder.Append(this.Condition, startIndex, nameStart - startIndex);
        string ident = this.Condition.Substring(nameStart, nameEnd - nameStart);
        if (ident.EndsWith("Property"))
          ident = ident.Substring(0, ident.Length - 8);
        SqlUtils.NavigationStep step;
        MetadataProperty prop;
        navigation.RegisterIdentifier(ident, out step, out prop);
        if (prop == null)
          stringBuilder.Append(this.Condition, nameStart, nameEnd - nameStart);
        else if (step != null)
          stringBuilder.AppendFormat("[{0}].[{1}]", (object) step.TargetAlias, (object) prop.DataField);
        else
          stringBuilder.AppendFormat("[{0}].[{1}]", (object) navigation.Alias, (object) prop.DataField);
      }
      if (startIndex < this.Condition.Length)
        stringBuilder.Append(this.Condition, startIndex, this.Condition.Length - startIndex);
      if (navigation.RootSteps.Count > 0)
      {
        navigation.AppendSqlTo(sql);
        sql.Append(stringBuilder.ToString());
        onePass = false;
      }
      else
      {
        sql.Append(this.SelectSql).AppendFormat(" [{0}]", (object) navigation.Alias);
        sql.Append(stringBuilder.ToString());
      }
    }
  }
}
