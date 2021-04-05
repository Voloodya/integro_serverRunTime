// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ChildRefNavigator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;

namespace Integro.InMeta.Runtime
{
  internal class ChildRefNavigator : Navigator
  {
    private readonly MetadataClass FChildClass;

    public ChildRefNavigator(MetadataClass childClass) => this.FChildClass = childClass;

    public override void GetNavigableObjects(
      DataObject sourceObject,
      DataObjectList navigableObjects)
    {
      foreach (object child in sourceObject.GetChilds(this.FChildClass.Name))
        navigableObjects.Add(child);
    }

    public override void GetNavigationLoadPlans(
      DataSession session,
      LoadPlan sourcePlan,
      LoadPlanList navigationPlans)
    {
      MetadataChildRef childRef = sourcePlan.Class.Childs.Need(this.FChildClass);
      foreach (ChildRefLoadPlan child in (ArrayList) sourcePlan.Childs)
      {
        if (child.ChildRef == childRef)
        {
          navigationPlans.Add(child.Plan);
          return;
        }
      }
      navigationPlans.Add(sourcePlan.EnsureChildRef(childRef, new LoadPlan(this.FChildClass)).Plan);
    }
  }
}
