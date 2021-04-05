// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewVirtualPropertyLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Collections.Generic;

namespace Integro.InMeta.Runtime
{
  internal class ObjectViewVirtualPropertyLoader : ObjectViewParameterLoader
  {
    private readonly Navigator[] FNavigation;
    private readonly MemberValueGetter FMemberValueGetter;
    private readonly ObjectViewVirtualPropertyLoader.ValueCardinality FValueCardinality;

    public new static bool CanCreate(MetadataObjectView view, string name) => view.VirtualProperties.Find(name) != null;

    public static ObjectViewParameterLoader TryCreate(
      MetadataObjectView view,
      string name)
    {
      MetadataVirtualProperty metadataVirtualProperty = view.VirtualProperties.Find(name);
      if (metadataVirtualProperty == null)
        return (ObjectViewParameterLoader) null;
      MemberValueGetter memberValueGetter = metadataVirtualProperty.RefMemberType != MetadataClassMember.ObjectView ? (MemberValueGetter) new PropertyValueGetter(metadataVirtualProperty.RefMemberName) : (MemberValueGetter) new ObjectViewTextGetter(metadataVirtualProperty.RefMemberName);
      if (metadataVirtualProperty.SourceMember is MetadataAssociation sourceMember)
        return (ObjectViewParameterLoader) new ObjectViewVirtualPropertyLoader(new Navigator[1]
        {
          (Navigator) new AssociationNavigator(sourceMember.Property.Name, (MetadataClass) null)
        }, memberValueGetter, ObjectViewVirtualPropertyLoader.ValueCardinality.Single);
      if (!(metadataVirtualProperty.SourceMember is MetadataChildRef sourceMember))
        return (ObjectViewParameterLoader) new ObjectViewVirtualPropertyLoader(Navigator.ParseNavigation(view.Class, metadataVirtualProperty.SourceNavigation), memberValueGetter, ObjectViewVirtualPropertyLoader.ValueCardinality.Multiple);
      return (ObjectViewParameterLoader) new ObjectViewVirtualPropertyLoader(new Navigator[1]
      {
        (Navigator) new ChildRefNavigator(sourceMember.ChildClass)
      }, memberValueGetter, ObjectViewVirtualPropertyLoader.ValueCardinality.Multiple);
    }

    private ObjectViewVirtualPropertyLoader(
      Navigator[] navigation,
      MemberValueGetter memberValueGetter,
      ObjectViewVirtualPropertyLoader.ValueCardinality valueCardinality)
    {
      this.FNavigation = navigation;
      this.FMemberValueGetter = memberValueGetter;
      this.FValueCardinality = valueCardinality;
    }

    public override void PrepareLoadPlan(LoadPlan plan, DataSession session)
    {
      LoadPlanList loadPlanList1 = new LoadPlanList();
      loadPlanList1.Add(plan);
      LoadPlanList loadPlanList2 = loadPlanList1;
      foreach (Navigator navigator in this.FNavigation)
      {
        LoadPlanList navigationPlans = new LoadPlanList();
        foreach (LoadPlan sourcePlan in (List<LoadPlan>) loadPlanList2)
          navigator.GetNavigationLoadPlans(session, sourcePlan, navigationPlans);
        loadPlanList2 = navigationPlans;
      }
      foreach (LoadPlan plan1 in (List<LoadPlan>) loadPlanList2)
        this.FMemberValueGetter.PrepareLoadPlan(plan1, session);
    }

    public override object GetValue(DataObject obj)
    {
      DataObjectList dataObjectList1 = new DataObjectList();
      dataObjectList1.Add((object) obj);
      DataObjectList dataObjectList2 = dataObjectList1;
      foreach (Navigator navigator in this.FNavigation)
      {
        DataObjectList navigableObjects = new DataObjectList();
        foreach (DataObject sourceObject in (ArrayList) dataObjectList2)
          navigator.GetNavigableObjects(sourceObject, navigableObjects);
        dataObjectList2 = navigableObjects;
      }
      List<object> values = new List<object>();
      foreach (DataObject dataObject in (ArrayList) dataObjectList2)
        values.Add(this.FMemberValueGetter.GetValue(dataObject));
      return this.FValueCardinality.GetValue(values);
    }

    private abstract class ValueCardinality
    {
      public static readonly ObjectViewVirtualPropertyLoader.ValueCardinality Single = (ObjectViewVirtualPropertyLoader.ValueCardinality) new ObjectViewVirtualPropertyLoader.ValueCardinality.SingleCardinality();
      public static readonly ObjectViewVirtualPropertyLoader.ValueCardinality Multiple = (ObjectViewVirtualPropertyLoader.ValueCardinality) new ObjectViewVirtualPropertyLoader.ValueCardinality.MultipleCardinality();

      public abstract object GetValue(List<object> values);

      private class SingleCardinality : ObjectViewVirtualPropertyLoader.ValueCardinality
      {
        public override object GetValue(List<object> values) => values.Count > 0 ? values[0] : (object) null;
      }

      private class MultipleCardinality : ObjectViewVirtualPropertyLoader.ValueCardinality
      {
        public override object GetValue(List<object> values) => (object) values.ToArray();
      }
    }
  }
}
