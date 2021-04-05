// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectListLoaderByIds
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System;
using System.Collections;
using System.Data;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal class ObjectListLoaderByIds : ObjectListLoader
  {
    private readonly Hashtable FIds;
    internal readonly MetadataProperty InProperty;

    internal ObjectListLoaderByIds(
      ObjectLoader objectLoader,
      MetadataProperty inProperty,
      DataId[] ids)
      : base(objectLoader)
    {
      this.FIds = new Hashtable();
      if (ids != null)
      {
        for (int index = 0; index < ids.Length; ++index)
          this.FIds[(object) ids[index]] = (object) null;
      }
      this.InProperty = inProperty;
    }

    internal void Add(DataId id)
    {
      if (this.LoadedObjects.ContainsKey((object) id))
        return;
      this.FIds[(object) id] = (object) null;
    }

    internal override void Load(InDbDatabase db, DataObjectList dstObjs)
    {
      int count = this.FIds.Count;
      if (count == 0)
        return;
      DataId[] dataIdArray = new DataId[count];
      this.FIds.Keys.CopyTo((Array) dataIdArray, 0);
      this.FIds.Clear();
      StringBuilder stringBuilder = new StringBuilder(this.SelectSql);
      stringBuilder.Append(" WHERE [").Append(this.InProperty.DataField).Append("] IN ('");
      int length = stringBuilder.Length;
      int index1 = 0;
      while (index1 < count)
      {
        stringBuilder.Length = length;
        for (int index2 = 0; index1 < count && index2 < 100; ++index2)
        {
          if (index2 > 0)
            stringBuilder.Append("','");
          stringBuilder.Append(dataIdArray[index1].ToString());
          ++index1;
        }
        stringBuilder.Append("')");
        InDbCommand command = db.CreateCommand(stringBuilder.ToString());
        IDataReader reader = command.ExecuteReader();
        try
        {
          this.Load(reader, dstObjs, (LoadContext) 0);
        }
        finally
        {
          reader.Dispose();
          command.Dispose();
        }
      }
      if (!this.InProperty.IsId)
        return;
      for (int index2 = 0; index2 < count; ++index2)
      {
        DataId id = dataIdArray[index2];
        if (!this.LoadedObjects.ContainsKey((object) id))
          this.FStorage.EnsureCacheItem(id).SetSessionState(ObjectSessionState.Error);
      }
    }

    public bool HasUnloadedObjects => this.FIds.Count > 0;
  }
}
