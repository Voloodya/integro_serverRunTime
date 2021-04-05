// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.StringProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class StringProperty : DataProperty
  {
    public StringProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    public string Value
    {
      get => (string) this.NotNullUntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public string ValueDef(string def) => this.IsNull ? def : this.Value;

    public string ValueDef() => this.ValueDef(string.Empty);

    internal override void SetValue(object value)
    {
      if (value == null)
      {
        base.SetValue((object) null);
      }
      else
      {
        string str = !(value is string) ? Convert.ToString(value) : (string) value;
        if (this.Metadata.DataType != DataType.Memo && str.Length > this.Metadata.DataLength)
          throw new DataException(string.Format("Значение свойства \"{0}\"=\"{1}\" не может превышать {2} символов.", (object) this.Metadata.Name, (object) str, (object) this.Metadata.DataLength));
        base.SetValue((object) str);
      }
    }

    public override string ToString(IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((string) untypedValue).ToString(provider);
    }

    protected internal override object ZeroValue => (object) string.Empty;
  }
}
