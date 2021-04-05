// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataChildRef
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataChildRef
  {
    public readonly MetadataAssociationRef AggregationRef;
    public readonly MetadataClass ChildClass;
    public readonly string MemberName;
    internal readonly int Index;

    internal MetadataChildRef(MetadataAssociationRef aggregationRef, int index)
    {
      this.AggregationRef = aggregationRef;
      aggregationRef.OwnerChildRef = this;
      this.ChildClass = aggregationRef.Association.Class;
      this.MemberName = aggregationRef.AggregationRoleMemberName;
      this.Index = index;
    }
  }
}
