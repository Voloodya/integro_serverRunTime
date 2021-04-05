// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.CentralServerConnection
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Xml;

namespace Integro.InMeta.Runtime.CentralServer
{
  public class CentralServerConnection : IDisposable
  {
    private CentralServerClient FNetClient;
    private readonly string FAddress;

    public CentralServerConnection(Integro.InMeta.Runtime.CentralServer.CentralServer owner, string address)
    {
      this.FNetClient = new CentralServerClient()
      {
        Owner = owner,
        Host = address
      };
      this.FAddress = address;
    }

    protected void Dispose(bool disposing)
    {
      if (disposing && this.FNetClient != null)
        this.FNetClient.Connected = false;
      this.FNetClient = (CentralServerClient) null;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public bool PermanentConnection
    {
      get => this.FNetClient.Connected;
      set
      {
        if (this.FNetClient.Connected == value)
          return;
        if (value)
          this.FNetClient.Host = this.FAddress;
        this.FNetClient.Connected = value;
      }
    }

    private XmlDocument CentralServerRequest(string requestXml)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(this.FNetClient.Query(requestXml, this.PermanentConnection ? string.Empty : this.FAddress));
      return xmlDocument;
    }

    public int GenerateCustomId(string appId, string generatorName) => int.Parse(this.CentralServerRequest(string.Format("<GenerateCustomId AppID='{0}' Name='{1}'/>", (object) appId, (object) generatorName)).DocumentElement.SelectSingleNode("CustomId").InnerText);

    public int[] GenerateCustomIds(string appId, string generatorName, int count)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.AppendChild((XmlNode) xmlDocument.CreateElement("GenerateCustomId"));
      xmlDocument.DocumentElement.SetAttribute("AppID", appId);
      xmlDocument.DocumentElement.SetAttribute("Name", generatorName);
      xmlDocument.DocumentElement.SetAttribute("Count", count.ToString());
      XmlNodeList xmlNodeList = this.CentralServerRequest(xmlDocument.OuterXml).DocumentElement.SelectNodes("CustomId");
      int[] numArray = new int[xmlNodeList.Count];
      for (int i = 0; i < numArray.Length; ++i)
        numArray[i] = int.Parse(xmlNodeList[i].InnerText);
      return numArray;
    }

    public void ReleaseCustomIds(string appId, string generatorName, int[] ids)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.AppendChild((XmlNode) xmlDocument.CreateElement(nameof (ReleaseCustomIds)));
      xmlDocument.DocumentElement.SetAttribute("AppID", appId);
      xmlDocument.DocumentElement.SetAttribute("Name", generatorName);
      foreach (int id in ids)
      {
        XmlElement element = xmlDocument.CreateElement("Id");
        xmlDocument.DocumentElement.AppendChild((XmlNode) element);
        element.InnerText = id.ToString();
      }
      this.CentralServerRequest(xmlDocument.OuterXml);
    }

    public int GenerateRegNo(string appId) => int.Parse(this.CentralServerRequest(string.Format("<GenerateRegNo AppID='{0}'/>", (object) appId)).DocumentElement.InnerText);

    public void GenerateUpdateLogId(string appId, out int majorId, out int minorId)
    {
      XmlDocument xmlDocument = this.CentralServerRequest(string.Format("<GenerateUpdateLogId AppID='{0}'/>", (object) appId));
      majorId = int.Parse(xmlDocument.DocumentElement.GetAttribute("MajorId"));
      minorId = int.Parse(xmlDocument.DocumentElement.GetAttribute("MinorId"));
    }

    public CustomIdGenerator[] GetCustomIdGenerators(string appId)
    {
      XmlNodeList xmlNodeList = this.CentralServerRequest(string.Format("<GetCustomIdGenerators AppID='{0}'/>", (object) appId)).DocumentElement.SelectNodes("CustomIdGenerator");
      CustomIdGenerator[] customIdGeneratorArray = new CustomIdGenerator[xmlNodeList.Count];
      for (int i = 0; i < xmlNodeList.Count; ++i)
      {
        XmlElement xmlElement = (XmlElement) xmlNodeList[i];
        customIdGeneratorArray[i] = new CustomIdGenerator(xmlElement.GetAttribute("Name"), int.Parse(xmlElement.GetAttribute("LastNumber")));
      }
      return customIdGeneratorArray;
    }

    public DataId GenerateId(string appId) => new DataId(this.CentralServerRequest(string.Format("<GenerateIDs AppID='{0}'/>", (object) appId)).DocumentElement.SelectSingleNode("ID").InnerText);

    public DataId[] GenerateIds(string appId, int count)
    {
      XmlNodeList xmlNodeList = this.CentralServerRequest(string.Format("<GenerateIDs AppID='{0}' Count='{1}'/>", (object) appId, (object) count)).DocumentElement.SelectNodes("ID");
      DataId[] dataIdArray = new DataId[xmlNodeList.Count];
      for (int i = 0; i < xmlNodeList.Count; ++i)
        dataIdArray[i] = new DataId(xmlNodeList[i].InnerText);
      return dataIdArray;
    }

    public IdGroupList GetIdGroups(string appId)
    {
      XmlDocument xmlDocument = this.CentralServerRequest(string.Format("<GetIdGroups AppID='{0}'/>", (object) appId));
      IdGroupList idGroupList = new IdGroupList();
      idGroupList.Load((XmlNode) xmlDocument.DocumentElement);
      return idGroupList;
    }

    public IdConverter GetIdConverter(string appId, IdGroupList sourceGroups)
    {
      XmlDocument xmlDocument1 = new XmlDocument();
      xmlDocument1.AppendChild((XmlNode) xmlDocument1.CreateElement(nameof (GetIdConverter)));
      xmlDocument1.DocumentElement.SetAttribute("AppID", appId);
      sourceGroups.Save(xmlDocument1.DocumentElement);
      XmlDocument xmlDocument2 = this.CentralServerRequest(xmlDocument1.OuterXml);
      IdConverter idConverter = new IdConverter();
      idConverter.Load((XmlNode) xmlDocument2.DocumentElement);
      return idConverter;
    }
  }
}
