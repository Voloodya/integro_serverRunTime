// Decompiled with JetBrains decompiler
// Type: Integro.InMeta.Runtime.MetadataProperty
// Assembly: InMeta.ServerRuntime, Version=1.9.112.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B76D0ED-50A9-498A-8B76-23FDD608972C
// Assembly location: V:\20210126 УЖКХ\InMeta_ServerRuntime_dll\InMeta.ServerRuntime.dll

using Integro.InDbs;
using Integro.Utils;
using System;
using System.Runtime.InteropServices;
using System.Xml;

namespace Integro.InMeta.Runtime
{
  [ComVisible(false)]
  public class MetadataProperty
  {
    private static readonly string[] FPurposeNames = new string[4]
    {
      "id",
      "data",
      "association",
      "aggregation"
    };
    public readonly MetadataClass Class;
    public readonly string Name;
    private MetadataAssociation FAssociation;
    private XmlNode FSourceNode;
    private string FCaption;
    private string FDataField;
    private string FDataTypeName;
    private DataType FDataType;
    private int FDataLength;
    private string FInitialValue;
    private bool FIsExtension;
    private string FExtensionClass;
    private string FExtensionProperty;
    private MetadataPropertyPurpose FPurpose;
    private string FMemberName;
    public readonly MetadataLookupValueList LookupValues;
    public readonly int Index;
    private static readonly MetadataProperty.InDbTypeToGenTypes[] FGenTypes = new MetadataProperty.InDbTypeToGenTypes[8]
    {
      new MetadataProperty.InDbTypeToGenTypes(DataType.Boolean, "bool", "BooleanProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.Integer, "int", "IntegerProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.Float, "double", "DoubleProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.Currency, "decimal", "CurrencyProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.DateTime, "DateTime", "DateTimeProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.String, "string", "StringProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.Memo, "string", "StringProperty"),
      new MetadataProperty.InDbTypeToGenTypes(DataType.Binary, "byte[]", "BinaryProperty")
    };

    internal void SetAssociation(MetadataAssociation ass) => this.FAssociation = ass;

    public MetadataProperty(MetadataClass cls, string name, int index)
    {
      this.Class = cls;
      this.Name = name;
      this.Index = index;
      this.FPurpose = MetadataPropertyPurpose.Data;
      this.FDataType = DataType.Unknown;
      this.LookupValues = new MetadataLookupValueList();
    }

    internal void LoadFromXml(XmlNode node)
    {
      try
      {
        this.FSourceNode = node;
        this.FPurpose = (MetadataPropertyPurpose) XmlUtils.GetEnumAttr(node, "purpose", MetadataProperty.FPurposeNames);
        this.FCaption = XmlUtils.GetAttr(node, "caption", this.FCaption);
        this.FDataField = XmlUtils.GetAttr(node, "data-field", this.FDataField);
        this.FDataTypeName = XmlUtils.GetAttr(node, "data-type", this.FDataTypeName);
        this.FDataType = InDbUtils.InMetaDataTypeToDataType(this.FDataTypeName);
        this.FDataLength = XmlUtils.GetIntAttr(node, "data-length", this.FDataLength);
        this.FInitialValue = XmlUtils.GetAttr(node, "default-value", this.FInitialValue);
        this.FIsExtension = XmlUtils.GetBoolAttr(node, "is-virtual", this.FIsExtension);
        this.FExtensionClass = XmlUtils.GetAttr(node, "virtual-aggregation", this.FExtensionClass);
        this.FExtensionProperty = XmlUtils.GetAttr(node, "virtual-ref-property", this.FExtensionProperty);
        if (this.FDataField.Length == 0)
          this.FDataField = this.Name;
        if (this.FDataType == DataType.Unknown)
          throw new MetadataException("Не задан тип данных");
        this.FMemberName = XmlUtils.GetAttr(node, "prog-id", this.Name);
        this.LookupValues.LoadFromXml(node);
      }
      catch (Exception ex)
      {
        throw new MetadataException(string.Format("Ошибка загрузки метаданных свойства {0}", (object) this.Name), ex);
      }
    }

    public MetadataAssociation Association => this.FAssociation;

    public XmlNode SourceNode => this.FSourceNode;

