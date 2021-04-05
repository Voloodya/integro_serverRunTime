// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.CentralServerClient
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Xml;

namespace Integro.InMeta.Runtime.CentralServer
{
  [ComVisible(true)]
  [Guid("B8353C41-8BEB-3D45-BEA5-6A7A7247CD93")]
  public class CentralServerClient
  {
    public const int DefaultPort = 3063;
    private string FAddress;
    private Integro.InMeta.Runtime.CentralServer.CentralServer FOwner;
    private Integro.InMeta.Runtime.CentralServer.CentralServer FServer;

    internal Integro.InMeta.Runtime.CentralServer.CentralServer Owner
    {
      set => this.FOwner = value;
    }

    internal static string GetHost(string address)
    {
      if (string.IsNullOrEmpty(address))
        return "localhost";
      int length = address.IndexOf(':');
      string str = length >= 0 ? address.Substring(0, length).Trim() : address.Trim();
      return str.Length > 0 ? str : "localhost";
    }

    internal static int GetPort(string address)
    {
      if (StrUtils.IsNullOrEmpty(address))
        return 3063;
      int num = address.IndexOf(':');
      string s = num >= 0 ? address.Substring(num + 1).Trim() : (string) null;
      return !string.IsNullOrEmpty(s) ? int.Parse(s) : 3063;
    }

    private static string MakeAddress(string host, int port) => string.Format("{0}:{1}", (object) host, (object) port);

    internal static XmlDocument QuerySpecified(
      string centralServerAddress,
      string requestFmt,
      params object[] paramArray)
    {
      CentralServerClient centralServerClient = new CentralServerClient();
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(centralServerClient.Query(string.Format(requestFmt, paramArray), centralServerAddress));
      return xmlDocument;
    }

    private void DropServerConnection()
    {
      if (this.FServer == null)
        return;
      RemotingServices.Disconnect((MarshalByRefObject) this.FServer);
      this.FServer = (Integro.InMeta.Runtime.CentralServer.CentralServer) null;
    }

    private static bool IsSameHost(string hostName1, string hostName2)
    {
      if (StrUtils.IsNullOrEmpty(hostName1))
        hostName1 = "localhost";
      if (StrUtils.IsNullOrEmpty(hostName2))
        hostName2 = "localhost";
      return string.Compare(hostName1, hostName2, true) == 0;
    }

    private Integro.InMeta.Runtime.CentralServer.CentralServer ConnectToServer(
      string address)
    {
      string host = CentralServerClient.GetHost(address);
      int port = CentralServerClient.GetPort(address);
      if (this.FOwner != null && this.FOwner.ListeningPort == port && CentralServerClient.IsSameHost(host, "localhost"))
        return this.FOwner;
      return (Integro.InMeta.Runtime.CentralServer.CentralServer) RemotingServices.Connect(typeof (Integro.InMeta.Runtime.CentralServer.CentralServer), string.Format("tcp://{0}:{1}/InMetaCentralServer/CentralServer", (object) host, (object) port));
    }

    public string Host
    {
      get => CentralServerClient.GetHost(this.FAddress);
      set
      {
        string str = CentralServerClient.MakeAddress(CentralServerClient.GetHost(value), CentralServerClient.GetPort(value));
        if (!(str != this.FAddress))
          return;
        this.DropServerConnection();
        this.FAddress = str;
      }
    }

    public int Port
    {
      get => CentralServerClient.GetPort(this.FAddress);
      set
      {
        string str = CentralServerClient.MakeAddress(CentralServerClient.GetHost(this.FAddress), value);
        if (!(str != this.FAddress))
          return;
        this.DropServerConnection();
        this.FAddress = str;
      }
    }

    public bool Connected
    {
      get => this.FServer != null;
      set
      {
        if (value == this.Connected)
          return;
        this.DropServerConnection();
        if (!value)
          return;
        this.FServer = this.ConnectToServer(this.FAddress);
      }
    }

    [ComVisible(false)]
    public XmlDocument Query(XmlDocument request, string address)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(this.Query(request.OuterXml, address));
      return xmlDocument;
    }

    public string Query(string requestXml, string address)
    {
      if (StrUtils.IsNullOrEmpty(address))
        address = this.FAddress;
      return address == this.FAddress && this.FServer != null ? this.FServer.XmlRequest(requestXml) : this.ConnectToServer(address).XmlRequest(requestXml);
    }
  }
}
