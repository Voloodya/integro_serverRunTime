// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewTextGetter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Integro.InMeta.Runtime
{
  internal class ObjectViewTextGetter : MemberValueGetter
  {
    private readonly string FObjectViewName;

    public ObjectViewTextGetter(string objectViewName) => this.FObjectViewName = objectViewName;

    internal override object GetValue(DataObject obj) => (object) obj.GetView(this.FObjectViewName);

    internal override void PrepareLoadPlan(LoadPlan plan, DataSession session)
    {
      MetadataObjectView viewMetadata = plan.Class.ObjectViews.Need(this.FObjectViewName);
      plan.MergeWith(session[plan.Class].GetObjectViewEvaluator(viewMetadata).GetLoadPlan(session));
    }
  }
}
