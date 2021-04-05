// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.AssociationRefLoadPlan
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class AssociationRefLoadPlan
  {
    public readonly MetadataAssociationRef Ref;
    public readonly LoadPlan Plan;

    public AssociationRefLoadPlan(MetadataAssociationRef assRef, LoadPlan plan)
    {
      this.Ref = assRef;
      this.Plan = LoadPlan.UseExistingOrCreateNew(plan, assRef.RefClass);
    }
  }
}
