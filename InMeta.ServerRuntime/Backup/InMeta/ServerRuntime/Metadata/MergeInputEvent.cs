// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Metadata.MergeInputEvent
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections.Generic;
using System.Xml;

namespace InMeta.ServerRuntime.Metadata
{
  internal class MergeInputEvent : MergeInfo
  {
    public MergeInputEvent()
      : base(MergeFlags.AppendNewLineAfterTextSections)
    {
    }

    public override string GetElementKey(
      Dictionary<string, MetadataElementLoader> childrenLoaders,
      XmlElement srcChild,
      MergeAction mergeAction)
    {
      string attr1 = XmlUtils.GetAttr((XmlNode) srcChild, "name", (string) null);
      string attr2 = XmlUtils.GetAttr((XmlNode) srcChild, "id", (string) null);
      if (attr1 == null && attr2 == null)
        return (string) null;
      string key = srcChild.Name + ":" + attr1 + ":" + attr2;
      return mergeAction != MergeAction.Supplement || !childrenLoaders.ContainsKey(key) ? key : (string) null;
    }
  }
}
