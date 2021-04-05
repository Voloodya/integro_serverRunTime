// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ScriptLibraries
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Scripting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Integro.InMeta.Runtime
{
  public class ScriptLibraries : IDisposable
  {
    private readonly DataSession FSession;
    private Dictionary<string, ScriptControl> FScriptControls = new Dictionary<string, ScriptControl>();
    private readonly object FSync = new object();

    internal ScriptLibraries(DataSession session) => this.FSession = session;

    private MetadataScriptLibraries Metadata => this.FSession.Application.Metadata.ScriptLibraries;

    private void CollectUsing(Hashtable collectedLibs, string[] usingLibNames)
    {
      for (int index = 0; index < usingLibNames.Length; ++index)
      {
        string upper = usingLibNames[index].Trim().ToUpper();
        if (upper.Length > 0 && !collectedLibs.ContainsKey((object) upper))
        {
          MetadataScriptLibrary metadataScriptLibrary = this.Metadata.Need(upper);
          collectedLibs.Add((object) upper, (object) metadataScriptLibrary);
          this.CollectUsing(collectedLibs, metadataScriptLibrary.Using);
        }
      }
    }

    public void AddScriptLibs(ScriptControl control, string[] usingLibNames)
    {
      Hashtable collectedLibs = new Hashtable();
      this.CollectUsing(collectedLibs, usingLibNames);
      if (this.Metadata.DefaultScriptLibraryText != null)
      {
        try
        {
          control.AddCode(this.Metadata.DefaultScriptLibraryText);
        }
        catch (Exception ex)
        {
          throw new Exception(string.Format("Ошибка инициализации библиотеки скриптов \"Default\":\r\n{0}", (object) ex.Message));
        }
      }
      lock (this.FSync)
      {
        foreach (DictionaryEntry dictionaryEntry in collectedLibs)
        {
          string key = (string) dictionaryEntry.Key;
          if (!(key == "DEFAULT"))
          {
            MetadataScriptLibrary metadataScriptLibrary = (MetadataScriptLibrary) dictionaryEntry.Value;
            ScriptControl scriptControl;
            if (this.FScriptControls.ContainsKey(key))
            {
              scriptControl = this.FScriptControls[key];
            }
            else
            {
              scriptControl = new ScriptControl()
              {
                Language = metadataScriptLibrary.Language,
                Timeout = -1
              };
              try
              {
                scriptControl.AddCode(metadataScriptLibrary.Text);
              }
              catch (Exception ex)
              {
                throw new Exception(string.Format("Ошибка инициализации библиотеки скриптов \"{0}\":\r\n{1}", (object) metadataScriptLibrary.Name, (object) ex.Message));
              }
              this.FScriptControls.Add(key, scriptControl);
            }
            control.AddObject(metadataScriptLibrary.Name, scriptControl.CodeObject, true);
          }
        }
      }
    }

    protected void Dispose(bool disposing)
    {
      if (disposing && this.FScriptControls != null)
      {
        foreach (ScriptControl scriptControl in this.FScriptControls.Values)
          scriptControl.Dispose();
      }
      this.FScriptControls = (Dictionary<string, ScriptControl>) null;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~ScriptLibraries() => this.Dispose(false);
  }
}
