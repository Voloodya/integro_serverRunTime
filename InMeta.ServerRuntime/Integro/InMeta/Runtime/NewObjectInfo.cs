// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.NewObjectInfo
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Integro.InMeta.Runtime
{
  public class NewObjectInfo
  {
    public readonly string Id;
    public readonly Dictionary<MetadataProperty, object> Properties = new Dictionary<MetadataProperty, object>();

    internal NewObjectInfo(string id) => this.Id = id;

    internal IDictionary ToLogJson()
    {
      ListDictionary listDictionary = new ListDictionary();
      foreach (KeyValuePair<MetadataProperty, object> property in this.Properties)
        listDictionary.Add((object) property.Key.Name, property.Value);
      return (IDictionary) listDictionary;
    }
  }
}
