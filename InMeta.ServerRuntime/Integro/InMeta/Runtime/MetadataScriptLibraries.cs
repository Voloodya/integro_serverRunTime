// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataScriptLibraries
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataScriptLibraries
  {
    private readonly Dictionary<string, MetadataScriptLibrary> FLibrariesByName = new Dictionary<string, MetadataScriptLibrary>();
    public readonly string DefaultScriptLibraryText;

    internal MetadataScriptLibraries(XmlNode sourceNode)
    {
      foreach (XmlNode selectNode in sourceNode.SelectNodes("script-library"))
      {
        MetadataScriptLibrary metadataScriptLibrary = new MetadataScriptLibrary(selectNode);
        this.FLibrariesByName.Add(metadataScriptLibrary.Name.ToUpper(), metadataScriptLibrary);
      }
      MetadataScriptLibrary metadataScriptLibrary1;
      if (!this.FLibrariesByName.TryGetValue("DEFAULT", out metadataScriptLibrary1))
        return;
      this.DefaultScriptLibraryText = metadataScriptLibrary1.Text;
    }

    public MetadataScriptLibrary Need(string name)
    {
      MetadataScriptLibrary metadataScriptLibrary;
      if (this.FLibrariesByName.TryGetValue(name, out metadataScriptLibrary))
        return metadataScriptLibrary;
      throw new MetadataException(string.Format("Не найдена библиотека скриптов \"{0}\".", (object) name));
    }
  }
}
