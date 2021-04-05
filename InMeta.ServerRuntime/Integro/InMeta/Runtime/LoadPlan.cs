// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.LoadPlan
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class LoadPlan
  {
    public readonly MetadataClass Class;
    public readonly MetadataPropertyList Data;
    public readonly AssociationRefLoadPlanList Links;
    public readonly ChildRefLoadPlanList Childs;
    public readonly MetadataObjectViewList Views;

    public LoadPlan(MetadataClass cls)
    {
      this.Class = cls;
      this.Data = new MetadataPropertyList();
      this.Links = new AssociationRefLoadPlanList();
      this.Childs = new ChildRefLoadPlanList();
      this.Views = new MetadataObjectViewList(cls);
    }

    public void EnsureDataProperty(MetadataProperty propertyMetadata)
    {
      if (!propertyMetadata.IsData)
        throw new System.Data.DataException(string.Format("DataLoadPlan.AddData: Свойство {0}.{1}, добавляемое в план загрузки простых типов, являться ассоциацией. Используйте метод AddLink.", (object) propertyMetadata.Class.QTypeName, (object) propertyMetadata.Name));
      if (this.Data.Contains(propertyMetadata))
        return;
      this.Data.Add(propertyMetadata);
    }

    internal static LoadPlan UseExistingOrCreateNew(LoadPlan plan, MetadataClass cls)
    {
      if (plan.Class == cls)
        return plan;
      LoadPlan loadPlan = new LoadPlan(cls);
      loadPlan.MergeWith(plan);
      return loadPlan;
    }

    public AssociationRefLoadPlan EnsureAssociationRef(
      MetadataAssociationRef assRef,
      LoadPlan plan)
    {
      return this.EnsureAssociationRef(assRef, plan, new LoadPlan.MergedPlans());
    }

    private AssociationRefLoadPlan EnsureAssociationRef(
      MetadataAssociationRef assRef,
      LoadPlan plan,
      LoadPlan.MergedPlans mergedPlans)
    {
      for (int index = 0; index < this.Links.Count; ++index)
      {
        AssociationRefLoadPlan link = this.Links[index];
        if (link.Ref == assRef)
        {
          link.Plan.MergeWith(plan, mergedPlans);
          return link;
        }
      }
      AssociationRefLoadPlan associationRefLoadPlan = new AssociationRefLoadPlan(assRef, plan);
      this.Links.Add((object) associationRefLoadPlan);
      return associationRefLoadPlan;
    }

    public ChildRefLoadPlan EnsureChildRef(MetadataChildRef childRef, LoadPlan plan) => this.EnsureChildRef(childRef, plan, new LoadPlan.MergedPlans());

    private ChildRefLoadPlan EnsureChildRef(
      MetadataChildRef childRef,
      LoadPlan plan,
      LoadPlan.MergedPlans mergedPlans)
    {
      for (int index = 0; index < this.Childs.Count; ++index)
      {
        ChildRefLoadPlan child = this.Childs[index];
        if (child.ChildRef == childRef)
        {
          child.Plan.MergeWith(plan, mergedPlans);
          return child;
        }
      }
      ChildRefLoadPlan childRefLoadPlan = new ChildRefLoadPlan(childRef, plan);
      this.Childs.Add((object) childRefLoadPlan);
      return childRefLoadPlan;
    }

    public void EnsureProperty(MetadataProperty propertyMetadata)
    {
      if (propertyMetadata.IsData)
      {
        this.EnsureDataProperty(propertyMetadata);
      }
      else
      {
        if (!propertyMetadata.IsLink)
          throw new DataException(string.Format("DataLoadPlan.EnsureProperty: Свойство {0}.{1}, добавляемое в план загрузки, не является простым типом или связанным объектом.", (object) this.Class.QTypeName, (object) propertyMetadata.Name));
        LoadPlan plan = (LoadPlan) null;
        for (int index = 0; index < propertyMetadata.Association.Refs.Count; ++index)
        {
          MetadataAssociationRef assRef = propertyMetadata.Association.Refs[index];
          if (plan == null)
            plan = new LoadPlan(assRef.RefClass);
          this.EnsureAssociationRef(assRef, plan);
        }
      }
    }

    public void EnsureObjectView(string viewName, DataSession session)
    {
      if (StrUtils.IsNullOrEmpty(viewName))
        viewName = "default";
      MetadataObjectView metadataObjectView = this.Class.ObjectViews.Need(viewName);
      if (!this.Views.Contains(metadataObjectView))
        this.Views.Add(metadataObjectView);
      this.MergeWith(session[this.Class].GetObjectViewEvaluator(metadataObjectView).GetLoadPlan(session));
    }

    internal void MergeWith(LoadPlan plan, LoadPlan.MergedPlans mergedPlans)
    {
      if (mergedPlans != null)
      {
        if (mergedPlans.Contains(this, plan))
          return;
        mergedPlans.Add(this, plan);
      }
      for (int index = 0; index < plan.Data.Count; ++index)
      {
        string name = plan.Data[index].Name;
        MetadataProperty propertyMetadata = this.Class.Properties.Need(name);
        if (!propertyMetadata.IsData)
          throw new DataException(string.Format("Ошибка слияния планов загрузки.\nКласс '{0}' не содержит свойство данных '{1}'.", (object) this.Class.Name, (object) name));
        this.EnsureDataProperty(propertyMetadata);
      }
      for (int index = 0; index < plan.Links.Count; ++index)
      {
        AssociationRefLoadPlan link = plan.Links[index];
        string name = link.Ref.Association.Property.Name;
        MetadataProperty metadataProperty = this.Class.Properties.Need(name);
        if (!metadataProperty.IsLink)
          throw new DataException(string.Format("Ошибка слияния планов загрузки.\nКласс '{0}' не содержит свойство связи '{1}'.", (object) this.Class.Name, (object) name));
        this.EnsureAssociationRef(metadataProperty.Association.Refs.Need(link.Ref.RefClass), link.Plan, mergedPlans);
      }
      for (int index = 0; index < plan.Childs.Count; ++index)
      {
        ChildRefLoadPlan child = plan.Childs[index];
        this.EnsureChildRef(this.Class.Childs.Need(child.ChildRef.ChildClass), child.Plan, mergedPlans);
      }
    }

    internal void MergeWith(LoadPlan plan) => this.MergeWith(plan, new LoadPlan.MergedPlans());

    private void SaveToXml(XmlNode planNode, ArrayList savedPlans)
    {
      int num = savedPlans.IndexOf((object) this);
      if (num >= 0)
      {
        XmlUtils.SetAttr(planNode, "use", (num + 1).ToString());
      }
      else
      {
        savedPlans.Add((object) this);
        XmlUtils.SetAttr(planNode, "Name", savedPlans.Count.ToString());
        foreach (MetadataObjectView view in this.Views)
          XmlUtils.SetAttr((XmlNode) XmlUtils.AppendElement(planNode, "query-view"), "name", view.Name);
        foreach (MetadataProperty metadataProperty in this.Data)
          XmlUtils.AppendElement(planNode, metadataProperty.Name);
        foreach (AssociationRefLoadPlan link in (ArrayList) this.Links)
        {
          XmlNode xmlNode = (XmlNode) XmlUtils.AppendElement(planNode, link.Ref.Association.Property.Name);
          XmlUtils.SetAttr(xmlNode, "ref-class", link.Ref.RefClass.Name);
          link.Plan.SaveToXml(xmlNode, savedPlans);
        }
        foreach (ChildRefLoadPlan child in (ArrayList) this.Childs)
        {
          XmlNode planNode1 = (XmlNode) XmlUtils.AppendElement(planNode, child.ChildRef.ChildClass.IdentName);
          child.Plan.SaveToXml(planNode1, savedPlans);
        }
      }
    }

    public void SaveToXml(XmlNode planNode) => this.SaveToXml(planNode, new ArrayList());

    internal class MergedPlans
    {
      private ArrayList DstPlans = new ArrayList();
      private ArrayList SrcPlans = new ArrayList();

      public void Add(LoadPlan dst, LoadPlan src)
      {
        this.DstPlans.Add((object) dst);
        this.SrcPlans.Add((object) src);
      }

      public bool Contains(LoadPlan dst, LoadPlan src)
      {
        for (int index = 0; index < this.DstPlans.Count; ++index)
        {
          if (this.DstPlans[index] == dst && this.SrcPlans[index] == src)
            return true;
        }
        return false;
      }
    }
  }
}
