// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.DateTimeProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class DateTimeProperty : DataProperty
  {
    private static readonly object FZeroValue = (object) DateTime.MinValue;

    public DateTimeProperty(DataObject obj, MetadataProperty metadata)
      : base(obj, metadata)
    {
    }

    public DateTime Value
    {
      get => (DateTime) this.NotNullUntypedValue;
      set => this.UntypedValue = (object) value;
    }

    public DateTime ValueDef(DateTime def) => this.IsNull ? def : this.Value;

    internal override void SetValue(object value)
    {
      object obj1;
      switch (value)
      {
        case null:
        case DateTime _:
          obj1 = value;
          break;
        default:
          obj1 = (object) Convert.ToDateTime(value);
          break;
      }
      object obj2 = obj1;
      if (obj2 != null && !this.Session.Db.IsAcceptable((DateTime) obj2))
        throw new DataException(string.Format("Ошибка присваивания значения свойству \"{0}\" объекта \"{1}\": значение даты выходит за допустимые пределы ({2} - {3}).", (object) this.Metadata.Name, (object) this.Object.Class.Name, (object) this.Session.Db.DateTimeMinValue, (object) this.Session.Db.DateTimeMaxValue));
      base.SetValue(obj2);
    }

    public override string ToString(IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((DateTime) untypedValue).ToString(provider);
    }

    public override string ToString(string format)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((DateTime) untypedValue).ToString(format);
    }

    protected internal override object ZeroValue => DateTimeProperty.FZeroValue;

    public string ToString(string format, IFormatProvider provider)
    {
      object untypedValue = this.UntypedValue;
      return untypedValue == null ? string.Empty : ((DateTime) untypedValue).ToString(format, provider);
    }
  }
}
