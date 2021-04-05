// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataLookupValueList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataLookupValueList : ArrayList
  {
    internal void LoadFromXml(XmlNode node)
    {
      foreach (XmlNode selectNode in node.SelectNodes("lookup-value"))
      {
        string defaultValue = selectNode.InnerText.Trim();
        this.Add((object) new MetadataLookupValue(defaultValue, XmlUtils.GetAttr(selectNode, "caption", defaultValue)));
      }
    }

    public MetadataLookupValue this[int index] => (MetadataLookupValue) base[index];

    public MetadataLookupValue Find(string value)
    {
      for (int index = 0; index < this.Count; ++index)
      {
        MetadataLookupValue metadataLookupValue = (MetadataLookupValue) base[index];
        if (metadataLookupValue.Value == value)
          return metadataLookupValue;
      }
      return (MetadataLookupValue) null;
    }
  }
}
