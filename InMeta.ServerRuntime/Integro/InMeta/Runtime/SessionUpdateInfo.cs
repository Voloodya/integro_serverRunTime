// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.SessionUpdateInfo
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System.Collections;
using System.Collections.Specialized;

namespace Integro.InMeta.Runtime
{
  public class SessionUpdateInfo
  {
    public readonly ClassUpdatesInfo[] UpdatesByClass;

    internal SessionUpdateInfo(ClassUpdatesInfo[] updatesByClass) => this.UpdatesByClass = updatesByClass;

    internal IDictionary ToLogJson(InDbDatabase dbForOriginalValues)
    {
      ListDictionary listDictionary = new ListDictionary();
      foreach (ClassUpdatesInfo classUpdatesInfo in this.UpdatesByClass)
        listDictionary.Add((object) classUpdatesInfo.Class.Name, (object) classUpdatesInfo.ToLogJson(dbForOriginalValues));
      return (IDictionary) listDictionary;
    }
  }
}
