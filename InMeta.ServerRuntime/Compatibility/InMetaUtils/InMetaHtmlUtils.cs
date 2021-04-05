// Decompiled with JetBrains decompiler
// Type: Compatibility.InMetaUtils.InMetaHtmlUtils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Compatibility.InMetaUtils
{
  [ComVisible(true)]
  public class InMetaHtmlUtils
  {
    public static readonly InMetaHtmlUtils Instance = new InMetaHtmlUtils();

    public string DecodeText(string text) => text.Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");

    public string EncodeText(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    public string HtmlToText(string html) => html.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");

    public string TextToHtml(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
  }
}
