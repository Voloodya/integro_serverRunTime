// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataObjectChildList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataObjectChildList : IDataObjectList, ICollection, IEnumerable
  {
    private readonly ArrayList FItems;
    public readonly DataObject Object;
    public readonly MetadataChildRef ChildRef;
    private bool FIsComplete;
    private bool FIsModified;

    private void CheckWriting()
    {
      string str = (string) null;
      if (this.Object.IsDeleted)
        str = "удаленного объекта";
      else if (this.Object.IsError)
        str = "несуществующего объекта";
      else if (this.Object.IsNull)
        str = "пустого объекта";
      if (str != null)
        throw new DataException("Попытка изменения списка дочерних объектов " + str);
    }

    public void CopyTo(Array array, int index)
    {
      this.CheckComplete();
      this.FItems.CopyTo(array, index);
    }

    public object SyncRoot => this.FItems.SyncRoot;

    public bool IsSynchronized => this.FItems.IsSynchronized;

    protected internal DataObjectChildList(DataObject owner, MetadataChildRef childRef)
    {
      this.Object = owner;
      this.ChildRef = childRef;
      this.FIsComplete = owner.IsNew;
      this.FItems = new ArrayList();
    }

    public void SetCompleted(bool value) => this.FIsComplete = value;

    private void CheckComplete()
    {
      if (this.FIsComplete)
        return;
      if (this.Object.IsNull)
      {
        this.FIsComplete = true;
      }
      else
      {
        DataStorage storage = this.Object.Storage;
        DataSession masterSession = this.Session.MasterSession;
        if (masterSession != null)
        {
          DataObjectChildList childs = masterSession[storage.Class][this.Object.Id].GetChilds(this.ChildRef);
          DataStorage dataStorage = this.Session[this.ChildRef.ChildClass];
          MetadataProperty property = this.ChildRef.AggregationRef.Association.Property;
          for (int index = 0; index < childs.Count; ++index)
          {
            DataObject untypedValue = (DataObject) dataStorage[childs[index].Id][property].UntypedValue;
          }
          this.FIsComplete = true;
        }
        else if (storage.IsChildListCompleted(this.ChildRef))
        {
          this.FIsComplete = true;
        }
        else
        {
          this.Session.TraceUnplannedChildrenLoading(this.ChildRef);
          LoadPlan plan = new LoadPlan(storage.Class);
          plan.EnsureChildRef(this.ChildRef, new LoadPlan(this.ChildRef.ChildClass));
          storage.Session.LoadData(plan, (DataObjectList) null, new DataId[1]
          {
            this.Object.Id
          }, (string) null);
        }
      }
    }

    private void SetModified(bool value)
    {
      this.FIsModified = value;
      if (!value || this.Object.SessionStateContainsAny(ObjectSessionState.ChildsModified))
        return;
      this.Object.IncludeSessionState(ObjectSessionState.ChildsModified);
      if (!this.Object.IsNew)
        this.Object.Storage.EnqueueObjectForUpdate(this.Object);
    }

    internal void AppendChild(DataObject child)
    {
      this.CheckWriting();
      this.FItems.Add((object) child);
      this.SetModified(true);
    }

    internal void RemoveChild(DataObject child, bool checkWriting)
    {
      if (!this.Object.IsError && checkWriting)
        this.CheckWriting();
      this.FItems.Remove((object) child);
      this.SetModified(true);
    }

    internal void AttachChild(DataObject child, bool isLoading)
    {
      if (isLoading)
      {
        if (this.FItems.Contains((object) child))
          return;
        this.FItems.Add((object) child);
      }
      else
        this.AppendChild(child);
    }

    internal void DetachChild(DataObject child, bool isLoading)
    {
      if (isLoading)
      {
        if (!this.FItems.Contains((object) child))
          return;
        this.FItems.Remove((object) child);
      }
      else
        this.RemoveChild(child, true);
    }

    public DataObject AddNew()
    {
      this.CheckWriting();
      DataObject dataObject = this.Session[this.ChildRef.ChildClass].AddNew();
      dataObject[this.ChildRef.AggregationRef.Association.Property].UntypedValue = (object) this.Object;
      return dataObject;
    }

    public DataSession Session => this.Object.Session;

    internal void Commit() => this.FIsModified = false;

    internal void Rollback()
    {
      if (!this.FIsModified)
        return;
      this.FIsModified = false;
      this.FIsComplete = false;
      this.FItems.Clear();
    }

    internal void DropCache()
    {
      MetadataProperty property = this.ChildRef.AggregationRef.Association.Property;
      for (int index = this.FItems.Count - 1; index >= 0; --index)
      {
        if (!((DataObject) this.FItems[index]).IsPropertyModified(property))
          this.FItems.RemoveAt(index);
      }
      this.FIsComplete = false;
    }

    public void DeleteAll()
    {
      this.CheckComplete();
      this.CheckWriting();
      if (this.FItems.Count <= 0)
        return;
      ArrayList arrayList = new ArrayList((ICollection) this.FItems);
      for (int index = 0; index < arrayList.Count; ++index)
        ((DataObject) arrayList[index]).Delete();
      this.FItems.Clear();
      this.SetModified(true);
    }

    public IEnumerator GetEnumerator()
    {
      this.CheckComplete();
      return this.FItems.GetEnumerator();
    }

    public DataObject[] ToArray()
    {
      this.CheckComplete();
      DataObject[] dataObjectArray = new DataObject[this.FItems.Count];
      for (int index = 0; index < this.FItems.Count; ++index)
        dataObjectArray[index] = (DataObject) this.FItems[index];
      return dataObjectArray;
    }

    public void Sort(string propNames)
    {
      this.CheckComplete();
      this.FItems.Sort((IComparer) new ObjByPropsComparer(propNames));
    }

    public int Count
    {
      get
      {
        this.CheckComplete();
        return this.FItems.Count;
      }
    }

    public DataObject this[int index]
    {
      get
      {
        this.CheckComplete();
        return (DataObject) this.FItems[index];
      }
    }

    public DataObject First
    {
      get
      {
        this.CheckComplete();
        return this.FItems.Count > 0 ? (DataObject) this.FItems[0] : this.Session[this.ChildRef.ChildClass].NullObject;
      }
    }
  }
}
