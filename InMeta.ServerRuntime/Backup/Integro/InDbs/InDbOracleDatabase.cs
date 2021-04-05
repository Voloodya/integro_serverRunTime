// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbOracleDatabase
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  internal class InDbOracleDatabase : InDbDatabase
  {
    private OracleConnection FConnection;
    private OracleTransaction FTransaction;
    private string FSchemaName = string.Empty;
    private static readonly List<string> FReservedWords = InDbOracleDatabase.CreateSortedWordList(new char[3]
    {
      ' ',
      '\r',
      '\n'
    }, "\r\nADD AFTER AGGREGATE ALIAS ALL ALLOCATE ALTER AND ANY APPEND ARE ARRAY AS ASC ASCII AT\r\nBACKUP BEFORE BETWEEN BIGINT BINARY BIT BLOB BLOCK BOOLEAN BOTH BROWSE BULK BY\r\nCASE CAST CHANGE CHAR CLASS CLOB CHR CLEAR CLUSTERED COLUMN COMMIT CONSTRAINT CONTAINS CONVERT CREATE CROSS\r\nDATABASE DEC DECIMAL DEFAULT DELETE DESC DISTINCT DOUBLE DOWN DROP\r\nELSE END EQUALS ERASE ESCAPE EVERY EXCEPT EXCLUDE EXCLUDING EXCLUSIVE EXISTS EXPLAIN EXPLICIT EXTENT EXTERNAL EXTRACT\r\nFALSE FETCH FIELD FIRST FLOAT FLUSH FOR FOREIGN FOUND FROM FREE FULL FUNCTION\r\nGENERAL GET GRANT GREATEST GROUP GROUPING\r\nHAVING\r\nIDENTITY IGNORE IF IMMEDIATE IN INDEX INITIAL INNER INSENSITIVE INSERT INT INTEGER INTERSECT INTERVAL INTO IS\r\nJOIN\r\nKEY\r\nLABEL LARGE LAST LEADING LEAST LEFT LESS LEVEL LIKE LIMIT LINK LONG LONGINT LOWER LVARBINARY LVARCHAR\r\nMOD \r\nNCHAR NCLOB NOT NULL\r\nOF OFF ON ONCE ONLY OPTION OR ORDER OUTER OVER\r\nPRECISION PRIMARY PRIOR \r\nREAL RIGHT ROLLBACK\r\nSELECT SET SHORT SMALLINT SORT\r\nTABLE THAN THEN TINYINT TO TOP TRANSACTION TRUE TYPE\r\nUNION UNIQUE UNTIL UPDATE USE\r\nVALUE VARBINARY VARCHAR VIEW\r\nWHEN WHERE WHILE WITH\r\n");

    private string GetSchemaName()
    {
      if (this.FSchemaName == string.Empty)
      {
        OracleCommand command = this.FConnection.CreateCommand();
        command.CommandText = string.Format("SELECT CURRENT_USER");
        using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
        {
          dataReader.Read();
          this.FSchemaName = dataReader.GetString(0);
        }
      }
      return this.FSchemaName;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.FConnection != null)
        this.FConnection.Dispose();
      this.FConnection = (OracleConnection) null;
    }

    public override string[] GetTableNames() => new string[0];

    protected void CreateTable(InDbTableDef tableDef)
    {
    }

    internal InDbOracleDatabase(OracleConnection connection) => this.FConnection = connection;

    public override InDbCommand CreateCommand(string sql, params DataType[] paramTypes) => (InDbCommand) new InDbOracleCommand(this.FConnection, this.FTransaction, sql, paramTypes);

    public override void BeginTransaction() => this.FTransaction = this.FConnection.BeginTransaction();

    public override void CommitTransaction()
    {
      this.FTransaction.Commit();
      this.FTransaction.Dispose();
      this.FTransaction = (OracleTransaction) null;
    }

    public override void RollbackTransaction()
    {
      this.FTransaction.Rollback();
      this.FTransaction.Dispose();
      this.FTransaction = (OracleTransaction) null;
    }

    public override bool TableExists(string tableName)
    {
      OracleCommand command = this.FConnection.CreateCommand();
      command.CommandText = string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = '{1}'", (object) tableName, (object) this.GetSchemaName());
      using (IDataReader dataReader = (IDataReader) command.ExecuteReader())
        return dataReader.Read();
    }

    protected internal override void ApplyTableStructure(InDbTableDef tableDef)
    {
    }

    protected override void LoadTableDef(InDbTableDef tableDef)
    {
    }

    public static bool IsReservedWord(string name) => InDbOracleDatabase.FReservedWords.BinarySearch(name, (IComparer<string>) StringComparer.InvariantCultureIgnoreCase) >= 0;

    private static List<string> CreateSortedWordList(char[] separators, string source)
    {
      List<string> stringList = new List<string>();
      foreach (string str1 in source.Split(separators))
      {
        string str2 = str1.Trim();
        if (!string.IsNullOrEmpty(str2))
          stringList.Add(str2);
      }
      stringList.Sort((IComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      return stringList;
    }
  }
}
