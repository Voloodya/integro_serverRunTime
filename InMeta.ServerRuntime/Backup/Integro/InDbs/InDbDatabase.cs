// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbDatabase
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public abstract class InDbDatabase : IDisposable
  {
    public readonly DateTime DateTimeMinValue;
    public readonly DateTime DateTimeMaxValue;

    public virtual bool SameIdentifiers(string identifier1, string identifier2) => identifier1 == identifier2;

    public bool IsAcceptable(DateTime value) => value >= this.DateTimeMinValue && value <= this.DateTimeMaxValue;

    public abstract InDbCommand CreateCommand(string sql, params DataType[] paramTypes);

    public void Execute(string sql) => this.CreateCommand(sql).Execute();

    public abstract void BeginTransaction();

    public abstract void CommitTransaction();

    public abstract void RollbackTransaction();

    public abstract bool TableExists(string tableName);

    protected internal abstract void ApplyTableStructure(InDbTableDef tableDef);

    protected abstract void LoadTableDef(InDbTableDef tableDef);

    public InDbTableDef GetTableDef(string tableName)
    {
      InDbTableDef tableDef = this.TableExists(tableName) ? new InDbTableDef(this, tableName, false) : throw new InDbException(string.Format("Ошибка загрузки структуры таблицы {0}: таблица не существует.", (object) tableName));
      this.LoadTableDef(tableDef);
      return tableDef;
    }

    public InDbTableDef NewTable(string tableName) => !this.TableExists(tableName) ? new InDbTableDef(this, tableName, true) : throw new InDbException(string.Format("Ошибка создания таблицы {0}: таблица уже существует.", (object) tableName));

    public virtual void DeleteTable(string tableName)
    {
      if (!this.TableExists(tableName))
        throw new InDbException(string.Format("Ошибка удаления таблицы {0}: таблица не существует.", (object) tableName));
      this.Execute(string.Format("DROP TABLE [{0}]", (object) tableName));
    }

    protected abstract void Dispose(bool disposing);

    public abstract string[] GetTableNames();

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected internal InDbDatabase()
    {
      this.DateTimeMinValue = DateTime.MinValue;
      this.DateTimeMaxValue = DateTime.MaxValue;
    }

    protected internal InDbDatabase(DateTime dateTimeMinValue, DateTime dateTimeMaxValue)
    {
      this.DateTimeMinValue = dateTimeMinValue;
      this.DateTimeMaxValue = dateTimeMaxValue;
    }

    ~InDbDatabase() => this.Dispose(false);
  }
}
