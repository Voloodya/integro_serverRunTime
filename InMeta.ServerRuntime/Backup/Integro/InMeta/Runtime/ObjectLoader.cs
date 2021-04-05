// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectLoader
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  internal class ObjectLoader : List<ObjectPartLoader>
  {
    internal readonly LoadPlan BasePlan;

    internal ObjectLoader(LoadPlan basePlan) => this.BasePlan = basePlan;

    private LinkPropertyLoader FindLinkPropertyLoader(
      MetadataAssociation association)
    {
      for (int index = 0; index < this.Count; ++index)
      {
        if (this[index] is LinkPropertyLoader linkPropertyLoader && linkPropertyLoader.Association == association)
          return linkPropertyLoader;
      }
      return (LinkPropertyLoader) null;
    }

    private void EnsureLinkPropertyLoader(Loader loader, AssociationRefLoadPlan refPlan)
    {
      MetadataAssociation association = refPlan.Ref.Association;
      LinkPropertyLoader linkPropertyLoader = this.FindLinkPropertyLoader(association);
      if (linkPropertyLoader == null)
      {
        linkPropertyLoader = new LinkPropertyLoader(association);
        this.Add((ObjectPartLoader) linkPropertyLoader);
      }
      linkPropertyLoader.RefLoaders[refPlan.Ref.Index] = loader.EnsureObjectListLoaderByIds(loader.EnsureObjectLoader(refPlan.Plan), refPlan.Ref.RefClass.IDProperty);
    }

    internal void Prepare(Loader loader)
    {
      this.CreateDataPropertyLoaders();
      this.CreateLinkPropertyLoaders(loader);
      this.CreateChildrenLoaders(loader);
      this.CreateObjectViewLoaders(loader);
    }

    private void CreateDataPropertyLoaders()
    {
      for (int index = 0; index < this.BasePlan.Data.Count; ++index)
        this.Add((ObjectPartLoader) new DataPropertyLoader(this.BasePlan.Data[index]));
    }

    private void CreateLinkPropertyLoaders(Loader loader)
    {
      for (int index = 0; index < this.BasePlan.Links.Count; ++index)
        this.EnsureLinkPropertyLoader(loader, this.BasePlan.Links[index]);
    }

    private void CreateChildrenLoaders(Loader loader)
    {
      for (int index = 0; index < this.BasePlan.Childs.Count; ++index)
      {
        ChildRefLoadPlan child = this.BasePlan.Childs[index];
        ObjectLoader objectLoader = loader.EnsureObjectLoader(child.Plan);
        MetadataAssociation association = child.ChildRef.AggregationRef.Association;
        objectLoader.Add((ObjectPartLoader) new LinkPropertyLoader(association));
        ObjectListLoaderByIds refLoader = loader.EnsureObjectListLoaderByIds(objectLoader, association.Property);
        this.Add((ObjectPartLoader) new ChildsLoader(child.ChildRef, refLoader));
      }
    }

    private void CreateObjectViewLoaders(Loader loader)
    {
      for (int index = 0; index < this.BasePlan.Views.Count; ++index)
        this.Add((ObjectPartLoader) loader.EnsureObjectViewLoader(this.BasePlan.Views[index]));
    }
  }
}
