// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.UpdatePlan
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;

namespace Integro.InMeta.Runtime
{
  internal class UpdatePlan : List<ObjectsByPropertyIndexSet>
  {
    private readonly PropertyStateFilter FFilter;

    public UpdatePlan(PropertyStateFilter filter) => this.FFilter = filter;

    public void AddObject(DataObject obj)
    {
      IndexSet propertyIndexSet1 = obj.GetPropertyIndexSet(this.FFilter);
      for (int index = 0; index < this.Count; ++index)
      {
        ObjectsByPropertyIndexSet propertyIndexSet2 = this[index];
        if (propertyIndexSet2.IndexSet.Equals(propertyIndexSet1))
        {
          propertyIndexSet2.Add(obj);
          return;
        }
      }
      ObjectsByPropertyIndexSet propertyIndexSet3 = new ObjectsByPropertyIndexSet(propertyIndexSet1);
      propertyIndexSet3.Add(obj);
      this.Add(propertyIndexSet3);
    }
  }
}
