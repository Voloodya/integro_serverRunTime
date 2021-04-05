// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.IdConverter
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class IdConverter
  {
    private readonly List<IdConverter.GroupConverter> FGroupConverters = new List<IdConverter.GroupConverter>();

    public void AddGroupConverter(int sourceLocalId, int destinationLocalId)
    {
      if (sourceLocalId == destinationLocalId)
        return;
      this.FGroupConverters.Add(new IdConverter.GroupConverter(sourceLocalId, destinationLocalId));
    }

    public DataId ConvertId(DataId sourceId)
    {
      int localGroupId;
      int number;
      if (this.FGroupConverters.Count > 0 && sourceId.TryExtractParts(out localGroupId, out number))
      {
        for (int index = 0; index < this.FGroupConverters.Count; ++index)
        {
          IdConverter.GroupConverter fgroupConverter = this.FGroupConverters[index];
          if (fgroupConverter.SourceLocalId == localGroupId)
            return new DataId(fgroupConverter.DestinationLocalId, number);
        }
      }
      return sourceId;
    }

    public void Load(XmlNodeList source)
    {
      for (int i = 0; i < source.Count; ++i)
      {
        XmlElement xmlElement = (XmlElement) source[i];
        this.AddGroupConverter(int.Parse(xmlElement.GetAttribute("SourceLocalId")), int.Parse(xmlElement.GetAttribute("DestinationLocalId")));
      }
    }

    public void Load(XmlNode source) => this.Load(source.SelectNodes("IdGroupConverter"));

    public void Save(XmlElement parentNode)
    {
      foreach (IdConverter.GroupConverter fgroupConverter in this.FGroupConverters)
      {
        XmlElement element = parentNode.OwnerDocument.CreateElement("IdGroupConverter");
        parentNode.AppendChild((XmlNode) element);
        XmlElement xmlElement1 = element;
        int num = fgroupConverter.SourceLocalId;
        string str1 = num.ToString();
        xmlElement1.SetAttribute("SourceLocalId", str1);
        XmlElement xmlElement2 = element;
        num = fgroupConverter.DestinationLocalId;
        string str2 = num.ToString();
        xmlElement2.SetAttribute("DestinationLocalId", str2);
      }
    }

    private class GroupConverter
    {
      public readonly int SourceLocalId;
      public readonly int DestinationLocalId;

      public GroupConverter(int sourceLocalId, int destinationLocalId)
      {
        this.SourceLocalId = sourceLocalId;
        this.DestinationLocalId = destinationLocalId;
      }
    }
  }
}
