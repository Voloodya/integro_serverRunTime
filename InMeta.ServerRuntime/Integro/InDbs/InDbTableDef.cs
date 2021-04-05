// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbTableDef
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public class InDbTableDef
  {
    private readonly InDbDatabase FDb;
    public readonly InDbFieldDefs FieldDefs;
    private readonly InDbFieldDefs FFieldDefsToDelete;
    public readonly InDbIndexDefs IndexDefs;
    private readonly InDbIndexDefs FIndexDefsToDelete;
    private readonly InDbIndexDefs FModifiedIndexDefs;
    internal InDbIndexDef OriginalPrimaryKey;
    public InDbIndexDef PrimaryKey;
    internal string OriginalName = string.Empty;
    public string Name = string.Empty;

    internal InDbTableDef(InDbDatabase db, string tableName, bool isNew)
    {
      this.FDb = db;
      this.FieldDefs = new InDbFieldDefs(this.FDb);
      this.FFieldDefsToDelete = new InDbFieldDefs(this.FDb);
      this.IndexDefs = new InDbIndexDefs(this.FDb);
      this.FIndexDefsToDelete = new InDbIndexDefs(this.FDb);
      this.FModifiedIndexDefs = new InDbIndexDefs(this.FDb);
      this.Name = tableName;
      if (isNew)
        return;
      this.OriginalName = tableName;
    }

    public int AddNewFieldDef(string name, DataType dataType, int size) => this.FieldDefs.Add(new InDbFieldDef(name, dataType, size, true));

    public int AddNewFieldDef(string name, DataType dataType) => this.AddNewFieldDef(name, dataType, 0);

    public void DeleteFieldDef(string name, bool autoCorrectIndexes)
    {
      try
      {
        InDbFieldDef fieldDef = this.FieldDefs[name];
        if (!StrUtils.IsNullOrEmpty(fieldDef.OriginalName))
          this.FFieldDefsToDelete.Add(fieldDef);
        this.FieldDefs.RemoveAt(this.FieldDefs.IndexOf(name));
        if (!autoCorrectIndexes)
          return;
        for (int index1 = this.IndexDefs.Count - 1; index1 >= 0; --index1)
        {
          InDbIndexDef indexDef = this.IndexDefs[index1];
          for (int index2 = indexDef.FieldDefs.Count - 1; index2 >= 0; --index2)
          {
            if (indexDef.FieldDefs[index2] == fieldDef)
            {
              indexDef.FieldDefs.RemoveAt(index2);
              if (this.FModifiedIndexDefs.IndexOf(indexDef) == -1)
                this.FModifiedIndexDefs.Add(indexDef);
            }
          }
          if (indexDef.FieldDefs.Count == 0)
            this.DeleteIndexDef(index1);
        }
      }
      catch (Exception ex)
      {
        throw new InDbException(string.Format("Ошибка удаления поля '{0}' таблицы '{1}'.", (object) name, (object) this.Name), ex);
      }
    }

    public InDbIndexDef AddNewIndexDef(params string[] fieldNames)
    {
      InDbFieldDefs fieldDefs = new InDbFieldDefs(this.FDb);
      foreach (string fieldName in fieldNames)
        fieldDefs.Add(this.FieldDefs[fieldName]);
      return this.IndexDefs[this.IndexDefs.Add(new InDbIndexDef("", fieldDefs))];
    }

    public void DeleteIndexDef(int index)
    {
      InDbIndexDef indexDef = this.IndexDefs[index];
      if (!StrUtils.IsNullOrEmpty(indexDef.Name))
        this.FIndexDefsToDelete.Add(indexDef);
      this.IndexDefs.RemoveAt(index);
      if (this.PrimaryKey != indexDef)
        return;
      this.PrimaryKey = (InDbIndexDef) null;
    }

    public bool Modified
    {
      get
      {
        if (this.Name != this.OriginalName || this.OriginalPrimaryKey != this.PrimaryKey || (this.FIndexDefsToDelete.Count > 0 || this.FModifiedIndexDefs.Count > 0))
          return true;
        foreach (InDbIndexDef indexDef in this.IndexDefs)
        {
          if (StrUtils.IsNullOrEmpty(indexDef.Name))
            return true;
        }
        if (this.FFieldDefsToDelete.Count > 0)
          return true;
        foreach (InDbFieldDef fieldDef in this.FieldDefs)
        {
          if (InDbTableDef.IsFieldDefModified(fieldDef))
            return true;
        }
        return false;
      }
    }

    private static bool IsFieldDefModified(InDbFieldDef fieldDef) => fieldDef.Name != fieldDef.OriginalName || fieldDef.DataType != fieldDef.OriginalDataType || fieldDef.Size != fieldDef.OriginalSize;

    public void ApplyChanges()
    {
      if (!this.Modified)
        return;
      this.FDb.ApplyTableStructure(this);
      this.OriginalName = this.Name;
      this.OriginalPrimaryKey = this.PrimaryKey;
      this.FFieldDefsToDelete.Clear();
      this.FIndexDefsToDelete.Clear();
      this.FModifiedIndexDefs.Clear();
      foreach (InDbFieldDef fieldDef in this.FieldDefs)
      {
        fieldDef.FOriginalName = fieldDef.Name;
        fieldDef.FOriginalDataType = fieldDef.DataType;
        fieldDef.FOriginalSize = fieldDef.Size;
      }
    }
  }
}
