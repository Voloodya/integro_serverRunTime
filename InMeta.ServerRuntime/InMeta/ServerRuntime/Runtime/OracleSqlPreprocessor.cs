// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Runtime.OracleSqlPreprocessor
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro;
using Integro.InDbs;
using System;
using System.Text;

namespace InMeta.ServerRuntime.Runtime
{
  internal class OracleSqlPreprocessor : SqlPreprocessor
  {
    private static readonly OracleSqlPreprocessor FPreprocessor = new OracleSqlPreprocessor();

    private OracleSqlPreprocessor()
    {
    }

    public static string Translate(string sql) => OracleSqlPreprocessor.FPreprocessor.Run(sql);

    protected override void OutputQuestionMark(
      int questionMarkIndex,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.AppendFormat(":{0}", (object) (questionMarkIndex + 1));
    }

    protected override void OutputIdentifier(
      string identifier,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      int num = pos;
      TextScanner.SkipWhiteSpaces(sourceSql, ref num);
      if (TextScanner.Pass('(', sourceSql, ref num))
      {
        TextScanner.SkipWhiteSpaces(sourceSql, ref num);
        if (TextScanner.Pass(')', sourceSql, ref num) && string.Equals(identifier, "GETDATE", StringComparison.InvariantCultureIgnoreCase))
        {
          pos = num;
          output.Append("SYSDATE ");
        }
        else
          base.OutputIdentifier(identifier, output, sourceSql, ref pos);
      }
      else if (InDbOracleDatabase.IsReservedWord(identifier))
        base.OutputIdentifier(identifier, output, sourceSql, ref pos);
      else
        this.OutputQualifiedIdentifier(identifier, output, sourceSql, ref pos);
    }
  }
}
