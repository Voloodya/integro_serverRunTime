// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CentralServer.DirectIdGenerator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using Integro.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace Integro.InMeta.Runtime.CentralServer
{
  internal class DirectIdGenerator : IdGenerator
  {
    private int FGeneratorIdSpace;
    private readonly int FGeneratorInc;
    private int FGeneratorNextNumber;
    private int FGeneratorLastNumber;

    public DirectIdGenerator(ApplicationDbConfig connectionParams)
      : base(connectionParams)
      => this.FGeneratorInc = 100;

    private static void ReadIdGenerator(InDbDatabase db, out int idSpace, out int nextNumber)
    {
      using (InDbCommand command = db.CreateCommand("SELECT [LocalGroupID],[NextObjectNumber] FROM [_InSC_IDGenerator]"))
      {
        using (IDataReader dataReader = command.ExecuteReader())
        {
          idSpace = dataReader.Read() ? dataReader.GetInt32(0) : throw new System.Data.DataException("Ошибка генератора идентификаторов: таблица [_InSC_IDGenerator] не содержит ни одной записи.");
          nextNumber = dataReader.GetInt32(1);
        }
      }
    }

    private static void WriteIdGenerator(InDbDatabase db, int idSpace, int nextNumber) => db.Execute(string.Format("UPDATE [_InSC_IDGenerator] SET [LocalGroupID]={0},[NextObjectNumber]={1}", (object) idSpace, (object) nextNumber));

    public override void Shutdown()
    {
      if (this.FGeneratorNextNumber >= this.FGeneratorLastNumber)
        return;
      using (InDbDatabase db = this.OpenDatabase())
      {
        int idSpace;
        int nextNumber;
        DirectIdGenerator.ReadIdGenerator(db, out idSpace, out nextNumber);
        if (this.FGeneratorIdSpace == idSpace && this.FGeneratorLastNumber == nextNumber)
          DirectIdGenerator.WriteIdGenerator(db, idSpace, this.FGeneratorNextNumber);
      }
      this.FGeneratorLastNumber = 0;
      this.FGeneratorNextNumber = 0;
    }

    private InDbDatabase OpenDatabase() => InDbManager.OpenDatabase(this.ConnectionParams.DriverName, this.ConnectionParams.Parameters);

    public override IdGroupList GetIdGroups()
    {
      IdGroupList idGroupList = new IdGroupList();
      using (InDbDatabase inDbDatabase = this.OpenDatabase())
      {
        using (InDbCommand command = inDbDatabase.CreateCommand("SELECT [GlobalGroupID],[LocalGroupID] FROM [_InSC_IDGroups]"))
        {
          using (IDataReader dataReader = command.ExecuteReader())
          {
            while (dataReader.Read())
              idGroupList.Add(new IdGroup(dataReader.GetString(0), dataReader.GetInt32(1)));
          }
        }
      }
      return idGroupList;
    }

    private static int QueryInt(InDbDatabase db, string sql, int defValue)
    {
      using (InDbCommand command = db.CreateCommand(sql))
      {
        using (IDataReader dataReader = command.ExecuteReader())
        {
          if (dataReader.Read())
            return dataReader.IsDBNull(0) ? defValue : dataReader.GetInt32(0);
        }
      }
      return defValue;
    }

    public override IdConverter GetIdConverter(IdGroupList sourceGroups)
    {
      IdGroupList idGroups = this.GetIdGroups();
      IdConverter idConverter = new IdConverter();
      for (int index = 0; index < sourceGroups.Count; ++index)
      {
        IdGroup sourceGroup = sourceGroups[index];
        IdGroup idGroup = idGroups.FindByGlobalId(sourceGroup.GlobalId);
        if (idGroup == null)
        {
          using (InDbDatabase db = this.OpenDatabase())
          {
            int num = DirectIdGenerator.QueryInt(db, "SELECT MAX([LocalGroupID]) FROM [_InSC_IDGroups]", -1);
            int localId = num >= 1 ? num + 1 : 1;
            idGroup = new IdGroup(sourceGroup.GlobalId, localId);
            idGroups.Add(idGroup);
            db.Execute(string.Format("INSERT INTO [_InSC_IDGroups]([GlobalGroupID],[LocalGroupID]) VALUES ('{0}',{1})", (object) sourceGroup.GlobalId, (object) localId));
          }
        }
        idConverter.AddGroupConverter(sourceGroup.LocalId, idGroup.LocalId);
      }
      return idConverter;
    }

    public override string GenerateId()
    {
      if (this.FGeneratorNextNumber >= this.FGeneratorLastNumber)
      {
        using (InDbDatabase db = this.OpenDatabase())
        {
          DirectIdGenerator.ReadIdGenerator(db, out this.FGeneratorIdSpace, out this.FGeneratorNextNumber);
          this.FGeneratorLastNumber = this.FGeneratorNextNumber + this.FGeneratorInc;
          if (this.FGeneratorLastNumber > int.MaxValue - this.FGeneratorInc)
            this.CreateNewIdGroup(db);
          DirectIdGenerator.WriteIdGenerator(db, this.FGeneratorIdSpace, this.FGeneratorLastNumber);
        }
      }
      return IdGenerator.IdToStr(((long) this.FGeneratorIdSpace << 32) + (long) (uint) this.FGeneratorNextNumber++);
    }

    public override void GenerateIds(int count, XmlNode resultParent)
    {
      while (count-- > 0)
        XmlUtils.AppendElement(resultParent, "ID").InnerText = this.GenerateId();
    }

    public override void GenerateCustomId(int count, string generatorName, XmlNode resultParent)
    {
      int[] numArray = new int[count];
      using (InDbDatabase db = this.OpenDatabase())
      {
        int num = 0;
        using (InDbCommand command = db.CreateCommand("SELECT [ReleaseNumber] FROM [_InMetaReleaseNoObject] WHERE [ObjectKindShortcut]=? ORDER BY [ReleaseNumber]", DataType.String))
        {
          using (IDataReader dataReader = command.ExecuteReader((object) generatorName))
          {
            while (num < count && dataReader.Read())
              numArray[num++] = dataReader.GetInt32(0);
          }
        }
        if (num > 0)
        {
          using (InDbCommand command = db.CreateCommand("DELETE FROM [_InMetaReleaseNoObject] WHERE [ObjectKindShortcut]=? AND [ReleaseNumber]=?", DataType.String, DataType.Integer))
          {
            for (int index = 0; index < num; ++index)
              command.Execute((object) generatorName, (object) numArray[index]);
          }
        }
        if (num < count)
        {
          int nextId;
          bool isNew;
          DirectIdGenerator.ReadCustomIdGenerator(db, generatorName, out nextId, out isNew);
          while (num < count)
            numArray[num++] = ++nextId;
          DirectIdGenerator.WriteCustomIdGenerator(db, generatorName, nextId, isNew);
        }
      }
      for (int index = 0; index < count; ++index)
        XmlUtils.AppendElement(resultParent, "CustomId").InnerText = numArray[index].ToString();
    }

    public override void ReleaseCustomIds(string generatorName, int[] ids)
    {
      using (InDbDatabase inDbDatabase = this.OpenDatabase())
      {
        using (InDbCommand command = inDbDatabase.CreateCommand("INSERT INTO [_InMetaReleaseNoObject]([ObjectKindShortcut],[GroupNumber],[ReleaseNumber]) VALUES (?,1,?)", DataType.String, DataType.Integer))
        {
          foreach (int id in ids)
            command.Execute((object) generatorName, (object) id);
        }
      }
    }

    public override CustomIdGenerator[] GetCustomIdGenerators()
    {
      List<CustomIdGenerator> customIdGeneratorList = new List<CustomIdGenerator>();
      using (InDbDatabase inDbDatabase = this.OpenDatabase())
      {
        using (InDbCommand command = inDbDatabase.CreateCommand("SELECT [ObjectKindShortcut],[LastObjectNumber] FROM [_InMetaCustomNoObject]"))
        {
          using (IDataReader dataReader = command.ExecuteReader())
          {
            while (dataReader.Read())
              customIdGeneratorList.Add(new CustomIdGenerator(dataReader.GetString(0), dataReader.GetInt32(1)));
          }
        }
      }
      return customIdGeneratorList.ToArray();
    }

    public override int GenerateRegNo()
    {
      int num;
      using (InDbDatabase inDbDatabase = this.OpenDatabase())
      {
        bool flag;
        using (InDbCommand command = inDbDatabase.CreateCommand("SELECT [ObjectNumber] FROM [_InMetaRegNo] WHERE [GroupNumber]=0"))
        {
          using (IDataReader dataReader = command.ExecuteReader())
          {
            flag = !dataReader.Read();
            num = flag ? 1 : dataReader.GetInt32(0) + 1;
          }
        }
        if (flag)
        {
          using (InDbCommand command = inDbDatabase.CreateCommand("INSERT INTO [_InMetaRegNo]([GroupNumber],[ObjectNumber]) VALUES (0,?)", DataType.Integer))
            command.Execute((object) num);
        }
        else
        {
          using (InDbCommand command = inDbDatabase.CreateCommand("UPDATE [_InMetaRegNo] SET [ObjectNumber]=? WHERE [GroupNumber]=0", DataType.Integer))
            command.Execute((object) num);
        }
      }
      return num;
    }

    public override void GenerateUpdateLogId(out int majorId, out int minorId)
    {
      using (InDbDatabase inDbDatabase = this.OpenDatabase())
      {
        bool flag;
        using (InDbCommand command = inDbDatabase.CreateCommand("SELECT [MajorID], [MinorID] FROM [_UpdateLogID]"))
        {
          using (IDataReader dataReader = command.ExecuteReader())
          {
            flag = !dataReader.Read();
            if (flag)
            {
              majorId = 0;
              minorId = 0;
            }
            else
            {
              majorId = dataReader.GetInt32(0);
              minorId = dataReader.GetInt32(1);
              if (minorId < int.MaxValue)
              {
                ++minorId;
              }
              else
              {
                ++majorId;
                minorId = 0;
              }
            }
          }
        }
        if (flag)
        {
          string sql = "INSERT INTO [_UpdateLogID] ([MajorID], [MinorID]) VALUES (" + (object) majorId + ", " + (object) minorId + ")";
          using (InDbCommand command = inDbDatabase.CreateCommand(sql))
            command.Execute();
        }
        else
        {
          string sql = "UPDATE [_UpdateLogID] SET [MajorID] = " + (object) majorId + ", [MinorID] = " + (object) minorId;
          using (InDbCommand command = inDbDatabase.CreateCommand(sql))
            command.Execute();
        }
      }
    }

    private void CreateNewIdGroup(InDbDatabase db)
    {
      this.FGeneratorNextNumber = -2147483645;
      this.FGeneratorLastNumber = this.FGeneratorNextNumber + this.FGeneratorInc;
      using (InDbCommand command = db.CreateCommand("SELECT [LocalGroupID] FROM [_InSC_IDGroups] ORDER BY [LocalGroupID]"))
      {
        using (IDataReader dataReader = command.ExecuteReader())
        {
          this.FGeneratorIdSpace = 0;
          while (dataReader.Read())
          {
            int int32 = dataReader.GetInt32(0);
            if (int32 - this.FGeneratorIdSpace <= 1)
              this.FGeneratorIdSpace = int32;
            else
              break;
          }
        }
      }
      ++this.FGeneratorIdSpace;
      db.Execute(string.Format("INSERT INTO [_InSC_IDGroups]([GlobalGroupID],[LocalGroupID]) VALUES ('{0}', {1})", (object) Guid.NewGuid().ToString("B"), (object) this.FGeneratorIdSpace));
    }

    private static void ReadCustomIdGenerator(
      InDbDatabase db,
      string generatorName,
      out int nextId,
      out bool isNew)
    {
      using (InDbCommand command = db.CreateCommand("SELECT [LastObjectNumber] FROM [_InMetaCustomNoObject] WHERE [ObjectKindShortcut]=?", DataType.String))
      {
        using (IDataReader dataReader = command.ExecuteReader((object) generatorName))
        {
          isNew = !dataReader.Read();
          nextId = isNew ? 0 : dataReader.GetInt32(0);
        }
      }
    }

    private static void WriteCustomIdGenerator(
      InDbDatabase db,
      string generatorName,
      int nextId,
      bool isNew)
    {
      if (isNew)
      {
        using (InDbCommand command = db.CreateCommand("INSERT INTO [_InMetaCustomNoObject]([ObjectKindShortcut],[GroupNumber],[LastObjectNumber]) VALUES (?,1,?)", DataType.String, DataType.Integer))
          command.Execute((object) generatorName, (object) nextId);
      }
      else
      {
        using (InDbCommand command = db.CreateCommand("UPDATE [_InMetaCustomNoObject] SET [LastObjectNumber]=? WHERE [ObjectKindShortcut]=?", DataType.Integer, DataType.String))
          command.Execute((object) nextId, (object) generatorName);
      }
    }
  }
}
