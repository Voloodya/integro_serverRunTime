// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.CentralServer
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using InMeta.ServerRuntime.Metadata;
using Integro.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime.CentralServer
{
  [ComVisible(false)]
  public class CentralServer : MarshalByRefObject
  {
    private readonly int FListeningPort;
    private readonly ApplicationMetadataXmlList FApps;
    private readonly Integro.InMeta.Runtime.CentralServer.CentralServer.IdGeneratorList FIdGenerators;
    private readonly XmlUtils.XmlRequestDispatcher FRequestDispatcher;
    private static readonly object FSync = new object();

    public override object InitializeLifetimeService() => (object) null;

    public CentralServer(int listeningPort)
    {
      this.FListeningPort = listeningPort;
      this.FApps = new ApplicationMetadataXmlList();
      this.FIdGenerators = new Integro.InMeta.Runtime.CentralServer.CentralServer.IdGeneratorList();
      this.FRequestDispatcher = new XmlUtils.XmlRequestDispatcher();
      this.FRequestDispatcher.Add("QueryServerInfo", "ServerInfo", new XmlUtils.XmlRequestHandler(Integro.InMeta.Runtime.CentralServer.CentralServer.XmlRequest_QueryServerInfo));
      this.FRequestDispatcher.Add("QueryAppList", "AppList", new XmlUtils.XmlRequestHandler(this.XmlRequest_QueryAppList));
      this.FRequestDispatcher.Add("GenerateIDs", "GenerateIDs", new XmlUtils.XmlRequestHandler(this.XmlRequest_GenerateIDs));
      this.FRequestDispatcher.Add("GetIdGroups", "IdGroups", new XmlUtils.XmlRequestHandler(this.XmlRequest_GetIdGroups));
      this.FRequestDispatcher.Add("GetIdConverter", "IdConverter", new XmlUtils.XmlRequestHandler(this.XmlRequest_GetIdConverter));
      this.FRequestDispatcher.Add("GetCustomIdGenerators", "CustomIdGenerators", new XmlUtils.XmlRequestHandler(this.XmlRequest_GetCustomIdGenerators));
      this.FRequestDispatcher.Add("GenerateCustomId", "GenerateCustomId", new XmlUtils.XmlRequestHandler(this.XmlRequest_GenerateCustomId));
      this.FRequestDispatcher.Add("ReleaseCustomIds", "CustomIds", new XmlUtils.XmlRequestHandler(this.XmlRequest_ReleaseCustomIds));
      this.FRequestDispatcher.Add("GenerateRegNo", "RegNo", new XmlUtils.XmlRequestHandler(this.XmlRequest_GenerateRegNo));
      this.FRequestDispatcher.Add("GenerateUpdateLogId", "UpdateLogId", new XmlUtils.XmlRequestHandler(this.XmlRequest_GenerateUpdateLogId));
      this.FRequestDispatcher.Add("ReloadApp", "ReloadApp", new XmlUtils.XmlRequestHandler(this.XmlRequest_ReloadApp));
      this.FRequestDispatcher.Add("QueryAppMetadata", "AppMetadata", new XmlUtils.XmlRequestHandler(this.XmlRequest_QueryAppMetadata));
      this.FRequestDispatcher.Add("StopServer", "StopServer", new XmlUtils.XmlRequestHandler(Integro.InMeta.Runtime.CentralServer.CentralServer.XmlRequest_StopServer));
      this.FRequestDispatcher.Add("ReloadServer", "ReloadServer", new XmlUtils.XmlRequestHandler(this.XmlRequest_ReloadServer));
      this.InternalStartup();
    }

    private void InternalStartup()
    {
      this.FApps.Reload();
      this.FIdGenerators.Clear();
    }

    private void InternalShutdown()
    {
      this.FApps.Clear();
      IdGenerator[] idGeneratorArray = new IdGenerator[this.FIdGenerators.Count];
      this.FIdGenerators.CopyTo((Array) idGeneratorArray, 0);
      this.FIdGenerators.Clear();
      for (int index = 0; index < idGeneratorArray.Length; ++index)
        idGeneratorArray[index].Shutdown();
    }

    private ApplicationMetadataXml NeedRequestApp(XmlNode request)
    {
      string appId = XmlUtils.NeedAttr(request, "AppID");
      return this.FApps.Find(appId) ?? throw new System.Data.DataException(string.Format("Не найдено приложение {0}.", (object) appId));
    }

    private IdGenerator EnsureIdGenerator(ApplicationMetadataXml app)
    {
      IdGenerator idGenerator = this.FIdGenerators.FindByApp(app);
      if (idGenerator != null)
        return idGenerator;
      ApplicationDbConfig connectionParams = new ApplicationDbConfig((XmlNode) app.ConnectionParams);
      for (int index = 0; index < this.FIdGenerators.Count; ++index)
      {
        IdGenerator fidGenerator = this.FIdGenerators[index];
        if (fidGenerator.IsSameConnectionParams(connectionParams))
        {
          idGenerator = fidGenerator;
          break;
        }
      }
      if (idGenerator == null)
      {
        idGenerator = IdGenerator.Create(this, connectionParams);
        this.FIdGenerators.Add((object) idGenerator);
      }
      idGenerator.ServedApps.Add((object) app);
      return idGenerator;
    }

    private static void XmlRequest_QueryServerInfo(XmlNode request, XmlNode response)
    {
      XmlUtils.SetAttr(response, "Name", "InMetaServer");
      XmlUtils.SetAttr(response, "Caption", "Сервер системы ИнМета");
      XmlUtils.SetAttr(response, "Version", "1.0.0");
    }

    private void XmlRequest_QueryAppList(XmlNode request, XmlNode response)
    {
      for (int index = 0; index < this.FApps.Count; ++index)
      {
        ApplicationMetadataXml fapp = this.FApps[index];
        XmlNode node = (XmlNode) XmlUtils.AppendElement(response, "App");
        XmlUtils.SetAttr(node, "ID", fapp.Id);
        XmlUtils.SetAttr(node, "Name", fapp.Name);
      }
    }

    private void XmlRequest_GenerateIDs(XmlNode request, XmlNode response) => this.EnsureIdGenerator(this.NeedRequestApp(request)).GenerateIds(XmlUtils.GetIntAttr(request, "Count", 1), response);

    private void XmlRequest_GenerateCustomId(XmlNode request, XmlNode response) => this.EnsureIdGenerator(this.NeedRequestApp(request)).GenerateCustomId(XmlUtils.GetIntAttr(request, "Count", 1), XmlUtils.NeedAttr(request, "Name"), response);

    private void XmlRequest_GetCustomIdGenerators(XmlNode request, XmlNode response)
    {
      foreach (CustomIdGenerator customIdGenerator in this.EnsureIdGenerator(this.NeedRequestApp(request)).GetCustomIdGenerators())
      {
        XmlElement element = response.OwnerDocument.CreateElement("CustomIdGenerator");
        response.AppendChild((XmlNode) element);
        element.SetAttribute("Name", customIdGenerator.Name);
        element.SetAttribute("LastNumber", customIdGenerator.LastNumber.ToString());
      }
    }

    private void XmlRequest_GenerateRegNo(XmlNode request, XmlNode response) => response.InnerText = this.EnsureIdGenerator(this.NeedRequestApp(request)).GenerateRegNo().ToString();

    private void XmlRequest_GenerateUpdateLogId(XmlNode request, XmlNode response)
    {
      int majorId;
      int minorId;
      this.EnsureIdGenerator(this.NeedRequestApp(request)).GenerateUpdateLogId(out majorId, out minorId);
      XmlElement xmlElement = (XmlElement) response;
      xmlElement.SetAttribute("MajorId", majorId.ToString());
      xmlElement.SetAttribute("MinorId", minorId.ToString());
    }

    private void XmlRequest_ReleaseCustomIds(XmlNode request, XmlNode response)
    {
      XmlNodeList xmlNodeList = request.SelectNodes("Id");
      int[] ids = new int[xmlNodeList.Count];
      for (int i = 0; i < xmlNodeList.Count; ++i)
        ids[i] = int.Parse(xmlNodeList[i].InnerText);
      this.EnsureIdGenerator(this.NeedRequestApp(request)).ReleaseCustomIds(((XmlElement) request).GetAttribute("Name"), ids);
    }

    private void XmlRequest_GetIdGroups(XmlNode request, XmlNode response) => this.EnsureIdGenerator(this.NeedRequestApp(request)).GetIdGroups().Save((XmlElement) response);

    private void XmlRequest_GetIdConverter(XmlNode request, XmlNode response)
    {
      IdGroupList sourceGroups = new IdGroupList();
      sourceGroups.Load(request);
      this.EnsureIdGenerator(this.NeedRequestApp(request)).GetIdConverter(sourceGroups).Save((XmlElement) response);
    }

    private void XmlRequest_ReloadApp(XmlNode request, XmlNode response)
    {
      ApplicationMetadataXml app = this.NeedRequestApp(request);
      IdGenerator byApp = this.FIdGenerators.FindByApp(app);
      if (byApp != null)
      {
        byApp.ServedApps.Remove((object) app);
        if (byApp.ServedApps.Count == 0)
        {
          this.FIdGenerators.Remove((object) byApp);
          byApp.Shutdown();
        }
      }
      app.Reload();
    }

    private void XmlRequest_QueryAppMetadata(XmlNode request, XmlNode response)
    {
      ApplicationMetadataXml applicationMetadataXml = this.NeedRequestApp(request);
      if (XmlUtils.GetBoolAttr(request, "Reduced"))
        this.FRequestDispatcher.ReplaceResponse(applicationMetadataXml.ReducedMetadata);
      else if (XmlUtils.GetBoolAttr(request, "Incomplete"))
        this.FRequestDispatcher.ReplaceResponse(applicationMetadataXml.GetIncompleteMetadata(out Dictionary<string, XmlElement> _, out List<XmlElement> _));
      else
        this.FRequestDispatcher.ReplaceResponse(applicationMetadataXml.Metadata);
    }

    private static void XmlRequest_StopServer(XmlNode request, XmlNode response) => throw new NotImplementedException();

    private void XmlRequest_ReloadServer(XmlNode request, XmlNode response) => this.Reload();

    public XmlDocument XmlRequest(XmlDocument request)
    {
      lock (Integro.InMeta.Runtime.CentralServer.CentralServer.FSync)
        return this.FRequestDispatcher.DispatchRequest(request);
    }

    public string XmlRequest(string requestXml)
    {
      try
      {
        XmlDocument request = new XmlDocument();
        request.LoadXml(requestXml);
        return this.XmlRequest(request).OuterXml;
      }
      catch (Exception ex)
      {
        throw new Exception("При обращении к центральному серверу данных возникла следующая ошибка: " + (object) ex);
      }
    }

    public void Reload()
    {
      this.InternalShutdown();
      this.InternalStartup();
    }

    public int ListeningPort => this.FListeningPort;

    private class IdGeneratorList : ArrayList
    {
      public IdGenerator this[int index] => (IdGenerator) base[index];

      public IdGenerator FindByApp(ApplicationMetadataXml app)
      {
        for (int index = 0; index < this.Count; ++index)
        {
          IdGenerator idGenerator = this[index];
          if (idGenerator.ServedApps.Contains((object) app))
            return idGenerator;
        }
        return (IdGenerator) null;
      }
    }
  }
}
