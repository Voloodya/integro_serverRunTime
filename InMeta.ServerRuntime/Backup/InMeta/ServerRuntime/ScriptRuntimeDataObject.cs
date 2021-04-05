// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.ScriptRuntimeDataObject
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using System;

namespace InMeta.ServerRuntime
{
  public class ScriptRuntimeDataObject
  {
    internal readonly DataObject FData;

    public ScriptRuntimeDataObject(DataObject data) => this.FData = data;

    public string Id => this.FData.Id.ToString();

    public string ClassName => this.FData.IsUntypedNull ? string.Empty : this.FData.Class.Name;

    public bool IsNull => this.FData.IsNull;

    public bool IsNew => this.FData.IsNew;

    public bool IsDeleted => this.FData.IsDeleted;

    private static bool DateTimeHasNoTimePart(DateTime value) => value == value.Date;

    public static object GetPropertyTypedValue(DataObject obj, string propertyName)
    {
      DataProperty dataProperty = obj.SelectSingleProperty(propertyName);
      if (dataProperty == null || dataProperty.IsNull)
        return (object) DBNull.Value;
      object untypedValue = dataProperty.UntypedValue;
      if (dataProperty.Metadata.IsLink)
      {
        DataObject dataObject = (DataObject) untypedValue;
        return DataObject.Assigned(dataObject) ? (object) dataObject.Id.ToString() : (object) DBNull.Value;
      }
      if (untypedValue is Decimal)
        untypedValue = (object) Convert.ToDouble(untypedValue);
      return untypedValue ?? (object) DBNull.Value;
    }

    public static string GetPropertyText(DataObject obj, string propertyName)
    {
      object propertyTypedValue = ScriptRuntimeDataObject.GetPropertyTypedValue(obj, propertyName);
      if (propertyTypedValue == DBNull.Value)
        return string.Empty;
      return propertyTypedValue is DateTime dateTime && ScriptRuntimeDataObject.DateTimeHasNoTimePart(dateTime) ? dateTime.ToString("d") : Convert.ToString(propertyTypedValue);
    }

    public static string GetPropertyDisplayText(DataObject obj, string propertyName)
    {
      string propertyText = ScriptRuntimeDataObject.GetPropertyText(obj, propertyName);
      MetadataLookupValue metadataLookupValue = obj.SelectSingleProperty(propertyName).Metadata.LookupValues.Find(propertyText);
      return metadataLookupValue != null ? metadataLookupValue.Caption : propertyText;
    }

    public bool IsPropertyNull(string propertyName)
    {
      DataProperty dataProperty = this.FData.SelectSingleProperty(propertyName);
      return dataProperty == null || dataProperty.IsNull;
    }

    public bool IsPropertyModified(string propertyName)
    {
      DataProperty dataProperty = this.FData.SelectSingleProperty(propertyName);
      return dataProperty != null && dataProperty.IsModified;
    }

    public object GetValue(string propertyName) => ScriptRuntimeDataObject.GetPropertyTypedValue(this.FData, propertyName);

    public void SetValue(string propertyName, object value)
    {
      DataProperty dataProperty = this.FData.SelectSingleProperty(propertyName);
      if (dataProperty == null)
        throw new Exception(string.Format("Ошибка изменения значение свойства \"{0}\" объекта \"{1}\": указанное свойство не существует.", (object) propertyName, (object) this.FData.SystemView));
      dataProperty.UntypedValue = value;
    }

    public ScriptRuntimeDataObject GetLink(string propertyName) => new ScriptRuntimeDataObject(this.FData.GetLink(propertyName));

    public void SetLink(string propertyName, ScriptRuntimeDataObject value)
    {
      DataProperty dataProperty = this.FData.SelectSingleProperty(propertyName);
      if (dataProperty == null)
        throw new Exception(string.Format("Ошибка изменения связи \"{0}\" объекта \"{1}\": указанное свойство не существует.", (object) propertyName, (object) this.FData.SystemView));
      if (!dataProperty.Metadata.IsLink)
        throw new Exception(string.Format("Ошибка изменения связи \"{0}\" объекта \"{1}\": указанное свойство не является ассоциацией.", (object) propertyName, (object) this.FData.SystemView));
      ((LinkProperty) dataProperty).Value = value?.FData;
    }

    public ScriptRuntimeDataObjectChildList GetChildren(
      string className)
    {
      return new ScriptRuntimeDataObjectChildList(this.FData.GetChilds(className));
    }
  }
}
