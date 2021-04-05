// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataObjectList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataObjectList : ArrayList, IDataObjectList, IEnumerable
  {
    private void CheckItem(object obj)
    {
      if (obj != null && !this.GetAcceptableType().IsAssignableFrom(obj.GetType()))
        throw new DataException(string.Format("Недопустимый объект {0} для списка объектов {1}.", (object) obj.GetType().Name, (object) this.GetType().Name));
    }

    protected virtual Type GetAcceptableType() => typeof (DataObject);

    public DataObject this[int index]
    {
      get => (DataObject) base[index];
      set
      {
        this.CheckItem((object) value);
        this[index] = (object) value;
      }
    }

    public DataObjectList()
    {
    }

    public DataObjectList(int capacity)
      : base(capacity)
    {
    }

    public DataObjectList(ICollection c)
      : base(c)
    {
    }

    public void Sort(string propNames)
    {
      if (this.Count <= 0)
        return;
      this.Sort((IComparer) new ObjByPropsComparer(propNames));
    }
  }
}
