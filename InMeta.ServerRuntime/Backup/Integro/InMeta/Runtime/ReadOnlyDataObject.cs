// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ReadOnlyDataObject
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(true)]
  public class ReadOnlyDataObject
  {
    private readonly DataObject FData;

    public ReadOnlyDataObject(DataObject data) => this.FData = data;

    public string Id => this.FData.Id.ToString();

    public bool IsNull => this.FData == null || this.FData.IsNull;

    public object GetValue(string propertyPath)
    {
      DataProperty dataProperty = this.FData.SelectSingleProperty(propertyPath);
      if (dataProperty == null || dataProperty.IsNull)
        return (object) DBNull.Value;
      switch (dataProperty)
      {
        case LinkProperty linkProperty:
          return (object) linkProperty.Value.Id;
        case CurrencyProperty currencyProperty:
          return (object) new CurrencyWrapper(currencyProperty.Value);
        default:
          return dataProperty.UntypedValue;
      }
    }

    public ReadOnlyDataObject GetLink(string propertyPath)
    {
      DataProperty dataProperty = this.FData.SelectSingleProperty(propertyPath);
      if (dataProperty == null)
        return new ReadOnlyDataObject(this.FData.Session.UntypedNullObject);
      return dataProperty is LinkProperty linkProperty ? new ReadOnlyDataObject(linkProperty.Value) : throw new InMetaException(string.Format("Ошибка получения связанного объекта (ThisObject.GetLink): свойство {0} не является ассоциацией.", (object) propertyPath));
    }

    public string GetView(string viewPath) => this.FData.GetView(viewPath);

    public int GetChildCount(string childClassName) => this.FData.GetChilds(childClassName).Count;

    public ReadOnlyDataObject GetChild(string childClassName, int index) => new ReadOnlyDataObject(this.FData.GetChilds(childClassName)[index]);

    public object[] GetChildArray(string childClassName)
    {
      DataObjectChildList childs = this.FData.GetChilds(childClassName);
      object[] objArray = new object[childs.Count];
      for (int index = 0; index < objArray.Length; ++index)
        objArray[index] = (object) new ReadOnlyDataObject(childs[index]);
      return objArray;
    }

    public object[] GetSortedChildArray(string childClassName, string sortProperties)
    {
      DataObjectList dataObjectList = new DataObjectList((ICollection) this.FData.GetChilds(childClassName));
      dataObjectList.Sort(sortProperties);
      object[] objArray = new object[dataObjectList.Count];
      for (int index = 0; index < objArray.Length; ++index)
        objArray[index] = (object) new ReadOnlyDataObject(dataObjectList[index]);
      return objArray;
    }
  }
}
