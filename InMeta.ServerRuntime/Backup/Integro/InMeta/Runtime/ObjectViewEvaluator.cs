// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.ObjectViewEvaluator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.Utils;
using Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal class ObjectViewEvaluator
  {
    private readonly MetadataObjectView FObjectView;
    private readonly DataStorage FStorage;
    private ScriptControl FScriptControl;
    private readonly ObjectViewScriptRuntime FViewScriptRuntime;
    private readonly List<ObjectViewParameterLoader> FParamLoaders = new List<ObjectViewParameterLoader>();
    private LoadPlan FOnDemandLoadPlan;
    private readonly List<DataId> FEvaluationStack = new List<DataId>();

    public ObjectViewEvaluator(MetadataObjectView view, DataStorage storage)
    {
      this.FObjectView = view;
      this.FStorage = storage;
      this.FViewScriptRuntime = new ObjectViewScriptRuntime(storage.Session);
    }

    private ScriptControl ScriptControl
    {
      get
      {
        if (this.FScriptControl == null)
        {
          string code = this.GenerateCode((ICollection<ObjectViewParameterLoader>) this.FParamLoaders);
          ScriptControl scriptControl = new ScriptControl()
          {
            Language = this.FObjectView.ScriptLanguage
          };
          scriptControl.AddObject("ViewScriptRuntime", (object) this.FViewScriptRuntime, true);
          this.FStorage.Session.AddScriptLibs(scriptControl, this.FObjectView.Using);
          try
          {
            scriptControl.AddCode(code);
          }
          catch (Exception ex)
          {
            throw new InMetaException(scriptControl, ScriptErrorOperation.AddCode, "Представление \"{0}\" класса \"{1}\".", new object[2]
            {
              (object) this.FObjectView.Name,
              (object) this.FObjectView.Class.Name
            });
          }
          this.FScriptControl = scriptControl;
        }
        return this.FScriptControl;
      }
    }

    private void ExtractParams(
      string code,
      StringCollection paramNames,
      ICollection<ObjectViewParameterLoader> paramLoaders)
    {
      int nameStart = 0;
      int nameEnd = 0;
      for (int startIndex = 0; StrUtils.FindNameInVb(code, startIndex, ref nameStart, ref nameEnd); startIndex = nameEnd)
      {
        string str1 = code.Substring(nameStart, nameEnd - nameStart);
        if (str1.EndsWith("Property", StringComparison.InvariantCultureIgnoreCase))
        {
          string name = str1.Substring(0, str1.Length - 8);
          string str2 = name + "Property";
          if (!paramNames.Contains(str2) && ObjectViewParameterLoader.CanCreate(this.FObjectView, name))
          {
            paramNames.Add(str2);
            paramLoaders?.Add(ObjectViewParameterLoader.Create(this.FObjectView, name));
          }
        }
      }
      paramNames.Add("ThisObject");
      paramLoaders?.Add((ObjectViewParameterLoader) new ObjectViewThisObjectLoader());
    }

    private string GenerateCode(
      ICollection<ObjectViewParameterLoader> paramLoaders)
    {
      string scriptText = StrUtils.ConvertActiveContentToScriptText(this.FObjectView.Script, "ViewText.Write");
      StringCollection paramNames = new StringCollection();
      this.ExtractParams(scriptText, paramNames, paramLoaders);
      StringBuilder stringBuilder = new StringBuilder("class internal_ViewText\ndim Content\nsub ClearContent()\nContent = \"\"\nend sub\nsub Write(aText)\nContent = Content & aText\nend sub\nsub AddColumnTotals(anIndex, aValue)\nend sub\nend class\nfunction internal_get_view(");
      for (int index = 0; index < paramNames.Count; ++index)
      {
        if (index > 0)
          stringBuilder.Append(',');
        stringBuilder.Append("byval ").Append(paramNames[index]);
      }
      stringBuilder.Append(")\n").Append("dim ViewText\nset ViewText=new internal_ViewText\n").Append("ViewText.ClearContent\n").Append(scriptText).Append("\r\ninternal_get_view = ViewText.Content\nend function");
      return stringBuilder.ToString();
    }

    internal LoadPlan GetLoadPlan(DataSession session)
    {
      if (this.FOnDemandLoadPlan != null)
        return this.FOnDemandLoadPlan;
      this.FOnDemandLoadPlan = new LoadPlan(this.FObjectView.Class);
      for (int index = 0; index < this.FParamLoaders.Count; ++index)
        this.FParamLoaders[index].PrepareLoadPlan(this.FOnDemandLoadPlan, session);
      return this.FOnDemandLoadPlan;
    }

    public string GetObjectViewText(DataObject obj)
    {
      if (this.FEvaluationStack.Contains(obj.Id))
        throw new MetadataException(string.Format("Ошибка вычисления представления \"{0}\" для объекта \"{1}[{2}]\": рекурсивный вызов.", (object) this.FObjectView.Name, (object) obj.Class.Name, (object) obj.Id));
      this.FEvaluationStack.Add(obj.Id);
      try
      {
        ScriptControl scriptControl = this.ScriptControl;
        object[] objArray = new object[this.FParamLoaders.Count];
        for (int index = 0; index < this.FParamLoaders.Count; ++index)
          objArray[index] = Utility.PrepareComParameter(this.FParamLoaders[index].GetValue(obj));
        try
        {
          return scriptControl.Run("internal_get_view", objArray)?.ToString();
        }
        catch (Exception ex)
        {
          throw new InMetaException(scriptControl, ScriptErrorOperation.Run, "Представление \"{0}\" объекта \"{1}[{2}]\".\r\n{3}", new object[4]
          {
            (object) this.FObjectView.Name,
            (object) obj.Class.Name,
            (object) obj.Id,
            (object) ex
          });
        }
      }
      finally
      {
        this.FEvaluationStack.RemoveAt(this.FEvaluationStack.Count - 1);
      }
    }

    public void Dispose() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (!disposing || this.FScriptControl == null)
        return;
      this.FScriptControl.Dispose();
    }
  }
}
