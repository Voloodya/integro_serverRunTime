// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.HtmlFormatter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Text;

namespace Integro.InMeta.Runtime
{
  public class HtmlFormatter
  {
    public static string EncodeHtml(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "<br>").Replace("\t", "&nbsp;&nbsp;").Replace(" ", "&nbsp;");

    public static void AppendText(StringBuilder html, string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      html.Append(HtmlFormatter.EncodeHtml(text));
    }

    public static void AppendText(StringBuilder html, string text, string style)
    {
      if (string.IsNullOrEmpty(style))
        HtmlFormatter.AppendText(html, text);
      else
        html.AppendFormat("<span style=\"{0}\">{1}</span>", (object) style, (object) HtmlFormatter.EncodeHtml(text));
    }
  }
}
