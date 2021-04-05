// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.BooleanProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class BooleanProperty : DataProperty
  {
    private static readonly object FZeroValue = (object) false;

    public BooleanProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    public bool Value
    {
      get => (bool) this.NotNullUntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public bool ValueDef(bool def) => this.IsNull ? def : this.Value;

    public bool ValueDef() => this.ValueDef(false);

    internal override void SetValue(object value)
    {
      object obj;
      switch (value)
      {
        case null:
        case bool _:
          obj = value;
          break;
        default:
          obj = (object) Convert.ToBoolean(value);
          break;
      }
      base.SetValue(obj);
    }

    public override string ToString(IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((bool) untypedValue).ToString(provider);
    }

    protected internal override object ZeroValue => BooleanProperty.FZeroValue;
  }
}
