// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbOracleCommand
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using InMeta.ServerRuntime.Runtime;
using System;
using System.Data;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  internal class InDbOracleCommand : InDbCommand
  {
    private OracleCommand FCommand;
    private readonly DataType[] FParamTypes;

    private void FillParams(params object[] paramValues)
    {
      if (paramValues == null || paramValues.Length <= 0)
        return;
      for (int index = 0; index < this.FParamTypes.Length; ++index)
      {
        IDataParameter parameter = (IDataParameter) this.FCommand.Parameters[index];
        DataType fparamType = this.FParamTypes[index];
        object paramValue = paramValues[index];
        if (paramValue == null || paramValue == DBNull.Value)
        {
          parameter.Value = (object) DBNull.Value;
        }
        else
        {
          object obj = InDbUtils.Convert(paramValues[index], fparamType);
          parameter.Value = obj;
          if (obj != null && (parameter.DbType == DbType.AnsiString || parameter.DbType == DbType.String || parameter.DbType == DbType.Binary))
          {
            OracleParameter oracleParameter = (OracleParameter) parameter;
            if (fparamType == DataType.Memo)
              oracleParameter.OracleType = OracleType.Clob;
            oracleParameter.Size = ((string) obj).Length + 1;
          }
        }
      }
    }

    private Exception ExecuteError(Exception innerException) => (Exception) new InDbException(string.Format("Ошибка выполнения SQL-запроса:\n{0}", (object) this.FCommand.CommandText), innerException);

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.FCommand != null)
        this.FCommand.Dispose();
      this.FCommand = (OracleCommand) null;
    }

    internal InDbOracleCommand(
      OracleConnection connection,
      OracleTransaction transaction,
      string sql,
      params DataType[] paramTypes)
    {
      this.FCommand = connection.CreateCommand();
      if (transaction != null)
        this.FCommand.Transaction = transaction;
      sql = OracleSqlPreprocessor.Translate(sql);
      this.FCommand.CommandText = sql;
      this.FParamTypes = paramTypes;
      if (paramTypes == null)
        return;
      for (int index = 0; index < paramTypes.Length; ++index)
      {
        IDbDataParameter parameter = (IDbDataParameter) this.FCommand.CreateParameter();
        parameter.ParameterName = string.Format("{0}", (object) (index + 1));
        parameter.DbType = InDbUtils.DataTypeToDbType(paramTypes[index]);
        this.FCommand.Parameters.Add((object) parameter);
      }
    }

    public override IDataReader ExecuteReader(params object[] paramValues)
    {
      this.FillParams(paramValues);
      try
      {
        return (IDataReader) this.FCommand.ExecuteReader();
      }
      catch (Exception ex)
      {
        throw this.ExecuteError(ex);
      }
    }

    public override int Execute(params object[] paramValues)
    {
      this.FillParams(paramValues);
      try
      {
        return this.FCommand.ExecuteNonQuery();
      }
      catch (Exception ex)
      {
        throw this.ExecuteError(ex);
      }
    }
  }
}
