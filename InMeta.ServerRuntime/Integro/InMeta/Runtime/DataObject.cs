// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataObject
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Web;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataObject : IComparable
  {
    private DataStorage FStorage;
    private DataId FId;
    private DataProperty[] FProperties;
    private DataObjectChildList[] FChilds;
    private ObjectSessionState FSessionState;
    private ObjectFileAttachments FAttachments;

    protected internal DataObject()
    {
    }

    internal void Init(DataStorage storage, DataId id, ObjectSessionState state)
    {
      this.FStorage = storage;
      this.FId = id;
      this.FSessionState = state;
    }

    protected internal DataProperty CreatePropertyInstance(
      MetadataProperty propertyMetadata)
    {
      switch (propertyMetadata.Purpose)
      {
        case MetadataPropertyPurpose.Data:
          DataType dataType = propertyMetadata.DataType & DataType.BaseMask;
          switch (dataType)
          {
            case DataType.Boolean:
              return (DataProperty) new BooleanProperty(this, propertyMetadata);
            case DataType.Integer:
              return (DataProperty) new IntegerProperty(this, propertyMetadata);
            case DataType.Float:
              return (DataProperty) new DoubleProperty(this, propertyMetadata);
            case DataType.Currency:
              return (DataProperty) new CurrencyProperty(this, propertyMetadata);
            case DataType.DateTime:
              return (DataProperty) new DateTimeProperty(this, propertyMetadata);
            case DataType.String:
              return (DataProperty) new StringProperty(this, propertyMetadata);
            case DataType.Memo:
              return (DataProperty) new StringProperty(this, propertyMetadata);
            case DataType.Binary:
              return (DataProperty) new BinaryProperty(this, propertyMetadata);
            default:
              throw new DataException(string.Format("Некорректный тип данных {0} свойства {1} объекта {2}", (object) dataType, (object) propertyMetadata.Name, (object) this.FStorage.Class.Name));
          }
        case MetadataPropertyPurpose.Association:
        case MetadataPropertyPurpose.Aggregation:
          MetadataAssociationRefList refs = propertyMetadata.Association.Refs;
          return propertyMetadata.Association.Selector == null ? (DataProperty) this.Session[refs[0].RefClass].CreateLinkPropertyInstance(this, propertyMetadata) : (DataProperty) new LinkProperty(this, propertyMetadata);
        default:
          throw new System.Data.DataException("Попытка создания объекта доступа к свойству ID.");
      }
    }

    private DataProperty EnsurePropertyInstance(MetadataProperty propertyMetadata)
    {
      this.CheckNotUntypedNull();
      DataProperty[] dataPropertyArray = this.FProperties;
      if (dataPropertyArray == null)
      {
        dataPropertyArray = new DataProperty[this.Class.Properties.Count];
        this.FProperties = dataPropertyArray;
      }
      DataProperty propertyInstance = dataPropertyArray[propertyMetadata.Index];
      if (propertyInstance == null)
      {
        propertyInstance = this.CreatePropertyInstance(propertyMetadata);
        dataPropertyArray[propertyMetadata.Index] = propertyInstance;
      }
      return propertyInstance;
    }

    internal void LoadPropertyValue(
      MetadataProperty propertyMetadata,
      object value,
      object exValue,
      LoadContext loadContext)
    {
      DataProperty dataProperty = this.EnsurePropertyInstance(propertyMetadata);
      if (dataProperty.IsModified)
        return;
      dataProperty.LoadValueFromDb(value, exValue, loadContext);
    }

    internal int SavePropertyValue(
      MetadataProperty metadataProperty,
      ref object value,
      ref object exValue)
    {
      return this.EnsurePropertyInstance(metadataProperty).SaveValueToDb(ref value, ref exValue);
    }

    internal void LoadNotAssignedProperties()
    {
      this.CheckNotUntypedNull();
      LoadPlan plan = new LoadPlan(this.FStorage.Class);
      MetadataPropertyList properties = this.Class.Properties;
      DataProperty[] dataPropertyArray = this.FProperties ?? (this.FProperties = new DataProperty[properties.Count]);
      for (int index = 0; index < properties.Count; ++index)
      {
        MetadataProperty propertyMetadata = properties[index];
        if (!propertyMetadata.IsId)
        {
          DataProperty dataProperty = dataPropertyArray[index];
          if (dataProperty == null || !dataProperty.IsAssigned)
            plan.EnsureProperty(propertyMetadata);
        }
      }
      this.Session.LoadData(plan, (DataObjectList) null, new DataId[1]
      {
        this.Id
      }, (string) null);
    }

    public DataProperty this[MetadataProperty propertyMetadata] => this.EnsurePropertyInstance(propertyMetadata);

    public DataProperty this[string propertyName] => this[this.Class.Properties.Need(propertyName)];

    public ObjectSessionState SessionState => this.FSessionState;

    internal bool SessionStateContainsAny(ObjectSessionState states) => (this.SessionState & states) != (ObjectSessionState) 0;

    internal bool SessionStateContainsAll(ObjectSessionState states) => (this.SessionState & states) == states;

    internal void IncludeSessionState(ObjectSessionState state) => this.FSessionState |= state;

    internal void ExcludeSessionState(ObjectSessionState state) => this.FSessionState &= ~state;

    internal void SetSessionState(ObjectSessionState state) => this.FSessionState = state;

    public bool IsNull => this.FStorage == null || (this.FSessionState & ObjectSessionState.NullObject) != (ObjectSessionState) 0;

    public bool IsNew => (this.FSessionState & ObjectSessionState.New) != (ObjectSessionState) 0;

    public bool IsDeleted => (this.FSessionState & ObjectSessionState.Deleted) != (ObjectSessionState) 0;

    public bool IsPropertiesModified => (this.FSessionState & ObjectSessionState.PropertiesModified) != (ObjectSessionState) 0;

    public bool IsAttachmentsModified => (this.FSessionState & ObjectSessionState.AttachmentsModified) != (ObjectSessionState) 0;

    public bool IsChildsModified => (this.FSessionState & ObjectSessionState.ChildsModified) != (ObjectSessionState) 0;

    internal void InvalidStateForOperation(string errorText)
    {
      string str = !this.SessionStateContainsAny(ObjectSessionState.Error) ? (!this.SessionStateContainsAny(ObjectSessionState.Deleted) ? (!this.SessionStateContainsAny(ObjectSessionState.NullObject) ? (!this.SessionStateContainsAny(ObjectSessionState.New) ? this.SessionState.ToString() : "Новый объект") : "Нулевой объект") : "Объект удален") : "Объект не существует";
      throw new Exception(string.Format("{0}. {1}.\n{2}", (object) errorText, (object) str, (object) this.SystemView));
    }

    public DataId Id
    {
      get
      {
        if (this.FId.IsEmpty && (this.FSessionState & ObjectSessionState.NullObject) == (ObjectSessionState) 0)
          this.Session.GenerateIdsForNewObjectsWithoutGeneratedId();
        return this.FId;
      }
    }

    public DataStorage Storage => this.FStorage;

    public MetadataClass Class => this.Storage.Class;

    public bool IsPropertyModified(MetadataProperty propertyMetadata)
    {
      if (this.FProperties == null)
        return false;
      DataProperty fproperty = this.FProperties[propertyMetadata.Index];
      return fproperty != null && fproperty.IsModified;
    }

    public bool IsPropertyAssigned(MetadataProperty propertyMetadata)
    {
      if (this.FProperties == null)
        return false;
      DataProperty fproperty = this.FProperties[propertyMetadata.Index];
      return fproperty != null && fproperty.IsAssigned;
    }

    public DataProperty SelectSingleProperty(string path)
    {
      DataObject dataObject = this;
      int startIndex = 0;
      while (true)
      {
        if (DataObject.Assigned(dataObject))
        {
          int num = path.IndexOf('.', startIndex);
          if (num >= 0)
          {
            string memberName = path.Substring(startIndex, num - startIndex);
            MetadataProperty property;
            MetadataChildRef childRef;
            dataObject.Class.NeedMember(memberName, out property, out childRef);
            if (property != null)
            {
              dataObject = ((LinkProperty) dataObject[property]).Value;
            }
            else
            {
              DataObjectChildList childs = dataObject.GetChilds(childRef);
              if (childs.Count != 0)
                dataObject = childs[0];
              else
                goto label_6;
            }
            startIndex = num + 1;
          }
          else
            goto label_9;
        }
        else
          break;
      }
      return (DataProperty) null;
label_6:
      return (DataProperty) null;
label_9:
      if (startIndex > 0)
        path = path.Substring(startIndex);
      return dataObject[path];
    }

    private DataProperty NeedSingleProperty(string propertyPath) => this.SelectSingleProperty(propertyPath) ?? throw new DataException(string.Format("Не задано значение \"{0}\".", (object) propertyPath));

    private object GetUntypedValue(string propertyPath)
    {
      DataProperty dataProperty = this.SelectSingleProperty(propertyPath);
      return dataProperty == null || dataProperty.IsNull ? (object) null : dataProperty.UntypedValue;
    }

    private object NeedNotNullUntypedValue(string propertyPath) => this.NeedSingleProperty(propertyPath).NotNullUntypedValue;

    public string GetString(string propertyPath, string defaultValue)
    {
      object untypedValue = this.GetUntypedValue(propertyPath);
      return untypedValue != null ? Convert.ToString(untypedValue) : defaultValue;
    }

    public string NeedString(string propertyPath) => Convert.ToString(this.NeedNotNullUntypedValue(propertyPath));

    public string GetString(string propertyPath) => this.GetString(propertyPath, string.Empty);

    public void SetString(string propertyPath, string value) => this.NeedSingleProperty(propertyPath).UntypedValue = (object) value;

    public int NeedInteger(string propertyPath) => Convert.ToInt32(this.NeedNotNullUntypedValue(propertyPath));

    public int GetInteger(string propertyPath, int defaultValue)
    {
      object untypedValue = this.GetUntypedValue(propertyPath);
      return untypedValue != null ? Convert.ToInt32(untypedValue) : defaultValue;
    }

    public int GetInteger(string propertyPath) => this.GetInteger(propertyPath, 0);

    public void SetInteger(string propertyPath, int value) => this.NeedSingleProperty(propertyPath).UntypedValue = (object) value;

    public double NeedDouble(string propertyPath) => Convert.ToDouble(this.NeedNotNullUntypedValue(propertyPath));

    public double GetDouble(string propertyPath, double defaultValue)
    {
      object untypedValue = this.GetUntypedValue(propertyPath);
      return untypedValue != null ? Convert.ToDouble(untypedValue) : defaultValue;
    }

    public double GetDouble(string propertyPath) => this.GetDouble(propertyPath, 0.0);

    public void SetDouble(string propertyPath, double value) => this.NeedSingleProperty(propertyPath).UntypedValue = (object) value;

    public Decimal NeedDecimal(string propertyPath) => Convert.ToDecimal(this.NeedNotNullUntypedValue(propertyPath));

    public Decimal GetDecimal(string propertyPath, Decimal defaultValue)
    {
      object untypedValue = this.GetUntypedValue(propertyPath);
      return untypedValue != null ? Convert.ToDecimal(untypedValue) : defaultValue;
    }

    public Decimal GetDecimal(string propertyPath) => this.GetDecimal(propertyPath, 0M);

    public void SetDecimal(string propertyPath, Decimal value) => this.NeedSingleProperty(propertyPath).UntypedValue = (object) value;

    public bool NeedBoolean(string propertyPath) => Convert.ToBoolean(this.NeedNotNullUntypedValue(propertyPath));

    public bool GetBoolean(string propertyPath, bool defaultValue)
    {
      object untypedValue = this.GetUntypedValue(propertyPath);
      return untypedValue != null ? Convert.ToBoolean(untypedValue) : defaultValue;
    }

    public bool GetBoolean(string propertyPath) => this.GetBoolean(propertyPath, false);

    public void SetBoolean(string propertyPath, bool value) => this.NeedSingleProperty(propertyPath).UntypedValue = (object) value;

    public DateTime NeedDateTime(string propertyPath) => Convert.ToDateTime(this.NeedNotNullUntypedValue(propertyPath));

    public DateTime GetDateTime(string propertyPath, DateTime defaultValue)
    {
      object untypedValue = this.GetUntypedValue(propertyPath);
      return untypedValue != null ? Convert.ToDateTime(untypedValue) : defaultValue;
    }

    public DateTime GetDateTime(string propertyPath) => this.GetDateTime(propertyPath, DateTime.MinValue);

    public void SetDateTime(string propertyPath, DateTime value) => this.NeedSingleProperty(propertyPath).UntypedValue = (object) value;

    public DataObject GetLink(string propertyPath)
    {
      DataProperty dataProperty = this.SelectSingleProperty(propertyPath);
      return dataProperty != null ? ((LinkProperty) dataProperty).Value : this.Session.UntypedNullObject;
    }

    public void SetLink(string propertyPath, DataObject value) => ((LinkProperty) this.NeedSingleProperty(propertyPath)).Value = value;

    public DataObjectChildList GetChilds(MetadataChildRef childRef)
    {
      this.CheckNotUntypedNull();
      this.CheckNotError();
      DataObjectChildList[] dataObjectChildListArray = this.FChilds ?? (this.FChilds = new DataObjectChildList[this.Class.Childs.Count]);
      return dataObjectChildListArray[childRef.Index] ?? (dataObjectChildListArray[childRef.Index] = this.Session[childRef.ChildClass].CreateChildListInstance(this, childRef));
    }

    private void CheckNotError()
    {
      if (this.SessionStateContainsAny(ObjectSessionState.Error))
        throw new DataException(string.Format("Ошибка доступа к данным: объект {0}[{1}]удален из базы данных.", this.FStorage != null ? (object) this.FStorage.Class.Name : (object) string.Empty, (object) this.FId));
    }

    public DataObjectChildList GetChilds(string childClassName)
    {
      this.CheckNotUntypedNull();
      return this.GetChilds(this.Class.Childs.Need(this.Class.Metadata.Classes.Need(childClassName)));
    }

    internal void RemoveChild(DataObject child, bool checkWriting)
    {
      DataObjectChildList[] fchilds = this.FChilds;
      if (fchilds == null)
        return;
      MetadataChildRef metadataChildRef = this.Class.Childs.Need(child.Class);
      fchilds[metadataChildRef.Index]?.RemoveChild(child, checkWriting);
    }

    public ObjectFileAttachments Attachments => this.FAttachments ?? (this.FAttachments = new ObjectFileAttachments(this));

    public bool SameObject(DataObject obj) => DataObject.SameObjects(this, obj);

    public static bool SameObjects(DataObject obj1, DataObject obj2)
    {
      if (object.ReferenceEquals((object) obj1, (object) obj2))
        return true;
      if (!DataObject.Assigned(obj1))
        return !DataObject.Assigned(obj2);
      return !DataObject.Assigned(obj2) ? !DataObject.Assigned(obj1) : obj1.Id == obj2.Id;
    }

    private MetadataObjectView GetMetadataObjectView(string viewName)
    {
      if (string.IsNullOrEmpty(viewName))
        viewName = "default";
      return this.Class.ObjectViews.Need(viewName);
    }

    private string GetView(MetadataObjectView objectView) => this.FStorage.GetObjectViewEvaluator(objectView).GetObjectViewText(this);

    public string GetView(string viewName)
    {
      if (this.IsNull)
        return string.Empty;
      this.CheckNotError();
      return this.GetView(this.GetMetadataObjectView(viewName));
    }

    public string GetViewHtml(string viewName)
    {
      if (this.IsNull)
        return string.Empty;
      this.CheckNotError();
      MetadataObjectView metadataObjectView = this.GetMetadataObjectView(viewName);
      string view = this.GetView(metadataObjectView);
      return metadataObjectView.ContentType == ContentType.Text ? HttpUtility.HtmlEncode(view) : view;
    }

    public void Delete()
    {
      if (this.SessionStateContainsAny(ObjectSessionState.Error | ObjectSessionState.NullObject))
        this.InvalidStateForOperation("Ошибка удаления объекта.");
      if (this.IsDeleted)
        return;
      this.RemoveFromChildLists(true);
      for (int index1 = 0; index1 < this.Class.Childs.Count; ++index1)
      {
        DataObjectList dataObjectList = new DataObjectList((ICollection) this.GetChilds(this.Class.Childs[index1]));
        for (int index2 = dataObjectList.Count - 1; index2 >= 0; --index2)
          dataObjectList[index2].Delete();
      }
      this.Attachments.DeleteAll();
      this.Storage.EnqueueObjectForUpdate(this);
      this.IncludeSessionState(ObjectSessionState.Deleted);
    }

    internal void RemoveFromChildLists(bool checkWriting)
    {
      DataProperty[] fproperties = this.FProperties;
      if (fproperties == null)
        return;
      for (int index = 0; index < fproperties.Length; ++index)
      {
        DataProperty dataProperty = fproperties[index];
        if (dataProperty != null && dataProperty.IsAssigned && dataProperty.Metadata.IsAggregation)
        {
          DataObject untypedValue = (DataObject) dataProperty.UntypedValue;
          if (!untypedValue.IsNull)
            untypedValue.RemoveChild(this, checkWriting);
        }
      }
    }

    public string SystemView
    {
      get
      {
        if (!this.IsNull)
          return string.Format("{0}[{1}] ({2})", (object) this.Class.Name, (object) this.Id, (object) this.Class.Caption);
        return this.Class == null ? "Нетипизированный нулевой объект" : string.Format("{0}[нулевой объект] ({1})", (object) this.Class.Name, (object) this.Class.Caption);
      }
    }

    private static bool IsAcceptable(DataProperty prop, PropertyStateFilter filter) => prop != null && !prop.Metadata.IsId && !prop.Metadata.IsSelector && (filter == PropertyStateFilter.Assigned ? prop.IsAssigned : prop.IsModified);

    internal IndexSet GetPropertyIndexSet(PropertyStateFilter filter)
    {
      IndexSet indexSet = new IndexSet();
      DataProperty[] fproperties = this.FProperties;
      if (fproperties != null)
      {
        for (int index = 0; index < fproperties.Length; ++index)
        {
          if (DataObject.IsAcceptable(fproperties[index], filter))
            indexSet[index] = true;
        }
      }
      return indexSet;
    }

    internal void Commit()
    {
      DataId id = this.Id;
      if (this.FAttachments != null)
        this.FAttachments.Commit();
      if (this.IsDeleted)
      {
        this.Storage.RemoveObjectFromCache(id);
        this.SetSessionState(ObjectSessionState.Error);
      }
      else
      {
        if (this.IsNew)
        {
          this.ExcludeSessionState(ObjectSessionState.New);
          this.IncludeSessionState(ObjectSessionState.Existing);
        }
        if (this.IsPropertiesModified)
        {
          foreach (DataProperty fproperty in this.FProperties)
            fproperty?.Commit();
          this.ExcludeSessionState(ObjectSessionState.PropertiesModified);
        }
        if (!this.IsChildsModified)
          return;
        foreach (DataObjectChildList fchild in this.FChilds)
          fchild?.Commit();
        this.ExcludeSessionState(ObjectSessionState.ChildsModified);
      }
    }

    internal void Rollback()
    {
      DataId id = this.Id;
      if (this.FAttachments != null)
        this.FAttachments.Rollback();
      if (this.IsNew)
      {
        this.Storage.RemoveObjectFromCache(id);
        this.SetSessionState(ObjectSessionState.Error);
      }
      else
      {
        if (this.IsDeleted)
          this.ExcludeSessionState(ObjectSessionState.Deleted);
        if (this.IsPropertiesModified)
        {
          foreach (DataProperty fproperty in this.FProperties)
            fproperty?.Rollback();
          this.ExcludeSessionState(ObjectSessionState.PropertiesModified);
        }
        if (!this.IsChildsModified)
          return;
        foreach (DataObjectChildList fchild in this.FChilds)
          fchild?.Rollback();
        this.ExcludeSessionState(ObjectSessionState.ChildsModified);
      }
    }

    internal void DropCache()
    {
      if (this.FProperties != null)
      {
        for (int index = 0; index < this.FProperties.Length; ++index)
          this.FProperties[index]?.DropCache();
      }
      if (this.FChilds == null)
        return;
      for (int index = 0; index < this.FChilds.Length; ++index)
        this.FChilds[index]?.DropCache();
    }

    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      return obj is DataObject dataObject ? this.Id.CompareTo(dataObject.Id) : throw new ArgumentException(string.Format("Сравниваемый объект {0} не является реестровым объектом.", (object) obj.GetType()));
    }

    internal bool IsUnknown => !this.SessionStateContainsAny(ObjectSessionState.New | ObjectSessionState.Existing | ObjectSessionState.Error | ObjectSessionState.NullObject);

    public bool IsError
    {
      get
      {
        if (this.IsUnknown)
          this.LoadNotAssignedProperties();
        return this.SessionStateContainsAny(ObjectSessionState.Error);
      }
    }

    public bool IsUntypedNull => this.FStorage == null;

    public static bool Assigned(DataObject obj) => obj != null && !obj.IsNull;

    public static string GetObjectRef(DataObject obj)
    {
      if (obj == null || obj.IsUntypedNull)
        return string.Empty;
      return obj.IsNull ? ':'.ToString() + obj.Class.Name : obj.Id.ToString() + (object) ':' + obj.Class.Name;
    }

    private void CheckNotUntypedNull()
    {
      if (this.FStorage == null)
        throw new DataException("Недопустимое использование не типизированного нулевого объекта");
    }

    internal void CheckNotNull()
    {
      if (this.IsNull)
        throw new DataException("Недопустимое использование нулевого объекта");
    }

    internal void MarkModified(ObjectSessionState state)
    {
      if (this.SessionStateContainsAny(state))
        return;
      this.IncludeSessionState(state);
      if (!this.IsNew)
        this.Storage.EnqueueObjectForUpdate(this);
    }

    internal void SetNewId(DataId id) => this.FId = id;

    public DataSession Session => this.Storage.Session;

    internal void ClearSessionState() => this.FSessionState = (ObjectSessionState) 0;

    internal void CopyPropertiesFrom(DataObject src)
    {
      if (src.FProperties == null)
        return;
      for (int index = 0; index < src.FProperties.Length; ++index)
      {
        DataProperty fproperty = src.FProperties[index];
        if (fproperty != null && fproperty.Metadata.IsUserField && fproperty.IsModified)
        {
          DataProperty dataProperty = this[fproperty.Metadata];
          if (!dataProperty.IsModified)
          {
            if (dataProperty.Metadata.IsLink)
              dataProperty.LoadValue((object) this.Storage.Session.EnsureSessionObject((DataObject) fproperty.UntypedValue));
            else if (fproperty.IsNull)
              dataProperty.LoadValue((object) null);
            else
              dataProperty.LoadValue(fproperty.GetValue());
          }
        }
      }
    }

    public abstract class Surrogate : SurrogateSelector, ISerializationSurrogate
    {
      protected abstract bool IsSupportedType(Type type);

      public override ISerializationSurrogate GetSurrogate(
        Type type,
        StreamingContext context,
        out ISurrogateSelector selector)
      {
        if (!this.IsSupportedType(type))
          return base.GetSurrogate(type, context, out selector);
        selector = (ISurrogateSelector) this;
        return (ISerializationSurrogate) this;
      }

      public virtual void GetObjectData(
        object obj,
        SerializationInfo info,
        StreamingContext context)
      {
        throw new NotImplementedException();
      }

      public virtual object SetObjectData(
        object obj,
        SerializationInfo info,
        StreamingContext context,
        ISurrogateSelector selector)
      {
        throw new NotImplementedException();
      }
    }

    [ComVisible(false)]
    public class SerializationSurrogate : DataObject.Surrogate
    {
      protected override bool IsSupportedType(Type type) => type == typeof (DataObject) || type.IsSubclassOf(typeof (DataObject));

      public override void GetObjectData(
        object obj,
        SerializationInfo info,
        StreamingContext context)
      {
        info.SetType(typeof (DataObjectRef));
        DataObject dataObject = (DataObject) obj;
        if (dataObject == null || dataObject.IsNull)
        {
          info.AddValue("Class", (object) string.Empty);
          info.AddValue("Id", (object) string.Empty);
        }
        else
        {
          info.AddValue("Class", (object) dataObject.Class.Name);
          info.AddValue("Id", (object) dataObject.Id.ToString());
        }
      }
    }

    public class DeserializationSurrogate : DataObject.Surrogate
    {
      private readonly DataSession FSession;

      public DeserializationSurrogate(DataSession session) => this.FSession = session;

      protected override bool IsSupportedType(Type type) => type == typeof (DataObjectRef);

      public override object SetObjectData(
        object obj,
        SerializationInfo info,
        StreamingContext context,
        ISurrogateSelector selector)
      {
        string className = info.GetString("Class");
        return className == string.Empty ? (object) null : (object) this.FSession[className].GetObject(new DataId(info.GetString("Id")));
      }
    }
  }
}
