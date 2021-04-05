// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.UpdateLogWriter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro;
using Integro.InMeta.Runtime;
using Integro.Utils;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace InMeta.ServerRuntime
{
  internal class UpdateLogWriter : IDisposable
  {
    private readonly DataApplication FApplication;

    public UpdateLogWriter(DataApplication application) => this.FApplication = application;

    ~UpdateLogWriter() => this.Dispose(false);

    public void Append(string userName, object details)
    {
      try
      {
        ApplicationSettings settings = this.FApplication.Settings;
        DateTime now = DateTime.Now;
        string str = Path.Combine(settings.RootFolder, "Log");
        if (!Directory.Exists(str))
          Directory.CreateDirectory(str);
        using (FileStream fileStream = SysUtils.OpenExclusivelyForAppend(Path.Combine(str, string.Format("{0:d4}-{1:d2}-{2:d2}.csv", (object) now.Year, (object) now.Month, (object) now.Day)), TimeSpan.FromSeconds(5.0)))
        {
          byte[] bytes = Encoding.UTF8.GetBytes(string.Format("{0},{1},{2}\r\n", (object) XmlConvert.ToString(now, XmlDateTimeSerializationMode.Unspecified), (object) userName, (object) UpdateLogWriter.EncodeCsvValue(Json.Encode(details))));
          fileStream.Write(bytes, 0, bytes.Length);
        }
      }
      catch (Exception ex)
      {
      }
    }

    private static string EncodeCsvValue(string value)
    {
      StringBuilder stringBuilder = new StringBuilder("\"");
      for (int index = 0; index < value.Length; ++index)
      {
        char ch = value[index];
        if (ch == '"')
          stringBuilder.Append('"');
        stringBuilder.Append(ch);
      }
      stringBuilder.Append("\"");
      return stringBuilder.ToString();
    }

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      GC.SuppressFinalize((object) this);
    }
  }
}
