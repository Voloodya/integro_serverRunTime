// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Metadata.MetadataElementLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System.Collections.Generic;
using System.Xml;

namespace InMeta.ServerRuntime.Metadata
{
  internal class MetadataElementLoader
  {
    public readonly XmlElement Element;
    public readonly Dictionary<string, MetadataElementLoader> ChildrenLoadersByKey = new Dictionary<string, MetadataElementLoader>();
    private static readonly Dictionary<string, MergeInfo> FMergeInfos = new Dictionary<string, MergeInfo>();
    private static readonly MergeInfo FClassMergeInfo = (MergeInfo) new MergeByNameAttr();
    private static readonly MergeInfo FDefaultMergeInfo = (MergeInfo) new NoMerge();

    public MetadataElementLoader(XmlElement element) => this.Element = element;

    static MetadataElementLoader()
    {
      MetadataElementLoader.FMergeInfos.Add("class", MetadataElementLoader.FClassMergeInfo);
      MetadataElementLoader.FMergeInfos.Add("property", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("case", (MergeInfo) new MergeByAttr("ref-class"));
      MetadataElementLoader.FMergeInfos.Add("lookup-value", (MergeInfo) new MergeByText());
      MetadataElementLoader.FMergeInfos.Add("input-event", (MergeInfo) new MergeInputEvent());
      MetadataElementLoader.FMergeInfos.Add("object-view", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("virtual-property", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("values-preset", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("user", (MergeInfo) new MergeByAttr("account"));
      MetadataElementLoader.FMergeInfos.Add("config", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("insc-db-id", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("app-path", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("method", (MergeInfo) new MergeByNameAttr(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("appearance", (MergeInfo) new MergeByAttr("form"));
      MetadataElementLoader.FMergeInfos.Add("context", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("doc", (MergeInfo) new MergeByAttr("purpose"));
      MetadataElementLoader.FMergeInfos.Add("deny", (MergeInfo) new MergeByAttr("id"));
      MetadataElementLoader.FMergeInfos.Add("sql-select-template", (MergeInfo) new MergeByNameAttr(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("child-search", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("param", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("script-library", (MergeInfo) new MergeScriptLibrary());
      MetadataElementLoader.FMergeInfos.Add("property-editor", (MergeInfo) new MergeByNameAttr(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("search-form-template", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("header", (MergeInfo) new NoMerge());
      MetadataElementLoader.FMergeInfos.Add("edit-form", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("HTML", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("script", (MergeInfo) new MergeByNodeName(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("report", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("report-form", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("report-script", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("XML-to-HTML-template", (MergeInfo) new MergeByNodeName());
      MetadataElementLoader.FMergeInfos.Add("sql", (MergeInfo) new NoMerge(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("property-input-action", (MergeInfo) new MergeByNameAttr(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("class-input-action", (MergeInfo) new MergeByNameAttr(MergeFlags.AppendNewLineAfterTextSections));
      MetadataElementLoader.FMergeInfos.Add("data-event", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("policy", (MergeInfo) new MergeByNameAttr());
      MetadataElementLoader.FMergeInfos.Add("role", (MergeInfo) new MergeByNameAttr());
    }

    private static void CloseTextSections(XmlNode element, MergeFlags mergeFlags)
    {
      if ((mergeFlags & MergeFlags.AppendNewLineAfterTextSections) == (MergeFlags) 0)
        return;
      element.AppendChild((XmlNode) element.OwnerDocument.CreateTextNode("\r\n"));
    }

    public void Merge(XmlElement src) => this.UpgradeElement(src, (MergeFlags) 0);

    internal void UpgradeElement(XmlElement src, MergeFlags mergeFlags)
    {
      if (src.NodeType == XmlNodeType.Element)
      {
        foreach (XmlAttribute attribute in (XmlNamedNodeMap) src.Attributes)
          this.Element.SetAttribute(attribute.Name, attribute.Value);
      }
      bool flag = false;
      foreach (XmlNode childNode in src.ChildNodes)
      {
        switch (childNode.NodeType)
        {
          case XmlNodeType.Element:
            if (flag)
              MetadataElementLoader.CloseTextSections((XmlNode) this.Element, mergeFlags);
            this.UpgradeChildElement((XmlElement) childNode);
            flag = false;
            break;
          case XmlNodeType.Text:
            this.Element.AppendChild(this.Element.OwnerDocument.ImportNode(childNode, false));
            flag = true;
            break;
          case XmlNodeType.CDATA:
            this.Element.AppendChild(this.Element.OwnerDocument.ImportNode(childNode, false));
            flag = true;
            break;
        }
      }
      if (!flag)
        return;
      MetadataElementLoader.CloseTextSections((XmlNode) this.Element, mergeFlags);
    }

    private void UpgradeChildElement(XmlElement srcChild)
    {
      MergeAction mergeAction = MetadataElementLoader.GetMergeAction((XmlNode) srcChild);
      MergeInfo mergeInfo = MetadataElementLoader.GetMergeInfo((XmlNode) srcChild);
      string elementKey = mergeInfo.GetElementKey(this.ChildrenLoadersByKey, srcChild, mergeAction);
      MetadataElementLoader metadataElementLoader;
      if (elementKey == null)
        metadataElementLoader = (MetadataElementLoader) null;
      else
        this.ChildrenLoadersByKey.TryGetValue(elementKey, out metadataElementLoader);
      XmlNode xmlNode = (XmlNode) null;
      switch (mergeAction)
      {
        case MergeAction.Create:
          if (metadataElementLoader != null)
            throw new MetadataException(string.Format("Повторное создание элемента метаданных {0}.", (object) mergeInfo.GetElementKey(this.ChildrenLoadersByKey, srcChild, MergeAction.Create)));
          xmlNode = (XmlNode) this.CreateChild(elementKey, srcChild, (XmlNode) null, mergeInfo.Flags);
          break;
        case MergeAction.Supplement:
          if (metadataElementLoader != null)
          {
            metadataElementLoader.UpgradeElement(srcChild, mergeInfo.Flags);
            xmlNode = (XmlNode) metadataElementLoader.Element;
            break;
          }
          xmlNode = (XmlNode) this.CreateChild(elementKey, srcChild, (XmlNode) null, mergeInfo.Flags);
          break;
        case MergeAction.Replace:
          XmlNode insertBefore = (XmlNode) null;
          if (metadataElementLoader != null)
          {
            insertBefore = metadataElementLoader.Element.NextSibling;
            this.Element.RemoveChild((XmlNode) metadataElementLoader.Element);
            this.ChildrenLoadersByKey.Remove(elementKey);
          }
          xmlNode = (XmlNode) this.CreateChild(elementKey, srcChild, insertBefore, mergeInfo.Flags);
          break;
        case MergeAction.Delete:
          if (metadataElementLoader != null)
          {
            this.Element.RemoveChild((XmlNode) metadataElementLoader.Element);
            this.ChildrenLoadersByKey.Remove(elementKey);
            break;
          }
          break;
      }
      if (xmlNode == null || !(mergeInfo is MergeByText))
        return;
      XmlUtils.ReplaceOwnText(xmlNode, XmlUtils.GetOwnText((XmlNode) srcChild));
    }

    private static MergeInfo GetMergeInfo(XmlNode elementDefinition)
    {
      MergeInfo mergeInfo;
      return MetadataElementLoader.FMergeInfos.TryGetValue(elementDefinition.Name, out mergeInfo) ? mergeInfo : MetadataElementLoader.FDefaultMergeInfo;
    }

    private static MergeAction GetMergeAction(XmlNode elementDefinition) => (MergeAction) XmlUtils.GetEnumAttr(elementDefinition, "merge", MergeInfo.MergeActionNames, 1);

    private XmlElement CreateChild(
      string childKey,
      XmlElement child,
      XmlNode insertBefore,
      MergeFlags mergeFlags)
    {
      MetadataElementLoader metadataElementLoader = new MetadataElementLoader(this.Element.OwnerDocument.CreateElement(child.Name));
      if (insertBefore != null)
        this.Element.InsertBefore((XmlNode) metadataElementLoader.Element, insertBefore);
      else
        this.Element.AppendChild((XmlNode) metadataElementLoader.Element);
      if (childKey != null)
        this.ChildrenLoadersByKey.Add(childKey, metadataElementLoader);
      metadataElementLoader.UpgradeElement(child, mergeFlags);
      return metadataElementLoader.Element;
    }
  }
}
