// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataStorage
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using InMeta.ServerRuntime.Load;
using Integro.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataStorage
  {
    public readonly DataSession Session;
    public readonly MetadataClass Class;
    private readonly Dictionary<DataId, DataObject> FObjects;
    private readonly Dictionary<DataId, DataObject> FObjectsToUpdate;
    public readonly DataObject NullObject;
    private ObjectViewEvaluator[] FObjectViewEvaluators;
    private bool[] FCompletedChildRefs;

    protected internal DataStorage(DataSession session, MetadataClass cls)
    {
      this.Session = session;
      this.Class = cls;
      this.FObjects = new Dictionary<DataId, DataObject>();
      this.FObjectsToUpdate = new Dictionary<DataId, DataObject>();
      this.NullObject = this.CreateObjectInstance();
      this.NullObject.Init(this, DataId.Empty, ObjectSessionState.NullObject);
    }

    internal bool HasUpdates => this.FObjectsToUpdate.Count > 0;

    protected internal virtual DataObject CreateObjectInstance() => new DataObject();

    protected internal virtual DataObjectList CreateListInstance() => new DataObjectList();

    protected internal virtual DataObjectChildList CreateChildListInstance(
      DataObject owner,
      MetadataChildRef childRef)
    {
      return new DataObjectChildList(owner, childRef);
    }

    protected internal virtual LinkProperty CreateLinkPropertyInstance(
      DataObject obj,
      MetadataProperty propertyMetadata)
    {
      return new LinkProperty(obj, propertyMetadata);
    }

    private DataObject NewCacheItem(DataId id, ObjectSessionState sessionState)
    {
      DataObject objectInstance = this.CreateObjectInstance();
      objectInstance.Init(this, id, sessionState);
      if (id.IsEmpty)
        this.Session.EnqueueIdGeneration(objectInstance);
      else
        this.FObjects.Add(id, objectInstance);
      return objectInstance;
    }

    public DataObject AddNew() => this.NewCacheItem(DataId.Empty, ObjectSessionState.New);

    public DataObject AddExisting(DataId id)
    {
      DataObject objectInstance;
      if (!this.FObjects.TryGetValue(id, out objectInstance))
      {
        objectInstance = this.CreateObjectInstance();
        this.FObjects.Add(id, objectInstance);
        objectInstance.Init(this, id, ObjectSessionState.New);
        this.FObjectsToUpdate[id] = objectInstance;
      }
      else if (objectInstance.IsError)
      {
        objectInstance.Init(this, id, ObjectSessionState.New);
        this.FObjectsToUpdate[id] = objectInstance;
      }
      return objectInstance;
    }

    public DataObject GetObject(DataId id)
    {
      DataObject dataObject1 = this.EnsureCacheItem(id);
      if (dataObject1.IsUnknown)
      {
        DataSession masterSession = this.Session.MasterSession;
        if (masterSession != null)
        {
          DataObject dataObject2 = masterSession[dataObject1.Class][id];
          dataObject1.ClearSessionState();
          if (dataObject2.IsError | dataObject2.IsDeleted)
            dataObject1.IncludeSessionState(ObjectSessionState.Error);
          else
            dataObject1.IncludeSessionState(ObjectSessionState.Existing);
        }
        else
          dataObject1.LoadNotAssignedProperties();
      }
      return dataObject1;
    }

    public DataObject this[DataId id]
    {
      get
      {
        DataObject dataObject = this.GetObject(id);
        return !dataObject.SessionStateContainsAny(ObjectSessionState.Error) ? dataObject : throw new DataException(string.Format("Объект {0}[{1}] не существует.", (object) this.Class.Name, (object) id));
      }
    }

    internal DataObject EnsureCacheItem(DataId id)
    {
      DataObject dataObject;
      return id.IsEmpty ? this.NullObject : (this.FObjects.TryGetValue(id, out dataObject) ? dataObject : this.NewCacheItem(id, (ObjectSessionState) 0));
    }

    [Obsolete("Следует использовать индексатор по DataId. В последующих версиях этот индексатор будет удален.")]
    public DataObject this[string strId] => this[new DataId(strId)];

    private LoadPlan CreatePlanFromString(string loadPlan)
    {
      XmlDocument xmlDocument = XmlUtils.LoadDocumentFromXml("<root>" + loadPlan + "</root>");
      LoadPlanBuilder loadPlanBuilder = new LoadPlanBuilder(this.Session);
      LoadPlan loadPlan1 = loadPlanBuilder.BuildPlan(this.Class, (XmlNode) xmlDocument.DocumentElement);
      loadPlanBuilder.ProcessObjectViewRequests();
      return loadPlan1;
    }

    public DataObjectList Query(LoadPlan loadPlan, params DataId[] ids)
    {
      DataObjectList listInstance = this.CreateListInstance();
      this.Session.LoadData(loadPlan, listInstance, ids, (string) null);
      return listInstance;
    }

    public DataObjectList Query(string loadPlan, params DataId[] ids) => this.Query(this.CreatePlanFromString(loadPlan), ids);

    public DataObjectList Query(
      LoadPlan loadPlan,
      string condition,
      params object[] paramArray)
    {
      DataObjectList listInstance = this.CreateListInstance();
      this.Session.TraceQueryStarted(this.Class);
      this.Session.LoadData(loadPlan, listInstance, (DataId[]) null, condition, paramArray);
      this.Session.TraceQueryFinished();
      return listInstance;
    }

    public DataObjectList Query(LoadPlan loadPlan)
    {
      DataObjectList listInstance = this.CreateListInstance();
      this.Session.LoadData(loadPlan, listInstance, (DataId[]) null, "");
      return listInstance;
    }

    public DataObjectList Query(
      string loadPlan,
      string condition,
      params object[] paramArray)
    {
      return this.Query(this.CreatePlanFromString(loadPlan), condition, paramArray);
    }

    public DataObjectList Query(string loadPlan) => this.Query(this.CreatePlanFromString(loadPlan), "");

    public DataObjectList Query(string loadPlan, QueryCondition conditionBuilder)
    {
      object[] array = conditionBuilder.Params.ToArray();
      return this.Query(loadPlan, conditionBuilder.Condition.ToString(), array);
    }

    public DataObject QueryObject(LoadPlan loadPlan, DataId id)
    {
      DataObjectList listInstance = this.CreateListInstance();
      this.Session.LoadData(loadPlan, listInstance, new DataId[1]
      {
        id
      }, (string) null);
      return listInstance.Count != 0 ? listInstance[0] : this.Session[loadPlan.Class].NullObject;
    }

    public DataObject QueryObject(string loadPlan, DataId id) => this.QueryObject(this.CreatePlanFromString(loadPlan), id);

    public Totals QueryTotals(
      string columns,
      string condition,
      object[] args,
      string groupBy)
    {
      return DataSession.QueryTotals(this, columns, condition, args, groupBy);
    }

    internal ObjectViewEvaluator GetObjectViewEvaluator(
      MetadataObjectView viewMetadata)
    {
      if (this.FObjectViewEvaluators == null)
        this.FObjectViewEvaluators = new ObjectViewEvaluator[this.Class.ObjectViews.Count];
      ObjectViewEvaluator objectViewEvaluator = this.FObjectViewEvaluators[viewMetadata.Index];
      if (objectViewEvaluator == null)
      {
        objectViewEvaluator = new ObjectViewEvaluator(viewMetadata, this);
        this.FObjectViewEvaluators[viewMetadata.Index] = objectViewEvaluator;
      }
      return objectViewEvaluator;
    }

    internal void CompleteChildLists(MetadataChildRef childRef)
    {
      if (this.FCompletedChildRefs == null)
        this.FCompletedChildRefs = new bool[this.Class.Childs.Count];
      this.FCompletedChildRefs[childRef.Index] = true;
    }

    internal bool IsChildListCompleted(MetadataChildRef childRef) => this.FCompletedChildRefs != null && this.FCompletedChildRefs[childRef.Index];

    internal void DropCache()
    {
      this.FCompletedChildRefs = (bool[]) null;
      foreach (DataObject dataObject in this.FObjects.Values)
        dataObject.DropCache();
      foreach (DataObject dataObject in this.FObjectsToUpdate.Values)
        dataObject.DropCache();
    }

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (!disposing || this.FObjectViewEvaluators == null)
        return;
      for (int index = 0; index < this.FObjectViewEvaluators.Length; ++index)
      {
        ObjectViewEvaluator fobjectViewEvaluator = this.FObjectViewEvaluators[index];
        if (fobjectViewEvaluator != null)
        {
          this.FObjectViewEvaluators[index] = (ObjectViewEvaluator) null;
          fobjectViewEvaluator.Dispose();
        }
      }
    }

    internal void EnqueueObjectForUpdate(DataObject dataObject) => this.FObjectsToUpdate[dataObject.Id] = dataObject;

    internal IEnumerable<DataObject> GetUpdateQueue() => (IEnumerable<DataObject>) this.FObjectsToUpdate.Values;

    internal DataObject GetCachedObject(DataId id) => this.FObjects[id];

    internal void ClearUpdateQueue() => this.FObjectsToUpdate.Clear();

    internal void AddObjectToCache(DataObject dataObject) => this.FObjects.Add(dataObject.Id, dataObject);

    internal ICollection GetCachedObjects() => (ICollection) this.FObjects.Values;

    internal bool IsObjectCached(DataId id) => this.FObjects.ContainsKey(id);

    internal void DequeueObjectFromUpdate(DataId id) => this.FObjectsToUpdate.Remove(id);

    internal void RemoveObjectFromCache(DataId id) => this.FObjects.Remove(id);
  }
}
