// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Load.LoadPlanBuilder
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System.Collections;
using System.Xml;

namespace InMeta.ServerRuntime.Load
{
  internal class LoadPlanBuilder
  {
    private Hashtable FNamedPlans = new Hashtable();
    private LoadPlanBuilder.ObjectViewRequestList FObjectViewRequests = new LoadPlanBuilder.ObjectViewRequestList();
    private DataSession FSession;

    public LoadPlanBuilder(DataSession session) => this.FSession = session;

    public LoadPlan NeedNamedPlan(string name, MetadataClass cls) => ((LoadPlanBuilder.NamedPlan) this.FNamedPlans[(object) name] ?? throw new Integro.InMeta.Runtime.DataException(string.Format("Не найден план загрузки с именем \"{0}\".", (object) name))).EnsurePlan(cls);

    private void LoadQueryInstruction(LoadPlan loadPlan, XmlNode childNode)
    {
      switch (childNode.Name)
      {
        case "query-all-properties":
          this.AddAllProperties(loadPlan);
          break;
        case "query-view":
          this.FObjectViewRequests.Ensure(loadPlan, XmlUtils.GetAttr(childNode, "name"));
          break;
        case "query-name":
          break;
        case "query-ref-class":
          break;
        default:
          throw new System.Data.DataException(string.Format("Некорректная управляющая инструкция плана загрузки: {0}.", (object) childNode.Name));
      }
    }

    private void LoadPropFromXml(LoadPlan plan, MetadataProperty prop, XmlNode node)
    {
      if (prop.IsData)
      {
        plan.EnsureDataProperty(prop);
      }
      else
      {
        XmlNodeList xmlNodeList = node.SelectNodes("query-ref-class");
        MetadataAssociationRefList associationRefList;
        if (xmlNodeList.Count == 0)
        {
          associationRefList = prop.Association.Refs;
        }
        else
        {
          associationRefList = new MetadataAssociationRefList();
          for (int i = 0; i < xmlNodeList.Count; ++i)
          {
            XmlNode node1 = xmlNodeList[i];
            associationRefList.Add(prop.Association.Refs.Need(plan.Class.Metadata.Classes.Need(XmlUtils.NeedAttr(node1, "name"))));
          }
        }
        if (associationRefList.Count == 0)
          return;
        string attr = XmlUtils.GetAttr(node, "use");
        for (int index = 0; index < associationRefList.Count; ++index)
        {
          MetadataAssociationRef assRef = associationRefList[index];
          MetadataClass refClass = assRef.RefClass;
          LoadPlan plan1 = StrUtils.IsNullOrEmpty(attr) ? this.BuildPlan(refClass, node) : this.NeedNamedPlan(attr, refClass);
          plan.EnsureAssociationRef(assRef, plan1);
        }
      }
    }

    private void LoadChildRefFromXml(LoadPlan plan, MetadataChildRef childRef, XmlNode node)
    {
      string attr = XmlUtils.GetAttr(node, "use");
      MetadataClass childClass = childRef.ChildClass;
      plan.EnsureChildRef(childRef, StrUtils.IsNullOrEmpty(attr) ? this.BuildPlan(childClass, node) : this.NeedNamedPlan(attr, childClass));
    }

    private void AddAllProperties(LoadPlan plan)
    {
      for (int index = 0; index < plan.Class.Properties.Count; ++index)
      {
        MetadataProperty property = plan.Class.Properties[index];
        if (!property.IsId)
          plan.EnsureProperty(property);
      }
    }

