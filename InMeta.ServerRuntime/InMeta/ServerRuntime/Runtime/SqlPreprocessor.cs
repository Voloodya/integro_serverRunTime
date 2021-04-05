// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Runtime.SqlPreprocessor
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro;
using System.Text;

namespace InMeta.ServerRuntime.Runtime
{
  internal class SqlPreprocessor : TextScanner
  {
    public string Run(string sourceSql)
    {
      StringBuilder output = new StringBuilder();
      int num = 0;
      int pos = 0;
      while (pos <= sourceSql.Length)
      {
        this.PassWhiteSpaces(output, sourceSql, ref pos);
        if (pos < sourceSql.Length)
        {
          if (TextScanner.Pass('?', sourceSql, ref pos))
          {
            this.OutputQuestionMark(num++, output, sourceSql, ref pos);
          }
          else
          {
            string str;
            if (TextScanner.PassQuoted(ref str, '\'', sourceSql, ref pos))
              this.OutputStringLiteral(str, output, sourceSql, ref pos);
            else if (TextScanner.PassQuoted(ref str, '"', sourceSql, ref pos))
              this.OutputQualifiedIdentifier(str, output, sourceSql, ref pos);
            else if (this.PassQuoted(ref str, '[', ']', sourceSql, ref pos))
              this.OutputQualifiedIdentifier(str, output, sourceSql, ref pos);
            else if (TextScanner.PassIdentifier(ref str, sourceSql, ref pos))
              this.OutputIdentifier(str, output, sourceSql, ref pos);
            else
              output.Append(sourceSql[pos++]);
          }
        }
        else
          break;
      }
      return output.ToString();
    }

    protected virtual void OutputIdentifier(
      string identifier,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.Append(identifier);
    }

    protected void PassWhiteSpaces(StringBuilder output, string sourceSql, ref int pos)
    {
      int startIndex = pos;
      TextScanner.SkipWhiteSpaces(sourceSql, ref pos);
      if (pos <= startIndex)
        return;
      this.OutputWhiteSpaces(sourceSql.Substring(startIndex, pos - startIndex), output, sourceSql, ref pos);
    }

    protected virtual void OutputWhiteSpaces(
      string whiteSpaces,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.Append(whiteSpaces);
    }

    protected virtual void OutputQuestionMark(
      int questionMarkIndex,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.Append('?');
    }

    protected virtual void OutputQualifiedIdentifier(
      string name,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.Append('"').Append(name.Replace("\"", "\"\"")).Append('"');
    }

    protected virtual void OutputStringLiteral(
      string value,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.Append('\'').Append(value.Replace("'", "''")).Append('\'');
    }

    public SqlPreprocessor() => base.\u002Ector();
  }
}
