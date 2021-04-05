// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Metadata.ApplicationMetadataXmlList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace InMeta.ServerRuntime.Metadata
{
  internal class ApplicationMetadataXmlList : List<ApplicationMetadataXml>
  {
    private readonly string FInMetaRootFolder;

    public ApplicationMetadataXmlList() => this.FInMetaRootFolder = SysUtils.ExcludeTrailingPathSeparator(DataApplication.InMetaInstallPath) ?? string.Empty;

    public void Reload()
    {
      this.Clear();
      foreach (string directory in Directory.GetDirectories(this.FInMetaRootFolder))
      {
        if (File.Exists(Path.Combine(directory, "Meta\\meta-app.xml")))
        {
          try
          {
            ApplicationMetadataXml applicationMetadataXml = new ApplicationMetadataXml(this.FInMetaRootFolder, directory.Substring(this.FInMetaRootFolder.Length + 1));
            applicationMetadataXml.Reload();
            this.Add(applicationMetadataXml);
          }
          catch (Exception ex)
          {
            EventLog.WriteEntry("InMetaServer", ex.ToString(), EventLogEntryType.Warning);
          }
        }
      }
    }

    public ApplicationMetadataXml Find(string appId)
    {
      for (int index = 0; index < this.Count; ++index)
      {
        ApplicationMetadataXml applicationMetadataXml = this[index];
        if (string.Compare(applicationMetadataXml.Id, appId, true) == 0)
          return applicationMetadataXml;
      }
      return (ApplicationMetadataXml) null;
    }
  }
}
