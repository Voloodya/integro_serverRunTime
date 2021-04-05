// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Metadata.ApplicationMetadataXml
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace InMeta.ServerRuntime.Metadata
{
  [ComVisible(false)]
  public class ApplicationMetadataXml
  {
    public readonly string FolderPath;
    public readonly string Name;
    private string FUrlPath;
    private XmlDocument FMetadataCache;
    private XmlElement FMetadataRoot;
    private Dictionary<string, XmlElement> FMetadataClassElements;
    private List<XmlElement> FOrderedClassElements;
    private string FMetadataXmlCache;
    private XmlDocument FReducedMetadataCache;
    private string FReducedMetadataXmlCache;
    private XmlDocument FDbInfo;
    private XmlElement FConnectionParamsCache;

    public ApplicationMetadataXml(string inMetaRootFolder, string appName)
    {
      this.Name = appName;
      this.FolderPath = SysUtils.ExcludeTrailingPathSeparator(Path.Combine(inMetaRootFolder, appName));
      this.FDbInfo = XmlUtils.CreateDocument("DbInfo");
    }

    public string Id { get; private set; }

    public XmlDocument Metadata => this.GetMetadata();

    public string MetadataXml => this.FMetadataXmlCache ?? (this.FMetadataXmlCache = this.Metadata.OuterXml);

    public XmlDocument ReducedMetadata
    {
      get
      {
        this.GetMetadata();
        return this.FReducedMetadataCache;
      }
    }

    public string ReducedMetadataXml => this.FReducedMetadataXmlCache ?? (this.FReducedMetadataXmlCache = this.ReducedMetadata.OuterXml);

    public XmlElement ConnectionParams => this.FConnectionParamsCache;

    internal void IncreaseSyncVersion() => Utility.IncreaseSyncVersion(Path.Combine(this.FolderPath, "MetadataSync.txt"));

    public void Reload()
    {
      this.IncreaseSyncVersion();
      this.FDbInfo = XmlUtils.LoadDocument(Path.Combine(this.FolderPath, "Meta\\_db_info.xml"), Encoding.GetEncoding(1251));
      this.Id = this.Name;
      if (this.FDbInfo.DocumentElement != null)
      {
        XmlElement xmlElement1 = (XmlElement) this.FDbInfo.DocumentElement.SelectSingleNode("id");
        if (xmlElement1 != null)
          this.Id = xmlElement1.InnerText.Trim();
        this.FConnectionParamsCache = (XmlElement) this.FDbInfo.DocumentElement.SelectSingleNode("driver");
        if (this.FConnectionParamsCache != null)
        {
          XmlElement xmlElement2 = (XmlElement) this.FConnectionParamsCache.SelectSingleNode("param[@name='login-password']");
          if (xmlElement2 != null)
          {
            string attribute = xmlElement2.GetAttribute("encryption");
            if (!string.IsNullOrEmpty(attribute))
            {
              xmlElement2.InnerText = Utility.Decrypt(xmlElement2.InnerText, attribute);
              xmlElement2.RemoveAttribute("encryption");
            }
          }
        }
      }
      else
        this.FConnectionParamsCache = (XmlElement) null;
      this.FMetadataClassElements = (Dictionary<string, XmlElement>) null;
      this.FOrderedClassElements = (List<XmlElement>) null;
      this.FMetadataRoot = (XmlElement) null;
      this.FMetadataCache = (XmlDocument) null;
      this.FMetadataXmlCache = (string) null;
      this.FReducedMetadataCache = (XmlDocument) null;
      this.FReducedMetadataXmlCache = (string) null;
    }

    private static string Sb(string ident) => !string.IsNullOrEmpty(ident) && ident[0] == '[' ? ident : string.Format("[{0}]", (object) ident.Replace(".", "].["));

    internal static void ReplaceOwnText(XmlNode element, string text)
    {
      List<XmlNode> xmlNodeList = new List<XmlNode>();
      foreach (XmlNode childNode in element.ChildNodes)
      {
        if (childNode.NodeType == XmlNodeType.CDATA || childNode.NodeType == XmlNodeType.Text || childNode.NodeType == XmlNodeType.Whitespace)
          xmlNodeList.Add(childNode);
      }
      foreach (XmlNode oldChild in xmlNodeList)
        element.RemoveChild(oldChild);
      element.AppendChild((XmlNode) element.OwnerDocument.CreateTextNode(text));
    }

    public XmlDocument GetIncompleteMetadata(
      out Dictionary<string, XmlElement> classElementsByName,
      out List<XmlElement> orderedClassElements)
    {
      string fileName1 = Path.Combine(this.FolderPath, "Meta\\meta-app.xml");
      double num = SysUtils.GetFileVersion(fileName1);
      XmlDocument document = XmlUtils.CreateDocument("AppMetadata");
      XmlElement documentElement = document.DocumentElement;
      documentElement.SetAttribute("AppRootFolder", this.FolderPath);
      XmlDocument xmlDocument1 = XmlUtils.LoadDocument(fileName1, Encoding.GetEncoding(1251));
      MetadataElementLoader metadataElementLoader = new MetadataElementLoader(documentElement);
      if (xmlDocument1.DocumentElement != null)
      {
        foreach (XmlNode selectNode in xmlDocument1.DocumentElement.SelectNodes("meta-data"))
        {
          string fileName2 = Path.Combine(this.FolderPath, "Meta\\" + XmlUtils.NeedAttr(selectNode, "src"));
          double fileVersion = SysUtils.GetFileVersion(fileName2);
          if (fileVersion > num)
            num = fileVersion;
          XmlDocument xmlDocument2 = XmlUtils.LoadDocument(fileName2, Encoding.GetEncoding(1251));
          metadataElementLoader.Merge(xmlDocument2.DocumentElement);
        }
      }
      classElementsByName = new Dictionary<string, XmlElement>();
      orderedClassElements = new List<XmlElement>();
      foreach (XmlElement selectNode in documentElement.SelectNodes("class"))
      {
        orderedClassElements.Add(selectNode);
        classElementsByName[selectNode.GetAttribute("name")] = selectNode;
      }
      documentElement.SetAttribute("Version", XStrUtils.ToXStr(num));
      XmlUtils.SetChildText((XmlNode) documentElement, "meta-version", XStrUtils.ToXStr(num));
      foreach (XmlNode childNode in this.FDbInfo.DocumentElement.ChildNodes)
        documentElement.AppendChild(document.ImportNode(childNode, true));
      foreach (XmlElement selectNode in documentElement.SelectNodes("class"))
        ApplicationMetadataXml.MakePrimaryClassCompletion(selectNode);
      return document;
    }

    private static void MakePrimaryClassCompletion(XmlElement classNode)
    {
      if (!XmlUtils.ContainsAttr((XmlNode) classNode, "data-table"))
        classNode.SetAttribute("data-table", XmlUtils.NeedAttr((XmlNode) classNode, "name").Replace('/', '_'));
      bool flag = false;
      foreach (XmlElement selectNode1 in classNode.SelectNodes("property"))
      {
        string attr = XmlUtils.GetAttr((XmlNode) selectNode1, "purpose", "data");
        ApplicationMetadataXml.CopyAttrIfNotExists(selectNode1, "data-field", "name");
        if (attr == "association" || attr == "aggregation")
        {
          ApplicationMetadataXml.SetAttrIfNotExists(selectNode1, "data-type", "string");
          ApplicationMetadataXml.SetAttrIfNotExists(selectNode1, "data-length", "12");
          if (string.IsNullOrEmpty(selectNode1.GetAttribute("ref-select-case")))
          {
            ApplicationMetadataXml.SetAttrIfNotExists(selectNode1, "ref-property", "id");
          }
          else
          {
            foreach (XmlElement selectNode2 in selectNode1.SelectNodes("case"))
              ApplicationMetadataXml.SetAttrIfNotExists(selectNode2, "ref-property", "id");
          }
        }
        else if (attr == "id")
          flag = true;
      }
      if (flag)
        return;
      XmlElement xmlElement = XmlUtils.AppendElement((XmlNode) classNode, "property");
      xmlElement.SetAttribute("purpose", "id");
      xmlElement.SetAttribute("name", "id");
      xmlElement.SetAttribute("caption", "Идентификатор");
      xmlElement.SetAttribute("data-field", "OID");
      xmlElement.SetAttribute("data-type", "string");
      xmlElement.SetAttribute("data-length", "12");
    }

    private XmlDocument GetMetadata()
    {
      if (this.FMetadataCache != null)
        return this.FMetadataCache;
      this.FMetadataCache = this.GetIncompleteMetadata(out this.FMetadataClassElements, out this.FOrderedClassElements);
      this.FMetadataRoot = this.FMetadataCache.DocumentElement;
      XmlElement xmlElement = (XmlElement) this.FMetadataRoot.SelectSingleNode("config");
      this.FUrlPath = xmlElement != null ? XmlUtils.GetChildText((XmlNode) xmlElement, "app-path", this.Name) : this.Name;
      this.CompleteClasses();
      this.CompleteScriptLibraries();
      this.FReducedMetadataCache = new XmlDocument();
      this.FReducedMetadataCache.AppendChild(this.FReducedMetadataCache.ImportNode((XmlNode) this.FMetadataRoot, false));
      foreach (XmlNode childNode in this.FMetadataRoot.ChildNodes)
      {
        if (childNode.NodeType == XmlNodeType.Element && childNode.Name != "property-editor" && childNode.Name != "class" && childNode.Name != "script-library")
          this.FReducedMetadataCache.DocumentElement.AppendChild(this.FReducedMetadataCache.ImportNode(childNode, true));
      }
      return this.FMetadataCache;
    }

    private static XmlElement NeedIdProperty(XmlNode classElement) => (XmlElement) classElement.SelectSingleNode("property[@purpose='id']") ?? throw new MetadataException(string.Format("Не найдено идентифицирующее свойство для класса {0}.", (object) XmlUtils.NeedAttr(classElement, "name")));

    private static void CopyAttrIfNotExists(
      XmlElement element,
      string attrName,
      string defAttrName)
    {
      if (XmlUtils.ContainsAttr((XmlNode) element, attrName))
        return;
      element.SetAttribute(attrName, XmlUtils.GetAttr((XmlNode) element, defAttrName));
    }

    private static void SetAttrIfNotExists(XmlElement element, string attrName, string defValue)
    {
      if (XmlUtils.ContainsAttr((XmlNode) element, attrName))
        return;
      element.SetAttribute(attrName, defValue);
    }

    private XmlElement NeedClassElement(string className)
    {
      XmlElement xmlElement;
      if (this.FMetadataClassElements.TryGetValue(className, out xmlElement))
        return xmlElement;
      throw new MetadataException(string.Format("Не найден класс {0}.", (object) className));
    }

    private void CompleteProp(XmlNode classElement, XmlElement propElement)
    {
      ApplicationMetadataXml.CopyAttrIfNotExists(propElement, "caption", "name");
      ApplicationMetadataXml.SetAttrIfNotExists(propElement, "purpose", "data");
      ApplicationMetadataXml.SetAttrIfNotExists(propElement, "data-nullable", "true");
      ApplicationMetadataXml.SetAttrIfNotExists(propElement, "searchable", "true");
      ApplicationMetadataXml.SetAttrIfNotExists(propElement, "editable", "true");
      if (XmlUtils.GetAttr((XmlNode) propElement, "purpose", "data") == "aggregation")
      {
        string refSelectCase = propElement.GetAttribute("ref-select-case");
        if (refSelectCase != null)
          refSelectCase = refSelectCase.Trim();
        string className = XmlUtils.NeedAttr(classElement, "name");
        string propName = XmlUtils.NeedAttr((XmlNode) propElement, "name");
        propElement.SetAttribute("aggregation-type", XmlUtils.GetAttr((XmlNode) propElement, "aggregation-type", "attribute"));
        if (string.IsNullOrEmpty(refSelectCase))
        {
          propElement.SetAttribute("ref-role", propElement.GetAttribute("ref-class"));
          this.AppendAggregationElement(className, propName, propElement, refSelectCase);
        }
        else
        {
          foreach (XmlElement selectNode in propElement.SelectNodes("case"))
          {
            selectNode.SetAttribute("ref-role", selectNode.GetAttribute("ref-class"));
            this.AppendAggregationElement(className, propName, selectNode, refSelectCase);
          }
        }
        ApplicationMetadataXml.SetAttrIfNotExists(propElement, "cardinality-min", "0");
        ApplicationMetadataXml.SetAttrIfNotExists(propElement, "cardinality-max", "0");
      }
      ApplicationMetadataXml.SetAttrIfNotExists(propElement, "is-virtual", "false");
    }

    private static void AppendDefaultView(XmlNode classElement)
    {
      string str = classElement.SelectSingleNode("property[@name='Name']") == null ? XmlUtils.NeedAttr(classElement, "caption") + "[<%=" + XmlUtils.NeedAttr(classElement, "id-property") + "Property%>]" : "<%=NameProperty%>";
      XmlElement xmlElement = XmlUtils.AppendElement(classElement, "object-view");
      xmlElement.SetAttribute("name", "default");
      xmlElement.InnerText = str;
    }

    private static void AddCdata(XmlNode node, string cdataText) => node.AppendChild((XmlNode) node.OwnerDocument.CreateCDataSection(cdataText));

    private static void AppendDescriptionView(XmlNode classElement)
    {
      List<XmlElement> xmlElementList = new List<XmlElement>();
      foreach (XmlElement selectNode in classElement.SelectNodes("property"))
      {
        if (selectNode.GetAttribute("purpose") == "data" && selectNode.GetAttribute("is-virtual") != "true")
          xmlElementList.Add(selectNode);
      }
      if (xmlElementList.Count == 0)
        return;
      StringBuilder stringBuilder = new StringBuilder("<%Dim sResult\r\nsResult = \"\"");
      foreach (XmlElement xmlElement in xmlElementList)
      {
        if (!(XmlUtils.GetAttr((XmlNode) xmlElement, "auto-load", "true") != "true"))
        {
          string str1 = XmlUtils.NeedAttr((XmlNode) xmlElement, "name") + "Property";
          string str2 = XmlUtils.NeedAttr((XmlNode) xmlElement, "caption");
          stringBuilder.AppendFormat("\r\nIf {0} <> \"\" Then\r\n\tIf sResult <> \"\" Then sResult = sResult & \"<BR>\"\r\n\tsResult = sResult & \"{1}: \" & {0}\r\nEnd If\r\n", (object) str1, (object) str2);
        }
      }
      stringBuilder.Append("\r\nViewText.Write sResult%>");
      XmlElement xmlElement1 = XmlUtils.AppendElement(classElement, "object-view");
      xmlElement1.SetAttribute("name", "description");
      ApplicationMetadataXml.AddCdata((XmlNode) xmlElement1, stringBuilder.ToString());
    }

    private static void AppendDefaultLookup(XmlNode classElement)
    {
      XmlElement xmlElement1 = (XmlElement) classElement.SelectSingleNode("property[@name='Name']");
      if (xmlElement1 == null)
        return;
      XmlElement xmlElement2 = XmlUtils.AppendElement(classElement, "lookup");
      xmlElement2.SetAttribute("name", "default");
      xmlElement2.SetAttribute("caption", XmlUtils.GetAttr((XmlNode) xmlElement1, "caption", "Название"));
      xmlElement2.SetAttribute("property", "Name");
      xmlElement2.SetAttribute("method", "like");
    }

    private static void AppendDefaultSqlSelectTemplate(XmlNode classElement)
    {
      XmlElement xmlElement1 = XmlUtils.AppendElement(classElement, "sql-select-template");
      xmlElement1.SetAttribute("name", "default");
      xmlElement1.SetAttribute("caption", "Запрос по умолчанию");
      XmlElement xmlElement2 = XmlUtils.AppendElement((XmlNode) xmlElement1, "param");
      xmlElement2.SetAttribute("name", "Criteria");
      xmlElement2.SetAttribute("data-type", "string");
      List<XmlElement> xmlElementList = new List<XmlElement>();
      foreach (XmlElement selectNode in classElement.SelectNodes("property"))
      {
        if (selectNode.GetAttribute("purpose") == "data" && selectNode.GetAttribute("searchable") == "true")
          xmlElementList.Add(selectNode);
      }
      StringBuilder stringBuilder1 = new StringBuilder();
      foreach (XmlElement xmlElement3 in xmlElementList)
      {
        string ident = XmlUtils.NeedAttr((XmlNode) xmlElement3, "data-field");
        if (stringBuilder1.Length > 0)
          stringBuilder1.Append(" OR ");
        bool flag = XmlUtils.GetAttr((XmlNode) xmlElement3, "data-type") == "text";
        stringBuilder1.AppendFormat(flag ? "({0} like '<%=UCase(Criteria)%>%')" : "(UPPER({0}) like '<%=UCase(Criteria)%>%')", (object) ApplicationMetadataXml.Sb(ident));
      }
      string ident1 = XmlUtils.NeedAttr(classElement, "id-data-field");
      string ident2 = XmlUtils.NeedAttr(classElement, "data-table");
      StringBuilder stringBuilder2 = new StringBuilder();
      stringBuilder2.AppendFormat("SELECT {0} id FROM {1}", (object) ApplicationMetadataXml.Sb(ident1), (object) ApplicationMetadataXml.Sb(ident2));
      if (stringBuilder1.Length > 0)
        stringBuilder2.AppendFormat("<%If Len(Criteria & \"\") > 0 Then%> WHERE {0} <%End If%>", (object) stringBuilder1);
      string attr1 = XmlUtils.GetAttr(classElement, "order-by");
      if (!string.IsNullOrEmpty(attr1))
      {
        string[] strArray;
        bool[] flagArray;
        StrUtils.ParseOrderBy(attr1, ref strArray, ref flagArray);
        int num = 0;
        for (int index = 0; index < strArray.Length; ++index)
        {
          XmlNode node = classElement.SelectSingleNode("property[@name='" + strArray[index] + "']");
          if (node != null)
          {
            if (num == 0)
              stringBuilder2.AppendFormat(" ORDER BY ");
            else if (num > 0)
              stringBuilder2.Append(',');
            string attr2 = XmlUtils.GetAttr(node, "data-field", XmlUtils.NeedAttr(node, "name"));
            stringBuilder2.AppendFormat("[{0}]", (object) attr2);
            if (flagArray[index])
              stringBuilder2.Append(" DESC");
            ++num;
          }
        }
      }
      ApplicationMetadataXml.AddCdata((XmlNode) xmlElement1, stringBuilder2.ToString());
    }

    private static void CompleteObjectViews(XmlNode classElement)
    {
      if (classElement == null)
        throw new ArgumentNullException(nameof (classElement));
      foreach (XmlElement selectNode in classElement.SelectNodes("object-view"))
        ApplicationMetadataXml.SetAttrIfNotExists(selectNode, "is-internal", "true");
    }

    private static bool IsNumeric(string s)
    {
      for (int index = 0; index < s.Length; ++index)
      {
        if (!char.IsDigit(s[index]))
          return false;
      }
      return true;
    }

    private static void SelectPropsWithOrder(
      XmlNode classElement,
      ICollection<XmlElement> props,
      string matchOrder)
    {
      props.Clear();
      foreach (XmlElement selectNode in classElement.SelectNodes("property"))
      {
        string attribute = selectNode.GetAttribute("editor-order");
        if (!string.IsNullOrEmpty(attribute) && (string.IsNullOrEmpty(matchOrder) || matchOrder == attribute))
          props.Add(selectNode);
      }
    }

    private static void ApplyEditorOrder(XmlNode classElement)
    {
      List<XmlElement> xmlElementList = new List<XmlElement>();
      ApplicationMetadataXml.SelectPropsWithOrder(classElement, (ICollection<XmlElement>) xmlElementList, string.Empty);
      foreach (XmlElement xmlElement in xmlElementList)
      {
        string attr = XmlUtils.GetAttr((XmlNode) xmlElement, "editor-order");
        if (ApplicationMetadataXml.IsNumeric(attr))
        {
          int i = int.Parse(attr);
          classElement.RemoveChild((XmlNode) xmlElement);
          XmlNodeList xmlNodeList = classElement.SelectNodes("property");
          if (i >= xmlNodeList.Count)
            classElement.AppendChild((XmlNode) xmlElement);
          else if (i < 0)
            classElement.InsertBefore((XmlNode) xmlElement, xmlNodeList[0]);
          else
            classElement.InsertBefore((XmlNode) xmlElement, xmlNodeList[i]);
        }
      }
      ApplicationMetadataXml.SelectPropsWithOrder(classElement, (ICollection<XmlElement>) xmlElementList, "begin");
      foreach (XmlElement xmlElement1 in xmlElementList)
      {
        classElement.RemoveChild((XmlNode) xmlElement1);
        XmlElement xmlElement2 = (XmlElement) classElement.SelectSingleNode("property");
        if (xmlElement2 != null)
          classElement.InsertBefore((XmlNode) xmlElement1, (XmlNode) xmlElement2);
        else
          classElement.AppendChild((XmlNode) xmlElement1);
      }
      ApplicationMetadataXml.SelectPropsWithOrder(classElement, (ICollection<XmlElement>) xmlElementList, "end");
      foreach (XmlElement xmlElement in xmlElementList)
      {
        classElement.RemoveChild((XmlNode) xmlElement);
        classElement.AppendChild((XmlNode) xmlElement);
      }
      ApplicationMetadataXml.SelectPropsWithOrder(classElement, (ICollection<XmlElement>) xmlElementList, string.Empty);
      foreach (XmlElement xmlElement1 in xmlElementList)
      {
        string attr = XmlUtils.GetAttr((XmlNode) xmlElement1, "editor-order");
        if (attr.StartsWith("before:"))
        {
          string str = attr.Substring("before:".Length);
          if (!(str == XmlUtils.GetAttr((XmlNode) xmlElement1, "name")))
          {
            XmlElement xmlElement2 = (XmlElement) classElement.SelectSingleNode("property[@name='" + str + "']");
            if (xmlElement2 != null)
            {
              classElement.RemoveChild((XmlNode) xmlElement1);
              classElement.InsertBefore((XmlNode) xmlElement1, (XmlNode) xmlElement2);
            }
          }
        }
      }
      ApplicationMetadataXml.SelectPropsWithOrder(classElement, (ICollection<XmlElement>) xmlElementList, string.Empty);
      foreach (XmlElement xmlElement1 in xmlElementList)
      {
        string attr = XmlUtils.GetAttr((XmlNode) xmlElement1, "editor-order");
        if (attr.StartsWith("after:"))
        {
          string str = attr.Substring("after:".Length);
          if (!(str == XmlUtils.GetAttr((XmlNode) xmlElement1, "name")))
          {
            XmlElement xmlElement2 = (XmlElement) classElement.SelectSingleNode("property[@name='" + str + "']");
            if (xmlElement2 != null)
            {
              classElement.RemoveChild((XmlNode) xmlElement1);
              if (xmlElement2.NextSibling != null)
                classElement.InsertBefore((XmlNode) xmlElement1, xmlElement2.NextSibling);
              else
                classElement.AppendChild((XmlNode) xmlElement1);
            }
          }
        }
      }
    }

    private void CompleteClass(XmlElement classElement)
    {
      if (!XmlUtils.ContainsAttr((XmlNode) classElement, "order-by") && classElement.SelectSingleNode("property[@name='Name']") != null)
        classElement.SetAttribute("order-by", "NameProperty");
      XmlElement xmlElement = ApplicationMetadataXml.NeedIdProperty((XmlNode) classElement);
      classElement.SetAttribute("id-property", XmlUtils.NeedAttr((XmlNode) xmlElement, "name"));
      classElement.SetAttribute("id-data-field", XmlUtils.NeedAttr((XmlNode) xmlElement, "data-field"));
      ApplicationMetadataXml.CopyAttrIfNotExists(classElement, "caption", "name");
      ApplicationMetadataXml.CopyAttrIfNotExists(classElement, "list-caption", "caption");
      foreach (XmlElement selectNode in classElement.SelectNodes("property"))
        this.CompleteProp((XmlNode) classElement, selectNode);
      ApplicationMetadataXml.SetAttrIfNotExists(classElement, "track-version", "none");
      ApplicationMetadataXml.SetAttrIfNotExists(classElement, "object-image", "ObjectDefault.gif");
      ApplicationMetadataXml.SetAttrIfNotExists(classElement, "list-image", "ListDefault.gif");
      ApplicationMetadataXml.SetAttrIfNotExists(classElement, "large-object-image", "LargeObjectDefault.gif");
      ApplicationMetadataXml.SetAttrIfNotExists(classElement, "large-list-image", "LargeListDefault.gif");
      XmlUtils.SetAttr((XmlNode) classElement, "object-image", "/" + this.FUrlPath + "/images/" + XmlUtils.NeedAttr((XmlNode) classElement, "object-image"));
      XmlUtils.SetAttr((XmlNode) classElement, "list-image", "/" + this.FUrlPath + "/images/" + XmlUtils.NeedAttr((XmlNode) classElement, "list-image"));
      XmlUtils.SetAttr((XmlNode) classElement, "large-object-image", "/" + this.FUrlPath + "/images/" + XmlUtils.NeedAttr((XmlNode) classElement, "large-object-image"));
      XmlUtils.SetAttr((XmlNode) classElement, "large-list-image", "/" + this.FUrlPath + "/images/" + XmlUtils.NeedAttr((XmlNode) classElement, "large-list-image"));
      if (!XmlUtils.ContainsAttr((XmlNode) classElement, "is-root"))
      {
        foreach (XmlElement selectNode in classElement.SelectNodes("property"))
        {
          if (selectNode.GetAttribute("purpose") == "aggregation" && selectNode.GetAttribute("aggregation-type") == "attribute")
          {
            classElement.SetAttribute("is-root", "false");
            break;
          }
        }
        ApplicationMetadataXml.SetAttrIfNotExists(classElement, "is-root", "true");
      }
      if (classElement.SelectSingleNode("object-view[@name='default']") == null)
        ApplicationMetadataXml.AppendDefaultView((XmlNode) classElement);
      if (classElement.SelectSingleNode("object-view[@name='description']") == null)
        ApplicationMetadataXml.AppendDescriptionView((XmlNode) classElement);
      if (classElement.SelectSingleNode("lookup[@name='default']") == null)
        ApplicationMetadataXml.AppendDefaultLookup((XmlNode) classElement);
      if (classElement.SelectSingleNode("sql-select-template[@name='default']") == null)
        ApplicationMetadataXml.AppendDefaultSqlSelectTemplate((XmlNode) classElement);
      ApplicationMetadataXml.CompleteObjectViews((XmlNode) classElement);
      ApplicationMetadataXml.ApplyEditorOrder((XmlNode) classElement);
    }

    private static string EncodeVirtualPropertyName(string aRole, string aRefProperty) => (aRole + (object) '_' + aRefProperty).Replace('/', '_');

    private void CompleteDenyForVirtualProperty(
      XmlElement classElement,
      XmlNode virtualProperty,
      Dictionary<string, List<XmlElement>> deniesByClassPath)
    {
      string key = classElement.ParentNode.Name == "aggregation" ? XmlUtils.NeedAttr(classElement.ParentNode, "role") : XmlUtils.NeedAttr((XmlNode) classElement, "name");
      XmlElement xmlElement1 = classElement;
      do
      {
        if (xmlElement1.ParentNode.Name == "aggregation")
        {
          if (key != "")
            key += ".";
          key += XmlUtils.NeedAttr(xmlElement1.ParentNode, "role");
          xmlElement1 = (XmlElement) xmlElement1.ParentNode.ParentNode;
        }
      }
      while (xmlElement1.Name == "aggregation");
      string str1 = XmlUtils.NeedAttr(virtualProperty, "virtual-ref-property");
      string str2 = XmlUtils.NeedAttr(virtualProperty, "name");
      List<XmlElement> xmlElementList;
      if (!deniesByClassPath.TryGetValue(key, out xmlElementList))
        return;
      foreach (XmlElement xmlElement2 in xmlElementList)
      {
        string attr = XmlUtils.GetAttr((XmlNode) xmlElement2, "property");
        if (!(attr != str1) || !(attr != "*"))
        {
          XmlElement xmlElement3 = (XmlElement) xmlElement2.CloneNode(false);
          xmlElement2.ParentNode.AppendChild((XmlNode) xmlElement3);
          xmlElement3.SetAttribute("class", key);
          xmlElement3.SetAttribute("property", str2);
        }
      }
    }

    private void CompleteAggregationVirtualProps(
      XmlElement classElement,
      Dictionary<string, List<XmlElement>> deniesByClassPath)
    {
      foreach (XmlElement selectNode1 in classElement.SelectNodes("aggregation[@is-built-in='true']"))
      {
        string aRole = XmlUtils.NeedAttr((XmlNode) selectNode1, "role");
        XmlElement classElement1 = this.NeedClassElement(XmlUtils.NeedAttr((XmlNode) selectNode1, "ref-class"));
        this.CompleteAggregationVirtualProps(classElement1, deniesByClassPath);
        foreach (XmlElement selectNode2 in classElement1.SelectNodes("property"))
        {
          string str1 = XmlUtils.NeedAttr((XmlNode) selectNode2, "name");
          string str2 = ApplicationMetadataXml.EncodeVirtualPropertyName(aRole, XmlUtils.NeedAttr((XmlNode) selectNode2, "name"));
          if (str1 != XmlUtils.GetAttr((XmlNode) selectNode1, "ref-property") && str1 != XmlUtils.GetAttr((XmlNode) selectNode1, "select-ref-property") && XmlUtils.NeedAttr((XmlNode) selectNode2, "purpose") != "id")
          {
            XmlElement element = (XmlElement) classElement.SelectSingleNode("property[@name='" + str2 + "']");
            if (element != null)
            {
              new MetadataElementLoader(element).UpgradeElement(selectNode2, (MergeFlags) 0);
            }
            else
            {
              element = (XmlElement) selectNode2.CloneNode(true);
              classElement.AppendChild((XmlNode) element);
            }
            element.SetAttribute("is-virtual", "true");
            element.SetAttribute("virtual-aggregation", aRole);
            element.SetAttribute("virtual-ref-property", XmlUtils.NeedAttr((XmlNode) selectNode2, "name"));
            element.SetAttribute("name", str2);
            if (!string.IsNullOrEmpty(element.GetAttribute("ref-select-case")))
              element.SetAttribute("ref-select-case", ApplicationMetadataXml.EncodeVirtualPropertyName(aRole, element.GetAttribute("ref-select-case")));
            this.CompleteDenyForVirtualProperty(classElement, (XmlNode) element, deniesByClassPath);
          }
        }
      }
    }

    private void CompleteDeny(XmlElement denyElement)
    {
      string attribute1 = denyElement.GetAttribute("class");
      string attribute2 = denyElement.GetAttribute("operation");
      string attribute3 = denyElement.GetAttribute("for");
      string attribute4 = denyElement.GetAttribute("for-not");
      string attribute5 = denyElement.GetAttribute("property");
      string attribute6 = denyElement.GetAttribute("object-view");
      string attribute7 = denyElement.GetAttribute("search-form-template");
      string attribute8 = denyElement.GetAttribute("method");
      bool flag1 = !string.IsNullOrEmpty(attribute1);
      bool flag2 = !string.IsNullOrEmpty(attribute2);
      bool flag3 = !string.IsNullOrEmpty(attribute3) || !string.IsNullOrEmpty(attribute4);
      if (flag1 && flag2 && flag3)
      {
        bool flag4 = attribute5 != "" || attribute6 != "" || attribute7 != "" || attribute8 != "";
        bool flag5 = !flag4 && (attribute2 == "create" || attribute2 == "delete" || attribute2 == "get");
        if (flag4 || flag5)
        {
          string[] strArray = attribute1.Split('.');
          string className = strArray[0];
          if (className == "*")
            className = strArray[1];
          this.NeedClassElement(className).AppendChild(denyElement.CloneNode(false));
        }
      }
      foreach (XmlElement selectNode in denyElement.SelectNodes("deny"))
      {
        foreach (XmlAttribute attribute9 in (XmlNamedNodeMap) denyElement.Attributes)
        {
          if (!XmlUtils.ContainsAttr((XmlNode) selectNode, attribute9.Name))
            selectNode.SetAttribute(attribute9.Name, attribute9.Value);
        }
        this.CompleteDeny(selectNode);
      }
    }

    private void CompleteDenies()
    {
      foreach (XmlElement selectNode in this.FMetadataRoot.SelectNodes("deny"))
        this.CompleteDeny(selectNode);
    }

    private void CompleteClasses()
    {
      foreach (XmlElement forderedClassElement in this.FOrderedClassElements)
        this.CompleteClass(forderedClassElement);
      this.CompleteDenies();
      Dictionary<string, List<XmlElement>> deniesByClassPath = new Dictionary<string, List<XmlElement>>();
      foreach (XmlElement selectNode in this.FMetadataRoot.SelectNodes(".//deny"))
      {
        string attr = XmlUtils.GetAttr((XmlNode) selectNode, "class");
        if (!string.IsNullOrEmpty(attr))
        {
          List<XmlElement> xmlElementList;
          if (!deniesByClassPath.TryGetValue(attr, out xmlElementList))
          {
            xmlElementList = new List<XmlElement>();
            deniesByClassPath.Add(attr, xmlElementList);
          }
          xmlElementList.Add(selectNode);
        }
      }
      foreach (XmlElement forderedClassElement in this.FOrderedClassElements)
        this.CompleteAggregationVirtualProps(forderedClassElement, deniesByClassPath);
    }

    private void AppendAggregationElement(
      string className,
      string propName,
      XmlElement refElement,
      string refSelectCase)
    {
      XmlElement xmlElement1 = this.NeedClassElement(className);
      XmlElement xmlElement2 = XmlUtils.AppendElement((XmlNode) this.NeedClassElement(XmlUtils.NeedAttr((XmlNode) refElement, "ref-class")), "aggregation");
      xmlElement2.SetAttribute("role", className);
      xmlElement2.SetAttribute("role-caption", XmlUtils.GetAttr((XmlNode) refElement, "role-caption", XmlUtils.NeedAttr((XmlNode) xmlElement1, "caption")));
      xmlElement2.SetAttribute("role-list-caption", XmlUtils.GetAttr((XmlNode) refElement, "role-list-caption", XmlUtils.NeedAttr((XmlNode) xmlElement1, "list-caption")));
      xmlElement2.SetAttribute("ref-class", className);
      xmlElement2.SetAttribute("ref-property", propName);
      if (!string.IsNullOrEmpty(refSelectCase))
      {
        xmlElement2.SetAttribute("select-ref-property", refSelectCase);
        xmlElement2.SetAttribute("select-value", refElement.GetAttribute("value"));
      }
      xmlElement2.SetAttribute("ref-object-view", XmlUtils.GetAttr((XmlNode) refElement, "aggregation-object-view", "default"));
      xmlElement2.SetAttribute("is-built-in", XmlUtils.GetAttr((XmlNode) refElement, "is-built-in", "false"));
      xmlElement2.SetAttribute("aggregation-type", refSelectCase == string.Empty ? XmlUtils.GetAttr((XmlNode) refElement, "aggregation-type", "attribute") : XmlUtils.GetAttr(refElement.ParentNode, "aggregation-type", "attribute"));
      xmlElement2.SetAttribute("cardinality-min", XmlUtils.GetAttr((XmlNode) refElement, "cardinality-min", "0"));
      xmlElement2.SetAttribute("cardinality-max", XmlUtils.GetAttr((XmlNode) refElement, "cardinality-max", "0"));
      string attribute = refElement.GetAttribute("order-by");
      if (string.IsNullOrEmpty(attribute))
        attribute = xmlElement1.GetAttribute("order-by");
      xmlElement2.SetAttribute("order-by", attribute);
      if (!(xmlElement2.GetAttribute("cardinality-min") != "1") && !(xmlElement2.GetAttribute("cardinality-max") != "1"))
        return;
      xmlElement2.SetAttribute("is-built-in", "false");
    }

    private void CompleteScriptLibraries()
    {
      Dictionary<string, ApplicationMetadataXml.LoadingScriptLib> dictionary = new Dictionary<string, ApplicationMetadataXml.LoadingScriptLib>();
      ApplicationMetadataXml.LoadingScriptLib loadingScriptLib1 = new ApplicationMetadataXml.LoadingScriptLib("Default");
      loadingScriptLib1.Merge((string) null, "VBScript", "class CViewText\r\n\tDim Content\r\n\tprivate FTotals\r\n\tSub ClearContent()\r\n\t\tContent = \"\"\r\n\tEnd Sub\r\n\tsub Write(aText)\r\n\t\tContent = Content & aText\r\n\tEnd Sub\r\n\tsub AddColumnTotals(aColumnIndex, aValue)\r\n\t\tdim i, j\r\n\t\tj = -1\r\n\t\tif IsEmpty(FTotals) then\r\n\t\t\tFTotals = Array()\r\n\t\telse\r\n\t\t\tfor i = 0 to UBound(FTotals)\r\n\t\t\t\tif FTotals(i)(0) = CLng(aColumnIndex) then\r\n\t\t\t\t\tj = i\r\n\t\t\t\t\texit for\r\n\t\t\t\tend if\r\n\t\t\tnext\r\n\t\tend if\r\n\t\tif j = -1 then\r\n\t\t\tj = UBound(FTotals) + 1\r\n\t\t\tredim preserve FTotals(j)\r\n\t\t\tFTotals(j) = Array(CLng(aColumnIndex), CDbl(aValue))\r\n\t\telse\r\n\t\t\tFTotals(j)(1) = FTotals(j)(1) + CDbl(aValue)\r\n\t\tend if\r\n\tend sub\r\n\tfunction GetTotals()\r\n\t\tif IsEmpty(FTotals) then\r\n\t\t\tGetTotals = Array()\r\n\t\telse\r\n\t\t\tGetTotals = FTotals\r\n\t\tend if\r\n\tend function\r\nend class\r\n");
      dictionary.Add("default", loadingScriptLib1);
      foreach (XmlElement selectNode in this.FMetadataRoot.SelectNodes("script-library"))
      {
        string name = selectNode.GetAttribute("name");
        if (!string.IsNullOrEmpty(name))
          name = name.Trim();
        if (string.IsNullOrEmpty(name))
          name = "Default";
        ApplicationMetadataXml.LoadingScriptLib loadingScriptLib2;
        if (!dictionary.TryGetValue(name.ToLower(), out loadingScriptLib2))
        {
          loadingScriptLib2 = new ApplicationMetadataXml.LoadingScriptLib(name);
          dictionary.Add(name.ToLower(), loadingScriptLib2);
        }
        loadingScriptLib2.Merge(XmlUtils.GetAttr((XmlNode) selectNode, "using"), XmlUtils.GetAttr((XmlNode) selectNode, "language"), XmlUtils.GetOwnText((XmlNode) selectNode));
        this.FMetadataRoot.RemoveChild((XmlNode) selectNode);
      }
      ApplicationMetadataXml.LoadingScriptLib[] array = new ApplicationMetadataXml.LoadingScriptLib[dictionary.Count];
      dictionary.Values.CopyTo(array, 0);
      for (int index = 0; index < array.Length; ++index)
      {
        ApplicationMetadataXml.LoadingScriptLib loadingScriptLib2 = array[index];
        XmlElement xmlElement = XmlUtils.AppendElement((XmlNode) this.FMetadataRoot, "script-library");
        xmlElement.SetAttribute("name", loadingScriptLib2.Name);
        if (loadingScriptLib2.Language.Length > 0)
          xmlElement.SetAttribute("language", loadingScriptLib2.Language);
        if (loadingScriptLib2.Using.Count > 0)
          xmlElement.SetAttribute("using", loadingScriptLib2.JoinUsing());
        if (loadingScriptLib2.Text.Length > 0)
          ApplicationMetadataXml.AddCdata((XmlNode) xmlElement, loadingScriptLib2.Text.ToString());
      }
    }

    private class LoadingScriptLib
    {
      public readonly string Name;
      public string Language = string.Empty;
      public readonly StringCollection Using = new StringCollection();
      public readonly StringBuilder Text = new StringBuilder();

      public LoadingScriptLib(string name) => this.Name = name;

      private static void MergeUsing(string commaSeparatedNames, StringCollection usingNames)
      {
        if (string.IsNullOrEmpty(commaSeparatedNames))
          return;
        string str1 = commaSeparatedNames;
        char[] chArray = new char[1]{ ',' };
        foreach (string str2 in str1.Split(chArray))
        {
          string strB = str2.Trim();
          if (strB.Length != 0)
          {
            bool flag = false;
            foreach (string usingName in usingNames)
            {
              if (string.Compare(usingName, strB, true) == 0)
              {
                flag = true;
                break;
              }
            }
            if (!flag)
              usingNames.Add(strB);
          }
        }
      }

      public void Merge(string @using, string language, string text)
      {
        ApplicationMetadataXml.LoadingScriptLib.MergeUsing(@using, this.Using);
        if (this.Text.Length > 0)
          this.Text.Append("\r\n");
        this.Text.Append(text);
        if (this.Language.Length != 0 || language.Length <= 0)
          return;
        this.Language = language;
      }

      public string JoinUsing()
      {
        if (this.Using.Count == 0)
          return string.Empty;
        StringBuilder stringBuilder = new StringBuilder(this.Using[0]);
        for (int index = 1; index < this.Using.Count; ++index)
          stringBuilder.Append(',').Append(this.Using[index]);
        return stringBuilder.ToString();
      }
    }
  }
}
