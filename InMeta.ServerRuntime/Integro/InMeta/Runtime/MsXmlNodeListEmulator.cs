// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MsXmlNodeListEmulator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class MsXmlNodeListEmulator : IEnumerable
  {
    private readonly MsXmlDocumentEmulator FDocument;
    private readonly XmlNodeList FNodes;

    public MsXmlNodeListEmulator(XmlNodeList nodes, MsXmlDocumentEmulator document)
    {
      this.FNodes = nodes;
      this.FDocument = document;
    }

    public int length => this.FNodes.Count;

    public MsXmlNodeEmulator this[int index] => new MsXmlNodeEmulator(this.FNodes[index], this.FDocument);

    public IEnumerator GetEnumerator() => (IEnumerator) new MsXmlNodeListEmulator.Enumerator(this.FNodes.GetEnumerator(), this.FDocument);

    private class Enumerator : IEnumerator
    {
      private readonly IEnumerator FNodeEnumerator;
      private readonly MsXmlDocumentEmulator FDocument;

      public Enumerator(IEnumerator nodeEnumerator, MsXmlDocumentEmulator document)
      {
        this.FNodeEnumerator = nodeEnumerator;
        this.FDocument = document;
      }

      public bool MoveNext() => this.FNodeEnumerator.MoveNext();

      public void Reset() => this.FNodeEnumerator.Reset();

      public object Current => (object) new MsXmlNodeEmulator((XmlNode) this.FNodeEnumerator.Current, this.FDocument);
    }
  }
}
