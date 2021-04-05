// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.IntegerProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class IntegerProperty : DataProperty
  {
    private static readonly object FZeroValue = (object) 0;

    public IntegerProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    public int Value
    {
      get => (int) this.NotNullUntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public int ValueDef(int def) => this.IsNull ? def : this.Value;

    public int ValueDef() => this.ValueDef(0);

    internal override void SetValue(object value)
    {
      object obj;
      switch (value)
      {
        case null:
        case int _:
          obj = value;
          break;
        default:
          obj = (object) Convert.ToInt32(value);
          break;
      }
      base.SetValue(obj);
    }

    public override string ToString(IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((int) untypedValue).ToString(provider);
    }

    public override string ToString(string format)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((int) untypedValue).ToString(format);
    }

    protected internal override object ZeroValue => IntegerProperty.FZeroValue;

    public string ToString(string format, IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((int) untypedValue).ToString(format, provider);
    }
  }
}
