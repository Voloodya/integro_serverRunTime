// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ExternalLinkNavigator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;

namespace Integro.InMeta.Runtime
{
  internal class ExternalLinkNavigator : Navigator
  {
    private readonly MetadataAssociation FExternalAssociation;

    internal ExternalLinkNavigator(MetadataAssociation externalAssociation) => this.FExternalAssociation = externalAssociation;

    public override void GetNavigableObjects(
      DataObject sourceObject,
      DataObjectList navigableObjects)
    {
      DataSession session = sourceObject.Session;
      DataObject[] dataObjectArray = new DataObject[1]
      {
        sourceObject
      };
      foreach (DataObjectExternalLink link in session.QueryExternalLinks(dataObjectArray)[0].Links)
      {
        if (link.Ref.Association == this.FExternalAssociation)
          navigableObjects.AddRange((ICollection) link.Objects);
      }
    }

    public override void GetNavigationLoadPlans(
      DataSession session,
      LoadPlan sourcePlan,
      LoadPlanList navigationPlans)
    {
    }
  }
}
