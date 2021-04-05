// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Web.WebPage
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using System;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace Integro.InMeta.Web
{
  [ComVisible(false)]
  public class WebPage : Page
  {
    private DataApplication FApplication;
    private DataSession FSession;
    private int FLogRecordId;

    public int LogRecordId => this.FLogRecordId;

    public HttpApplicationState WebApplication => base.Application;

    public HttpSessionState WebSession => base.Session;

    public HttpContext WebContext => this.Context;

    protected virtual DataApplication CreateApplicationInstance() => Utils.CreateApplication(this.Context);

    protected override void OnUnload(EventArgs e)
    {
      if (this.FSession != null)
      {
        this.FSession.TraceDisposingFromWebPage(this.FLogRecordId, this.GetType());
        this.FSession.Dispose();
        this.FSession = (DataSession) null;
      }
      base.OnUnload(e);
    }

    public DataApplication Application
    {
      get
      {
        if (this.FApplication == null)
          this.FApplication = this.CreateApplicationInstance();
        return this.FApplication;
      }
    }

    public DataSession Session
    {
      get
      {
        if (this.FSession == null)
        {
          this.FSession = this.Application.CreateSession(this.Request.ServerVariables["AUTH_USER"]);
          this.FLogRecordId = this.FSession.TraceCreationFromWebPage(this.GetType());
        }
        return this.FSession;
      }
    }
  }
}
