// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjByPropsComparer
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Collections;

namespace Integro.InMeta.Runtime
{
  internal class ObjByPropsComparer : IComparer
  {
    private readonly string[] FPropPaths;
    private readonly bool[] FDescs;

    public ObjByPropsComparer(string propNames) => StrUtils.ParseOrderBy(propNames, ref this.FPropPaths, ref this.FDescs);

    private static int ApplyDesc(int cmpResult, bool desc) => !desc || cmpResult == 0 ? cmpResult : (cmpResult > 0 ? -1 : 1);

    private static bool PropIsNull(DataProperty prop) => prop == null || prop.IsNull;

    public int Compare(object x, object y)
    {
      DataObject dataObject1 = (DataObject) x;
      DataObject dataObject2 = (DataObject) y;
      for (int index = 0; index < this.FPropPaths.Length; ++index)
      {
        string fpropPath = this.FPropPaths[index];
        bool fdesc = this.FDescs[index];
        if (string.Compare(fpropPath, "ID", true) == 0)
          return ObjByPropsComparer.ApplyDesc(DataId.Compare(dataObject1.Id, dataObject2.Id), fdesc);
        DataProperty prop1 = dataObject1.SelectSingleProperty(fpropPath);
        DataProperty prop2 = dataObject2.SelectSingleProperty(fpropPath);
        if (ObjByPropsComparer.PropIsNull(prop1))
          return ObjByPropsComparer.ApplyDesc(ObjByPropsComparer.PropIsNull(prop2) ? 0 : -1, fdesc);
        if (ObjByPropsComparer.PropIsNull(prop2))
          return ObjByPropsComparer.ApplyDesc(1, fdesc);
        int cmpResult = ((IComparable) prop1.UntypedValue).CompareTo(prop2.UntypedValue);
        if (cmpResult != 0)
          return ObjByPropsComparer.ApplyDesc(cmpResult, fdesc);
      }
      return 0;
    }
  }
}
