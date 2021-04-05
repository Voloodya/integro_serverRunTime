// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.IdGenerator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using Integro.Utils;
using System.Collections;
using System.Xml;

namespace Integro.InMeta.Runtime.CentralServer
{
  internal abstract class IdGenerator
  {
    private const string Chars = "0123456789ABCDEF";
    public readonly ArrayList ServedApps = new ArrayList();
    private readonly ApplicationDbConfig FConnectionParams;

    protected IdGenerator(ApplicationDbConfig connectionParams) => this.FConnectionParams = connectionParams;

    public abstract void Shutdown();

    public abstract IdGroupList GetIdGroups();

    public abstract IdConverter GetIdConverter(IdGroupList sourceGroups);

    public abstract string GenerateId();

    public abstract void GenerateIds(int count, XmlNode resultParent);

    public abstract void GenerateCustomId(int count, string generatorName, XmlNode resultParent);

    public abstract void ReleaseCustomIds(string generatorName, int[] ids);

    public abstract CustomIdGenerator[] GetCustomIdGenerators();

    public abstract int GenerateRegNo();

    public abstract void GenerateUpdateLogId(out int majorId, out int minorId);

    public ApplicationDbConfig ConnectionParams => this.FConnectionParams;

    public bool IsSameConnectionParams(ApplicationDbConfig connectionParams) => InDbManager.SameDatabase(this.FConnectionParams.DriverName, this.FConnectionParams.Parameters, connectionParams.DriverName, connectionParams.Parameters);

    public static string IdToStr(long id)
    {
      if (id == 0L)
        return string.Empty;
      return new string(new char[12]
      {
        "0123456789ABCDEF"[(int) (id >> 44 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 40 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 36 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 32 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 28 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 24 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 20 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 16 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 12 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 8 & 15L)],
        "0123456789ABCDEF"[(int) (id >> 4 & 15L)],
        "0123456789ABCDEF"[(int) (id & 15L)]
      });
    }

    public static string GetIdGeneratorAddress(ApplicationDbConfig connectionParams) => connectionParams.Parameters.ContainsKey((object) "id-generator") ? (string) connectionParams.Parameters[(object) "id-generator"] : (string) null;

    public static IdGenerator Create(
      Integro.InMeta.Runtime.CentralServer.CentralServer owner,
      ApplicationDbConfig connectionParams)
    {
      return StrUtils.IsNullOrEmpty(IdGenerator.GetIdGeneratorAddress(connectionParams)) ? (IdGenerator) new DirectIdGenerator(connectionParams) : (IdGenerator) new RemoteIdGenerator(owner, connectionParams);
    }
  }
}
