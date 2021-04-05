// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DataApplication
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime.CentralServer;
using Integro.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DataApplication : IDisposable
  {
    private DataApplicationState FState;

    public ApplicationMetadata Metadata => this.FState.Metadata;

    public ApplicationSettings Settings => this.FState.Settings;

    public DataApplication.ApplicationLog Log => this.FState.Log;

    protected virtual DataSession CreateSessionInstance(string userAccount) => new DataSession(this, userAccount);

    public string Id => this.FState.Id;

    public string CentralServerAddress => this.FState.CentralServerAddress;

    public DataApplication(string id, string centralServerAddress) => this.Init(id, centralServerAddress);

    public DataApplication(string id) => this.Init(id, (string) null);

    public void Init(string id, string centralServerAddress) => this.FState = DataApplicationState.Get(id, centralServerAddress);

    protected static void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
      DataApplication.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~DataApplication() => DataApplication.Dispose(false);

    public DataSession CreateSession(string userAccount) => this.CreateSessionInstance(userAccount);

    public DataSession CreateSession() => this.CreateSessionInstance(WindowsIdentity.GetCurrent().Name);

    private static string ReadRegistryLocalMachine(
      string keyName,
      string valueName,
      string defaultValue)
    {
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keyName))
      {
        if (registryKey == null)
          return defaultValue;
        object obj = registryKey.GetValue(valueName);
        return obj != null ? obj.ToString() : defaultValue;
      }
    }

    public static string InMetaInstallPath
    {
      get
      {
        string str = DataApplication.ReadRegistryLocalMachine("Software\\Integro\\InMeta\\Install", "RootFolder", (string) null);
        if (string.IsNullOrEmpty(str))
          str = DataApplication.ReadRegistryLocalMachine("Software\\Wow6432Node\\Integro\\InMeta\\Install", "RootFolder", (string) null);
        return str;
      }
    }

    public static string[] GetAppIds(string centralServerAddress)
    {
      List<string> stringList = new List<string>();
      foreach (XmlElement selectNode in CentralServerClient.QuerySpecified(centralServerAddress, "<QueryAppList/>").DocumentElement.SelectNodes("App"))
        stringList.Add(selectNode.GetAttribute("ID"));
      return stringList.ToArray();
    }

    public void Reload() => this.FState.Reload();

    private bool LoadDbConfigAndFindPasswordNode(
      out string fileName,
      out XmlDocument config,
      out XmlElement passwordNode)
    {
      fileName = Path.Combine(this.Settings.RootFolder, "Meta\\_db_info.xml");
      if (!File.Exists(fileName))
      {
        config = (XmlDocument) null;
        passwordNode = (XmlElement) null;
        return false;
      }
      config = XmlUtils.LoadDocument(fileName, Encoding.GetEncoding(1251));
      passwordNode = config.DocumentElement.SelectSingleNode("driver/param[@name='login-password']") as XmlElement;
      return passwordNode != null;
    }

    public bool IsDbLoginPasswordEncrypted
    {
      get
      {
        XmlElement passwordNode;
        return this.LoadDbConfigAndFindPasswordNode(out string _, out XmlDocument _, out passwordNode) && !StrUtils.IsNullOrEmpty(passwordNode.GetAttribute("encryption"));
      }
    }

    public void EncryptDbLoginPassword()
    {
      string fileName;
      XmlDocument config;
      XmlElement passwordNode;
      if (!this.LoadDbConfigAndFindPasswordNode(out fileName, out config, out passwordNode) || !StrUtils.IsNullOrEmpty(passwordNode.GetAttribute("encryption")))
        return;
      string method = "on";
      passwordNode.InnerText = Utility.Encrypt(passwordNode.InnerText, method);
      passwordNode.SetAttribute("encryption", method);
      FileAttributes attributes = File.GetAttributes(fileName);
      if ((attributes & FileAttributes.ReadOnly) != (FileAttributes) 0)
        File.SetAttributes(fileName, attributes & ~FileAttributes.ReadOnly);
      XmlTextWriter xmlTextWriter = new XmlTextWriter(fileName, Encoding.GetEncoding(1251));
      try
      {
        xmlTextWriter.Formatting = Formatting.Indented;
        config.Save((XmlWriter) xmlTextWriter);
      }
      finally
      {
        xmlTextWriter.Close();
      }
      if ((attributes & FileAttributes.ReadOnly) != (FileAttributes) 0)
        File.SetAttributes(fileName, attributes);
    }

    public class ApplicationLog
    {
      private readonly DataApplicationState FApplicationState;

      internal ApplicationLog(DataApplicationState applicationState) => this.FApplicationState = applicationState;

      private static int ReadIntFromFileOrReturnDefault(string fileName, int defaultValue)
      {
        try
        {
          int result;
          if (File.Exists(fileName) && int.TryParse(File.ReadAllText(fileName).Trim(), out result))
            return result;
        }
        catch
        {
        }
        return defaultValue;
      }

      public int AppendRecord(int parentRecordId, string authUserName, string source, string text)
      {
        ApplicationSettings settings = this.FApplicationState.Settings;
        if (!settings.LogEnabled)
          return 0;
        DateTime now = DateTime.Now;
        string str1 = Path.Combine(settings.RootFolder, settings.LogFileName);
        using (FileStream fileStream = SysUtils.OpenExclusivelyForAppend(str1, TimeSpan.FromSeconds(5.0)))
        {
          string str2 = str1 + ".Id";
          int num = DataApplication.ApplicationLog.ReadIntFromFileOrReturnDefault(str2, 1);
          File.WriteAllText(str2, (num + 1).ToString());
          byte[] bytes = settings.LogEncoding.GetBytes(string.Format("{0:d}\t{1:hh:mm:ss.fffffff}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n", (object) now, (object) now, (object) parentRecordId, (object) num, (object) authUserName, (object) StrUtils.EncodeMultilineString(source), (object) StrUtils.EncodeMultilineString(text)));
          fileStream.Write(bytes, 0, bytes.Length);
          return num;
        }
      }
    }
  }
}
