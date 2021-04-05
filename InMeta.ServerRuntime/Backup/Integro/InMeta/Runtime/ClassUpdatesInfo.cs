// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ClassUpdatesInfo
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Collections;
using System.Collections.Specialized;
using System.Data;

namespace Integro.InMeta.Runtime
{
  public class ClassUpdatesInfo
  {
    [JsonEncode(Member = "Name")]
    public readonly MetadataClass Class;
    public readonly NewObjectInfo[] New;
    public readonly ModifiedObjectInfo[] Modified;
    public readonly string[] Deleted;

    internal ClassUpdatesInfo(
      MetadataClass @class,
      NewObjectInfo[] @new,
      ModifiedObjectInfo[] modified,
      string[] deleted)
    {
      this.Class = @class;
      this.New = @new;
      this.Modified = modified;
      this.Deleted = deleted;
    }

    internal IDictionary ToLogJson(InDbDatabase dbForOriginalValues)
    {
      ListDictionary listDictionary = new ListDictionary();
      if (this.New != null && this.New.Length > 0)
        listDictionary.Add((object) "New", (object) this.GetNewObjectsLogJson());
      if (this.Modified != null && this.Modified.Length > 0)
        listDictionary.Add((object) "Modified", (object) this.GetModifiedObjectsLogJson(dbForOriginalValues));
      if (this.Deleted != null && this.Deleted.Length > 0)
        listDictionary.Add((object) "Deleted", this.GetDeletedObjectsLogJson(dbForOriginalValues));
      return (IDictionary) listDictionary;
    }

    private IDictionary GetNewObjectsLogJson()
    {
      ListDictionary listDictionary = new ListDictionary();
      foreach (NewObjectInfo newObjectInfo in this.New)
        listDictionary.Add((object) newObjectInfo.Id, (object) newObjectInfo.ToLogJson());
      return (IDictionary) listDictionary;
    }

    private IDictionary GetModifiedObjectsLogJson(InDbDatabase dbForOriginalValues)
    {
      ListDictionary listDictionary = new ListDictionary();
      foreach (ModifiedObjectInfo modifiedObjectInfo in this.Modified)
        listDictionary.Add((object) modifiedObjectInfo.Id, (object) modifiedObjectInfo.ToLogJson(dbForOriginalValues));
      return (IDictionary) listDictionary;
    }

    private object GetDeletedObjectsLogJson(InDbDatabase dbForOriginalValues)
    {
      if (dbForOriginalValues == null)
        return (object) this.Deleted;
      ListDictionary listDictionary = new ListDictionary();
      foreach (string deletedId in this.Deleted)
        listDictionary.Add((object) deletedId, (object) this.GetOriginalValues(deletedId, dbForOriginalValues));
      return (object) listDictionary;
    }

    private ListDictionary GetOriginalValues(
      string deletedId,
      InDbDatabase dbForOriginalValues)
    {
      ListDictionary properties = new ListDictionary();
      string sql = string.Format("SELECT * FROM [{0}] WHERE [{1}]=?", (object) this.Class.DataTable, (object) this.Class.IDProperty.DataField);
      using (InDbCommand command = dbForOriginalValues.CreateCommand(sql, DataType.String))
      {
        using (IDataReader originalValuesReader = command.ExecuteReader((object) deletedId))
        {
          if (originalValuesReader.Read())
            this.AddOriginalValues(properties, originalValuesReader);
        }
      }
      return properties;
    }

    private void AddOriginalValues(ListDictionary properties, IDataReader originalValuesReader)
    {
      foreach (MetadataProperty property in this.Class.Properties)
      {
        if (!property.IsId && !property.IsSelector)
        {
          ClassUpdatesInfo.AddOriginalValue(property, properties, originalValuesReader);
          if (property.IsLink && property.Association.Selector != null)
            ClassUpdatesInfo.AddOriginalValue(property.Association.Selector, properties, originalValuesReader);
        }
      }
    }

    private static void AddOriginalValue(
      MetadataProperty property,
      ListDictionary properties,
      IDataReader originalValuesReader)
    {
      properties.Add((object) property.Name, originalValuesReader.GetValue(originalValuesReader.GetOrdinal(property.DataField)));
    }
  }
}
