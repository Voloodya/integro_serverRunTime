// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.LinkProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class LinkProperty : DataProperty
  {
    public LinkProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    internal override int SaveValueToDb(ref object value, ref object exValue)
    {
      if (!this.IsAssigned)
        throw new DataException("Попытка записать в БД неприсвоенное значение.");
      MetadataAssociationRefList refs = this.Metadata.Association.Refs;
      DataObject dataObject = (DataObject) base.GetValue();
      if (dataObject != null && !dataObject.IsNull)
      {
        value = (object) dataObject.Id.ToString();
        if (this.Metadata.Association.Selector != null)
          exValue = InDbUtils.Convert((object) refs.Need(dataObject.Class).SelectorValue, this.Metadata.Association.Selector.DataType);
      }
      return this.Metadata.Association.Selector != null ? 2 : 1;
    }

    internal override void LoadValueFromDb(object value, object exValue, LoadContext loadContext)
    {
      DataId id = new DataId((string) DataProperty.EscapeFromDBNull(value));
      DataObject dataObject = (DataObject) null;
      DataObjectChildList dataObjectChildList = (DataObjectChildList) null;
      if (!id.IsEmpty)
      {
        MetadataAssociationRefList refs = this.Metadata.Association.Refs;
        MetadataAssociationRef metadataAssociationRef = this.Metadata.Association.Selector == null ? refs[0] : refs.FindBySelectorValue(exValue);
        if (metadataAssociationRef == null)
          throw new DataException(string.Format("Нарушение целостности данных.\n{0}\nОшибка: Невозможно определить класс связанного объекта по значению селектора '{1}'.\n", (object) this.SystemView, exValue));
        dataObject = this.Session[metadataAssociationRef.RefClass].EnsureCacheItem(id);
        if (dataObject.IsNull)
          dataObject = (DataObject) null;
        else if (this.Metadata.IsAggregation)
        {
          dataObjectChildList = dataObject.GetChilds(metadataAssociationRef.OwnerChildRef);
          if ((loadContext & LoadContext.FetchAllObjects) != (LoadContext) 0)
            dataObjectChildList.SetCompleted(true);
        }
      }
      base.SetValue((object) dataObject);
      dataObjectChildList?.AttachChild(this.Object, true);
    }

    private void AssignValue(object value, bool isLoading)
    {
      bool isAggregation = this.Metadata.IsAggregation;
      DataObject dataObject1 = !isAggregation || !this.IsAssigned ? (DataObject) null : (DataObject) this.GetValue();
      MetadataAssociationRef metadataAssociationRef1 = (MetadataAssociationRef) null;
      DataObject dataObject2 = (DataObject) value;
      if (!isLoading)
        dataObject2 = this.Session.EnsureSessionObject(dataObject2);
      if (DataObject.Assigned(dataObject2))
      {
        metadataAssociationRef1 = this.Metadata.Association.Refs.Find(dataObject2.Class);
        if (metadataAssociationRef1 == null)
          throw new DataException(string.Format("Попытка присвоить объект '{0}' свойству '{1}' объекта '{2}'", (object) dataObject2.Class.Name, (object) this.Metadata.Name, (object) this.Object.Class.Name));
      }
      else
        dataObject2 = (DataObject) null;
      base.SetValue((object) dataObject2);
      if (!isAggregation || DataObject.SameObjects(dataObject2, dataObject1))
        return;
      if (DataObject.Assigned(dataObject1))
      {
        MetadataAssociationRef metadataAssociationRef2 = this.Metadata.Association.Refs.Need(dataObject1.Class);
        dataObject1.GetChilds(metadataAssociationRef2.OwnerChildRef).DetachChild(this.Object, isLoading);
      }
      if (DataObject.Assigned(dataObject2))
        dataObject2.GetChilds(metadataAssociationRef1.OwnerChildRef).AttachChild(this.Object, isLoading);
    }

    internal override void LoadValue(object value) => this.AssignValue(value, true);

    internal override void SetValue(object value) => this.AssignValue(value, false);

    internal override object GetValue()
    {
      object obj = base.GetValue();
      if (obj != null)
        return obj;
      MetadataAssociationRefList refs = this.Metadata.Association.Refs;
      return this.Metadata.Association.Selector == null ? (object) this.Session[refs[0].RefClass].NullObject : (object) this.Session.UntypedNullObject;
    }

    public DataObject Value
    {
      get => (DataObject) this.UntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public override string ToString() => this.ToString(string.Empty);

    public override string ToString(string viewName) => this.Value.GetView(viewName);

    protected internal override object ZeroValue => this.UntypedValue;
  }
}
