// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbSqlCommand
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using InMeta.ServerRuntime.Runtime;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  internal class InDbSqlCommand : InDbCommand
  {
    private readonly SqlConnection FConnection;
    private readonly SqlTransaction FTransaction;
    private readonly int FCommandTimeout;
    private readonly string FSql;
    private readonly DataType[] FParamTypes;
    private readonly int[] FParamMaxSizes;
    private SqlCommand FCommand;

    internal InDbSqlCommand(
      SqlConnection connection,
      SqlTransaction transaction,
      int commandTimeout,
      string sql,
      params DataType[] paramTypes)
    {
      this.FConnection = connection;
      this.FTransaction = transaction;
      this.FCommandTimeout = commandTimeout;
      this.FSql = MsSqlPreprocessor.Translate(sql);
      if (paramTypes == null || paramTypes.Length <= 0)
        return;
      this.FParamTypes = paramTypes;
      this.FParamMaxSizes = new int[paramTypes.Length];
      for (int index = 0; index < paramTypes.Length; ++index)
      {
        if (InDbUtils.IsVariableLengthDataType(paramTypes[index]))
          this.FParamMaxSizes[index] = 1;
      }
    }

    private void RecreateCommand()
    {
      this.FCommand = this.FConnection.CreateCommand();
      this.FCommand.CommandTimeout = this.FCommandTimeout;
      if (this.FTransaction != null)
        this.FCommand.Transaction = this.FTransaction;
      this.FCommand.CommandText = this.FSql;
      if (this.FParamTypes != null)
      {
        for (int index = 0; index < this.FParamTypes.Length; ++index)
        {
          SqlParameter parameter = this.FCommand.CreateParameter();
          parameter.ParameterName = string.Format("@{0}", (object) (index + 1));
          parameter.DbType = InDbUtils.DataTypeToDbType(this.FParamTypes[index]);
          int fparamMaxSiz = this.FParamMaxSizes[index];
          if (fparamMaxSiz > 0)
            parameter.Size = fparamMaxSiz;
          this.FCommand.Parameters.Add(parameter);
        }
      }
      this.FCommand.Prepare();
    }

    private void FillParams(params object[] paramValues)
    {
      if (this.FParamTypes == null || paramValues == null || paramValues.Length == 0)
      {
        if (this.FCommand != null)
          return;
        this.RecreateCommand();
      }
      else
      {
        object[] objArray = new object[paramValues.Length];
        for (int index = 0; index < this.FParamTypes.Length; ++index)
        {
          DataType fparamType = this.FParamTypes[index];
          object paramValue = paramValues[index];
          if (paramValue == null || paramValue == DBNull.Value)
          {
            objArray[index] = (object) DBNull.Value;
          }
          else
          {
            object obj = InDbUtils.Convert(paramValue, fparamType);
            objArray[index] = obj;
            int fparamMaxSiz = this.FParamMaxSizes[index];
            if (fparamMaxSiz > 0)
            {
              int num = obj is string ? ((string) obj).Length : ((byte[]) obj).Length;
              if (num > fparamMaxSiz)
              {
                if (num > 4000 && num <= 8000)
                  num = 8001;
                this.FParamMaxSizes[index] = num;
                if (this.FCommand != null)
                {
                  this.FCommand.Dispose();
                  this.FCommand = (SqlCommand) null;
                }
              }
            }
          }
        }
        if (this.FCommand == null)
          this.RecreateCommand();
        for (int index = 0; index < objArray.Length; ++index)
          this.FCommand.Parameters[index].Value = objArray[index];
      }
    }

    private Exception ExecuteError(Exception innerException) => (Exception) new InDbException(string.Format("Ошибка выполнения SQL-запроса:\n{0}", (object) this.FCommand.CommandText), innerException);

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.FCommand != null)
        this.FCommand.Dispose();
      this.FCommand = (SqlCommand) null;
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
