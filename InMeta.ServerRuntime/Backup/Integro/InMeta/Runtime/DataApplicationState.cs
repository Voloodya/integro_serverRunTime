// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataApplicationState
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime.CentralServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  internal class DataApplicationState
  {
    private static readonly object FCacheSync = new object();
    private static readonly List<DataApplicationState> FCache = new List<DataApplicationState>();
    private readonly object FSync = new object();
    private readonly string FId;
    private readonly string FCentralServerAddress;
    private ApplicationMetadata FMetadata;
    private ApplicationSettings FSettings;
    private DataApplication.ApplicationLog FLog;
    private string FSyncVersion;

    private static bool Same(string x, string y) => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);

    internal static DataApplicationState Get(
      string id,
      string centralServerAddress)
    {
      centralServerAddress = CentralServerClient.GetHost(centralServerAddress) + (object) ':' + (object) CentralServerClient.GetPort(centralServerAddress);
      lock (DataApplicationState.FCacheSync)
      {
        for (int index = 0; index < DataApplicationState.FCache.Count; ++index)
        {
          DataApplicationState applicationState = DataApplicationState.FCache[index];
          if (DataApplicationState.Same(id, applicationState.FId) && DataApplicationState.Same(centralServerAddress, applicationState.FCentralServerAddress))
          {
            if (!applicationState.IsMetadataChanged())
              return applicationState;
            DataApplicationState.FCache.RemoveAt(index);
            break;
          }
        }
        DataApplicationState applicationState1 = new DataApplicationState(id, centralServerAddress);
        applicationState1.FSyncVersion = Utility.GetSyncVersion(applicationState1.MetadataSyncFile);
        DataApplicationState.FCache.Add(applicationState1);
        return applicationState1;
      }
    }

    public string Id => this.FId;

    public string CentralServerAddress => this.FCentralServerAddress;

    public DataApplicationState(string id, string centralServerAddress)
    {
      this.FId = id;
      this.FCentralServerAddress = centralServerAddress;
    }

    public ApplicationMetadata Metadata
    {
      get
      {
        lock (this.FSync)
        {
          if (this.FMetadata == null)
            this.FMetadata = new ApplicationMetadata((XmlNode) this.QueryThisCentralServer("<QueryAppMetadata AppID='{0}'/>", (object) this.FId).DocumentElement);
          return this.FMetadata;
        }
      }
    }

    private XmlDocument QueryThisCentralServer(
      string requestFmt,
      params object[] paramArray)
    {
      return CentralServerClient.QuerySpecified(this.FCentralServerAddress, requestFmt, paramArray);
    }

    public ApplicationSettings Settings
    {
      get
      {
        lock (this.FSync)
        {
          if (this.FSettings == null)
          {
            XmlNode sourceNode;
            if (this.FMetadata != null)
              sourceNode = this.FMetadata.SourceNode;
            else
              sourceNode = (XmlNode) this.QueryThisCentralServer("<QueryAppMetadata AppID='{0}' Reduced='true'/>", (object) this.FId).DocumentElement;
            this.FSettings = new ApplicationSettings(this, sourceNode);
          }
          return this.FSettings;
        }
      }
    }

    public DataApplication.ApplicationLog Log
    {
      get
      {
        lock (this.FSync)
          return this.FLog ?? (this.FLog = new DataApplication.ApplicationLog(this));
      }
    }

    private static void RewriteFileIfExists(string fileName)
    {
      if (!File.Exists(fileName))
        return;
      FileAttributes attributes = File.GetAttributes(fileName);
      try
      {
        File.SetAttributes(fileName, attributes & ~FileAttributes.ReadOnly);
        StreamReader streamReader = new StreamReader(fileName, Encoding.Default);
        string end = streamReader.ReadToEnd();
        streamReader.Close();
        StreamWriter streamWriter = new StreamWriter(fileName, false, Encoding.Default);
        streamWriter.Write(end);
        streamWriter.Close();
      }
      finally
      {
        File.SetAttributes(fileName, attributes);
      }
    }

    internal void Reload()
    {
      this.QueryThisCentralServer("<ReloadApp AppID='{0}'/>", (object) this.FId);
      this.FMetadata = (ApplicationMetadata) null;
      this.FSettings = (ApplicationSettings) null;
      this.FLog = (DataApplication.ApplicationLog) null;
      DataApplicationState.RewriteFileIfExists(this.Settings.RootFolder + "\\Web\\global.asa");
      DataApplicationState.RewriteFileIfExists(this.Settings.RootFolder + "\\Web\\global.asax");
    }

    private string MetadataSyncFile => Path.Combine(this.Settings.RootFolder, "MetadataSync.txt");

    private bool IsMetadataChanged() => Utility.GetSyncVersion(this.MetadataSyncFile) != this.FSyncVersion;
  }
}
