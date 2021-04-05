// Decompiled with JetBrains decompiler
// Type: Scripting.ScriptControl
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Scripting
{
  [ComVisible(true)]
  public class ScriptControl : IDisposable, IActiveScriptSite, IActiveScriptSiteWindow
  {
    private object FCodeObject;
    private IActiveScript FEngine;
    private ActiveScriptParseWrapper FParser;
    private readonly List<object> FSourceContexts = new List<object>();
    private readonly Dictionary<string, object> FNamedObjects = new Dictionary<string, object>();
    public readonly ScriptControlError Error = new ScriptControlError();

    public string Language { get; set; }

    public IntPtr SiteWindowHandle { get; set; }

    public int Timeout { get; set; }

    public void AddCode(string code, object sourceContext)
    {
      this.CreateScriptEngine();
      this.FSourceContexts.Add(sourceContext);
      System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo;
      if (this.FParser.ParseScriptText(code, (string) null, (object) null, (string) null, new IntPtr(this.FSourceContexts.Count - 1), 0U, ScriptText.None, out object _, out exceptionInfo) != HRESULT.Ok)
        throw new ScriptCodeException(exceptionInfo);
    }

    public void AddCode(string code) => this.AddCode(code, (object) code);

    public void AddObject(string name, object obj, bool addMembers)
    {
      this.CreateScriptEngine();
      if (!this.FNamedObjects.ContainsKey(name))
        this.FNamedObjects.Add(name, obj);
      ScriptItem flags = addMembers ? ScriptItem.IsVisible | ScriptItem.GlobalMembers : ScriptItem.IsVisible;
      this.FEngine.AddNamedItem(name, flags);
    }

    public void Dispose() => this.Close();

    public void Close()
    {
      if (this.FEngine != null)
      {
        this.FEngine.Close();
        Marshal.ReleaseComObject((object) this.FEngine);
        this.FEngine = (IActiveScript) null;
      }
      if (this.FParser != null)
      {
        this.FParser.Dispose();
        this.FParser = (ActiveScriptParseWrapper) null;
      }
      if (this.FCodeObject != null)
      {
        Marshal.ReleaseComObject(this.FCodeObject);
        this.FCodeObject = (object) null;
      }
      this.FSourceContexts.Clear();
      this.FNamedObjects.Clear();
      this.Error.Clear();
    }

    private object GetCodeObject()
    {
      this.CreateScriptEngine();
      ScriptControl.IDispatch dispatch;
      this.FEngine.GetScriptDispatch((string) null, out dispatch);
      return (object) dispatch;
    }

    public object CodeObject
    {
      get
      {
        if (this.FCodeObject == null)
          this.FCodeObject = this.GetCodeObject();
        return this.FCodeObject;
      }
    }

    public void ExecuteStatement(string statement)
    {
      this.CreateScriptEngine();
      System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo;
      HRESULT scriptText = this.FParser.ParseScriptText(statement, (string) null, (object) null, (string) null, IntPtr.Zero, 0U, ScriptText.None, out object _, out exceptionInfo);
      this.FEngine.SetScriptState(ScriptState.Connected);
      if (scriptText != HRESULT.Ok)
        throw new ScriptCodeException(exceptionInfo);
    }

    public object Eval(string statement)
    {
      this.CreateScriptEngine();
      object result;
      System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo;
      HRESULT scriptText = this.FParser.ParseScriptText(statement, (string) null, (object) null, (string) null, IntPtr.Zero, 0U, ScriptText.IsExpression, out result, out exceptionInfo);
      this.FEngine.SetScriptState(ScriptState.Connected);
      if (scriptText != HRESULT.Ok)
        throw new ScriptCodeException(exceptionInfo);
      return result;
    }

    public bool MemberExists(string name)
    {
      string[] names = new string[1]{ name };
      int[] dispIds = new int[1];
      Guid empty = Guid.Empty;
      return ((ScriptControl.IDispatch) this.CodeObject).GetIDsOfNames(ref empty, names, 1, CultureInfo.CurrentCulture.LCID, dispIds) == 0;
    }

    public object Run(string procedureName, params object[] parameters)
    {
      object codeObject = this.CodeObject;
      return codeObject.GetType().InvokeMember(procedureName, BindingFlags.InvokeMethod, (Binder) null, codeObject, parameters);
    }

    private void CreateScriptEngine()
    {
      if (this.FEngine != null)
        return;
      this.FEngine = (IActiveScript) Activator.CreateInstance(Type.GetTypeFromProgID(this.Language));
      this.FParser = new ActiveScriptParseWrapper((object) this.FEngine);
      this.FEngine.SetScriptSite((IActiveScriptSite) this);
      this.FParser.InitNew();
    }

    void IActiveScriptSite.GetLCID(out int lcid) => lcid = CultureInfo.CurrentCulture.LCID;

    void IActiveScriptSite.GetItemInfo(
      string name,
      ScriptInfo returnMask,
      object[] item,
      IntPtr[] typeInfo)
    {
      if ((returnMask & ScriptInfo.TypeInfo) != ScriptInfo.None)
        typeInfo[0] = IntPtr.Zero;
      object obj;
      if ((returnMask & ScriptInfo.Unknown) == ScriptInfo.None || !this.FNamedObjects.TryGetValue(name, out obj))
        return;
      item[0] = obj;
    }

    void IActiveScriptSite.GetDocVersionString(out string version) => version = (string) null;

    void IActiveScriptSite.OnScriptTerminate(
      object result,
      System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo)
    {
    }

    void IActiveScriptSite.OnStateChange(ScriptState scriptState)
    {
    }

    void IActiveScriptSite.OnScriptError(IActiveScriptError scriptError)
    {
      if (scriptError != null)
      {
        System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo;
        scriptError.GetExceptionInfo(out exceptionInfo);
        uint sourceContext;
        uint lineNumber;
        int characterPosition;
        scriptError.GetSourcePosition(out sourceContext, out lineNumber, out characterPosition);
        string sourceLine;
        try
        {
          scriptError.GetSourceLineText(out sourceLine);
        }
        catch (COMException ex)
        {
          if (ex.ErrorCode != -2147467259)
            throw;
          else
            sourceLine = (string) null;
        }
        this.Error.Number = exceptionInfo.scode;
        this.Error.Column = characterPosition + 1;
        this.Error.Line = (int) lineNumber + 1;
        this.Error.Source = exceptionInfo.bstrSource;
        this.Error.Description = exceptionInfo.bstrDescription;
        this.Error.HelpFile = exceptionInfo.bstrHelpFile;
        this.Error.HelpContext = exceptionInfo.dwHelpContext;
        this.Error.Text = sourceLine;
        this.Error.SourceContext = this.FSourceContexts[(int) sourceContext];
      }
      throw new COMException((string) null, 1);
    }

    void IActiveScriptSite.OnEnterScript()
    {
    }

    void IActiveScriptSite.OnLeaveScript()
    {
    }

    void IActiveScriptSiteWindow.GetWindow(out IntPtr windowHandle) => windowHandle = this.SiteWindowHandle;

    void IActiveScriptSiteWindow.EnableModeless(bool enable)
    {
    }

    public void Reset() => this.Close();

    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [ComImport]
    internal interface IDispatch
    {
      [MethodImpl(MethodImplOptions.PreserveSig)]
      int GetTypeInfoCount();

      ITypeInfo GetTypeInfo([MarshalAs(UnmanagedType.U4)] int typeInfoIndex, [MarshalAs(UnmanagedType.U4)] int localeId);

      [MethodImpl(MethodImplOptions.PreserveSig)]
      int GetIDsOfNames(
        ref Guid interfaceId,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] names,
        int nameCount,
        int localeId,
        [MarshalAs(UnmanagedType.LPArray)] int[] dispIds);

      [MethodImpl(MethodImplOptions.PreserveSig)]
      int Invoke(
        int memberDispId,
        ref Guid interfaceId,
        [MarshalAs(UnmanagedType.U4)] int localeId,
        [MarshalAs(UnmanagedType.U4)] int flags,
        ref System.Runtime.InteropServices.ComTypes.DISPPARAMS parameters,
        [MarshalAs(UnmanagedType.LPArray), Out] object[] result,
        ref System.Runtime.InteropServices.ComTypes.EXCEPINFO excepInfo,
        [MarshalAs(UnmanagedType.LPArray), Out] IntPtr[] errorParameters);
    }
  }
}
