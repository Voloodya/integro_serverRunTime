// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Web.Utils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using Integro.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace Integro.InMeta.Web
{
  public class Utils
  {
    private static string FindFileInDirOrParents(string dirPath, string fileName)
    {
      string path = Path.Combine(dirPath, fileName);
      if (File.Exists(path))
        return path;
      DirectoryInfo parent = Directory.GetParent(dirPath);
      return parent == null ? (string) null : Integro.InMeta.Web.Utils.FindFileInDirOrParents(parent.FullName, fileName);
    }

    public static string GetInMetaAppId(HttpContext context)
    {
      string appSetting = WebConfigurationManager.AppSettings["InMetaAppId"];
      if (!string.IsNullOrEmpty(appSetting))
        return appSetting;
      string fileInDirOrParents = Integro.InMeta.Web.Utils.FindFileInDirOrParents(context.Server.MapPath(context.Request.ApplicationPath), "Meta\\_db_info.xml");
      return !string.IsNullOrEmpty(fileInDirOrParents) ? XmlUtils.GetChildText((XmlNode) XmlUtils.LoadDocument(fileInDirOrParents).DocumentElement, "id") : (string) null;
    }

    internal static DataApplication CreateApplication(HttpContext context)
    {
      string inMetaAppId = Integro.InMeta.Web.Utils.GetInMetaAppId(context);
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type type = assembly.GetType("InMeta.Application");
        if (type != null)
          return (DataApplication) Activator.CreateInstance(type, (object) inMetaAppId, null);
      }
      return new DataApplication(inMetaAppId, ":3063");
    }
  }
}
