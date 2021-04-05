﻿// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Metadata.MergeInfo
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;
using System.Xml;

namespace InMeta.ServerRuntime.Metadata
{
  internal abstract class MergeInfo
  {
    public readonly MergeFlags Flags;
    public static readonly string[] MergeActionNames = new string[4]
    {
      "create",
      "supplement",
      "replace",
      "delete"
    };

    protected MergeInfo(MergeFlags flags) => this.Flags = flags;

    protected MergeInfo()
      : this((MergeFlags) 0)
    {
    }

    public abstract string GetElementKey(
      Dictionary<string, MetadataElementLoader> childrenLoaders,
      XmlElement srcChild,
      MergeAction mergeAction);
  }
}
