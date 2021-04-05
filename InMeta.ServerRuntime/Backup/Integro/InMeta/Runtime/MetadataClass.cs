// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataClass
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataClass
  {
    private XmlNode FSourceNode;
    private string FSessionMemberProgId;
    private string FCaption;
    private string FDataTable;
    private string FImage;
    private MetadataClass.RootFlag FRootFlag;
    private string FListCaption;
    private string FLargeImage;
    private string FListImage;
    private string FLargeListImage;
    private bool FViewAsTree;
    private bool FSelectOnlyLeaves;
    private string FOrderBy;
    private MetadataClassPurpose FPurpose;
    private string FDefaultEditForm;
    private MetadataProperty FIdProperty;
    public readonly MetadataPropertyList Properties;
    public readonly MetadataPropertyList AllProperties;
    public readonly MetadataAssociationList Associations;
    public readonly MetadataAssociationList Owners;
    public readonly MetadataChildRefList Childs;
    public readonly MetadataAssociationRefList ExternalRefs;
    public readonly MetadataObjectViewList ObjectViews;
    public readonly int Index;
    public readonly ApplicationMetadata Metadata;
    public readonly string Name;
    public readonly string TypeNamespace;
    public readonly string TypeName;
    public readonly string QTypeName;
    public readonly string IdentName;

    public string SessionMemberProgId => this.FSessionMemberProgId;

    internal MetadataClass(ApplicationMetadata metadata, string name, int index)
    {
      this.Metadata = metadata;
      this.Name = name;
      this.Index = index;
      string str = name.Replace('/', '.');
      this.QTypeName = "InMeta." + str;
      this.IdentName = name.Replace('/', '_');
      int length = str.LastIndexOf(".");
      if (length >= 0)
      {
        this.TypeNamespace = str.Substring(0, length);
        this.TypeName = str.Substring(length + 1, str.Length - length - 1);
      }
      else
      {
        this.TypeNamespace = string.Empty;
        this.TypeName = str;
      }
      this.Properties = new MetadataPropertyList();
      this.AllProperties = new MetadataPropertyList();
      this.Associations = new MetadataAssociationList();
      this.Owners = new MetadataAssociationList();
      this.Childs = new MetadataChildRefList(this);
      this.ObjectViews = new MetadataObjectViewList(this);
      this.ExternalRefs = new MetadataAssociationRefList();
    }

    internal void LoadFromXml(XmlNode node)
    {
      try
      {
        this.FSourceNode = node;
        this.FSessionMemberProgId = XmlUtils.GetAttr(node, "prog-id", this.IdentName);
        this.FDataTable = XmlUtils.NeedAttr(node, "data-table");
        this.FCaption = XmlUtils.GetAttr(node, "caption", this.Name);
        this.FImage = XmlUtils.GetAttr(node, "object-image");
        this.FPurpose = (MetadataClassPurpose) XmlUtils.GetEnumAttr(node, "data-purpose", new string[2]
        {
          "registry",
          "dictionary"
        });
        this.FDefaultEditForm = XmlUtils.GetAttr(node, "default-edit-form");
        if (XmlUtils.ContainsAttr(node, "is-root"))
          this.FRootFlag = XmlUtils.GetBoolAttr(node, "is-root") ? MetadataClass.RootFlag.True : MetadataClass.RootFlag.False;
        this.FListCaption = XmlUtils.GetAttr(node, "list-caption", "Реестр объектов: " + this.FCaption);
        this.FLargeImage = XmlUtils.GetAttr(node, "large-object-image", this.FImage);
        this.FListImage = XmlUtils.GetAttr(node, "list-image", this.FImage);
        this.FLargeListImage = XmlUtils.GetAttr(node, "large-list-image", this.FLargeImage);
        this.FViewAsTree = XmlUtils.GetBoolAttr(node, "view-as-tree");
        this.FSelectOnlyLeaves = XmlUtils.GetBoolAttr(node, "select-only-leaves");
        this.FOrderBy = XmlUtils.GetAttr(node, "order-by");
        this.LoadPropertiesFromXml(node);
      }
      catch (Exception ex)
      {
        throw new MetadataException(string.Format("Ошибка загрузки метаданных класса {0}", (object) this.Name), ex);
      }
    }

    private void LoadPropertiesFromXml(XmlNode node)
    {
      foreach (XmlNode selectNode in node.SelectNodes("property"))
      {
        string name = XmlUtils.NeedAttr(selectNode, "name");
        if (this.Properties.Find(name) != null)
          throw new MetadataException(string.Format("Свойство {0} уже определено.", (object) name));
        MetadataProperty property = new MetadataProperty(this, name, this.Properties.Count);
        property.LoadFromXml(selectNode);
        this.AllProperties.Add(property);
        if (!property.IsExtension)
        {
          this.Properties.Add(property);
          switch (property.Purpose)
          {
            case MetadataPropertyPurpose.Id:
              this.FIdProperty = property;
              break;
            case MetadataPropertyPurpose.Association:
            case MetadataPropertyPurpose.Aggregation:
              MetadataAssociation ass = new MetadataAssociation(property);
              this.Associations.Add(ass);
              if (property.Purpose == MetadataPropertyPurpose.Aggregation)
              {
                this.Owners.Add(ass);
                break;
              }
              break;
          }
        }
      }
    }

    internal void LoadAssociations()
    {
      try
      {
        for (int index = 0; index < this.Associations.Count; ++index)
        {
          MetadataAssociation association = this.Associations[index];
          association.LoadFromXml(association.Property.SourceNode);
        }
      }
      catch (Exception ex)
      {
        throw new MetadataException(string.Format("Ошибка загрузки метаданных класса {0}", (object) this.Name), ex);
      }
    }

    internal void LoadObjectViews()
    {
      try
      {
        foreach (XmlNode selectNode in this.SourceNode.SelectNodes("object-view"))
        {
          MetadataObjectView metadataObjectView = new MetadataObjectView(this, selectNode, this.ObjectViews.Count);
          try
          {
            if (this.ObjectViews.Contains(metadataObjectView.Name))
              throw new MetadataException("Представление объекта уже определено.");
            this.ObjectViews.Add(metadataObjectView);
          }
          catch (Exception ex)
          {
            throw new MetadataException(string.Format("Ошибка загрузки метаданных представления объекта {0}", (object) metadataObjectView.Name), ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MetadataException(string.Format("Ошибка загрузки метаданных класса {0}", (object) this.Name), ex);
      }
    }

    internal void LoadVirtualProperties()
    {
      try
      {
        for (int index = 0; index < this.ObjectViews.Count; ++index)
        {
          MetadataObjectView objectView = this.ObjectViews[index];
          try
          {
            objectView.LoadVirtualProperties();
          }
          catch (Exception ex)
          {
            throw new MetadataException(string.Format("Ошибка загрузки метаданных представления объекта {0}", (object) objectView.Name), ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MetadataException(string.Format("Ошибка загрузки метаданных класса {0}", (object) this.Name), ex);
      }
    }

    public string Caption => this.FCaption;

    public string DataTable => this.FDataTable;

    public string Image => this.FImage;

    public bool IsRoot => this.FRootFlag == MetadataClass.RootFlag.True;

    public string ListCaption => this.FListCaption;

    public string LargeImage => this.FLargeImage;

    public string ListImage => this.FListImage;

    public string LargeListImage => this.FLargeListImage;

    public bool ViewAsTree => this.FViewAsTree;

    public bool SelectOnlyLeaves => this.FSelectOnlyLeaves;

    public string OrderBy => this.FOrderBy;

    public MetadataClassPurpose Purpose => this.FPurpose;

    public string DefaultEditForm => this.FDefaultEditForm;

    public MetadataProperty IDProperty => this.FIdProperty;

    public XmlNode SourceNode => this.FSourceNode;

    public override string ToString() => string.Format("Класс: {0}", (object) this.Name);

    public void NeedMember(
      string memberName,
      out MetadataProperty property,
      out MetadataChildRef childRef)
    {
      if (!this.TryFindMember(memberName, out property, out childRef))
        throw new DataException(string.Format("\"{0}\" не является ни свойством, ни дочерним классом.", (object) memberName));
    }

    public bool TryFindMember(
      string memberName,
      out MetadataProperty property,
      out MetadataChildRef childRef)
    {
      property = this.Properties.Find(memberName);
      if (property != null)
      {
        childRef = (MetadataChildRef) null;
        return true;
      }
      MetadataClass byIdentName = this.Metadata.Classes.FindByIdentName(memberName);
      if (byIdentName != null)
      {
        childRef = this.Childs.Find(byIdentName);
        if (childRef != null)
          return true;
      }
      else
        childRef = (MetadataChildRef) null;
      return false;
    }

    private enum RootFlag
    {
      False,
      True,
    }
  }
}
