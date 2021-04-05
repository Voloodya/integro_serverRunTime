// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DoubleProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DoubleProperty : DataProperty
  {
    private static readonly object FZeroValue = (object) 0.0;

    public DoubleProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    public double Value
    {
      get => (double) this.NotNullUntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public double ValueDef(double def) => this.IsNull ? def : this.Value;

    public double ValueDef() => this.ValueDef(0.0);

    internal override void SetValue(object value)
    {
      object obj;
      switch (value)
      {
        case null:
        case double _:
          obj = value;
          break;
        default:
          obj = (object) Convert.ToDouble(value);
          break;
      }
      base.SetValue(obj);
    }

    public override string ToString(IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((double) untypedValue).ToString(provider);
    }

    public override string ToString(string format)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((double) untypedValue).ToString(format);
    }

    protected internal override object ZeroValue => DoubleProperty.FZeroValue;

    public string ToString(string format, IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((double) untypedValue).ToString(format, provider);
    }
  }
}
