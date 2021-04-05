// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.AssociationNavigator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Integro.InMeta.Runtime
{
  internal class AssociationNavigator : Navigator
  {
    private readonly string FAssociationPropertyName;
    private readonly MetadataClass FRefClass;

    public AssociationNavigator(string associationPropertyName, MetadataClass refClass)
    {
      this.FAssociationPropertyName = associationPropertyName;
      this.FRefClass = refClass;
    }

    public override void GetNavigableObjects(
      DataObject sourceObject,
      DataObjectList navigableObjects)
    {
      DataObject link = sourceObject.GetLink(this.FAssociationPropertyName);
      if (!DataObject.Assigned(link) || this.FRefClass != null && link.Class != this.FRefClass)
        return;
      navigableObjects.Add((object) link);
    }

    public override void GetNavigationLoadPlans(
      DataSession session,
      LoadPlan sourcePlan,
      LoadPlanList navigationPlans)
    {
      sourcePlan.EnsureProperty(sourcePlan.Class.Properties.Need(this.FAssociationPropertyName));
      for (int index = 0; index < sourcePlan.Links.Count; ++index)
      {
        AssociationRefLoadPlan link = sourcePlan.Links[index];
        if (!(link.Ref.Association.Property.Name != this.FAssociationPropertyName))
        {
          if (this.FRefClass != null && link.Ref.RefClass != this.FRefClass)
            break;
          navigationPlans.Add(link.Plan);
        }
      }
    }
  }
}
