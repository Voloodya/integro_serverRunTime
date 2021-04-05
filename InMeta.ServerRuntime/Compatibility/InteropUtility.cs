// Decompiled with JetBrains decompiler
// Type: Compatibility.InteropUtility
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Reflection;

namespace Compatibility
{
  internal class InteropUtility
  {
    public static object Invoke(object target, string methodName, params object[] parameters) => target.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod, (Binder) null, target, parameters);

    public static object PropertyGet(
      object target,
      string propertyName,
      params object[] parameters)
    {
      return target.GetType().InvokeMember(propertyName, BindingFlags.GetProperty, (Binder) null, target, parameters);
    }
  }
}
