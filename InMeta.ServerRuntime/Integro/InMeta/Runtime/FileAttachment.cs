// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.FileAttachment
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using System.IO;
using System.Text;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  public class FileAttachment
  {
    public readonly ObjectFileAttachments Owner;
    public readonly string FileName;
    private string FCaption;
    private string FDescription;
    private bool FReadOnly;
    private string FTempFileName;
    private FileAttachment.State FTransactionState;

    public string Caption
    {
      get => this.FCaption;
      set
      {
        if (this.FCaption == value)
          return;
        this.MarkOwnerModified();
        this.FCaption = value;
        this.FTransactionState |= FileAttachment.State.Modified;
      }
    }

    public string Description
    {
      get => this.FDescription;
      set
      {
        if (this.FDescription == value)
          return;
        this.MarkOwnerModified();
        this.FDescription = value;
        this.FTransactionState |= FileAttachment.State.Modified;
      }
    }

    public bool ReadOnly
    {
      get => this.FReadOnly;
      set
      {
        if (this.FReadOnly == value)
          return;
        this.MarkOwnerModified();
        this.FReadOnly = value;
        this.FTransactionState |= FileAttachment.State.Modified;
      }
    }

    public void GetFile(string dstFileName)
    {
      if (!File.Exists(this.SessionFilePath))
        return;
      File.Copy(this.SessionFilePath, dstFileName, true);
    }

    public string GetString(Encoding encoding) => File.Exists(this.SessionFilePath) ? File.ReadAllText(this.SessionFilePath, encoding) : (string) null;

    public string GetString() => File.Exists(this.SessionFilePath) ? File.ReadAllText(this.SessionFilePath, SysUtils.DetectTextFileEncoding(this.SessionFilePath)) : (string) null;

    private string EnsureTransactionFileName()
    {
      this.MarkOwnerModified();
      this.FTransactionState |= FileAttachment.State.Modified;
      if (StrUtils.IsNullOrEmpty(this.FTempFileName))
        this.FTempFileName = Path.GetTempFileName();
      return this.FTempFileName;
    }

    public void SetFile(string srcFileName) => File.Copy(srcFileName, this.EnsureTransactionFileName(), true);

    public void SetString(string data) => this.SetString(data, Encoding.Default);

    public void SetString(string data, Encoding encoding) => File.WriteAllText(this.EnsureTransactionFileName(), data, encoding);

    public void Delete()
    {
      this.MarkOwnerModified();
      this.DeleteTempFile();
      this.Deleted = true;
    }

    internal FileAttachment(ObjectFileAttachments owner, string fileName)
    {
      this.Owner = owner;
      this.FileName = fileName;
      this.FTransactionState = FileAttachment.State.Added;
    }

    internal FileAttachment(ObjectFileAttachments owner, XmlElement attachmentNode)
    {
      this.Owner = owner;
      this.FileName = attachmentNode.GetAttribute("file");
      this.FCaption = attachmentNode.GetAttribute("caption");
      this.FDescription = attachmentNode.InnerText;
      this.FReadOnly = XmlUtils.GetBoolAttr((XmlNode) attachmentNode, "read-only");
    }

    private void MarkOwnerModified() => this.Owner.MarkModified();

    private bool TempFileExists => !StrUtils.IsNullOrEmpty(this.FTempFileName) && File.Exists(this.FTempFileName);

    internal void DeleteTempFile()
    {
      if (this.TempFileExists)
        File.Delete(this.FTempFileName);
      this.FTempFileName = (string) null;
    }

    private string SessionFilePath => this.TempFileExists ? this.FTempFileName : Path.Combine(this.Owner.FolderPath, this.FileName);

    private string StorageFilePath => Path.Combine(this.Owner.FolderPath, this.FileName);

    internal bool Modified => (this.FTransactionState & FileAttachment.State.Modified) != (FileAttachment.State) 0;

    internal bool Added => (this.FTransactionState & FileAttachment.State.Added) != (FileAttachment.State) 0;

    internal bool Deleted
    {
      get => (this.FTransactionState & FileAttachment.State.Deleted) != (FileAttachment.State) 0;
      set
      {
        if (value)
          this.FTransactionState |= FileAttachment.State.Deleted;
        else
          this.FTransactionState &= ~FileAttachment.State.Deleted;
      }
    }

    public bool IsModified => this.Modified || this.Added;

    public void SaveToXml(XmlElement attachmentElement)
    {
      attachmentElement.SetAttribute("file", this.FileName);
      attachmentElement.SetAttribute("caption", this.FCaption);
      attachmentElement.SetAttribute("read-only", XStrUtils.ToXStr(this.FReadOnly));
      attachmentElement.InnerText = this.FDescription;
    }

    public XmlElement AppendXmlElement(XmlElement parentElement)
    {
      XmlElement element = parentElement.OwnerDocument.CreateElement("attachment");
      element.SetAttribute("file", this.FileName);
      element.SetAttribute("caption", this.FCaption);
      element.SetAttribute("read-only", XStrUtils.ToXStr(this.FReadOnly));
      element.InnerText = this.FDescription;
      parentElement.AppendChild((XmlNode) element);
      return element;
    }

    internal void Commit(XmlElement attachmentsNode)
    {
      if (this.Deleted)
      {
        if (File.Exists(this.StorageFilePath))
          File.Delete(this.StorageFilePath);
      }
      else
      {
        if (this.Added || this.Modified)
        {
          if (!StrUtils.IsNullOrEmpty(this.FTempFileName))
            File.Copy(this.FTempFileName, this.StorageFilePath, true);
          this.FTransactionState = (FileAttachment.State) 0;
        }
        this.AppendXmlElement(attachmentsNode);
      }
      this.DeleteTempFile();
    }

    internal void Rollback() => this.DeleteTempFile();

    public void CopyFrom(FileAttachment srcAttachment)
    {
      this.Caption = srcAttachment.Caption;
      this.Description = srcAttachment.Description;
      this.SetFile(srcAttachment.SessionFilePath);
    }

    internal void CopyChangesFrom(FileAttachment srcAttachment)
    {
      this.Caption = srcAttachment.Caption;
      this.Description = srcAttachment.Description;
      if (!srcAttachment.TempFileExists)
        return;
      this.SetFile(srcAttachment.SessionFilePath);
    }

    [System.Flags]
    internal enum State
    {
      Added = 1,
      Modified = 2,
      Deleted = 4,
    }
  }
}
