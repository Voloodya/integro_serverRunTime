// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.SessionCommitPlan
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections.Generic;

namespace Integro.InMeta.Runtime
{
  internal class SessionCommitPlan
  {
    private readonly SessionCommitPlan.StorageCommitPlanList FStorageCommitPlans = new SessionCommitPlan.StorageCommitPlanList();

    public SessionCommitPlan(IEnumerable<DataStorage> storages)
    {
      foreach (DataStorage storage in storages)
        this.FStorageCommitPlans.Add(new StorageCommitPlan(storage));
    }

    internal SessionUpdateInfo GetUpdateInfo()
    {
      List<ClassUpdatesInfo> classUpdatesInfoList = new List<ClassUpdatesInfo>();
      foreach (StorageCommitPlan fstorageCommitPlan in (List<StorageCommitPlan>) this.FStorageCommitPlans)
      {
        if (fstorageCommitPlan.HasUpdates)
          classUpdatesInfoList.Add(fstorageCommitPlan.GetUpdateInfo());
      }
      return new SessionUpdateInfo(classUpdatesInfoList.ToArray());
    }

    public void CommitToDb()
    {
      foreach (StorageCommitPlan fstorageCommitPlan in (List<StorageCommitPlan>) this.FStorageCommitPlans)
        fstorageCommitPlan.CommitToDb();
    }

    public void CommitToMasterSession()
    {
      foreach (StorageCommitPlan fstorageCommitPlan in (List<StorageCommitPlan>) this.FStorageCommitPlans)
        fstorageCommitPlan.CommitCreationToMasterSession();
      foreach (StorageCommitPlan fstorageCommitPlan in (List<StorageCommitPlan>) this.FStorageCommitPlans)
        fstorageCommitPlan.CommitToMasterSession();
    }

    public void CommitToMemory()
    {
      foreach (StorageCommitPlan fstorageCommitPlan in (List<StorageCommitPlan>) this.FStorageCommitPlans)
        fstorageCommitPlan.CommitToMemory();
    }

    private class StorageCommitPlanList : List<StorageCommitPlan>
    {
    }
  }
}
