// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.QueryCondition
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class QueryCondition
  {
    internal readonly StringBuilder Condition;
    internal readonly ArrayList Params;
    private static readonly string[] CompareOpTexts = new string[6]
    {
      "=",
      "<>",
      "<",
      ">",
      "<=",
      ">="
    };

    public QueryCondition()
    {
      this.Condition = new StringBuilder();
      this.Params = new ArrayList();
    }

    public bool IsEmpty => this.Condition.Length == 0;

    private bool IsIdentChar(char c) => c == '?' || StrUtils.CharCanBeUsedInIdentifier(c);

    private void AppendToken(string text)
    {
      if (text == null || text.Length == 0)
        return;
      if (this.Condition.Length > 0)
      {
        bool flag1 = this.IsIdentChar(this.Condition[this.Condition.Length - 1]);
        bool flag2 = this.IsIdentChar(text[0]);
        if (flag1 && flag2)
          this.Condition.Append(' ');
      }
      this.Condition.Append(text);
    }

    private QueryCondition Append(string name, string op, params object[] paramArray)
    {
      this.AppendToken(name);
      this.AppendToken(op);
      for (int index = 0; index < paramArray.Length; ++index)
      {
        if (index < 0)
          this.AppendToken(",");
        this.AppendToken("?");
        this.Params.Add(paramArray[index]);
      }
      return this;
    }

    public QueryCondition IsNull(string name) => this.Append(name, "IS NULL");

    public QueryCondition IsNotNull(string name) => this.Append(name, "IS NOT NULL");

    public QueryCondition Compare(string name, CompareOp op, object value) => this.Append(name, QueryCondition.CompareOpTexts[(int) op], value);

    public QueryCondition Like(string name, object value) => this.Append(name, "LIKE", value);

    public QueryCondition In(string name, params object[] value)
    {
      if (value == null || value.Length == 0)
        return this;
      this.Append(name, "IN(");
      for (int index = 0; index < value.Length; ++index)
      {
        if (index > 0)
          this.AppendToken(",");
        this.AppendToken("?");
        this.Params.Add(value[index]);
      }
      this.AppendToken(")");
      return this;
    }

    public QueryCondition Between(string name, object minValue, object maxValue)
    {
      this.Append(name, "BETWEEN ? AND ?");
      this.Params.Add(minValue);
      this.Params.Add(maxValue);
      return this;
    }

    protected QueryCondition PropIsNull(MetadataProperty prop) => this.IsNull(prop.Name);

    protected QueryCondition PropIsNotNull(MetadataProperty prop) => this.IsNotNull(prop.Name);

    protected QueryCondition PropCompare(
      MetadataProperty prop,
      CompareOp op,
      object value)
    {
      return this.Compare(prop.Name, op, value);
    }

    protected QueryCondition PropIs(MetadataProperty prop, object value) => this.PropCompare(prop, CompareOp.Equal, value);

    protected QueryCondition PropLike(MetadataProperty prop, object value) => this.Like(prop.Name, value);

    protected QueryCondition PropIn(MetadataProperty prop, params object[] value) => this.In(prop.Name, value);

    protected QueryCondition PropBetween(
      MetadataProperty prop,
      object minValue,
      object maxValue)
    {
      return this.Between(prop.Name, minValue, maxValue);
    }

    public QueryCondition Begin
    {
      get
      {
        this.AppendToken("(");
        return this;
      }
    }

    public QueryCondition End
    {
      get
      {
        this.AppendToken(")");
        return this;
      }
    }

    public QueryCondition And
    {
      get
      {
        this.AppendToken("AND");
        return this;
      }
    }

    public QueryCondition Or
    {
      get
      {
        this.AppendToken("OR");
        return this;
      }
    }

    public QueryCondition Not
    {
      get
      {
        this.AppendToken("NOT");
        return this;
      }
    }
  }
}
