// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.SqlUtils
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using Integro.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integro.InMeta.Runtime
{
  internal class SqlUtils
  {
    private const char UnassignedClosingQuote = '\0';

    public static bool CharCanBeUsedInCompoundInMetaIdentifier(char c) => char.IsLetterOrDigit(c) || c == '_' || (c == '.' || c == ':') || c == '^';

    public static bool FindInMetaCompoundIdentifierInSQL(
      string sql,
      int startIndex,
      ref int nameStart,
      ref int nameEnd)
    {
      char ch = char.MinValue;
      for (int index1 = startIndex; index1 < sql.Length; ++index1)
      {
        char c = sql[index1];
        if (ch != char.MinValue)
        {
          if ((int) c == (int) ch)
          {
            if (index1 < sql.Length - 1 && (int) sql[index1 + 1] == (int) c)
              ++index1;
            else
              ch = char.MinValue;
          }
        }
        else if (c == '"' || c == '\'')
          ch = c;
        else if (c == '[')
          ch = ']';
        else if (StrUtils.CharCanBeUsedAsIdentifierFirstChar(c))
        {
          int index2 = index1 + 1;
          while (index2 < sql.Length && SqlUtils.CharCanBeUsedInCompoundInMetaIdentifier(sql[index2]))
            ++index2;
          nameStart = index1;
          nameEnd = index2;
          return true;
        }
      }
      return false;
    }

    public static object GetInDbCompatibleParamValue(object value)
    {
      switch (value)
      {
        case DataId _:
          return (object) value.ToString();
        case DataObject _:
          return (object) ((DataObject) value).Id.ToString();
        default:
          return value;
      }
    }

    public static void ConvertObjectsToInDbParams(
      object[] args,
      out DataType[] types,
      out object[] values)
    {
      if (args == null || args.Length == 0)
      {
        types = (DataType[]) null;
        values = (object[]) null;
      }
      else
      {
        types = new DataType[args.Length];
        values = new object[args.Length];
        for (int index = 0; index < args.Length; ++index)
        {
          object compatibleParamValue = SqlUtils.GetInDbCompatibleParamValue(args[index]);
          types[index] = InDbUtils.TypeCodeToDataType(Type.GetTypeCode(compatibleParamValue.GetType()));
          values[index] = compatibleParamValue;
        }
      }
    }

    public enum NavigationThrough
    {
      Link,
      Child,
      ExternalLink,
    }

    public class NavigationPathItem
    {
      public readonly SqlUtils.NavigationThrough Through;
      public readonly string Name;
      public readonly MetadataClass RefClass;

      public NavigationPathItem(
        SqlUtils.NavigationThrough through,
        string name,
        MetadataClass refClass)
      {
        this.Through = through;
        this.Name = name;
        this.RefClass = refClass;
      }

      public static SqlUtils.NavigationPathItem[] Parse(
        MetadataClassList classes,
        string path)
      {
        List<SqlUtils.NavigationPathItem> navigationPathItemList = new List<SqlUtils.NavigationPathItem>();
        int num;
        for (int startIndex = 0; startIndex < path.Length; startIndex = num + 1)
        {
          num = path.IndexOf('.', startIndex);
          if (num < 0)
            num = path.Length;
          navigationPathItemList.Add(SqlUtils.NavigationPathItem.ParseItem(classes, path.Substring(startIndex, num - startIndex)));
        }
        return navigationPathItemList.ToArray();
      }

      public static SqlUtils.NavigationPathItem ParseItem(
        MetadataClassList classes,
        string pathItem)
      {
        int length1 = pathItem.LastIndexOf('^');
        if (length1 >= 0)
        {
          string identName = pathItem.Substring(0, length1);
          return new SqlUtils.NavigationPathItem(SqlUtils.NavigationThrough.ExternalLink, pathItem.Substring(length1 + 1), classes.FindByIdentName(identName) ?? throw new Exception(string.Format("Не найден класс \"{0}\".", (object) identName)));
        }
        int length2 = pathItem.LastIndexOf(':');
        if (length2 >= 0)
        {
          string identName = pathItem.Substring(length2 + 1);
          MetadataClass byIdentName = classes.FindByIdentName(identName);
          if (byIdentName == null)
          {
            string format = "Не найден класс \"{0}\".";
            if (identName.IndexOf("/") >= 0)
              format += "Замечание: при задании составных идентификаторов в условиях поиска, символ \"/\" в именах классов необходимо заменять на символ \"_\".";
            throw new Exception(string.Format(format, (object) identName));
          }
          return new SqlUtils.NavigationPathItem(SqlUtils.NavigationThrough.Link, pathItem.Substring(0, length2), byIdentName);
        }
        MetadataClass byIdentName1 = classes.FindByIdentName(pathItem);
        return byIdentName1 != null ? new SqlUtils.NavigationPathItem(SqlUtils.NavigationThrough.Child, pathItem, byIdentName1) : new SqlUtils.NavigationPathItem(SqlUtils.NavigationThrough.Link, pathItem, (MetadataClass) null);
      }
    }

    public abstract class NavigationStep
    {
      public string Path;
      public SqlUtils.Navigation Owner;
      public SqlUtils.NavigationStep Predecessor;
      public string TargetAlias;

      protected NavigationStep(
        SqlUtils.Navigation navigation,
        SqlUtils.NavigationStep predecessor,
        string path)
      {
        this.Owner = navigation;
        this.Predecessor = predecessor;
        this.Path = path;
      }

      public MetadataClass SourceClass => this.Predecessor != null ? this.Predecessor.TargetClass : this.Owner.Class;

      public string SourceAlias => this.Predecessor != null ? this.Predecessor.TargetAlias : this.Owner.Alias;

      public abstract string SourceField { get; }

      public abstract MetadataClass TargetClass { get; }

      public abstract string TargetField { get; }
    }

    public class AssociationNavigationStep : SqlUtils.NavigationStep
    {
      public MetadataAssociationRef AssociationRef;

      public AssociationNavigationStep(
        SqlUtils.Navigation navigation,
        SqlUtils.NavigationStep predecessor,
        string path,
        MetadataAssociationRef associationRef)
        : base(navigation, predecessor, path)
      {
        this.AssociationRef = associationRef;
      }

      public override string SourceField => this.AssociationRef.Association.Property.DataField;

      public override MetadataClass TargetClass => this.AssociationRef.RefClass;

      public override string TargetField => this.AssociationRef.RefClass.IDProperty.DataField;
    }

    public class ChildNavigationStep : SqlUtils.NavigationStep
    {
      public MetadataChildRef ChildRef;

      public ChildNavigationStep(
        SqlUtils.Navigation navigation,
        SqlUtils.NavigationStep predecessor,
        string path,
        MetadataChildRef childRef)
        : base(navigation, predecessor, path)
      {
        this.ChildRef = childRef;
      }

      public override string SourceField => this.SourceClass.IDProperty.DataField;

      public override MetadataClass TargetClass => this.ChildRef.ChildClass;

      public override string TargetField => this.ChildRef.AggregationRef.Association.Property.DataField;
    }

    public class ExternalAssociationNavigationStep : SqlUtils.NavigationStep
    {
      public MetadataAssociationRef ExternalAssociationRef;

      public ExternalAssociationNavigationStep(
        SqlUtils.Navigation navigation,
        SqlUtils.NavigationStep predecessor,
        string path,
        MetadataAssociationRef externalAssociationRef)
        : base(navigation, predecessor, path)
      {
        this.ExternalAssociationRef = externalAssociationRef;
      }

      public override string SourceField => this.ExternalAssociationRef.RefClass.IDProperty.DataField;

      public override MetadataClass TargetClass => this.ExternalAssociationRef.Association.Class;

      public override string TargetField => this.ExternalAssociationRef.Association.Property.DataField;
    }

    public class Navigation
    {
      public MetadataClass Class;
      public string Alias;
      public readonly List<SqlUtils.NavigationStep> RootSteps = new List<SqlUtils.NavigationStep>();
      public readonly Dictionary<string, string> Aliases = new Dictionary<string, string>();

      public Navigation(MetadataClass cls)
      {
        this.Class = cls;
        this.Alias = this.CreateUniqueAlias(cls.DataTable);
      }

      public string CreateUniqueAlias(string tableName)
      {
        int num = 1;
        string str;
        string upper;
        do
        {
          str = string.Format("{0}{1}", (object) tableName, (object) num++);
          upper = str.ToUpper();
        }
        while (this.Aliases.ContainsKey(upper));
        this.Aliases.Add(upper, str);
        return str;
      }

      internal static void ParseLastPartOfPath(
        MetadataClassList classes,
        string path,
        out string predecessorPath,
        out string identifier,
        out MetadataClass refClass,
        out bool isExternalRef)
      {
        int length = path.LastIndexOf('.');
        predecessorPath = length >= 0 ? path.Substring(0, length) : (string) null;
        string pathItem = length >= 0 ? path.Substring(length + 1) : path;
        if (string.IsNullOrEmpty(pathItem))
        {
          identifier = (string) null;
          refClass = (MetadataClass) null;
          isExternalRef = false;
        }
        else
        {
          SqlUtils.NavigationPathItem navigationPathItem = SqlUtils.NavigationPathItem.ParseItem(classes, pathItem);
          identifier = navigationPathItem.Name;
          refClass = navigationPathItem.RefClass;
          isExternalRef = navigationPathItem.Through == SqlUtils.NavigationThrough.ExternalLink;
        }
      }

      private SqlUtils.NavigationStep FindRootStep(string path)
      {
        for (int index = 0; index < this.RootSteps.Count; ++index)
        {
          SqlUtils.NavigationStep rootStep = this.RootSteps[index];
          if (string.Compare(rootStep.Path, path, true) == 0)
            return rootStep;
        }
        return (SqlUtils.NavigationStep) null;
      }

      public SqlUtils.NavigationStep EnsureRootStep(string path)
      {
        SqlUtils.NavigationStep navigationStep1 = !string.IsNullOrEmpty(path) ? this.FindRootStep(path) : throw new ArgumentNullException(nameof (path));
        if (navigationStep1 != null)
          return navigationStep1;
        string predecessorPath;
        string identifier;
        MetadataClass refClass1;
        bool isExternalRef;
        SqlUtils.Navigation.ParseLastPartOfPath(this.Class.Metadata.Classes, path, out predecessorPath, out identifier, out refClass1, out isExternalRef);
        SqlUtils.NavigationStep predecessor = !string.IsNullOrEmpty(predecessorPath) ? this.EnsureRootStep(predecessorPath) : (SqlUtils.NavigationStep) null;
        MetadataClass refClass2 = predecessor != null ? predecessor.TargetClass : this.Class;
        SqlUtils.NavigationStep navigationStep2;
        if (isExternalRef)
        {
          MetadataProperty metadataProperty = refClass1.Properties.Need(identifier);
          if (!metadataProperty.IsLink)
            throw new Exception(string.Format("Свойство \"{0}.{1}\" не является отношением.", (object) refClass1.Name, (object) identifier));
          navigationStep2 = (SqlUtils.NavigationStep) new SqlUtils.ExternalAssociationNavigationStep(this, predecessor, path, metadataProperty.Association.Refs.Need(refClass2));
        }
        else
        {
          MetadataProperty property;
          MetadataChildRef childRef;
          refClass2.NeedMember(identifier, out property, out childRef);
          if (property != null)
          {
            if (!property.IsLink)
              throw new DataException(string.Format("\"{0}\" не является ассоциацией или дочерним объектом.", (object) identifier));
            MetadataAssociationRefList refs = property.Association.Refs;
            if (refClass1 == null && refs.Count != 1)
              throw new DataException(string.Format("\"{0}\" является ассоциацией с вариантами связи. В тексте запроса такое поле следует указывать в форме \"имя-свойства:имя-связанного-класса\" (например, \"Subject:Person\").", (object) identifier));
            navigationStep2 = (SqlUtils.NavigationStep) new SqlUtils.AssociationNavigationStep(this, predecessor, path, refClass1 == null ? refs[0] : refs.Need(refClass1));
          }
          else
            navigationStep2 = (SqlUtils.NavigationStep) new SqlUtils.ChildNavigationStep(this, predecessor, path, childRef);
        }
        navigationStep2.TargetAlias = this.CreateUniqueAlias(navigationStep2.TargetClass.DataTable);
        this.RootSteps.Add(navigationStep2);
        return navigationStep2;
      }

      public void AppendSqlTo(StringBuilder sql)
      {
        sql.AppendFormat("SELECT DISTINCT([{0}].[{1}]) FROM [{2}] [{0}]", (object) this.Alias, (object) this.Class.IDProperty.DataField, (object) this.Class.DataTable);
        foreach (SqlUtils.NavigationStep rootStep in this.RootSteps)
          sql.AppendFormat(" LEFT OUTER JOIN [{0}] [{1}] ON [{2}].[{3}]=[{4}].[{5}]", (object) rootStep.TargetClass.DataTable, (object) rootStep.TargetAlias, (object) rootStep.SourceAlias, (object) rootStep.SourceField, (object) rootStep.TargetAlias, (object) rootStep.TargetField);
      }

      public void AppendFromSection(StringBuilder sql)
      {
        sql.AppendFormat("FROM [{0}] [{1}]", (object) this.Class.DataTable, (object) this.Alias);
        foreach (SqlUtils.NavigationStep rootStep in this.RootSteps)
          sql.AppendFormat(" LEFT OUTER JOIN [{0}] [{1}] ON [{2}].[{3}]=[{4}].[{5}]", (object) rootStep.TargetClass.DataTable, (object) rootStep.TargetAlias, (object) rootStep.SourceAlias, (object) rootStep.SourceField, (object) rootStep.TargetAlias, (object) rootStep.TargetField);
      }

      public void RegisterIdentifier(
        string ident,
        out SqlUtils.NavigationStep step,
        out MetadataProperty prop)
      {
        int length = ident.LastIndexOf('.');
        string name;
        if (length >= 0)
        {
          step = this.EnsureRootStep(ident.Substring(0, length));
          name = ident.Substring(length + 1);
        }
        else
        {
          step = (SqlUtils.NavigationStep) null;
          name = ident;
        }
        MetadataClass metadataClass = step != null ? step.TargetClass : this.Class;
        prop = metadataClass.Properties.Find(name);
      }

      private void AppendField(
        SqlUtils.NavigationStep step,
        MetadataProperty prop,
        StringBuilder sb)
      {
        sb.AppendFormat("[{0}].[{1}]", step != null ? (object) step.TargetAlias : (object) this.Alias, (object) prop.DataField);
      }

      public void ReplaceCompoundIdentifiers(string source, StringBuilder dest)
      {
        if (string.IsNullOrEmpty(source))
          return;
        int startIndex = 0;
        int nameStart = 0;
        for (int nameEnd = 0; SqlUtils.FindInMetaCompoundIdentifierInSQL(source, startIndex, ref nameStart, ref nameEnd); startIndex = nameEnd)
        {
          if (nameStart > startIndex)
            dest.Append(source, startIndex, nameStart - startIndex);
          SqlUtils.NavigationStep step;
          MetadataProperty prop;
          this.RegisterIdentifier(source.Substring(nameStart, nameEnd - nameStart), out step, out prop);
          if (prop == null)
            dest.Append(source, nameStart, nameEnd - nameStart);
          else
            this.AppendField(step, prop, dest);
        }
        if (startIndex >= source.Length)
          return;
        dest.Append(source, startIndex, source.Length - startIndex);
      }
    }
  }
}
