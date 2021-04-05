// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CurrencyProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class CurrencyProperty : DataProperty
  {
    private static readonly object FZeroValue = (object) 0M;

    public CurrencyProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    public Decimal Value
    {
      get => (Decimal) this.NotNullUntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public Decimal ValueDef(Decimal def) => this.IsNull ? def : this.Value;

    public Decimal ValueDef() => this.ValueDef(0M);

    internal override void SetValue(object value)
    {
      object obj;
      switch (value)
      {
        case null:
        case Decimal _:
          obj = value;
          break;
        default:
          obj = (object) Convert.ToDecimal(value);
          break;
      }
      base.SetValue(obj);
    }

    public override string ToString(IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((Decimal) untypedValue).ToString(provider);
    }

    public override string ToString(string format)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((Decimal) untypedValue).ToString(format);
    }

    protected internal override object ZeroValue => CurrencyProperty.FZeroValue;

    public string ToString(string format, IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((Decimal) untypedValue).ToString(format, provider);
    }
  }
}
