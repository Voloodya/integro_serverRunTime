// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.Utility
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using Scripting;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Integro.InMeta.Runtime
{
  public class Utility
  {
    public const string SimpleEncryptionMethod = "on";
    private const string SimpleEncryptorTrail = "q3tgdxsw56";

    private static string EncryptSimple(string decrypted)
    {
      decrypted += "q3tgdxsw56";
      byte[] bytes = Encoding.Unicode.GetBytes(decrypted);
      for (int index = 0; index < bytes.Length; ++index)
        bytes[index] = ~bytes[index];
      return Convert.ToBase64String(bytes);
    }

    private static string DecryptSimple(string encrypted)
    {
      byte[] bytes = Convert.FromBase64String(encrypted.Trim());
      for (int index = 0; index < bytes.Length; ++index)
        bytes[index] = ~bytes[index];
      string str = Encoding.Unicode.GetString(bytes);
      return str.Substring(0, str.Length - "q3tgdxsw56".Length);
    }

    public static string Encrypt(string decrypted, string method)
    {
      if (StrUtils.IsNullOrEmpty(method))
        return decrypted;
      switch (method.ToLower())
      {
        case "on":
          return Utility.EncryptSimple(decrypted);
        default:
          throw new Exception("Ошибка расшифровки пароля доступа к базе данных: неизвестный тип шифрования.");
      }
    }

    public static string Decrypt(string encrypted, string method)
    {
      if (StrUtils.IsNullOrEmpty(method))
        return encrypted;
      switch (method.ToLower())
      {
        case "on":
          return Utility.DecryptSimple(encrypted);
        default:
          throw new Exception("Ошибка расшифровки пароля доступа к базе данных: неизвестный тип шифрования.");
      }
    }

    public static string GetSyncVersion(string syncFileName) => File.Exists(syncFileName) ? File.GetLastWriteTime(syncFileName).ToString() : (string) null;

    public static void IncreaseSyncVersion(string syncFileName)
    {
      DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(1.0);
      while (DateTime.Now < dateTime)
      {
        try
        {
          File.WriteAllText(syncFileName, DateTime.Now.ToString());
          break;
        }
        catch (Exception ex)
        {
        }
      }
    }

    public static object PrepareComParameter(object value)
    {
      if (value == null)
        return (object) null;
      if (value is Decimal num)
        return (object) new CurrencyWrapper(num);
      if (value.GetType().IsArray)
      {
        Array array = (Array) value;
        for (int index = 0; index < array.Length; ++index)
          array.SetValue(Utility.PrepareComParameter(array.GetValue(index)), index);
      }
      return value;
    }

    public static string GetScriptErrorDetailsHtml(
      ScriptControlError error,
      ScriptErrorOperation errorOperation,
      string scriptMetadataSource)
    {
      StringBuilder html = new StringBuilder();
      html.AppendFormat("{0}<br>\r\nКод ошибки: {1}<br>\r\nСтрока, символ: {2}, {3}<br>\r\nИсточник: {4}<br>\r\nОписание: {5}<br>\r\nИсточник метаданных: {6}<br><br>\r\n<b>Текст программы</b>\r\n<pre style='margin: 4pt; padding: 4pt; background-color: LightGoldenrodYellow; border: 1px solid Goldenrod;'>", errorOperation == ScriptErrorOperation.AddCode ? (object) "Ошибка компиляции скрипта" : (object) "Ошибка выполнения скрипта", (object) error.Number, (object) error.Line, (object) error.Column, (object) HtmlUtils.EncodeText(error.Source), (object) HtmlUtils.EncodeText(error.Description), (object) HtmlUtils.EncodeText(scriptMetadataSource));
      Utility.AppendScriptText(html, string.Concat(error.SourceContext), error.Line, error.Column);
      html.Append("</pre>");
      return html.ToString();
    }

    private static void AppendEncodedScritText(StringBuilder html, string scriptText)
    {
      if (string.IsNullOrEmpty(scriptText))
        return;
      html.Append(HtmlUtils.EncodeText(scriptText.Replace("\t", "  ")));
    }

    private static void AppendScriptText(
      StringBuilder html,
      string scriptText,
      int errorLine,
      int errorColumn)
    {
      StringReader stringReader = new StringReader(scriptText);
      int num = 0;
      while (true)
      {
        string scriptText1 = stringReader.ReadLine();
        if (scriptText1 != null)
        {
          if (num > 0)
            html.AppendLine();
          if (num == errorLine - 1)
          {
            html.Append("<font color=red><b>");
            Utility.AppendEncodedScritText(html, scriptText1.Substring(0, errorColumn - 1));
            html.Append("<u>");
            Utility.AppendEncodedScritText(html, new string(scriptText1[errorColumn - 1], 1));
            html.Append("</u>");
            Utility.AppendEncodedScritText(html, scriptText1.Substring(errorColumn));
            html.Append("</b></font>");
          }
          else
            Utility.AppendEncodedScritText(html, scriptText1);
          ++num;
        }
        else
          break;
      }
    }
  }
}