    public LoadPlan BuildPlan(MetadataClass cls, XmlNode node)
    {
      XmlNode xmlNode1 = node.SelectSingleNode("query-name");
      string s = xmlNode1 != null ? xmlNode1.InnerText.Trim() : XmlUtils.GetAttr(node, "name");
      LoadPlan loadPlan = new LoadPlan(cls);
      if (!StrUtils.IsNullOrEmpty(s))
      {
        LoadPlanBuilder.NamedPlan fnamedPlan = (LoadPlanBuilder.NamedPlan) this.FNamedPlans[(object) s];
        if (fnamedPlan == null)
        {
          LoadPlanBuilder.NamedPlan namedPlan = new LoadPlanBuilder.NamedPlan(this, node);
          namedPlan.PlansByClass.Add(loadPlan);
          this.FNamedPlans.Add((object) s, (object) namedPlan);
        }
        else if (fnamedPlan.SourceNode != node)
          throw new Integro.InMeta.Runtime.DataException(string.Format("План загрузки с именем '{0}' уже определен.", (object) s));
      }
      if (XmlUtils.GetBoolAttr(node, "query-all-properties"))
        this.AddAllProperties(loadPlan);
      XmlNodeList childNodes = node.ChildNodes;
      for (int i = 0; i < childNodes.Count; ++i)
      {
        XmlNode xmlNode2 = childNodes[i];
        if (xmlNode2.NodeType == XmlNodeType.Element)
        {
          string name = xmlNode2.Name;
          if (name.StartsWith("query-"))
          {
            this.LoadQueryInstruction(loadPlan, xmlNode2);
          }
          else
          {
            MetadataProperty property;
            MetadataChildRef childRef;
            cls.NeedMember(name, out property, out childRef);
            if (property != null && !property.IsId)
            {
              this.LoadPropFromXml(loadPlan, property, xmlNode2);
            }
            else
            {
              if (childRef == null)
                throw new Integro.InMeta.Runtime.DataException(string.Format("{0} не является ни свойством, ни дочерним классом.", (object) name));
              this.LoadChildRefFromXml(loadPlan, childRef, xmlNode2);
            }
          }
        }
      }
      return loadPlan;
    }

    public void ProcessObjectViewRequests()
    {
      LoadPlan.MergedPlans mergedPlans = new LoadPlan.MergedPlans();
      foreach (LoadPlanBuilder.ObjectViewRequest fobjectViewRequest in (ArrayList) this.FObjectViewRequests)
      {
        MetadataClass cls = fobjectViewRequest.Plan.Class;
        MetadataObjectView viewMetadata = cls.ObjectViews.Need(fobjectViewRequest.RequestedObjectViewName);
        fobjectViewRequest.Plan.MergeWith(this.FSession[cls].GetObjectViewEvaluator(viewMetadata).GetLoadPlan(this.FSession), mergedPlans);
      }
    }

    private class NamedPlan
    {
      public readonly LoadPlanBuilder Builder;
      public readonly XmlNode SourceNode;
      public readonly LoadPlanList PlansByClass = new LoadPlanList();

      public NamedPlan(LoadPlanBuilder builder, XmlNode sourceNode)
      {
        this.Builder = builder;
        this.SourceNode = sourceNode;
      }

      public LoadPlan EnsurePlan(MetadataClass cls)
      {
        for (int index = 0; index < this.PlansByClass.Count; ++index)
        {
          LoadPlan loadPlan = this.PlansByClass[index];
          if (loadPlan.Class == cls)
            return loadPlan;
        }
        LoadPlan loadPlan1 = this.Builder.BuildPlan(cls, this.SourceNode);
        this.PlansByClass.Add(loadPlan1);
        return loadPlan1;
      }
    }

    private class ObjectViewRequest
    {
      public readonly LoadPlan Plan;
      public readonly string RequestedObjectViewName;

      public ObjectViewRequest(LoadPlan plan, string requestedObjectViewName)
      {
        this.Plan = plan;
        this.RequestedObjectViewName = requestedObjectViewName;
      }
    }

    private class ObjectViewRequestList : ArrayList
    {
      public LoadPlanBuilder.ObjectViewRequest this[int index] => (LoadPlanBuilder.ObjectViewRequest) base[index];

      public void Ensure(LoadPlan plan, string objectViewName)
      {
        for (int index = 0; index < this.Count; ++index)
        {
          LoadPlanBuilder.ObjectViewRequest objectViewRequest = this[index];
          if (objectViewRequest.Plan == plan && objectViewRequest.RequestedObjectViewName == objectViewName)
            return;
        }
        this.Add((object) new LoadPlanBuilder.ObjectViewRequest(plan, objectViewName));
      }
    }
  }
}
