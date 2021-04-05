// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectFileAttachments
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class ObjectFileAttachments : IEnumerable<FileAttachment>, IEnumerable
  {
    public readonly DataObject Object;
    private List<FileAttachment> FFileAttachments;
    private string FFolderPath;
    private static readonly char[] FInvalidFileNameChars = new char[6]
    {
      ':',
      '\\',
      '/',
      '"',
      '*',
      '?'
    };

    internal ObjectFileAttachments(DataObject obj) => this.Object = obj;

    internal void MarkModified()
    {
      this.Object.CheckNotNull();
      this.Object.MarkModified(ObjectSessionState.AttachmentsModified);
    }

    public int Count
    {
      get
      {
        int num = 0;
        for (int index = 0; index < this.FileAttachments.Count; ++index)
        {
          if (!this.FileAttachments[index].Deleted)
            ++num;
        }
        return num;
      }
    }

    public FileAttachment this[int index]
    {
      get
      {
        for (int index1 = 0; index1 < this.FileAttachments.Count; ++index1)
        {
          FileAttachment fileAttachment = this.FileAttachments[index1];
          if (!fileAttachment.Deleted && --index < 0)
            return fileAttachment;
        }
        throw new IndexOutOfRangeException("Индекс прикрепленного файла выходит за допустимые пределы");
      }
    }

    public FileAttachment Find(string fileName)
    {
      for (int index = 0; index < this.FileAttachments.Count; ++index)
      {
        FileAttachment fileAttachment = this.FileAttachments[index];
        if (!fileAttachment.Deleted && string.Compare(fileAttachment.FileName, fileName, true) == 0)
          return fileAttachment;
      }
      return (FileAttachment) null;
    }

    public FileAttachment Ensure(string fileName)
    {
      if (fileName.IndexOfAny(ObjectFileAttachments.FInvalidFileNameChars) >= 0)
        throw new InMetaException(string.Format("Имя прикрепленного файла \"{0}\" содержит недопустимые символы.", (object) fileName));
      for (int index = 0; index < this.FileAttachments.Count; ++index)
      {
        FileAttachment fileAttachment = this.FileAttachments[index];
        if (string.Compare(fileAttachment.FileName, fileName, true) == 0)
        {
          fileAttachment.Deleted = false;
          return fileAttachment;
        }
      }
      this.MarkModified();
      FileAttachment fileAttachment1 = new FileAttachment(this, fileName);
      this.FileAttachments.Add(fileAttachment1);
      return fileAttachment1;
    }

    private List<FileAttachment> FileAttachments
    {
      get
      {
        if (this.FFileAttachments == null)
        {
          this.FFileAttachments = new List<FileAttachment>();
          if (!this.Object.IsNew && File.Exists(this.IndexFilePath))
          {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
              xmlDocument.Load(this.IndexFilePath);
            }
            catch (XmlException ex)
            {
              xmlDocument.LoadXml(File.ReadAllText(this.IndexFilePath, Encoding.GetEncoding(1251)));
            }
            foreach (XmlElement selectNode in xmlDocument.DocumentElement.SelectNodes("attachment"))
              this.FFileAttachments.Add(new FileAttachment(this, selectNode));
          }
        }
        return this.FFileAttachments;
      }
    }

    private string IndexFilePath => Path.Combine(this.FolderPath, "_sys_attachment_list.xml");

    public string FolderPath => this.FFolderPath ?? (this.FFolderPath = this.Object.Session.Application.Settings.Attachments.GetObjectFolder(this.Object.Class, this.Object.Id));

    private void DeleteFolder()
    {
      string folderPath = this.FolderPath;
      if (!Directory.Exists(folderPath))
        return;
      foreach (string file in Directory.GetFiles(folderPath))
        File.Delete(file);
      Directory.Delete(folderPath);
    }

    internal void Commit()
    {
      if (this.Object.IsDeleted)
      {
        for (int index = 0; index < this.FileAttachments.Count; ++index)
          this.FileAttachments[index].Delete();
      }
      if (this.FFileAttachments == null || !this.Object.IsAttachmentsModified)
        return;
      if (this.Count == 0)
      {
        this.DeleteFolder();
      }
      else
      {
        if (!Directory.Exists(this.FolderPath))
          Directory.CreateDirectory(this.FolderPath);
        XmlDocument document = XmlUtils.CreateDocument("attachments");
        foreach (FileAttachment ffileAttachment in this.FFileAttachments)
          ffileAttachment.Commit(document.DocumentElement);
        document.Save(this.IndexFilePath);
      }
      this.FFileAttachments = (List<FileAttachment>) null;
    }

    internal void Rollback()
    {
      if (this.FFileAttachments == null)
        return;
      foreach (FileAttachment ffileAttachment in this.FFileAttachments)
        ffileAttachment.Rollback();
      this.FFileAttachments = (List<FileAttachment>) null;
    }

    IEnumerator<FileAttachment> IEnumerable<FileAttachment>.GetEnumerator() => (IEnumerator<FileAttachment>) new ObjectFileAttachments.AttachmentsEnumerator(this);

    public IEnumerator GetEnumerator() => (IEnumerator) new ObjectFileAttachments.AttachmentsEnumerator(this);

    public void DeleteAll()
    {
      if (this.Count == 0)
        return;
      this.MarkModified();
      for (int index = 0; index < this.FileAttachments.Count; ++index)
        this.FileAttachments[index].Delete();
    }

    internal void CopyChangesFrom(ObjectFileAttachments srcAttachments)
    {
      foreach (FileAttachment srcAttachment in srcAttachments)
      {
        if (this.Object.IsNew || srcAttachment.Added)
          this.Ensure(srcAttachment.FileName).CopyFrom(srcAttachment);
        else if (srcAttachment.Deleted)
          this.Find(srcAttachment.FileName)?.Delete();
        else if (srcAttachment.Modified)
          this.Ensure(srcAttachment.FileName).CopyChangesFrom(srcAttachment);
      }
    }

    private class AttachmentsEnumerator : IEnumerator<FileAttachment>, IDisposable, IEnumerator
    {
      private int FCurrentIndex;
      private readonly ObjectFileAttachments FOwner;

      public AttachmentsEnumerator(ObjectFileAttachments owner)
      {
        this.FOwner = owner;
        this.Reset();
      }

      public bool MoveNext()
      {
        while (this.FCurrentIndex + 1 < this.FOwner.FileAttachments.Count)
        {
          if (!this.FOwner.FileAttachments[++this.FCurrentIndex].Deleted)
            return true;
        }
        return false;
      }

      public void Reset() => this.FCurrentIndex = -1;

      object IEnumerator.Current => (object) this.Current;

      public FileAttachment Current
      {
        get
        {
          if (this.FCurrentIndex < 0 || this.FCurrentIndex >= this.FOwner.FileAttachments.Count)
            throw new InvalidOperationException("Ошибка перебора прикрепленных файлов: не установлен текущий элемент.");
          return this.FOwner.FileAttachments[this.FCurrentIndex];
        }
      }

      public void Dispose()
      {
      }
    }
  }
}
