// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbManager
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections;
using System.Data.OracleClient;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public class InDbManager
  {
    internal static InDbManager.ConnectionParam[] SqlParams = new InDbManager.ConnectionParam[7]
    {
      new InDbManager.ConnectionParam("Data Source", "server", "."),
      new InDbManager.ConnectionParam("User ID", "login-name", ""),
      new InDbManager.ConnectionParam("Password", "login-password", ""),
      new InDbManager.ConnectionParam("Initial Catalog", "database", (string) null),
      new InDbManager.ConnectionParam("Connect Timeout", "connection-timeout", ""),
      new InDbManager.ConnectionParam("Max Pool Size", "max-pool-size", ""),
      new InDbManager.ConnectionParam("Pooling", "pooling", "")
    };
    private static readonly string[] FSqlDatabaseIdentificationParameterNames = new string[3]
    {
      "server",
      "database",
      "login-name"
    };
    private static readonly InDbManager.ConnectionParam[] FOracleParams = new InDbManager.ConnectionParam[5]
    {
      new InDbManager.ConnectionParam("Data Source", "service-name", (string) null),
      new InDbManager.ConnectionParam("User ID", "login-name", ""),
      new InDbManager.ConnectionParam("Password", "login-password", ""),
      new InDbManager.ConnectionParam("Max Pool Size", "max-pool-size", ""),
      new InDbManager.ConnectionParam("Pooling", "pooling", "")
    };
    private static readonly string[] FOracleDatabaseIdentificationParameterNames = new string[2]
    {
      "service-name",
      "login-name"
    };

    public static bool SameDatabase(
      string driver1,
      Hashtable driverConfiguration1,
      string driver2,
      Hashtable driverConfiguration2)
    {
      if (driver1 != driver2)
        return false;
      string[] identificationParameterNames;
      if (driver1 == "ADO.MSSQL")
      {
        identificationParameterNames = InDbManager.FSqlDatabaseIdentificationParameterNames;
      }
      else
      {
        if (!(driver1 == "ADO.ORACLE"))
          return false;
        identificationParameterNames = InDbManager.FOracleDatabaseIdentificationParameterNames;
      }
      foreach (string str1 in identificationParameterNames)
      {
        bool flag1 = driverConfiguration1.ContainsKey((object) str1);
        bool flag2 = driverConfiguration2.ContainsKey((object) str1);
        if (flag1 && flag2)
        {
          string str2 = (string) driverConfiguration1[(object) str1];
          string str3 = (string) driverConfiguration2[(object) str1];
          if (string.Compare(str2.Trim(), str3.Trim(), true) != 0)
            return false;
        }
        else if (flag1 || flag2)
          return false;
      }
      return true;
    }

    internal static string BuildConnectionString(
      Hashtable inDbParameters,
      InDbManager.ConnectionParam[] paramInfos)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < paramInfos.Length; ++index)
      {
        InDbManager.ConnectionParam paramInfo = paramInfos[index];
        string inDbParameter = (string) inDbParameters[(object) paramInfo.InDbName];
        if (inDbParameter == null && paramInfo.DefaultValue == null)
          throw new InDbException(string.Format("Элемент <driver> не содержит обязательный параметр {0}.", (object) paramInfo.InDbName));
        string str = inDbParameter ?? paramInfo.DefaultValue;
        if (str.Length > 0)
          stringBuilder.Append(paramInfo.ConnectionStringName).Append("=\"").Append(str).Append("\";");
      }
      return stringBuilder.ToString();
    }

    public static InDbDatabase OpenDatabase(string driverName, Hashtable parameters)
    {
      if (string.Compare(driverName, "ADO.MSSQL", true) == 0)
        return (InDbDatabase) new InDbSqlDatabase(parameters);
      if (string.Compare(driverName, "ADO.ORACLE", true) != 0)
        throw new InDbException(string.Format("Неизвестный тип драйвера InDb: {0}", (object) driverName));
      OracleConnection connection = new OracleConnection(InDbManager.BuildConnectionString(parameters, InDbManager.FOracleParams));
      connection.Open();
      return (InDbDatabase) new InDbOracleDatabase(connection);
    }

    public static InDbDatabase OpenDatabase(XmlNode connectionParams)
    {
      Hashtable parameters = new Hashtable();
      XmlNodeList xmlNodeList = connectionParams.SelectNodes("param");
      for (int i = 0; i < xmlNodeList.Count; ++i)
      {
        XmlNode node = xmlNodeList[i];
        parameters[(object) XmlUtils.NeedAttr(node, "name")] = (object) node.InnerText;
      }
      return InDbManager.OpenDatabase(XmlUtils.GetAttr(connectionParams, "name"), parameters);
    }

    internal struct ConnectionParam
    {
      public readonly string ConnectionStringName;
      public readonly string InDbName;
      public readonly string DefaultValue;

      public ConnectionParam(string connectionStringName, string inDbName, string defaultValue)
      {
        this.ConnectionStringName = connectionStringName;
        this.InDbName = inDbName;
        this.DefaultValue = defaultValue;
      }
    }
  }
}