    public string Caption => this.FCaption;

    public string DataField => this.FDataField;

    public string DataTypeName => this.FDataTypeName;

    public DataType DataType => this.FDataType;

    public int DataLength => this.FDataLength;

    public string InitialValue => this.FInitialValue;

    public bool IsExtension => this.FIsExtension;

    public string ExtensionClass => this.FExtensionClass;

    public string ExtensionProperty => this.FExtensionProperty;

    public MetadataPropertyPurpose Purpose => this.FPurpose;

    public bool IsSelector => this.FAssociation != null && this.FAssociation.Selector == this;

    public bool IsUserField => !this.IsSelector && (this.FPurpose == MetadataPropertyPurpose.Aggregation || this.FPurpose == MetadataPropertyPurpose.Association || this.FPurpose == MetadataPropertyPurpose.Data);

    public bool IsLink => this.FPurpose == MetadataPropertyPurpose.Aggregation || this.FPurpose == MetadataPropertyPurpose.Association;

    public bool IsData => this.FPurpose == MetadataPropertyPurpose.Data;

    public bool IsId => this.FPurpose == MetadataPropertyPurpose.Id;

    public bool IsAssociation => this.FPurpose == MetadataPropertyPurpose.Association;

    public bool IsAggregation => this.FPurpose == MetadataPropertyPurpose.Aggregation;

    public string MemberName => this.FMemberName;

    private void GetDataPropertyTypes(out string valueType, out string propertyType)
    {
      DataType dataType = this.DataType & DataType.BaseMask;
      for (int index = 0; index < MetadataProperty.FGenTypes.Length; ++index)
      {
        if (MetadataProperty.FGenTypes[index].InDbType == dataType)
        {
          valueType = MetadataProperty.FGenTypes[index].ValueType;
          propertyType = MetadataProperty.FGenTypes[index].PropertryType;
          return;
        }
      }
      valueType = "object";
      propertyType = "DataProperty";
    }

    private void GetAssociationPropertyTypes(out string valueType, out string propertyType)
    {
      if (this.Association.Selector == null)
      {
        MetadataClass refClass = this.Association.Refs[0].RefClass;
        valueType = refClass.QTypeName;
        propertyType = refClass.QTypeName + "LinkProperty";
      }
      else
      {
        valueType = "DataObject";
        propertyType = "LinkProperty";
      }
    }

    internal void GetMemberTypes(out string valueType, out string propertyType)
    {
      if (this.IsLink)
        this.GetAssociationPropertyTypes(out valueType, out propertyType);
      else
        this.GetDataPropertyTypes(out valueType, out propertyType);
    }

    public object ValueFromXStr(DataSession session, string xStr)
    {
      if (StrUtils.IsNullOrEmpty(xStr) || xStr == "{{null}}")
        return (object) null;
      if (this.IsLink)
      {
        if (this.Association.Refs.Count > 1)
          throw new MetadataException("Невозможно получить связанный объект по идентификатору: для ассоциации определено более одного варианта связи.");
        return (object) session[this.Association.Refs[0].RefClass][new DataId(xStr)];
      }
      switch (this.DataType & DataType.BaseMask)
      {
        case DataType.Boolean:
          return (object) XStrUtils.ToBool(xStr, false);
        case DataType.Integer:
          return (object) XStrUtils.ToInt(xStr, 0);
        case DataType.Float:
          return (object) XStrUtils.ToDouble(xStr, 0.0);
        case DataType.Currency:
          return (object) XStrUtils.ToCurrency(xStr, 0M);
        case DataType.DateTime:
          return (object) XStrUtils.ToDateTime(xStr, DateTime.MinValue);
        case DataType.Binary:
          return (object) Convert.FromBase64String(xStr);
        default:
          return (object) XStrUtils.ToString(xStr, string.Empty);
      }
    }

    private struct InDbTypeToGenTypes
    {
      public readonly DataType InDbType;
      public readonly string ValueType;
      public readonly string PropertryType;

      public InDbTypeToGenTypes(DataType inDbType, string valueType, string propertryType)
      {
        this.InDbType = inDbType;
        this.ValueType = valueType;
        this.PropertryType = propertryType;
      }
    }
  }
}
