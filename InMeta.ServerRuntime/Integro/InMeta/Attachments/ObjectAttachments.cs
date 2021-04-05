// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Attachments.ObjectAttachments
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InMeta.Runtime;
using System;
using System.Runtime.InteropServices;

namespace Integro.InMeta.Attachments
{
  [ComVisible(false)]
  public class ObjectAttachments
  {
    private readonly ObjectFileAttachments FAttachments;

    public int Count => this.FAttachments.Count;

    public ObjectAttachments(DataObject obj)
    {
      if (obj == null)
        throw new NullReferenceException("Попытка создания объекта управления прикрепленными файлами для не существующего реестрового объекта");
      this.FAttachments = !obj.IsNull ? obj.Attachments : throw new Exception("Попытка создания объекта управления прикрепленными файлами для нулевого реестрового объекта");
    }

    public ObjectAttachment FindByFileName(string fileName)
    {
      FileAttachment attachment = this.FAttachments.Find(fileName);
      return attachment != null ? new ObjectAttachment(attachment) : (ObjectAttachment) null;
    }

    public void Add(ObjectAttachment attachment)
    {
      FileAttachment fileAttachment = this.FAttachments.Ensure(attachment.FileName);
      fileAttachment.Caption = attachment.Caption;
      fileAttachment.Description = attachment.Description;
      if (attachment.FData != null)
        fileAttachment.SetString(attachment.FData);
      else if (attachment.FDataFile != null)
        fileAttachment.SetFile(attachment.FDataFile);
      attachment.FAttachment = fileAttachment;
    }

    public ObjectAttachment this[int index] => new ObjectAttachment(this.FAttachments[index]);

    public void Commit() => this.FAttachments.Object.Session.Commit();
  }
}
