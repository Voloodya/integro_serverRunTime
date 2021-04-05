// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataVirtualProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataVirtualProperty
  {
    public readonly MetadataObjectView View;
    public readonly XmlNode SourceNode;
    public readonly string Name;
    public readonly object SourceMember;
    public readonly string SourceNavigation;
    public readonly MetadataClassMember RefMemberType;
    public readonly string RefMemberName;

    private static bool TryGetNotEmptyAttribute(XmlNode node, string attrName, out string value)
    {
      if (string.IsNullOrEmpty(attrName))
      {
        value = (string) null;
        return false;
      }
      value = XmlUtils.GetAttr(node, attrName);
      return !string.IsNullOrEmpty(value);
    }

    private static int GetOneOf(
      XmlNode node,
      string attrName1,
      string attrName2,
      string attrName3,
      out string value)
    {
      if (MetadataVirtualProperty.TryGetNotEmptyAttribute(node, attrName1, out value))
        return 1;
      if (MetadataVirtualProperty.TryGetNotEmptyAttribute(node, attrName2, out value))
        return 2;
      return MetadataVirtualProperty.TryGetNotEmptyAttribute(node, attrName3, out value) ? 3 : 0;
    }

    internal MetadataVirtualProperty(MetadataObjectView view, XmlNode sourceNode)
    {
      this.View = view;
      this.SourceNode = sourceNode;
      this.Name = XmlUtils.NeedAttr(sourceNode, "name");
      string str;
      switch (MetadataVirtualProperty.GetOneOf(sourceNode, "association", "aggregation", "navigation", out str))
      {
        case 1:
          this.SourceMember = (object) view.Class.Associations.Need(str);
          break;
        case 2:
          this.SourceMember = (object) view.Class.Childs.Need(str);
          break;
        case 3:
          this.SourceNavigation = XmlUtils.GetAttr(sourceNode, "navigation", (string) null);
          break;
        default:
          throw new MetadataException(string.Format("Не заданы атрибуты \"association\" или \"aggregation\" или \"navigation\" ({0})", (object) this));
      }
      switch (MetadataVirtualProperty.GetOneOf(sourceNode, "ref-property", "ref-view", (string) null, out this.RefMemberName))
      {
        case 1:
          this.RefMemberType = MetadataClassMember.Property;
          break;
        case 2:
          this.RefMemberType = MetadataClassMember.ObjectView;
          break;
        default:
          this.RefMemberType = MetadataClassMember.ObjectView;
          this.RefMemberName = "default";
          break;
      }
    }

    public override string ToString() => string.Format("Виртуальное свойство: {0}.{1}.{2}", (object) this.View.Class.Name, (object) this.View.Name, (object) this.Name);
  }
}
