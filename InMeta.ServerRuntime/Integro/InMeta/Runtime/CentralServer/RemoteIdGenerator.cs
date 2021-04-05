// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.RemoteIdGenerator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Xml;

namespace Integro.InMeta.Runtime.CentralServer
{
  internal class RemoteIdGenerator : IdGenerator
  {
    private readonly CentralServerConnection FCentralServerConnection;
    private readonly string FAppId;

    public RemoteIdGenerator(Integro.InMeta.Runtime.CentralServer.CentralServer owner, ApplicationDbConfig connectionParams)
      : base(connectionParams)
    {
      string centralServerAddress;
      RemoteIdGenerator.TryParseAddress(IdGenerator.GetIdGeneratorAddress(connectionParams), out centralServerAddress, out this.FAppId);
      this.FCentralServerConnection = new CentralServerConnection(owner, centralServerAddress);
    }

    private static void TryParseAddress(
      string address,
      out string centralServerAddress,
      out string appId)
    {
      int length = address != null ? address.IndexOf('@') : -1;
      if (length < 0)
      {
        appId = (string) null;
        centralServerAddress = (string) null;
      }
      else
      {
        appId = address.Substring(0, length);
        centralServerAddress = address.Substring(length + 1);
      }
    }

    public override void Shutdown() => this.FCentralServerConnection.Dispose();

    public override string GenerateId() => this.FCentralServerConnection.GenerateId(this.FAppId).ToString();

    public override IdGroupList GetIdGroups() => this.FCentralServerConnection.GetIdGroups(this.FAppId);

    public override IdConverter GetIdConverter(IdGroupList sourceGroups) => this.FCentralServerConnection.GetIdConverter(this.FAppId, sourceGroups);

    public override void GenerateIds(int count, XmlNode resultParent)
    {
      foreach (DataId id in this.FCentralServerConnection.GenerateIds(this.FAppId, count))
        XmlUtils.AppendElement(resultParent, "ID").InnerText = id.ToString();
    }

    public override void GenerateCustomId(int count, string generatorName, XmlNode resultParent)
    {
      foreach (int customId in this.FCentralServerConnection.GenerateCustomIds(this.FAppId, generatorName, count))
        XmlUtils.AppendElement(resultParent, "CustomId").InnerText = customId.ToString();
    }

    public override void ReleaseCustomIds(string generatorName, int[] ids) => this.FCentralServerConnection.ReleaseCustomIds(this.FAppId, generatorName, ids);

    public override CustomIdGenerator[] GetCustomIdGenerators() => this.FCentralServerConnection.GetCustomIdGenerators(this.FAppId);

    public override int GenerateRegNo() => this.FCentralServerConnection.GenerateRegNo(this.FAppId);

    public override void GenerateUpdateLogId(out int majorId, out int minorId) => this.FCentralServerConnection.GenerateUpdateLogId(this.FAppId, out majorId, out minorId);
  }
}
