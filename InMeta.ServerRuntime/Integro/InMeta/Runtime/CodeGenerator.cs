// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.CodeGenerator
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class CodeGenerator
  {
    public readonly DataApplication Application;
    public readonly CodeGeneratorOptions Options;

    public CodeGenerator(DataApplication application, CodeGeneratorOptions options)
    {
      this.Application = application;
      this.Options = options;
    }

    private static void ApplyTemplate(MetadataClass cls, StringBuilder sb, string template) => sb.Append(template.Replace("{Name}", cls.Name).Replace("{Type}", cls.TypeName).Replace("{QType}", cls.QTypeName).Replace("{SessionMember}", cls.SessionMemberProgId));

    public void GenerateHeader(StringBuilder sb) => sb.Append("// Эта сборка автоматически сгенерирована серверной средой выполнения.\n\nusing System;\nusing System.Collections;\nusing Integro.InMeta.Runtime;\n\nnamespace InMeta\n{\n");

    public void GenerateFooter(StringBuilder sb) => sb.Append("}");

    public void GenerateClasses(MetadataClassList classes, StringBuilder sb)
    {
      Hashtable hashtable = new Hashtable();
      for (int index = 0; index < classes.Count; ++index)
      {
        MetadataClass metadataClass = classes[index];
        ArrayList arrayList = (ArrayList) hashtable[(object) metadataClass.TypeNamespace];
        if (arrayList == null)
        {
          arrayList = new ArrayList();
          hashtable[(object) metadataClass.TypeNamespace] = (object) arrayList;
        }
        arrayList.Add((object) metadataClass);
      }
      foreach (string key in (IEnumerable) hashtable.Keys)
      {
        if (key.Trim().Length > 0)
          sb.Append("namespace ").Append(key).Append("{\n");
        foreach (MetadataClass cls in (ArrayList) hashtable[(object) key])
        {
          this.GenerateMeta(cls, sb);
          if ((this.Options & CodeGeneratorOptions.GenerateConditionBuilder) != (CodeGeneratorOptions) 0)
            this.GenerateConditionBuilder(cls, sb);
          this.GenerateClass(cls, sb);
          this.GenerateIList(cls, sb);
          this.GenerateList(cls, sb);
          this.GenerateChildList(cls, sb);
          this.GenerateLinkProperty(cls, sb);
          this.GenerateStorage(cls, sb);
        }
        if (key.Trim().Length > 0)
          sb.Append("}\n");
      }
    }

    private static void GenerateDoc(string prefix, string name, string text, StringBuilder sb)
    {
      string str1 = '<'.ToString() + name + (object) '>' + text + "</" + name + (object) '>';
      char[] chArray = new char[1]{ '\n' };
      foreach (string str2 in str1.Split(chArray))
        sb.Append(prefix).Append("/// ").Append(str2).Append("\n");
    }

    public void GenerateConditionBuilder(MetadataClass cls, StringBuilder sb)
    {
      CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}ConditionBuilder: QueryCondition\n{\n\tpublic readonly {Type}Meta Meta;\n\tpublic {Type}ConditionBuilder({Type}Meta meta): base() { Meta = meta; }\n");
      for (int index = 0; index < cls.Properties.Count; ++index)
      {
        MetadataProperty property = cls.Properties[index];
        if (!property.IsSelector && !property.IsId)
        {
          string valueType;
          property.GetMemberTypes(out valueType, out string _);
          sb.AppendFormat("\tpublic {0} {1}IsNull {{ get {{ return ({0})base.PropIsNull({2}); }}}}\n\tpublic {0} {1}IsNotNull {{ get {{ return ({0})base.PropIsNotNull({2}); }}}}\n\tpublic {0} {1}Is({3} value) {{ return ({0})base.PropIs({2}, value); }}\n\tpublic {0} {1}(CompareOp op, {3} value) {{ return ({0})base.PropCompare({2}, op, value); }}\n\tpublic {0} {1}Like(string value) {{ return ({0})base.PropLike({2}, value); }}\n\tpublic {0} {1}In(params {3}[] value) {{ return ({0})base.PropIn({2}, value); }}\n\tpublic {0} {1}Between({3} minValue, {3} maxValue) {{ return ({0})base.PropBetween({2}, minValue, maxValue); }}\n", (object) (cls.TypeName + "ConditionBuilder"), (object) property.MemberName, (object) ("Meta." + property.MemberName), (object) valueType);
        }
      }
      sb.AppendFormat("\tnew public {0} Begin {{ get {{ return ({0})base.Begin; }}}}\n\tnew public {0} End {{ get {{ return ({0})base.End; }}}}\n\tnew public {0} And {{ get {{ return ({0})base.And; }}}}\n\tnew public {0} Or {{ get {{ return ({0})base.Or; }}}}\n\tnew public {0} Not {{ get {{ return ({0})base.Not; }}}}\n}}\n", (object) (cls.TypeName + "ConditionBuilder"));
    }

    private static void GenerateProperties(MetadataClass cls, StringBuilder sb)
    {
      for (int index = 0; index < cls.Properties.Count; ++index)
      {
        MetadataProperty property = cls.Properties[index];
        if (!property.IsSelector && !property.IsId)
          CodeGenerator.GenerateProperty(property, sb);
      }
    }

    private static void GenerateProperty(MetadataProperty prop, StringBuilder sb)
    {
      string valueType;
      string propertyType;
      prop.GetMemberTypes(out valueType, out propertyType);
      string memberName = prop.MemberName;
      CodeGenerator.GenerateDoc("\t", "summary", prop.Caption, sb);
      sb.AppendFormat("\tpublic {0} {1}Property {{ get {{ return ({0})base[Meta.{1}]; }} }}\n", (object) propertyType, (object) memberName);
      CodeGenerator.GenerateDoc("\t", "summary", prop.Caption, sb);
      sb.AppendFormat("\tpublic {0} {1} {{\n\tget {{ return {1}Property.Value; }}\n\tset {{ {1}Property.Value = value; }}\n\t}}\n", (object) valueType, (object) memberName);
    }

    private static void GenerateChilds(MetadataClass cls, StringBuilder sb)
    {
      for (int index = 0; index < cls.Childs.Count; ++index)
      {
        MetadataChildRef child = cls.Childs[index];
        MetadataClass childClass = child.ChildClass;
        CodeGenerator.GenerateDoc("\t", "summary", childClass.Caption, sb);
        sb.AppendFormat("\tpublic {0}ChildList {1} {{ get {{ return ({0}ChildList)base.GetChilds(Meta.{1}); }} }}\n", (object) childClass.QTypeName, (object) child.MemberName);
      }
    }

    public void GenerateClass(MetadataClass cls, StringBuilder sb)
    {
      CodeGenerator.GenerateDoc(string.Empty, "summary", cls.Caption, sb);
      CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}: DataObject\n{\n\tpublic {Type}(): base() { }\n\tpublic {Type}Meta Meta { get { return (({QType}Storage)Storage).Meta; } }\n\tnew public {Type}Storage Storage { get { return ({QType}Storage)base.Storage; } }\n");
      CodeGenerator.GenerateProperties(cls, sb);
      CodeGenerator.GenerateChilds(cls, sb);
      sb.Append("}\n");
    }

    public void GenerateIList(MetadataClass cls, StringBuilder sb) => CodeGenerator.ApplyTemplate(cls, sb, "\npublic interface I{Type}List: IDataObjectList\n{\n\tnew {QType} this[int index] { get; }\n}\n");

    public void GenerateList(MetadataClass cls, StringBuilder sb) => CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}List: DataObjectList, I{Type}List\n{\n\tpublic {Type}List(): base() { }\n\tpublic {Type}List(ICollection collection): base(collection) { }\n\tpublic {Type}List(int capacity): base(capacity) { }\n\tnew public {QType} this[int index] { get { return ({QType})base[index]; } set { base[index] = value; } }\n\tprotected override Type GetAcceptableType() { return typeof({QType}); }\n}\n");

    public void GenerateChildList(MetadataClass cls, StringBuilder sb) => CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}ChildList: DataObjectChildList, I{Type}List\n{\n\tinternal {Type}ChildList(DataObject obj, MetadataChildRef childRef): base(obj, childRef) { }\n\tnew public {QType} First { get { return ({QType})base.First; } }\n\tnew public {QType} this[int index] { get { return ({QType})base[index]; } }\n\tnew public {QType} AddNew() { return ({QType})base.AddNew(); }\n}\n");

    public void GenerateLinkProperty(MetadataClass cls, StringBuilder sb) => CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}LinkProperty: LinkProperty\n{\n\tinternal {Type}LinkProperty(DataObject obj, MetadataProperty propMetadata): base(obj, propMetadata) { }\n\tnew public {QType} Value { get { return ({QType})base.Value; } set { base.Value = value; } }\n}\n");

    public void GenerateMeta(MetadataClass cls, StringBuilder sb)
    {
      CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}Meta\n{\n\tprivate MetadataClass FClass;\n");
      for (int index = 0; index < cls.Properties.Count; ++index)
      {
        MetadataProperty property = cls.Properties[index];
        if (!property.IsSelector && !property.IsId)
          sb.AppendFormat("\tprivate MetadataProperty F{0};\n\tpublic MetadataProperty {0} {{ get {{ if(F{0} == null) {{ F{0} = FClass.Properties.Need(\"{1}\"); }} return F{0}; }} }}\n", (object) property.MemberName, (object) property.Name);
      }
      for (int index = 0; index < cls.Childs.Count; ++index)
        sb.AppendFormat("\tprivate MetadataChildRef F{0};\n\tpublic MetadataChildRef {0} {{ get {{ if(F{0} == null) {{ F{0} = FClass.Childs.Need(\"{1}\"); }} return F{0}; }} }}\n", (object) cls.Childs[index].MemberName, (object) cls.Childs[index].ChildClass.Name);
      CodeGenerator.ApplyTemplate(cls, sb, "\tpublic {Type}Meta(MetadataClass cls) { FClass = cls; }\n");
      sb.Append("}\n");
    }

    public void GenerateStorage(MetadataClass cls, StringBuilder sb)
    {
      CodeGenerator.ApplyTemplate(cls, sb, "\npublic class {Type}Storage: DataStorage\n{\n\tpublic readonly {Type}Meta Meta;\n\tnew public InMeta.Session Session { get { return (InMeta.Session)base.Session; } }\n\tinternal {Type}Storage(DataSession session, MetadataClass cls): base(session, cls) { Meta = new {Type}Meta(cls); }\n\tprotected override LinkProperty CreateLinkPropertyInstance(DataObject obj, MetadataProperty propMetadata) { return new {QType}LinkProperty(obj, propMetadata); }\n\tprotected override DataObject CreateObjectInstance() { return new {QType}(); }\n\tprotected override DataObjectList CreateListInstance() { return new {QType}List(); }\n\tprotected override DataObjectChildList CreateChildListInstance(DataObject obj, MetadataChildRef childRef){ return new {QType}ChildList(obj, childRef); }\n\tnew public {QType} NullObject { get { return ({QType})base.NullObject; } }\n\tnew public {QType} this[string strId] { get { return ({QType})base[strId]; } }\n\tnew public {QType} this[DataId id] { get { return ({QType})base[id]; } }\n\tnew public {QType} AddNew() { return ({QType})base.AddNew(); }\n\tnew public {QType}List Query(string plan, string condition, params object[] paramArray) { return ({QType}List)base.Query(plan, condition, paramArray); }\n\tnew public {QType}List Query(string plan, {QType}ConditionBuilder conditionBuilder) { return ({QType}List)base.Query(plan, conditionBuilder); }\n\tnew public {QType}List Query(string plan) { return ({QType}List)base.Query(plan); }\n\tnew public {QType}List Query(string plan, params DataId[] ids) { return ({QType}List)base.Query(plan, ids); }\n\tnew public {QType} QueryObject(string plan, DataId id) { return ({QType})base.QueryObject(plan, id); }\n");
      if ((this.Options & CodeGeneratorOptions.GenerateConditionBuilder) != (CodeGeneratorOptions) 0)
        CodeGenerator.ApplyTemplate(cls, sb, "\tpublic {Type}ConditionBuilder Condition { get { return new {Type}ConditionBuilder(Meta); } }\n");
      sb.Append("}\n");
    }

    public void GenerateSession(MetadataClassList classes, StringBuilder sb)
    {
      sb.Append("\npublic class Session: DataSession\n{\n\tpublic Application Application { get { return (InMeta.Application)base.Application; } }\n\tpublic Session(DataApplication application, string authUser): base(application, authUser) { }\n");
      for (int index = 0; index < classes.Count; ++index)
        CodeGenerator.ApplyTemplate(classes[index], sb, "\tprivate {QType}Storage F{SessionMember};\n\tpublic {QType}Storage {SessionMember} { get { if(F{SessionMember} == null) F{SessionMember} = ({QType}Storage)base[\"{Name}\"]; return F{SessionMember}; } }\n");
      sb.Append("\tprotected override DataStorage CreateStorageInstance(MetadataClass cls)\n\t{\n\t\tswitch(cls.Name)\n\t\t{\n");
      for (int index = 0; index < classes.Count; ++index)
        CodeGenerator.ApplyTemplate(classes[index], sb, "\t\t\tcase \"{Name}\": return new {QType}Storage(this, cls);\n");
      sb.Append("\t\t\tdefault: return base.CreateStorageInstance(cls);\n\t\t}\n\t}\n}\n");
    }

    public void GenerateAplication(StringBuilder sb, string id, string centralAddress) => sb.AppendFormat("\npublic class Application: DataApplication\n{{\n\tpublic Application(string appId, string centralAddress): base(appId, centralAddress) {{ }}\n\tpublic Application(): base(\"{0}\", \"{1}\") {{ }}\n\toverride protected DataSession CreateSessionInstance(string authUser) {{ return new Session(this, authUser); }}\n\tnew public Session CreateSession(string authUser) {{ return (Session)base.CreateSession(authUser); }}\n\tnew public Session CreateSession() {{ return (Session)base.CreateSession(); }}\n}}\n", (object) id, (object) centralAddress);

    public void GenerateWebPage(StringBuilder sb) => sb.Append("\npublic class WebPage: Integro.InMeta.Web.WebPage\n{\n\toverride protected DataApplication CreateApplicationInstance() { return new Application(); }\n\tnew public Application Application { get { return (Application)base.Application; } }  \n\tnew public Session Session { get { return (Session)base.Session; } }\n}\n");

    public void GenerateWebService(StringBuilder sb) => sb.Append("\npublic class WebService: Integro.InMeta.Web.WebService\n{\n\toverride protected DataApplication CreateApplicationInstance() { return new Application(); }\n\tnew public Application Application { get { return (Application)base.Application; } }  \n\tnew public Session Session { get { return (Session)base.Session; } }\n}\n");
  }
}
