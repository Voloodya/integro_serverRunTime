// Decompiled with JetBrains decompiler
// Type: Integro.InDbs.InDbIndexDef
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Runtime.InteropServices;

namespace Integro.InDbs
{
  [ComVisible(false)]
  public class InDbIndexDef
  {
    internal string FName;
    public readonly InDbFieldDefs FieldDefs;

    internal InDbIndexDef(string name, InDbFieldDefs fieldDefs)
    {
      this.FName = name;
      this.FieldDefs = fieldDefs;
    }

    public string Name => this.FName;
  }
}
