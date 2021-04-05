// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbSqlDatabase
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  internal class InDbSqlDatabase : InDbDatabase
  {
    private readonly string FConnectionString;
    private SqlConnection FConnection;
    private SqlTransaction FTransaction;
    private string FSchemaName = string.Empty;
    private readonly int FCommandTimeout;

    private static string InDbFieldDefToSqlDataType(InDbFieldDef fieldDef)
    {
      switch (fieldDef.DataType & DataType.BaseMask)
      {
        case DataType.Boolean:
          return "bit";
        case DataType.Integer:
          return "Int";
        case DataType.Float:
          return "Float";
        case DataType.Currency:
          return "Money";
        case DataType.DateTime:
          return "datetime";
        case DataType.String:
          return string.Format("varchar({0})", (object) fieldDef.Size);
        case DataType.Memo:
          return "text";
        case DataType.Binary:
          return "image";
        default:
          throw new InDbException("Тип не поддерживается");
      }
    }

    private string GetSchemaName()
    {
      if (this.FSchemaName == string.Empty)
      {
        SqlCommand command = this.Connection.CreateCommand();
        command.CommandText = string.Format("SELECT CURRENT_USER");
        using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
        {
          dataReader.Read();
          this.FSchemaName = dataReader.GetString(0);
        }
      }
      return this.FSchemaName;
    }

    private void LoadFieldDefs(InDbTableDef tableDef)
    {
      SqlCommand command = this.Connection.CreateCommand();
      command.CommandText = string.Format("SELECT\tCOLUMN_NAME, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH,  DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE  FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = '{1}'  ORDER BY ORDINAL_POSITION", (object) tableDef.Name, (object) this.GetSchemaName());
      using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
      {
        object[] values = new object[6];
        while (dataReader.Read())
        {
          dataReader.GetValues(values);
          int size = 0;
          string name = values[0].ToString();
          DataType inDbDataType = InDbSqlDatabase.SqlDataTypeToInDbDataType(values[3].ToString(), values[5] == DBNull.Value ? 0 : Convert.ToInt32(values[5]));
          if (inDbDataType == DataType.String)
            size = values[2] == DBNull.Value ? 0 : Convert.ToInt32(values[2]);
          tableDef.FieldDefs.Add(new InDbFieldDef(name, inDbDataType, size, false));
        }
      }
    }

    private void LoadIndexDefs(InDbTableDef tableDef)
    {
      SqlCommand command = this.Connection.CreateCommand();
      command.CommandText = string.Format("EXEC sp_indexes_rowset @table_name = '{0}', @table_schema = '{1}'", (object) tableDef.Name, (object) this.GetSchemaName());
      Hashtable hashtable = new Hashtable();
      string str1 = string.Empty;
      using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
      {
        while (dataReader.Read())
        {
          string str2 = dataReader.GetString(dataReader.GetOrdinal("INDEX_NAME"));
          string str3 = dataReader.GetString(dataReader.GetOrdinal("COLUMN_NAME"));
          int index = dataReader.GetInt32(dataReader.GetOrdinal("ORDINAL_POSITION")) - 1;
          if (Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("PRIMARY_KEY"))) != 0)
            str1 = str2;
          StringCollection stringCollection = (StringCollection) hashtable[(object) str2];
          if (stringCollection == null)
          {
            stringCollection = new StringCollection();
            hashtable.Add((object) str2, (object) stringCollection);
          }
          while (index > stringCollection.Count - 1)
            stringCollection.Add(string.Empty);
          if (stringCollection[index] == str3)
            throw new InDbException(string.Format("Столбец {0} входит более одного раза в индекс {1}.", (object) str3, (object) str2));
          stringCollection[index] = str3;
        }
      }
      foreach (string key in (IEnumerable) hashtable.Keys)
      {
        StringCollection stringCollection = (StringCollection) hashtable[(object) key];
        string[] array = new string[stringCollection.Count];
        stringCollection.CopyTo(array, 0);
        InDbFieldDefs fieldDefs = new InDbFieldDefs((InDbDatabase) this);
        foreach (string name in stringCollection)
          fieldDefs.Add(tableDef.FieldDefs[name]);
        InDbIndexDef indexDef = new InDbIndexDef(key, fieldDefs);
        tableDef.IndexDefs.Add(indexDef);
        if (key == str1)
        {
          tableDef.OriginalPrimaryKey = indexDef;
          tableDef.PrimaryKey = indexDef;
        }
      }
    }

    private string GetPrimaryIndexName(InDbTableDef tableDef)
    {
      SqlCommand command = this.Connection.CreateCommand();
      command.CommandText = string.Format("EXEC sp_indexes_rowset @table_name = '{0}', @table_schema = '{1}'", (object) tableDef.Name, (object) this.GetSchemaName());
      using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
      {
        while (dataReader.Read())
        {
          string str = dataReader.GetString(dataReader.GetOrdinal("INDEX_NAME"));
          if (Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("PRIMARY_KEY"))) != 0)
            return str;
        }
      }
      return string.Empty;
    }

    private void CreateIndex(string tableName, InDbIndexDef indexDef)
    {
      string str = StrUtils.Join((IList) indexDef.FieldDefs.GetFieldNames(), "[{0}]", ",");
      this.Execute(string.Format("CREATE INDEX [{0}] ON {1}.[{2}] ({3})", (object) indexDef.Name, (object) this.GetSchemaName(), (object) tableName, (object) str));
    }

    private string GetTempTableName(string tableName)
    {
      string tableName1 = tableName + "_tmp";
      int num = 0;
      while (this.TableExists(tableName1))
      {
        tableName1 = tableName + "_tmp" + (object) num;
        ++num;
      }
      return tableName1;
    }

    private void RenameTable(string oldName, string newName)
    {
      if (!(oldName != newName))
        return;
      this.Execute(string.Format("EXECUTE sp_rename '{0}.[{1}]', '{2}'", (object) this.GetSchemaName(), (object) oldName, (object) newName));
    }

    private void RestructureTable(InDbTableDef tableDef)
    {
      InDbTableDef tableDef1 = new InDbTableDef((InDbDatabase) this, this.GetTempTableName(tableDef.Name), true);
      foreach (InDbFieldDef fieldDef in tableDef.FieldDefs)
        tableDef1.FieldDefs.Add(fieldDef);
      foreach (InDbIndexDef indexDef in tableDef.IndexDefs)
        tableDef1.IndexDefs.Add(indexDef);
      tableDef1.PrimaryKey = tableDef.PrimaryKey;
      this.CreateTable(tableDef1);
      try
      {
        StringCollection stringCollection1 = new StringCollection();
        StringCollection stringCollection2 = new StringCollection();
        foreach (InDbFieldDef fieldDef in tableDef.FieldDefs)
        {
          if (!StrUtils.IsNullOrEmpty(fieldDef.OriginalName))
          {
            stringCollection1.Add(fieldDef.OriginalName);
            stringCollection2.Add(fieldDef.Name);
          }
        }
        this.Execute(string.Format("INSERT INTO {0}.[{1}] ({2}) SELECT {3} FROM {0}.[{4}] A", (object) this.GetSchemaName(), (object) tableDef1.Name, (object) StrUtils.Join((IList) stringCollection2, "[{0}]", ", "), (object) StrUtils.Join((IList) stringCollection1, "A.[{0}]", ", "), (object) tableDef.OriginalName));
        if (!this.SameIdentifiers(tableDef.OriginalName, tableDef.Name) && this.TableExists(tableDef.Name))
          throw new InDbException(string.Format("Ошибка модификации таблицы {0}->{1}: таблица {1} уже существует.", (object) tableDef.OriginalName, (object) tableDef.Name));
        this.DeleteTable(tableDef.OriginalName);
      }
      catch (Exception ex)
      {
        this.DeleteTable(tableDef1.Name);
        throw;
      }
      this.RenameTable(tableDef1.Name, tableDef.Name);
    }

    private void UpdateTableStructure(InDbTableDef tableDef)
    {
      if (!tableDef.Modified)
        return;
      this.RestructureTable(tableDef);
    }

    private static DataType SqlDataTypeToInDbDataType(string dataType, int scale)
    {
      switch (dataType.ToLower())
      {
        case "decimal":
        case "numeric":
          return scale == 0 ? DataType.Integer : DataType.Float;
        case "bigint":
        case "int":
        case "smallint":
        case "tinyint":
          return DataType.Integer;
        case "bit":
          return DataType.Boolean;
        case "money":
        case "smallmoney":
          return DataType.Currency;
        case "float":
        case "real":
          return DataType.Float;
        case "datetime":
        case "smalldatetime":
          return DataType.DateTime;
        case "char":
        case "varchar":
        case "nchar":
        case "nvarchar":
          return DataType.String;
        case "text":
        case "ntext":
          return DataType.Memo;
        case "binary":
        case "varbinary":
        case "image":
          return DataType.Binary;
        default:
          return DataType.Unknown;
      }
    }

    protected void CreateTable(InDbTableDef tableDef)
    {
      StringCollection stringCollection = new StringCollection();
      foreach (InDbFieldDef fieldDef in tableDef.FieldDefs)
        stringCollection.Add(string.Format("[{0}] {1}", (object) fieldDef.Name, (object) InDbSqlDatabase.InDbFieldDefToSqlDataType(fieldDef)));
      string str = string.Format("CREATE TABLE {0}.[{1}] ({2}", (object) this.GetSchemaName(), (object) tableDef.Name, (object) StrUtils.Join((IList) stringCollection, "{0}", ","));
      if (tableDef.PrimaryKey != null)
        str = str + ", PRIMARY KEY (" + StrUtils.Join((IList) tableDef.PrimaryKey.FieldDefs.GetFieldNames(), "[{0}]", ",") + ")";
      this.Execute(str + ")");
      try
      {
        for (int index = 0; index < tableDef.IndexDefs.Count; ++index)
        {
          InDbIndexDef indexDef = tableDef.IndexDefs[index];
          if (indexDef != tableDef.PrimaryKey)
          {
            indexDef.FName = "Index" + (object) index;
            this.CreateIndex(tableDef.Name, indexDef);
          }
        }
      }
      catch (Exception ex)
      {
        this.DeleteTable(tableDef.Name);
        throw;
      }
    }

    protected override void LoadTableDef(InDbTableDef tableDef)
    {
      this.LoadFieldDefs(tableDef);
      this.LoadIndexDefs(tableDef);
    }

    protected internal override void ApplyTableStructure(InDbTableDef tableDef)
    {
      if (StrUtils.IsNullOrEmpty(tableDef.OriginalName))
        this.CreateTable(tableDef);
      else
        this.UpdateTableStructure(tableDef);
      if (tableDef.PrimaryKey == null)
        return;
      tableDef.PrimaryKey.FName = this.GetPrimaryIndexName(tableDef);
    }

    private SqlConnection Connection
    {
      get
      {
        if (this.FConnection == null)
        {
          SqlConnection sqlConnection = new SqlConnection(this.FConnectionString);
          sqlConnection.Open();
          this.FConnection = sqlConnection;
        }
        return this.FConnection;
      }
    }

    internal InDbSqlDatabase(Hashtable parameters)
      : base(new DateTime(1754, 1, 1), new DateTime(9999, 1, 1))
    {
      this.FConnectionString = InDbManager.BuildConnectionString(parameters, InDbManager.SqlParams);
      object parameter = parameters[(object) "command-timeout"];
      this.FCommandTimeout = parameter != null ? Convert.ToInt32(parameter) : 0;
    }

    public override InDbCommand CreateCommand(string sql, params DataType[] paramTypes) => (InDbCommand) new InDbSqlCommand(this.Connection, this.FTransaction, this.FCommandTimeout, sql, paramTypes);

    public override void BeginTransaction() => this.FTransaction = this.Connection.BeginTransaction();

    public override void CommitTransaction()
    {
      this.FTransaction.Commit();
      this.FTransaction.Dispose();
      this.FTransaction = (SqlTransaction) null;
    }

    public override void RollbackTransaction()
    {
      this.FTransaction.Rollback();
      this.FTransaction.Dispose();
      this.FTransaction = (SqlTransaction) null;
    }

    public override bool TableExists(string tableName)
    {
      SqlCommand command = this.Connection.CreateCommand();
      command.CommandText = string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = '{1}'", (object) tableName, (object) this.GetSchemaName());
      using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
        return dataReader.Read();
    }

    public override string[] GetTableNames()
    {
      SqlCommand command = this.Connection.CreateCommand();
      command.CommandText = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = '{0}'", (object) this.GetSchemaName());
      StringCollection stringCollection = new StringCollection();
      using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
      {
        while (dataReader.Read())
          stringCollection.Add(dataReader.GetString(0));
      }
      string[] array = new string[stringCollection.Count];
      stringCollection.CopyTo(array, 0);
      return array;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.FConnection != null)
        this.FConnection.Dispose();
      this.FConnection = (SqlConnection) null;
    }
  }
}
