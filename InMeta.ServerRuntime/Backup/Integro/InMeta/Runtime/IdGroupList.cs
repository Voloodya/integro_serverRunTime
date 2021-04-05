// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.IdGroupList
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class IdGroupList
  {
    private readonly List<IdGroup> FItems = new List<IdGroup>();

    public int Count => this.FItems.Count;

    public IdGroup this[int index] => this.FItems[index];

    public void Add(IdGroup idGroup) => this.FItems.Add(idGroup);

    public IdGroup FindByGlobalId(string globalId)
    {
      for (int index = 0; index < this.FItems.Count; ++index)
      {
        IdGroup fitem = this.FItems[index];
        if (string.Compare(fitem.GlobalId, globalId, true) == 0)
          return fitem;
      }
      return (IdGroup) null;
    }

    public void Load(XmlNodeList source)
    {
      foreach (XmlElement xmlElement in source)
        this.Add(new IdGroup(xmlElement.GetAttribute("GlobalId"), int.Parse(xmlElement.GetAttribute("LocalId"))));
    }

    public void Load(XmlNode source) => this.Load(source.SelectNodes("IdGroup"));

    public void Save(XmlElement parentNode)
    {
      foreach (IdGroup fitem in this.FItems)
      {
        XmlElement element = parentNode.OwnerDocument.CreateElement("IdGroup");
        parentNode.AppendChild((XmlNode) element);
        element.SetAttribute("GlobalId", fitem.GlobalId);
        element.SetAttribute("LocalId", fitem.LocalId.ToString());
      }
    }
  }
}
