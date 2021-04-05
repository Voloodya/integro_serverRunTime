// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataSession
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using InMeta.ServerRuntime;
using Integro.InDbs;
using Integro.InMeta.Runtime.CentralServer;
using Integro.Utils;
using Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataSession : IDisposable
  {
    private CentralServerConnection FCentralServerConnection;
    private InDbDatabase FDb;
    private ApplicationUser FUser;
    private ScriptLibraries FScriptLibraries;
    private readonly List<DataObject> FNewObjectsWithoutGeneratedId;
    private readonly DataStorage[] FStorages;
    public readonly DataApplication Application;
    public readonly string UserAccount;
    private XmlNode FContextNode;
    public bool TreatNullsAsZero;
    private DataSession FMasterSession;
    private static readonly object FSync = new object();
    private static readonly List<WeakReference> FLiveSessions = new List<WeakReference>();
    public readonly DataObject UntypedNullObject = new DataObject();
    private UpdateLogMode FUpdateLogMode;

    public event EventHandler Disposed;

    public DataSession(DataApplication application, string userAccount)
    {
      this.Application = application;
      this.UserAccount = userAccount;
      this.FStorages = new DataStorage[application.Metadata.Classes.Count];
      this.FCentralServerConnection = new CentralServerConnection((Integro.InMeta.Runtime.CentralServer.CentralServer) null, application.CentralServerAddress);
      this.FScriptLibraries = new ScriptLibraries(this);
      this.FNewObjectsWithoutGeneratedId = new List<DataObject>();
      this.FUpdateLogMode = application.Settings.UpdateLogMode;
      this.CheckRefIntegrity = application.Settings.CheckRefIntegrity;
      this.RegisterLiveSessions();
    }

    private void RegisterLiveSessions()
    {
      lock (DataSession.FSync)
      {
        for (int index = 0; index < DataSession.FLiveSessions.Count; ++index)
        {
          WeakReference fliveSession = DataSession.FLiveSessions[index];
          if (!fliveSession.IsAlive)
          {
            fliveSession.Target = (object) this;
            return;
          }
        }
        DataSession.FLiveSessions.Add(new WeakReference((object) this));
      }
    }

    public static DataSession[] GetLiveSessions()
    {
      lock (DataSession.FSync)
      {
        List<DataSession> dataSessionList = new List<DataSession>();
        for (int index = 0; index < DataSession.FLiveSessions.Count; ++index)
        {
          DataSession target = (DataSession) DataSession.FLiveSessions[index].Target;
          if (target != null)
            dataSessionList.Add(target);
        }
        return dataSessionList.ToArray();
      }
    }

    public bool IsSlave => this.FMasterSession != null;

    public ApplicationUser CurrentUser => this.FUser ?? (this.FUser = this.Application.Settings.Users.NeedByAccount(this.UserAccount));

    public XmlNode ContextNode
    {
      get
      {
        if (this.FContextNode == null)
        {
          this.FContextNode = this.Application.Settings.SourceNode.SelectSingleNode("context[@name='" + this.CurrentUser.ContextName + "']");
          if (this.FContextNode == null)
            throw new DataException(string.Format("Не найден контекст пользователя \"{0}\".", (object) this.CurrentUser.ContextName));
        }
        return this.FContextNode;
      }
    }

    public InDbDatabase Db => this.FDb ?? (this.FDb = InDbManager.OpenDatabase(this.Application.Settings.DbConfig.DriverName, this.Application.Settings.DbConfig.Parameters));

    public ScriptLibraries ScriptLibraries => this.FScriptLibraries;

    protected virtual DataStorage CreateStorageInstance(MetadataClass cls) => new DataStorage(this, cls);

    public DataStorage this[MetadataClass cls]
    {
      get
      {
        DataStorage fstorage = this.FStorages[cls.Index];
        if (fstorage != null)
          return fstorage;
        DataStorage storageInstance = this.CreateStorageInstance(cls);
        this.FStorages[cls.Index] = storageInstance;
        return storageInstance;
      }
    }

    public DataStorage this[string className] => this[this.Application.Metadata.Classes.Need(className)];

    protected void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (this.FCentralServerConnection != null)
          this.FCentralServerConnection.Dispose();
        if (this.FScriptLibraries != null)
          this.FScriptLibraries.Dispose();
        if (this.FStorages != null)
        {
          for (int index = 0; index < this.FStorages.Length; ++index)
          {
            DataStorage fstorage = this.FStorages[index];
            if (fstorage != null)
            {
              this.FStorages[index] = (DataStorage) null;
              fstorage.Dispose();
            }
          }
        }
        if (this.FDb != null)
          this.FDb.Dispose();
        if (this.Disposed != null)
          this.Disposed((object) this, EventArgs.Empty);
      }
      this.FCentralServerConnection = (CentralServerConnection) null;
      this.FDb = (InDbDatabase) null;
      this.FScriptLibraries = (ScriptLibraries) null;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~DataSession() => this.Dispose(false);

    public bool PermanentCentralServerConnection
    {
      get => this.FCentralServerConnection.PermanentConnection;
      set => this.FCentralServerConnection.PermanentConnection = value;
    }

    public string GenerateRegNo()
    {
      int regNo = this.FCentralServerConnection.GenerateRegNo(this.Application.Id);
      XmlElement xmlElement = (XmlElement) this.Application.Settings.Config.SourceNode.SelectSingleNode("reg-no-prefix");
      string s = xmlElement != null ? xmlElement.InnerText : string.Empty;
      if (!StrUtils.IsNullOrEmpty(s))
        s = s.Trim() + (object) ':';
      return s + regNo.ToString("000 000 000");
    }

    public int GenerateCustomId(string generatorName) => this.FCentralServerConnection.GenerateCustomId(this.Application.Id, generatorName);

    public int[] GenerateCustomIds(string generatorName, int count) => this.FCentralServerConnection.GenerateCustomIds(this.Application.Id, generatorName, count);

    public void ReleaseCustomIds(string generatorName, int[] ids) => this.FCentralServerConnection.ReleaseCustomIds(this.Application.Id, generatorName, ids);

    public CustomIdGenerator[] GetCustomIdGenerators() => this.FCentralServerConnection.GetCustomIdGenerators(this.Application.Id);

    public IdGroupList GetIdGroups() => this.FCentralServerConnection.GetIdGroups(this.Application.Id);

    public IdConverter GetIdConverter(IdGroupList sourceGroups) => this.FCentralServerConnection.GetIdConverter(this.Application.Id, sourceGroups);

    public DataId GenerateId() => this.FCentralServerConnection.GenerateId(this.Application.Id);

    public DataId[] GenerateIds(int count) => this.FCentralServerConnection.GenerateIds(this.Application.Id, count);

    public void LoadData(
      LoadPlan plan,
      DataObjectList dstObjs,
      DataId[] ids,
      string condition,
      params object[] paramArray)
    {
      if (this.MasterSession != null)
      {
        DataObjectList dstObjs1 = new DataObjectList();
        this.MasterSession.LoadData(plan, dstObjs1, ids, condition, paramArray);
        foreach (DataObject dataObject1 in (ArrayList) dstObjs1)
        {
          if (!dataObject1.IsDeleted)
          {
            DataObject dataObject2 = this.EnsureSessionObject(dataObject1);
            if (!dataObject2.IsDeleted)
              dstObjs.Add((object) dataObject2);
          }
        }
      }
      else
      {
        Loader loader = new Loader(plan);
        if (ids != null)
          loader.Load(this, dstObjs, ids);
        else
          loader.Load(this, dstObjs, condition, paramArray);
      }
    }

    private void ResolveUnknowns(DataStorage storage, DataSession.CommitErrorList errors)
    {
      List<DataObject> dataObjectList = new List<DataObject>();
      foreach (DataObject update in storage.GetUpdateQueue())
      {
        if (!update.SessionStateContainsAny(ObjectSessionState.New | ObjectSessionState.Existing))
          dataObjectList.Add(update);
      }
      if (dataObjectList.Count == 0)
        return;
      StringBuilder stringBuilder = new StringBuilder(string.Format("SELECT [{0}] FROM [{1}] WHERE {0} IN (", (object) storage.Class.IDProperty.DataField, (object) storage.Class.DataTable), Math.Min(dataObjectList.Count, 100) * 13);
      int length = stringBuilder.Length;
      int index1 = 0;
      while (index1 < dataObjectList.Count)
      {
        stringBuilder.Length = length;
        for (int index2 = 0; index1 < dataObjectList.Count && index2 < 100; ++index2)
        {
          DataObject dataObject = dataObjectList[index1];
          if (index2 > 0)
            stringBuilder.Append(",");
          stringBuilder.Append("'").Append(dataObject.Id.ToString()).Append("'");
          ++index1;
        }
        stringBuilder.Append(")");
        using (InDbCommand command = this.Db.CreateCommand(stringBuilder.ToString()))
        {
          using (IDataReader dataReader = command.ExecuteReader())
          {
            while (dataReader.Read())
              storage.GetCachedObject(new DataId(dataReader.GetString(0))).IncludeSessionState(ObjectSessionState.Existing);
          }
        }
      }
      for (int index2 = 0; index2 < dataObjectList.Count; ++index2)
      {
        DataObject dataObject = dataObjectList[index2];
        if (!dataObject.SessionStateContainsAny(ObjectSessionState.Existing))
        {
          if (dataObject.IsDeleted)
            errors.AddCritical(string.Format("Удаление несуществующего объекта {0}", (object) dataObject.SystemView));
          else if (dataObject.SessionStateContainsAny(ObjectSessionState.PropertiesModified))
            errors.AddCritical(string.Format("Обновление несуществующего объекта {0}", (object) dataObject.SystemView));
          else
            errors.AddWarning(string.Format("Получение несуществующего объекта {0}", (object) dataObject.SystemView));
          dataObject.IncludeSessionState(ObjectSessionState.Error);
        }
      }
    }

    private bool HasStorageUpdates(IEnumerable<DataStorage> storages)
    {
      this.GenerateIdsForNewObjectsWithoutGeneratedId();
      foreach (DataStorage storage in storages)
      {
        if (storage.HasUpdates)
          return true;
      }
      return false;
    }

    public bool HasUpdates => this.HasStorageUpdates(this.GetInstantiatedStorages());

    public void Commit(DataSession.CommitErrorList errors)
    {
      if (errors == null)
        errors = new DataSession.CommitErrorList();
      IEnumerable<DataStorage> instantiatedStorages = this.GetInstantiatedStorages();
      if (!this.HasStorageUpdates(instantiatedStorages))
        return;
      foreach (DataStorage storage in instantiatedStorages)
        this.ResolveUnknowns(storage, errors);
      if (!this.CheckRefIntegrity)
        ;
      if (errors.HasCritical)
        return;
      SessionCommitPlan sessionCommitPlan = new SessionCommitPlan(instantiatedStorages);
      if (this.MasterSession != null)
      {
        sessionCommitPlan.CommitToMasterSession();
      }
      else
      {
        this.Db.BeginTransaction();
        try
        {
          if (this.FUpdateLogMode != UpdateLogMode.None)
          {
            using (UpdateLogWriter updateLogWriter = new UpdateLogWriter(this.Application))
            {
              InDbDatabase dbForOriginalValues = this.FUpdateLogMode == UpdateLogMode.ChangesWithOriginalValues ? this.Db : (InDbDatabase) null;
              ListDictionary listDictionary = new ListDictionary()
              {
                {
                  (object) "Stack",
                  (object) DataSession.GetStackDetailsForUpdateLog()
                },
                {
                  (object) "Updates",
                  (object) sessionCommitPlan.GetUpdateInfo().ToLogJson(dbForOriginalValues)
                }
              };
              updateLogWriter.Append(this.UserAccount, (object) listDictionary);
            }
          }
          sessionCommitPlan.CommitToDb();
        }
        catch
        {
          this.Db.RollbackTransaction();
          throw;
        }
        this.Db.CommitTransaction();
        this.TraceCommitToDbFinished();
        this.OnBeforeCommitToMemory();
      }
      if (errors.HasCritical)
        throw new DataException("При фиксации изменений возникли следующие ошибки:\n" + (object) errors);
      sessionCommitPlan.CommitToMemory();
    }

    private static ArrayList GetStackDetailsForUpdateLog()
    {
      ArrayList arrayList = new ArrayList();
      foreach (StackFrame frame in new StackTrace().GetFrames())
      {
        MethodBase method = frame.GetMethod();
        arrayList.Add((object) (method.DeclaringType.FullName + "." + method.Name));
      }
      arrayList.RemoveAt(0);
      arrayList.RemoveAt(0);
      arrayList.RemoveAt(0);
      return arrayList;
    }

    public void Commit() => this.Commit((DataSession.CommitErrorList) null);

    public void Rollback()
    {
      this.FNewObjectsWithoutGeneratedId.Clear();
      for (int index1 = 0; index1 < this.FStorages.Length; ++index1)
      {
        DataStorage fstorage = this.FStorages[index1];
        if (fstorage != null)
        {
          List<DataObject> dataObjectList = new List<DataObject>(fstorage.GetUpdateQueue());
          for (int index2 = 0; index2 < dataObjectList.Count; ++index2)
            dataObjectList[index2].Rollback();
          fstorage.ClearUpdateQueue();
        }
      }
    }

    public void DropCache()
    {
      for (int index = 0; index < this.FStorages.Length; ++index)
        this.FStorages[index]?.DropCache();
    }

    public void SaveChangesToXml(XmlNode node)
    {
      this.GenerateIdsForNewObjectsWithoutGeneratedId();
      bool flag = false;
      for (int index = 0; index < this.FStorages.Length; ++index)
      {
        DataStorage fstorage = this.FStorages[index];
        if (fstorage != null && fstorage.HasUpdates)
        {
          flag = true;
          break;
        }
      }
      if (!flag)
        return;
      for (int index = 0; index < this.FStorages.Length; ++index)
      {
        DataStorage fstorage = this.FStorages[index];
        if (fstorage != null)
          DataSession.SaveStorageChangesToXml(fstorage, node);
      }
    }

    private static void SaveStorageChangesToXml(DataStorage storage, XmlNode node)
    {
      foreach (DataObject update in storage.GetUpdateQueue())
      {
        if (!update.IsDeleted || !update.IsNew)
        {
          XmlNode xmlNode = (XmlNode) XmlUtils.AppendElement(node, storage.Class.IdentName);
          XmlUtils.SetAttr(xmlNode, "Id", update.Id.ToString());
          if (update.IsDeleted)
          {
            XmlUtils.SetAttr(xmlNode, "Action", "Delete");
          }
          else
          {
            if (update.IsNew)
              XmlUtils.SetAttr(xmlNode, "Action", "Create");
            IndexSet propertyIndexSet = update.GetPropertyIndexSet(PropertyStateFilter.Modified);
            if (propertyIndexSet.IsEmpty && !update.IsNew)
            {
              xmlNode.ParentNode.RemoveChild(xmlNode);
            }
            else
            {
              for (int index = 0; index < storage.Class.Properties.Count; ++index)
              {
                MetadataProperty property = storage.Class.Properties[index];
                if (property.IsUserField && propertyIndexSet[property.Index])
                {
                  XmlNode node1 = (XmlNode) XmlUtils.AppendElement(xmlNode, property.Name);
                  DataProperty dataProperty = update[property];
                  if (dataProperty.IsNull)
                    node1.InnerText = "{{null}}";
                  else if (property.IsLink)
                  {
                    DataObject dataObject = ((LinkProperty) dataProperty).Value;
                    if (DataObject.Assigned(dataObject))
                    {
                      XmlUtils.SetAttr(node1, "RefClass", dataObject.Class.Name);
                      node1.InnerText = dataObject.Id.ToString();
                    }
                    else
                      node1.InnerText = "{{null}}";
                  }
                  else
                    node1.InnerText = XStrUtils.ToXStr(dataProperty.UntypedValue);
                }
              }
            }
          }
        }
      }
    }

    private static void SetIdForNewObject(DataObject obj, DataId id)
    {
      obj.SetNewId(id);
      DataStorage storage = obj.Storage;
      storage.AddObjectToCache(obj);
      storage.EnqueueObjectForUpdate(obj);
    }

    internal void GenerateIdsForNewObjectsWithoutGeneratedId()
    {
      switch (this.FNewObjectsWithoutGeneratedId.Count)
      {
        case 0:
          return;
        case 1:
          DataSession.SetIdForNewObject(this.FNewObjectsWithoutGeneratedId[0], this.GenerateId());
          break;
        default:
          DataId[] ids = this.GenerateIds(this.FNewObjectsWithoutGeneratedId.Count);
          for (int index = 0; index < ids.Length; ++index)
            DataSession.SetIdForNewObject(this.FNewObjectsWithoutGeneratedId[index], ids[index]);
          break;
      }
      this.FNewObjectsWithoutGeneratedId.Clear();
    }

    public void QueryCachedObjects(MetadataClass cls, DataObjectList list)
    {
      DataStorage fstorage = this.FStorages[cls.Index];
      if (fstorage != null)
        list.AddRange(fstorage.GetCachedObjects());
      for (int index = 0; index < this.FNewObjectsWithoutGeneratedId.Count; ++index)
      {
        DataObject dataObject = this.FNewObjectsWithoutGeneratedId[index];
        if (dataObject.Class == cls)
          list.Add((object) dataObject);
      }
    }

    public void QueryCachedObjects(DataObjectList list)
    {
      for (int index = 0; index < this.FStorages.Length; ++index)
      {
        DataStorage fstorage = this.FStorages[index];
        if (fstorage != null)
          list.AddRange(fstorage.GetCachedObjects());
      }
      list.AddRange((ICollection) this.FNewObjectsWithoutGeneratedId);
    }

    private static DataObject[] QueryExternalLinkObjects(
      DataObject obj,
      MetadataAssociationRef assRef)
    {
      DataObjectList dataObjectList = obj.Session[assRef.Association.Class].Query("", string.Format("{0}='{1}'", (object) assRef.Association.Property.DataField, (object) obj.Id));
      DataObject[] dataObjectArray = new DataObject[dataObjectList.Count];
      dataObjectList.CopyTo((Array) dataObjectArray);
      return dataObjectArray;
    }

    private static DataObjectExternalLink[] QueryObjectExternalLinks(
      DataObject obj)
    {
      List<DataObjectExternalLink> objectExternalLinkList = new List<DataObjectExternalLink>();
      foreach (MetadataClass metadataClass in obj.Class.Metadata.Classes)
      {
        foreach (MetadataProperty property in metadataClass.Properties)
        {
          if (property.IsUserField && property.IsLink)
          {
            foreach (MetadataAssociationRef metadataAssociationRef in property.Association.Refs)
            {
              if (metadataAssociationRef.RefClass == obj.Class)
                objectExternalLinkList.Add(new DataObjectExternalLink(metadataAssociationRef, DataSession.QueryExternalLinkObjects(obj, metadataAssociationRef)));
            }
          }
        }
      }
      return objectExternalLinkList.ToArray();
    }

    public DataObjectExternalLinks[] QueryExternalLinks(
      params DataObject[] objects)
    {
      DataObjectExternalLinks[] objectExternalLinksArray = new DataObjectExternalLinks[objects.Length];
      for (int index = 0; index < objects.Length; ++index)
      {
        DataObject dataObject = objects[index];
        objectExternalLinksArray[index] = new DataObjectExternalLinks(dataObject, DataSession.QueryObjectExternalLinks(dataObject));
      }
      return objectExternalLinksArray;
    }

    public string GetContextStr(string xPath, string defaultValue)
    {
      XmlNode xmlNode = this.ContextNode.SelectSingleNode(xPath);
      return xmlNode != null ? xmlNode.InnerText : defaultValue;
    }

    public string GetContextStr(string xPath) => this.GetContextStr(xPath, string.Empty);

    public bool ContextContains(string xPath) => this.ContextNode.SelectSingleNode(xPath) != null;

    public bool IsPolicyEnabled(string policyName) => this.ContextContains(policyName + "[@type='policy']");

    public void CheckPolicy(string policyName)
    {
      if (!this.IsPolicyEnabled(policyName))
        throw new DataException(string.Format("У пользователя \"{0}\" нет прав на выполнение функции \"{1}\".", (object) this.CurrentUser.Account, (object) this.GetPolicyCaption(policyName)));
    }

    public string GetPolicyCaption(string policyName)
    {
      string str = Path.Combine(this.Application.Settings.RootFolder, "Meta\\_policies.xml");
      XmlNode node = (XmlNode) null;
      if (File.Exists(str))
        node = XmlUtils.LoadDocument(str, Encoding.GetEncoding(1251)).DocumentElement.SelectSingleNode("policy[@name='" + policyName + "']");
      return node != null ? XmlUtils.GetAttr(node, "caption", policyName) : policyName;
    }

    public void AddScriptLibs(ScriptControl control, string[] usingLibNames) => this.FScriptLibraries.AddScriptLibs(control, usingLibNames);

    public DataObject GetObjectFromRef(string objectRef)
    {
      if (string.IsNullOrEmpty(objectRef))
        return this.UntypedNullObject;
      int length = objectRef.IndexOf(':');
      DataStorage dataStorage = length >= 0 ? this[objectRef.Substring(length + 1)] : throw new DataException(string.Format("Некорректная текстовая ссылка на объект \"{0}\". Формат текстовой ссылки: \"идентификатор:класс\".", (object) objectRef));
      return length == 0 ? dataStorage.NullObject : dataStorage[new DataId(objectRef.Substring(0, length))];
    }

    internal static Totals QueryTotals(
      DataStorage storage,
      string columns,
      string condition,
      object[] args,
      string groupBy)
    {
      return new Integro.InMeta.Runtime.QueryTotals(storage, columns, condition, args, groupBy).Execute();
    }

    public DataSession CreateSlaveSession()
    {
      DataSession session = this.Application.CreateSession(this.UserAccount);
      session.MasterSession = this;
      return session;
    }

    public DataSession MasterSession
    {
      get => this.FMasterSession;
      set => this.FMasterSession = value;
    }

    public bool IgnoreErrorsInObjectViewVirtualProperties { get; set; }

    public UpdateLogMode UpdateLogMode
    {
      get => this.FUpdateLogMode;
      set => this.FUpdateLogMode = value;
    }

    public bool CheckRefIntegrity { get; set; }

    public DataObject EnsureSessionObject(DataObject obj)
    {
      if (obj == null)
        return (DataObject) null;
      if (obj.IsUntypedNull)
        return this.UntypedNullObject;
      if (obj.Session == this)
        return obj;
      DataStorage dataStorage = obj.Session.Application == this.Application ? this[obj.Class] : this[obj.Class.Name];
      return obj.IsNull ? dataStorage.NullObject : dataStorage[obj.Id];
    }

    public event EventHandler BeforeCommitToMemory;

    protected virtual void OnBeforeCommitToMemory()
    {
      if (this.BeforeCommitToMemory == null)
        return;
      this.BeforeCommitToMemory((object) this, EventArgs.Empty);
    }

    private static void CopyProperties(DataObject dst, DataObject src) => dst.CopyPropertiesFrom(src);

    public void CopyChangesFrom(DataSession outerSession)
    {
      if (!outerSession.HasUpdates)
        return;
      foreach (DataStorage fstorage in outerSession.FStorages)
      {
        if (fstorage != null)
        {
          DataStorage storage = this[fstorage.Class];
          foreach (DataObject update in fstorage.GetUpdateQueue())
          {
            DataId id = update.Id;
            if (update.IsNew && !storage.IsObjectCached(id))
            {
              DataObject objectInstance = storage.CreateObjectInstance();
              objectInstance.Init(storage, id, ObjectSessionState.Existing);
              storage.AddObjectToCache(objectInstance);
            }
          }
        }
      }
      foreach (DataStorage fstorage in outerSession.FStorages)
      {
        if (fstorage != null)
        {
          DataStorage dataStorage = this[fstorage.Class];
          foreach (DataObject update in fstorage.GetUpdateQueue())
          {
            DataId id = update.Id;
            if (update.IsDeleted && dataStorage.IsObjectCached(id))
            {
              DataObject cachedObject = dataStorage.GetCachedObject(update.Id);
              cachedObject.RemoveFromChildLists(false);
              cachedObject.SetSessionState(ObjectSessionState.Error);
              dataStorage.DequeueObjectFromUpdate(id);
            }
          }
          foreach (DataObject update in fstorage.GetUpdateQueue())
          {
            if (!update.IsDeleted && !update.IsError)
            {
              if (update.IsNew || update.IsPropertiesModified)
                DataSession.CopyProperties(dataStorage[update.Id], update);
              if (update.IsNew || update.IsAttachmentsModified)
                dataStorage[update.Id].Attachments.CopyChangesFrom(update.Attachments);
            }
          }
        }
      }
    }

    public event EventHandler<UnplannedPropertyLoadingEventArgs> UnplannedPropertyLoading;

    public event EventHandler<UnplannedChildrenLoadingEventArgs> UnplannedChildrenLoading;

    public void TraceUnplannedPropertyLoading(MetadataProperty property)
    {
      this.Application.Log.AppendRecord(0, this.UserAccount, "DataProperty.CheckReading", string.Format("Не загружено свойство объекта {0}.{1}.", (object) property.Class.Name, (object) property.Name));
      if (this.UnplannedPropertyLoading == null)
        return;
      this.UnplannedPropertyLoading((object) this, new UnplannedPropertyLoadingEventArgs(property));
    }

    internal void TraceUnplannedChildrenLoading(MetadataChildRef childRef)
    {
      this.Application.Log.AppendRecord(0, this.UserAccount, "DataObjectChildList.CheckComplete", string.Format("Не загружен список дочерних объектов {0}.{1}.", (object) childRef.AggregationRef.RefClass.Name, (object) childRef.ChildClass.Name));
      if (this.UnplannedChildrenLoading == null)
        return;
      this.UnplannedChildrenLoading((object) this, new UnplannedChildrenLoadingEventArgs(childRef));
    }

    internal void TraceQueryStarted(MetadataClass cls) => this.Application.Log.AppendRecord(0, this.UserAccount, string.Format("Storage[{0}].Query", (object) cls.Name), "Загрузка данных из БД.");

    internal void TraceQueryFinished() => this.Application.Log.AppendRecord(0, this.UserAccount, "", "Окончание загрузки из БД.");

    private void TraceCommitToDbFinished() => this.Application.Log.AppendRecord(0, this.UserAccount, "Session.Commit", "Сохранение изменений в БД.");

    internal void TraceDisposingFromWebPage(int logRecordId, Type webPageType) => this.Application.Log.AppendRecord(logRecordId, this.UserAccount, webPageType.FullName, "Data session disposed");

    internal int TraceCreationFromWebPage(Type webPageType)
    {
      int parentRecordId = this.Application.Log.AppendRecord(0, this.UserAccount, webPageType.FullName, string.Empty);
      this.Application.Log.AppendRecord(parentRecordId, this.UserAccount, webPageType.FullName, "Data session created");
      return parentRecordId;
    }

    internal void EnqueueIdGeneration(DataObject dataObject) => this.FNewObjectsWithoutGeneratedId.Add(dataObject);

    internal IEnumerable<DataStorage> GetInstantiatedStorages()
    {
      int length = 0;
      for (int index = 0; index < this.FStorages.Length; ++index)
      {
        if (this.FStorages[index] != null)
          ++length;
      }
      DataStorage[] dataStorageArray = new DataStorage[length];
      if (length > 0)
      {
        int num = 0;
        for (int index = 0; index < this.FStorages.Length; ++index)
        {
          DataStorage fstorage = this.FStorages[index];
          if (fstorage != null)
            dataStorageArray[num++] = fstorage;
        }
      }
      return (IEnumerable<DataStorage>) dataStorageArray;
    }

    public enum ErrorLevel
    {
      Warning,
      Critical,
    }

    public class CommitError
    {
      public readonly DataSession.ErrorLevel Level;
      public readonly string Text;

      internal CommitError(DataSession.ErrorLevel level, string text)
      {
        this.Level = level;
        this.Text = text;
      }

      internal void AddTo(StringBuilder sb) => sb.Append(this.Level == DataSession.ErrorLevel.Critical ? "Ошибка: " : "Предупреждение: ").Append(this.Text);
    }

    public class CommitErrorList : IEnumerable
    {
      private readonly List<DataSession.CommitError> FItems = new List<DataSession.CommitError>();
      private bool FHasWarnings;
      private bool FHasCritical;

      internal CommitErrorList()
      {
      }

      public bool HasWarnings => this.FHasWarnings;

      public bool HasCritical => this.FHasCritical;

      public int Count => this.FItems.Count;

      public DataSession.CommitError this[int index] => this.FItems[index];

      internal void AddWarning(string text)
      {
        this.FItems.Add(new DataSession.CommitError(DataSession.ErrorLevel.Warning, text));
        this.FHasWarnings = true;
      }

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < this.Count; ++index)
        {
          if (index > 0)
            sb.Append("\n");
          this[index].AddTo(sb);
        }
        return sb.ToString();
      }

      internal void AddCritical(string text)
      {
        this.FItems.Add(new DataSession.CommitError(DataSession.ErrorLevel.Critical, text));
        this.FHasCritical = true;
      }

      public IEnumerator GetEnumerator() => (IEnumerator) this.FItems.GetEnumerator();
    }
  }
}
