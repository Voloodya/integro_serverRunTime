// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public abstract class DataProperty
  {
    public readonly MetadataProperty Metadata;
    public readonly DataObject Object;
    private static readonly object NotAssigned = new object();
    private object FValue;
    private bool FModified;

    public DataProperty(DataObject obj, MetadataProperty metadata)
    {
      this.Object = obj;
      this.Metadata = metadata;
      this.FValue = DataProperty.NotAssigned;
      this.FModified = false;
    }

    internal static object EscapeFromDBNull(object value) => value == DBNull.Value ? (object) null : value;

    private void CheckReading()
    {
      if (this.Object.SessionStateContainsAny(ObjectSessionState.Error | ObjectSessionState.Deleted))
        this.Object.InvalidStateForOperation(string.Format("Ошибка получения {0}({1})", (object) this.Metadata.Name, (object) this.Metadata.Caption));
      if (this.FValue != DataProperty.NotAssigned)
        return;
      if (this.Object.IsNull)
        this.FValue = (object) null;
      else if (this.Object.IsNew)
      {
        this.FValue = (object) null;
      }
      else
      {
        DataSession masterSession = this.Session.MasterSession;
        if (masterSession != null)
        {
          this.Load(masterSession[this.Object.Storage.Class][this.Object.Id][this.Metadata]);
        }
        else
        {
          this.Session.TraceUnplannedPropertyLoading(this.Metadata);
          this.Object.LoadNotAssignedProperties();
        }
        if (this.Object.SessionStateContainsAny(ObjectSessionState.Error))
          this.Object.InvalidStateForOperation(string.Format("Ошибка получения {0}({1})", (object) this.Metadata.Name, (object) this.Metadata.Caption));
      }
    }

    internal void Load(DataProperty property)
    {
      if (this.Metadata.IsLink)
      {
        if (!property.Metadata.IsLink)
          throw new DataException("Ошибка копирования значения свойства: несовместимые типы.");
        this.LoadValue((object) this.Session.EnsureSessionObject((DataObject) property.UntypedValue));
      }
      else
      {
        if (property.Metadata.DataType != this.Metadata.DataType)
          throw new DataException("Ошибка копирования значения свойства: несовместимые типы.");
        if (property.IsNull)
          this.LoadValue((object) null);
        else
          this.LoadValue(property.UntypedValue);
      }
    }

    public void Assign(DataProperty property)
    {
      if (this.Metadata.IsLink)
      {
        if (!property.Metadata.IsLink)
          throw new DataException("Ошибка копирования значения свойства: несовместимые типы.");
        this.UntypedValue = (object) this.Session.EnsureSessionObject((DataObject) property.UntypedValue);
      }
      else
      {
        if (property.Metadata.DataType != this.Metadata.DataType)
          throw new DataException("Ошибка копирования значения свойства: несовместимые типы.");
        if (property.IsNull)
          this.Clear();
        else
          this.UntypedValue = property.UntypedValue;
      }
    }

    public DataSession Session => this.Object.Session;

    private void CheckWriting()
    {
      if (!this.Object.SessionStateContainsAny(ObjectSessionState.Error | ObjectSessionState.Deleted | ObjectSessionState.NullObject))
        return;
      this.Object.InvalidStateForOperation(string.Format("Ошибка изменения {0}({1})", (object) this.Metadata.Name, (object) this.Metadata.Caption));
    }

    internal virtual void LoadValueFromDb(object value, object exValue, LoadContext loadContext) => this.SetValue(DataProperty.EscapeFromDBNull(value));

    internal virtual void LoadValue(object value) => this.SetValue(DataProperty.EscapeFromDBNull(value));

    internal virtual int SaveValueToDb(ref object value, ref object exValue)
    {
      if (this.FValue == DataProperty.NotAssigned)
        throw new DataException("Попытка записать в БД неприсвоенное значение.");
      value = this.FValue ?? (object) DBNull.Value;
      return 1;
    }

    internal virtual object GetValue() => this.FValue;

    internal virtual void SetValue(object value) => this.FValue = value;

    public void Clear() => this.UntypedValue = (object) null;

    public string SystemView => string.Format("Объект: {0}\nСвойство: {1} ({2})", (object) this.Object.SystemView, (object) this.Metadata.Name, (object) this.Metadata.Caption);

    public override string ToString()
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : untypedValue.ToString();
    }

    public virtual string ToString(IFormatProvider provider) => this.ToString();

    public virtual string ToString(string format) => this.ToString();

    public object UntypedValue
    {
      get
      {
        this.CheckReading();
        return this.GetValue();
      }
      set
      {
        this.CheckWriting();
        this.SetValue(DataProperty.EscapeFromDBNull(value));
        this.FModified = true;
        this.Object.MarkModified(ObjectSessionState.PropertiesModified);
      }
    }

    public bool IsModified => this.FModified;

    protected internal abstract object ZeroValue { get; }

    public object NotNullUntypedValue
    {
      get
      {
        this.CheckReading();
        if (!this.IsNull)
          return this.GetValue();
        if (this.Session.TreatNullsAsZero)
          return this.ZeroValue;
        throw new DataException(string.Format("Не задано значение свойства '{0}' объекта '{1}'.", (object) this.Metadata.Caption, (object) this.Object.Class.Caption));
      }
    }

    public bool IsNull
    {
      get
      {
        this.CheckReading();
        return this.FValue == null;
      }
    }

    internal bool IsAssigned => this.FValue != DataProperty.NotAssigned;

    internal void Commit() => this.FModified = false;

    internal void Rollback()
    {
      if (this.FValue == DataProperty.NotAssigned || !this.FModified)
        return;
      this.FValue = DataProperty.NotAssigned;
      this.FModified = false;
    }

    internal void DropCache()
    {
      if (this.FModified)
        return;
      this.FValue = DataProperty.NotAssigned;
    }
  }
}
