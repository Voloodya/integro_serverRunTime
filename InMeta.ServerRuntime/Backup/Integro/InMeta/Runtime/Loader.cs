// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.Loader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class Loader
  {
    private readonly List<ObjectLoader> ObjectLoaders = new List<ObjectLoader>();
    private readonly List<ObjectListLoader> ObjectListLoaders = new List<ObjectListLoader>();
    private readonly List<ObjectViewLoader> ObjectViewLoaders = new List<ObjectViewLoader>();
    private readonly ObjectLoader RootLoader;

    public Loader(LoadPlan plan) => this.RootLoader = this.EnsureObjectLoader(plan);

    internal ObjectViewLoader EnsureObjectViewLoader(MetadataObjectView objectView)
    {
      for (int index = 0; index < this.ObjectViewLoaders.Count; ++index)
      {
        ObjectViewLoader objectViewLoader = this.ObjectViewLoaders[index];
        if (objectViewLoader.ObjectView == objectView)
          return objectViewLoader;
      }
      ObjectViewLoader objectViewLoader1 = new ObjectViewLoader(objectView);
      this.ObjectViewLoaders.Add(objectViewLoader1);
      return objectViewLoader1;
    }

    internal ObjectLoader EnsureObjectLoader(LoadPlan plan)
    {
      for (int index = 0; index < this.ObjectLoaders.Count; ++index)
      {
        ObjectLoader objectLoader = this.ObjectLoaders[index];
        if (objectLoader.BasePlan == plan)
          return objectLoader;
      }
      ObjectLoader objectLoader1 = new ObjectLoader(plan);
      this.ObjectLoaders.Add(objectLoader1);
      objectLoader1.Prepare(this);
      return objectLoader1;
    }

    internal ObjectListLoaderByIds EnsureObjectListLoaderByIds(
      ObjectLoader objectLoader,
      MetadataProperty inProperty)
    {
      for (int index = 0; index < this.ObjectListLoaders.Count; ++index)
      {
        if (this.ObjectListLoaders[index] is ObjectListLoaderByIds objectListLoader && objectListLoader.ObjectLoader == objectLoader && objectListLoader.InProperty == inProperty)
          return objectListLoader;
      }
      ObjectListLoaderByIds objectListLoaderByIds = new ObjectListLoaderByIds(objectLoader, inProperty, (DataId[]) null);
      this.ObjectListLoaders.Add((ObjectListLoader) objectListLoaderByIds);
      return objectListLoaderByIds;
    }

    private bool HasUnloadedObjects
    {
      get
      {
        for (int index = 0; index < this.ObjectListLoaders.Count; ++index)
        {
          if (this.ObjectListLoaders[index] is ObjectListLoaderByIds objectListLoader && objectListLoader.HasUnloadedObjects)
            return true;
        }
        return false;
      }
    }

    private void Load(DataSession session, DataObjectList dstObjs, ObjectListLoader listLoader)
    {
      listLoader.FStorage = session[listLoader.Class];
      for (int index = 0; index < this.ObjectListLoaders.Count; ++index)
      {
        ObjectListLoader objectListLoader = this.ObjectListLoaders[index];
        objectListLoader.FStorage = session[objectListLoader.Class];
      }
      do
      {
        listLoader.Load(session.Db, dstObjs);
        for (int index = 0; index < this.ObjectListLoaders.Count; ++index)
          this.ObjectListLoaders[index].Load(session.Db, (DataObjectList) null);
      }
      while (this.HasUnloadedObjects);
    }

    internal void Load(
      DataSession session,
      DataObjectList dstObjs,
      string condition,
      params object[] paramArray)
    {
      this.Load(session, dstObjs, (ObjectListLoader) new ObjectListLoaderByCondition(this.RootLoader, condition, paramArray));
    }

    internal void Load(DataSession session, DataObjectList dstObjs, DataId[] ids) => this.Load(session, dstObjs, (ObjectListLoader) new ObjectListLoaderByIds(this.RootLoader, this.RootLoader.BasePlan.Class.IDProperty, ids));
  }
}
