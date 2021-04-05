﻿// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Metadata.MergeByNameAttr
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections.Generic;
using System.Xml;

namespace InMeta.ServerRuntime.Metadata
{
  internal class MergeByNameAttr : MergeInfo
  {
    public MergeByNameAttr()
    {
    }

    public MergeByNameAttr(MergeFlags mergeFlags)
      : base(mergeFlags)
    {
    }

    public override string GetElementKey(
      Dictionary<string, MetadataElementLoader> childrenLoaders,
      XmlElement srcChild,
      MergeAction mergeAction)
    {
      string attr = XmlUtils.GetAttr((XmlNode) srcChild, "name", (string) null);
      return attr == null ? (string) null : srcChild.Name + ":" + attr;
    }
  }
}
