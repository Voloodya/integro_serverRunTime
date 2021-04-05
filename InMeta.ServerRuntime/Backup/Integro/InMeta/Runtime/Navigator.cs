// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.Navigator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections.Generic;

namespace Integro.InMeta.Runtime
{
  internal abstract class Navigator
  {
    public static readonly Navigator[] EmptyNavigation = new Navigator[0];

    public abstract void GetNavigableObjects(
      DataObject sourceObject,
      DataObjectList navigableObjects);

    public abstract void GetNavigationLoadPlans(
      DataSession session,
      LoadPlan sourcePlan,
      LoadPlanList navigationPlans);

    public static Navigator[] ParseNavigation(MetadataClass sourceClass, string navigation)
    {
      List<Navigator> navigatorList = new List<Navigator>();
      foreach (SqlUtils.NavigationPathItem navigationPathItem in SqlUtils.NavigationPathItem.Parse(sourceClass.Metadata.Classes, navigation))
      {
        switch (navigationPathItem.Through)
        {
          case SqlUtils.NavigationThrough.Link:
            navigatorList.Add((Navigator) new AssociationNavigator(navigationPathItem.Name, navigationPathItem.RefClass));
            break;
          case SqlUtils.NavigationThrough.Child:
            navigatorList.Add((Navigator) new ChildRefNavigator(navigationPathItem.RefClass));
            break;
          case SqlUtils.NavigationThrough.ExternalLink:
            navigatorList.Add((Navigator) new ExternalLinkNavigator(navigationPathItem.RefClass.Properties.Need(navigationPathItem.Name).Association ?? throw new Exception(string.Format("Ошибка в навигационном пути \"{0}\": свойство \"{1}\" класса \"{2}\", используемое в навигации по обратной ссылке, не является ассоциацией.", (object) navigation, (object) navigationPathItem.Name, (object) navigationPathItem.RefClass.Name))));
            break;
        }
      }
      return navigatorList.ToArray();
    }
  }
}
