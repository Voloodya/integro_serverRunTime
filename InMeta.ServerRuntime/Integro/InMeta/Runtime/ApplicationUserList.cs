// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ApplicationUserList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class ApplicationUserList : ArrayList
  {
    internal ApplicationUserList(XmlNodeList userSourceNodes)
    {
      for (int i = 0; i < userSourceNodes.Count; ++i)
        this.Add((object) new ApplicationUser(userSourceNodes[i]));
    }

    public ApplicationUser this[int index] => (ApplicationUser) base[index];

    public ApplicationUser FindByAccount(string account)
    {
      for (int index = 0; index < this.Count; ++index)
      {
        ApplicationUser applicationUser = this[index];
        if (string.Compare(applicationUser.Account, account, true) == 0)
          return applicationUser;
      }
      return (ApplicationUser) null;
    }

    public ApplicationUser NeedByAccount(string account) => this.FindByAccount(account) ?? throw new MetadataException(string.Format("Не найдена учетная запись \"{0}\".", (object) account));
  }
}
