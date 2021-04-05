// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ScriptErrorFormatter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.IO;
using System.Text;

namespace Integro.InMeta.Runtime
{
  public class ScriptErrorFormatter
  {
    public static void FormatCodeAndHighlightErrorLine(
      StringBuilder html,
      string code,
      int errorLine)
    {
      TextReader textReader = (TextReader) new StringReader(code);
      int num = 1;
      string codeLine;
      while ((codeLine = textReader.ReadLine()) != null)
      {
        if (num == errorLine)
          html.Append("<font color=red><b>");
        if (num > 1)
          html.Append("<br>");
        ScriptErrorFormatter.FormatCodeLine(html, codeLine);
        if (num == errorLine)
          html.Append("</b></font>");
        ++num;
      }
    }

    private static void FormatCodeLine(StringBuilder html, string codeLine) => html.Append(HtmlFormatter.EncodeHtml(codeLine));
  }
}
