// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectListLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Collections;
using System.Data;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal abstract class ObjectListLoader
  {
    internal readonly ObjectLoader ObjectLoader;
    private string FSelectSql;
    internal readonly Hashtable LoadedObjects;
    internal DataStorage FStorage;

    internal ObjectListLoader(ObjectLoader objectLoader)
    {
      this.ObjectLoader = objectLoader;
      this.LoadedObjects = new Hashtable();
    }

    public MetadataClass Class => this.ObjectLoader.BasePlan.Class;

    protected string SelectSql
    {
      get
      {
        if (this.FSelectSql == null)
        {
          MetadataClass metadataClass = this.ObjectLoader.BasePlan.Class;
          StringBuilder stringBuilder = new StringBuilder("SELECT [");
          stringBuilder.Append(metadataClass.IDProperty.DataField).Append("]");
          for (int index = 0; index < this.ObjectLoader.Count; ++index)
          {
            ObjectPartLoader objectPartLoader = this.ObjectLoader[index];
            if (objectPartLoader.FieldName != null)
              stringBuilder.Append(",[").Append(objectPartLoader.FieldName).Append("]");
            if (objectPartLoader.ExFieldName != null)
              stringBuilder.Append(",[").Append(objectPartLoader.ExFieldName).Append("]");
          }
          stringBuilder.Append(" FROM [").Append(metadataClass.DataTable).Append("]");
          this.FSelectSql = stringBuilder.ToString();
        }
        return this.FSelectSql;
      }
    }

    internal void Load(IDataReader reader, DataObjectList dstObjs, LoadContext loadContext)
    {
      object[] values = new object[reader.FieldCount];
      while (reader.Read())
      {
        reader.GetValues(values);
        DataId id = new DataId((string) values[0]);
        if (this.LoadedObjects[(object) id] == null)
        {
          DataObject dataObject = this.FStorage.EnsureCacheItem(id);
          if (!dataObject.IsDeleted)
          {
            int index = 0;
            int num = 1;
            for (; index < this.ObjectLoader.Count; ++index)
            {
              ObjectPartLoader objectPartLoader = this.ObjectLoader[index];
              object obj = objectPartLoader.FieldName != null ? values[num++] : (object) null;
              object exValue = objectPartLoader.ExFieldName != null ? values[num++] : (object) null;
              objectPartLoader.Load(dataObject, obj, exValue, loadContext);
            }
            dstObjs?.Add((object) dataObject);
          }
          this.LoadedObjects.Add((object) dataObject.Id, (object) dataObject);
          dataObject.IncludeSessionState(ObjectSessionState.Existing);
        }
      }
      if (loadContext != LoadContext.FetchAllObjects)
        return;
      for (int index1 = 0; index1 < this.ObjectLoader.Count; ++index1)
      {
        if (this.ObjectLoader[index1] is LinkPropertyLoader linkPropertyLoader && linkPropertyLoader.Association.Property.IsAggregation)
        {
          for (int index2 = 0; index2 < linkPropertyLoader.RefLoaders.Length; ++index2)
          {
            MetadataClass cls = linkPropertyLoader.RefLoaders[index2].Class;
            this.FStorage.Session[cls].CompleteChildLists(cls.Childs.Need(this.Class));
          }
        }
      }
    }

    internal abstract void Load(InDbDatabase connection, DataObjectList dstObjs);
  }
}
