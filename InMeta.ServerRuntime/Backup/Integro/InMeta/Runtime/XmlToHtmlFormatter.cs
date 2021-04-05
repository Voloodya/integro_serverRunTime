// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.XmlToHtmlFormatter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class XmlToHtmlFormatter : HtmlFormatter
  {
    private const string IndentText = "&nbsp;&nbsp;";

    private static void AppendXmlSymbol(StringBuilder html, string text) => HtmlFormatter.AppendText(html, text, "color: blue");

    private static void AppendXmlText(StringBuilder html, string text) => HtmlFormatter.AppendText(html, text, "");

    private static void AppendXmlComment(StringBuilder html, string text) => HtmlFormatter.AppendText(html, text, "color: gray");

    private static void AppendXmlElementName(StringBuilder html, string name) => HtmlFormatter.AppendText(html, name, "color: #990000");

    private static void AppendXmlAttributeName(StringBuilder html, string name) => HtmlFormatter.AppendText(html, name, "color: #990000");

    private static void AppendXmlAttributeValue(StringBuilder html, string value) => HtmlFormatter.AppendText(html, value, "");

    private static void AppendXmlAttributes(StringBuilder html, XmlNode element)
    {
      XmlAttributeCollection attributes = element.Attributes;
      if (attributes == null)
        return;
      for (int i = 0; i < attributes.Count; ++i)
      {
        XmlAttribute xmlAttribute = attributes[i];
        html.Append(' ');
        XmlToHtmlFormatter.AppendXmlAttributeName(html, xmlAttribute.Name);
        XmlToHtmlFormatter.AppendXmlSymbol(html, "=\"");
        XmlToHtmlFormatter.AppendXmlAttributeValue(html, xmlAttribute.Value);
        XmlToHtmlFormatter.AppendXmlSymbol(html, "\"");
      }
    }

    public static string GetElementHtml(XmlNode element, string indent)
    {
      StringBuilder html = new StringBuilder();
      XmlToHtmlFormatter.AppendXmlElement(html, element, indent);
      return html.ToString();
    }

    public static void AppendXmlElement(StringBuilder html, XmlNode element, string indent)
    {
      XmlToHtmlFormatter.AppendXmlSymbol(html, "<");
      XmlToHtmlFormatter.AppendXmlElementName(html, element.Name);
      XmlToHtmlFormatter.AppendXmlAttributes(html, element);
      StringBuilder html1 = new StringBuilder();
      bool flag1 = false;
      bool flag2 = false;
      if (element.ChildNodes.Count > 0)
      {
        for (int i = 0; i < element.ChildNodes.Count; ++i)
        {
          XmlNode childNode = element.ChildNodes[i];
          if (childNode.NodeType == XmlNodeType.Text)
          {
            if (flag2)
              html1.Append("<br>");
            XmlToHtmlFormatter.AppendXmlText(html1, childNode.InnerText);
            flag1 = true;
          }
          else
          {
            if (flag1 || flag2)
              html1.Append("<br>");
            flag2 = true;
            if (childNode.NodeType == XmlNodeType.Element)
              XmlToHtmlFormatter.AppendXmlElement(html1, childNode, indent + "&nbsp;&nbsp;");
            else if (childNode.NodeType == XmlNodeType.CDATA)
            {
              XmlToHtmlFormatter.AppendXmlSymbol(html1, "<![CDATA[");
              XmlToHtmlFormatter.AppendXmlText(html1, childNode.InnerText);
              XmlToHtmlFormatter.AppendXmlSymbol(html1, "]]>");
            }
            else if (childNode.NodeType == XmlNodeType.Comment)
            {
              XmlToHtmlFormatter.AppendXmlSymbol(html1, "<!--");
              XmlToHtmlFormatter.AppendXmlComment(html1, childNode.InnerText);
              XmlToHtmlFormatter.AppendXmlSymbol(html1, "-->");
            }
          }
        }
      }
      if (!flag1 && !flag2)
        XmlToHtmlFormatter.AppendXmlSymbol(html, "/>");
      else if (flag1 && !flag2)
      {
        XmlToHtmlFormatter.AppendXmlSymbol(html, ">");
        html.Append(html1.ToString());
        XmlToHtmlFormatter.AppendXmlSymbol(html, "</");
        XmlToHtmlFormatter.AppendXmlElementName(html, element.Name);
        XmlToHtmlFormatter.AppendXmlSymbol(html, ">");
      }
      else
      {
        XmlToHtmlFormatter.AppendXmlSymbol(html, ">");
        html.AppendFormat("<div style=\"{0}\">{1}</div>", (object) "border-left: 1px dotted gray; padding-left: 4mm", (object) html1);
        XmlToHtmlFormatter.AppendXmlSymbol(html, "</");
        XmlToHtmlFormatter.AppendXmlElementName(html, element.Name);
        XmlToHtmlFormatter.AppendXmlSymbol(html, ">");
      }
    }

    private static class Styles
    {
      public const string Symbol = "color: blue";
      public const string Text = "";
      public const string Comment = "color: gray";
      public const string ElementName = "color: #990000";
      public const string AttributeName = "color: #990000";
      public const string AttributeValue = "";
      public const string ElementChildren = "border-left: 1px dotted gray; padding-left: 4mm";
    }
  }
}
