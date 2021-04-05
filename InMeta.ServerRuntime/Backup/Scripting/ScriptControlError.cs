// Decompiled with JetBrains decompiler
// Type: Scripting.ScriptControlError
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

namespace Scripting
{
  public class ScriptControlError
  {
    public string Source { get; internal set; }

    public int Number { get; internal set; }

    public string Description { get; internal set; }

    public string Text { get; internal set; }

    public int Line { get; internal set; }

    public int Column { get; internal set; }

    public object SourceContext { get; internal set; }

    public string HelpFile { get; internal set; }

    public int HelpContext { get; internal set; }

    public void Clear()
    {
      this.Column = 0;
      this.Source = (string) null;
      this.HelpFile = (string) null;
      this.SourceContext = (object) null;
      this.Number = 0;
      this.Description = (string) null;
      this.HelpContext = 0;
      this.Line = 0;
      this.Text = (string) null;
    }
  }
}
