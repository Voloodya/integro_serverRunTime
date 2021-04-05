// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewScriptRuntime
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Compatibility.InMetaManager;
using Compatibility.InMetaUtils;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(true)]
  public class ObjectViewScriptRuntime
  {
    private InMetaXStrUtils FXStrUtils;
    private InMetaXmlUtils FXmlUtils;
    private CMetaManager FMetaManager;
    private readonly ScriptRuntimeCurrentUser FCurrentUser;

    public ObjectViewScriptRuntime(DataSession session) => this.FCurrentUser = new ScriptRuntimeCurrentUser(session);

    public object CurrentUser => (object) this.FCurrentUser;

    public object MetaManager => (object) this.EnsureMetaManager();

    private CMetaManager EnsureMetaManager() => this.FMetaManager ?? (this.FMetaManager = new CMetaManager());

    public object Meta => (object) this.EnsureMetaManager().Meta;

    public object XmlUtils => (object) (this.FXmlUtils ?? (this.FXmlUtils = new InMetaXmlUtils()));

    public object XStrUtils => (object) (this.FXStrUtils ?? (this.FXStrUtils = new InMetaXStrUtils()));
  }
}
