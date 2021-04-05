// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Attachments.ObjectAttachment
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Attachments
{
  [ComVisible(false)]
  public class ObjectAttachment
  {
    private string FFileName;
    private string FCaption;
    private string FDescription;
    internal string FData;
    internal string FDataFile;
    internal FileAttachment FAttachment;

    public bool IsDeleted => this.FAttachment != null && this.FAttachment.Deleted;

    internal ObjectAttachment(FileAttachment attachment) => this.FAttachment = attachment;

    public ObjectAttachment(string fileName, string caption, string description)
    {
      this.FFileName = fileName;
      this.FCaption = caption;
      this.FDescription = description;
    }

    public string FileName
    {
      get => this.FAttachment != null ? this.FAttachment.FileName : this.FFileName;
      set => this.FFileName = value;
    }

    public string Caption
    {
      get => this.FAttachment != null ? this.FAttachment.Caption : this.FCaption;
      set
      {
        if (this.FAttachment != null)
          this.FAttachment.Caption = value;
        else
          this.FCaption = value;
      }
    }

    public string Description
    {
      get => this.FAttachment != null ? this.FAttachment.Description : this.FDescription;
      set
      {
        if (this.FAttachment != null)
          this.FAttachment.Description = value;
        else
          this.FDescription = value;
      }
    }

    public void SetData(string data)
    {
      if (this.FAttachment == null)
        this.FData = data;
      else
        this.FAttachment.SetString(data);
    }

    public void SetDataFile(string dataSourceFileName)
    {
      if (this.FAttachment == null)
        this.FDataFile = dataSourceFileName;
      else
        this.FAttachment.SetFile(dataSourceFileName);
    }

    public void GetDataFile(string dstFileName)
    {
      if (this.FAttachment == null)
        File.Copy(this.FDataFile, dstFileName, true);
      else
        this.FAttachment.GetFile(dstFileName);
    }

    public string GetStringData() => this.FAttachment != null ? this.FAttachment.GetString() : this.FData;

    public void Delete()
    {
      if (this.FAttachment == null)
        throw new Exception(string.Format("Ошибка удаления файла \"{0}\": файл не прикреплен ни к одному реестровому объекту.", (object) this.FFileName));
      this.FAttachment.Delete();
    }
  }
}
