// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.StorageCommitPlan
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal class StorageCommitPlan
  {
    private readonly DataStorage FStorage;
    private readonly DataStorage FMasterStorage;
    private readonly MetadataClass FClass;
    private readonly DataSession FSession;
    private readonly UpdatePlan FUpdating;
    private readonly UpdatePlan FCreation;
    private readonly List<DataObject> FDeletion;

    public StorageCommitPlan(DataStorage storage)
    {
      this.FStorage = storage;
      if (storage.Session.MasterSession != null)
        this.FMasterStorage = storage.Session.MasterSession[storage.Class];
      this.FClass = storage.Class;
      this.FSession = storage.Session;
      this.FCreation = new UpdatePlan(PropertyStateFilter.Assigned);
      this.FUpdating = new UpdatePlan(PropertyStateFilter.Modified);
      this.FDeletion = new List<DataObject>();
      foreach (DataObject update in storage.GetUpdateQueue())
      {
        if (update.IsDeleted)
        {
          if (!update.IsNew)
            this.FDeletion.Add(update);
        }
        else if (update.IsNew)
          this.FCreation.AddObject(update);
        else if (update.IsPropertiesModified)
          this.FUpdating.AddObject(update);
      }
    }

    public bool HasUpdates => this.FUpdating.Count > 0 || this.FCreation.Count > 0 || this.FDeletion.Count > 0;

    private static void MetaPropToSql(
      MetadataProperty metaProp,
      StringBuilder sql,
      string divider,
      string format,
      ref int paramIndex,
      DataType[] paramTypes)
    {
      if (divider != null && sql != null)
        sql.Append(divider);
      sql?.Append(string.Format(format, (object) metaProp.DataField));
      if (paramTypes != null)
        paramTypes[paramIndex] = metaProp.DataType;
      ++paramIndex;
    }

    private static void MetaPropsToSql(
      IList<MetadataProperty> metaProps,
      StringBuilder sql,
      string divider,
      string format,
      DataType[] paramTypes,
      ref int paramIndex)
    {
      for (int index = 0; index < metaProps.Count; ++index)
      {
        MetadataProperty metaProp = metaProps[index];
        if (!metaProp.IsId && !metaProp.IsSelector)
        {
          StorageCommitPlan.MetaPropToSql(metaProp, sql, divider, format, ref paramIndex, paramTypes);
          divider = ",";
          if (metaProp.IsLink && metaProp.Association.Selector != null)
            StorageCommitPlan.MetaPropToSql(metaProp.Association.Selector, sql, divider, format, ref paramIndex, paramTypes);
        }
      }
    }

    private static void CopyPropToParam(
      DataObject obj,
      MetadataProperty metaProp,
      object[] values,
      ref int index)
    {
      object obj1 = (object) null;
      object exValue = (object) null;
      int num = obj.SavePropertyValue(metaProp, ref obj1, ref exValue);
      values[index++] = obj1 ?? (object) DBNull.Value;
      if (num <= 1)
        return;
      values[index++] = exValue ?? (object) DBNull.Value;
    }

    private static void CopyPropsToParams(
      DataObject obj,
      IList<MetadataProperty> metaProps,
      object[] values,
      ref int index)
    {
      for (int index1 = 0; index1 < metaProps.Count; ++index1)
      {
        MetadataProperty metaProp = metaProps[index1];
        if (!metaProp.IsId && !metaProp.IsSelector)
          StorageCommitPlan.CopyPropToParam(obj, metaProp, values, ref index);
      }
    }

    private List<MetadataProperty> GetMetaProps(IndexSet indexSet)
    {
      List<MetadataProperty> metadataPropertyList = new List<MetadataProperty>();
      for (int index = 0; index < this.FClass.Properties.Count; ++index)
      {
        if (indexSet[index])
          metadataPropertyList.Add(this.FClass.Properties[index]);
      }
      return metadataPropertyList;
    }

    private void CommitNewObjectsToDb(ObjectsByPropertyIndexSet plan)
    {
      List<MetadataProperty> metaProps = this.GetMetaProps(plan.IndexSet);
      StringBuilder sql = new StringBuilder(string.Format("INSERT INTO [{0}]([{1}]", (object) this.FClass.DataTable, (object) this.FClass.IDProperty.DataField));
      int paramIndex1 = 1;
      StorageCommitPlan.MetaPropsToSql((IList<MetadataProperty>) metaProps, sql, ",", "[{0}]", (DataType[]) null, ref paramIndex1);
      sql.Append(") VALUES (?");
      DataType[] paramTypes = new DataType[paramIndex1];
      object[] values = new object[paramIndex1];
      paramTypes[0] = DataType.String;
      int paramIndex2 = 1;
      StorageCommitPlan.MetaPropsToSql((IList<MetadataProperty>) metaProps, sql, ",", "?", paramTypes, ref paramIndex2);
      sql.Append(")");
      InDbCommand command = this.FSession.Db.CreateCommand(sql.ToString(), paramTypes);
      try
      {
        for (int index1 = 0; index1 < plan.Count; ++index1)
        {
          DataObject dataObject = plan[index1];
          int index2 = 1;
          values[0] = (object) dataObject.Id.ToString();
          StorageCommitPlan.CopyPropsToParams(dataObject, (IList<MetadataProperty>) metaProps, values, ref index2);
          command.Execute(values);
        }
      }
      finally
      {
        command.Dispose();
      }
    }

    private void CommitModifiedObjectsToDb(ObjectsByPropertyIndexSet plan)
    {
      if (plan.IndexSet.IsEmpty)
        return;
      List<MetadataProperty> metaProps = this.GetMetaProps(plan.IndexSet);
      StringBuilder sql = new StringBuilder(string.Format("UPDATE [{0}] SET ", (object) this.FClass.DataTable));
      int paramIndex1 = 0;
      StorageCommitPlan.MetaPropsToSql((IList<MetadataProperty>) metaProps, sql, (string) null, "[{0}]=?", (DataType[]) null, ref paramIndex1);
      sql.Append(string.Format(" WHERE [{0}]=?", (object) this.FClass.IDProperty.DataField));
      DataType[] paramTypes = new DataType[paramIndex1 + 1];
      object[] values = new object[paramIndex1 + 1];
      int paramIndex2 = 0;
      StorageCommitPlan.MetaPropsToSql((IList<MetadataProperty>) metaProps, (StringBuilder) null, (string) null, (string) null, paramTypes, ref paramIndex2);
      paramTypes[paramIndex2] = DataType.String;
      InDbCommand command = this.FSession.Db.CreateCommand(sql.ToString(), paramTypes);
      try
      {
        for (int index1 = 0; index1 < plan.Count; ++index1)
        {
          DataObject dataObject = plan[index1];
          int index2 = 0;
          StorageCommitPlan.CopyPropsToParams(dataObject, (IList<MetadataProperty>) metaProps, values, ref index2);
          values[index2] = (object) dataObject.Id.ToString();
          command.Execute(values);
        }
      }
      finally
      {
        command.Dispose();
      }
    }

    private void CommitDeletedObjectsToDb()
    {
      StringBuilder stringBuilder = new StringBuilder(string.Format("DELETE FROM [{0}] WHERE [{1}] IN (", (object) this.FClass.DataTable, (object) this.FClass.IDProperty.DataField));
      int length = stringBuilder.Length;
      int index1 = 0;
      while (index1 < this.FDeletion.Count)
      {
        stringBuilder.Length = length;
        for (int index2 = 0; index1 < this.FDeletion.Count && index2 < 100; ++index2)
        {
          if (index2 > 0)
            stringBuilder.Append(",");
          stringBuilder.Append("'").Append(this.FDeletion[index1].Id.ToString()).Append("'");
          ++index1;
        }
        stringBuilder.Append(")");
        this.FSession.Db.CreateCommand(stringBuilder.ToString()).Execute();
      }
    }

    public void CommitToDb()
    {
      for (int index = 0; index < this.FCreation.Count; ++index)
        this.CommitNewObjectsToDb(this.FCreation[index]);
      for (int index = 0; index < this.FUpdating.Count; ++index)
        this.CommitModifiedObjectsToDb(this.FUpdating[index]);
      if (this.FDeletion.Count <= 0)
        return;
      this.CommitDeletedObjectsToDb();
    }

    public ClassUpdatesInfo GetUpdateInfo()
    {
      List<NewObjectInfo> newObjectInfoList = new List<NewObjectInfo>();
      List<ModifiedObjectInfo> modifiedObjectInfoList = new List<ModifiedObjectInfo>();
      List<string> stringList = new List<string>();
      if (this.FCreation.Count > 0)
      {
        for (int index = 0; index < this.FCreation.Count; ++index)
          this.AddNewObjectInfos(this.FCreation[index], (ICollection<NewObjectInfo>) newObjectInfoList);
      }
      if (this.FUpdating.Count > 0)
      {
        for (int index = 0; index < this.FUpdating.Count; ++index)
          this.AddModifiedObjectInfos(this.FUpdating[index], (ICollection<ModifiedObjectInfo>) modifiedObjectInfoList);
      }
      if (this.FDeletion.Count > 0)
        this.AddDeletedObjectInfos((ICollection<string>) stringList);
      return new ClassUpdatesInfo(this.FStorage.Class, newObjectInfoList.ToArray(), modifiedObjectInfoList.ToArray(), stringList.ToArray());
    }

    private void AddNewObjectInfos(
      ObjectsByPropertyIndexSet plan,
      ICollection<NewObjectInfo> newObjects)
    {
      List<MetadataProperty> metaProps = this.GetMetaProps(plan.IndexSet);
      for (int index = 0; index < plan.Count; ++index)
      {
        DataObject dataObject = plan[index];
        NewObjectInfo newObjectInfo = new NewObjectInfo(dataObject.Id.ToString());
        StorageCommitPlan.CopyPropsToNewObjectInfoProperties(dataObject, (IEnumerable<MetadataProperty>) metaProps, (IDictionary<MetadataProperty, object>) newObjectInfo.Properties);
        newObjects.Add(newObjectInfo);
      }
    }

    private void AddModifiedObjectInfos(
      ObjectsByPropertyIndexSet plan,
      ICollection<ModifiedObjectInfo> modifiedObjects)
    {
      List<MetadataProperty> metaProps = this.GetMetaProps(plan.IndexSet);
      for (int index = 0; index < plan.Count; ++index)
      {
        DataObject dataObject = plan[index];
        ModifiedObjectInfo modifiedObjectInfo = new ModifiedObjectInfo(dataObject.Id.ToString());
        StorageCommitPlan.CopyPropsToModifiedObjectInfoProperties(dataObject, (IEnumerable<MetadataProperty>) metaProps, (IDictionary<MetadataProperty, object>) modifiedObjectInfo.Properties);
        modifiedObjects.Add(modifiedObjectInfo);
      }
    }

    private static void CopyPropsToModifiedObjectInfoProperties(
      DataObject dataObject,
      IEnumerable<MetadataProperty> metaProps,
      IDictionary<MetadataProperty, object> values)
    {
      StorageCommitPlan.CopyPropValues(dataObject, metaProps, values);
    }

    private static void CopyPropsToNewObjectInfoProperties(
      DataObject obj,
      IEnumerable<MetadataProperty> metaProps,
      IDictionary<MetadataProperty, object> values)
    {
      StorageCommitPlan.CopyPropValues(obj, metaProps, values);
    }

    private static void CopyPropValues(
      DataObject dataObject,
      IEnumerable<MetadataProperty> metaProps,
      IDictionary<MetadataProperty, object> properties)
    {
      foreach (MetadataProperty metaProp in metaProps)
      {
        if (!metaProp.IsId && !metaProp.IsSelector)
        {
          MetadataProperty prop1;
          object obj1;
          MetadataProperty prop2;
          object obj2;
          StorageCommitPlan.GetPropValues(dataObject, metaProp, out prop1, out obj1, out prop2, out obj2);
          properties.Add(prop1, obj1);
          if (prop2 != null)
            properties.Add(prop2, obj2);
        }
      }
    }

    private static void GetPropValues(
      DataObject obj,
      MetadataProperty metaProp,
      out MetadataProperty prop1,
      out object value1,
      out MetadataProperty prop2,
      out object value2)
    {
      value1 = (object) null;
      value2 = (object) null;
      int num = obj.SavePropertyValue(metaProp, ref value1, ref value2);
      prop1 = metaProp;
      prop2 = num > 1 ? metaProp.Association.Selector : (MetadataProperty) null;
    }

    private void AddDeletedObjectInfos(ICollection<string> deletedObjects)
    {
      for (int index = 0; index < this.FDeletion.Count; ++index)
        deletedObjects.Add(this.FDeletion[index].Id.ToString());
    }

    private static void CopyPropsToMasterObject(
      DataObject obj,
      DataObject masterObj,
      IList<MetadataProperty> metaProps)
    {
      for (int index = 0; index < metaProps.Count; ++index)
      {
        MetadataProperty metaProp = metaProps[index];
        if (!metaProp.IsId && !metaProp.IsSelector)
          masterObj[metaProp].Assign(obj[metaProp]);
      }
    }

    public void CommitCreationToMasterSession()
    {
      foreach (ObjectsByPropertyIndexSet propertyIndexSet in (List<ObjectsByPropertyIndexSet>) this.FCreation)
      {
        for (int index = 0; index < propertyIndexSet.Count; ++index)
          this.FMasterStorage.AddExisting(propertyIndexSet[index].Id);
      }
    }

    private void CommitUpdatingToMasterSession(IEnumerable<ObjectsByPropertyIndexSet> updatePlan)
    {
      foreach (ObjectsByPropertyIndexSet propertyIndexSet in updatePlan)
      {
        if (propertyIndexSet.IndexSet.IsEmpty)
          break;
        List<MetadataProperty> metadataPropertyList = new List<MetadataProperty>();
        IndexSet indexSet = propertyIndexSet.IndexSet;
        for (int index = 0; index < this.FStorage.Class.Properties.Count; ++index)
        {
          if (indexSet[index])
            metadataPropertyList.Add(this.FStorage.Class.Properties[index]);
        }
        for (int index = 0; index < propertyIndexSet.Count; ++index)
        {
          DataObject dataObject = propertyIndexSet[index];
          StorageCommitPlan.CopyPropsToMasterObject(dataObject, this.FMasterStorage[dataObject.Id], (IList<MetadataProperty>) metadataPropertyList);
        }
      }
    }

    public void CommitToMasterSession()
    {
      this.CommitUpdatingToMasterSession((IEnumerable<ObjectsByPropertyIndexSet>) this.FCreation);
      this.CommitUpdatingToMasterSession((IEnumerable<ObjectsByPropertyIndexSet>) this.FUpdating);
      for (int index = 0; index < this.FDeletion.Count; ++index)
      {
        DataObject dataObject = this.FMasterStorage[this.FDeletion[index].Id];
        if (!dataObject.IsDeleted)
          dataObject.Delete();
      }
    }

    public void CommitToMemory()
    {
      List<DataObject> dataObjectList = new List<DataObject>(this.FStorage.GetUpdateQueue());
      for (int index = 0; index < dataObjectList.Count; ++index)
        dataObjectList[index].Commit();
      this.FStorage.ClearUpdateQueue();
    }
  }
}
