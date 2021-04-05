// Decompiled with JetBrains decompiler
// Type: InMeta.ServerRuntime.Runtime.MsSqlPreprocessor
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Text;

namespace InMeta.ServerRuntime.Runtime
{
  internal class MsSqlPreprocessor : SqlPreprocessor
  {
    private static readonly SqlPreprocessor FPreprocessor = (SqlPreprocessor) new MsSqlPreprocessor();

    private MsSqlPreprocessor()
    {
    }

    public static string Translate(string sql) => MsSqlPreprocessor.FPreprocessor.Run(sql);

    protected override void OutputQuestionMark(
      int questionMarkIndex,
      StringBuilder output,
      string sourceSql,
      ref int pos)
    {
      output.AppendFormat("@{0}", (object) (questionMarkIndex + 1));
    }
  }
}
