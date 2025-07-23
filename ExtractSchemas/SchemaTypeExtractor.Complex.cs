using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Verse;

namespace ExtractSchemas;

partial class SchemaTypeExtractor
{
  private XmlSchemaComplexType DeriveComplexType()
  {
    var complexType = new XmlSchemaComplexType();
    var choice = new XmlSchemaChoice() { MinOccurs = 1, MaxOccursString = "unbounded" };
    complexType.Particle = choice;

    if (wildcardTypes.Any(x => x == assemblyType))
    {
      choice.Items.Add(DeriveWildcardType());
    }

    DeriveAttributesOfType().ToList().ForEach(x => complexType.Attributes.Add(x));

    DeriveFieldsOfType().ToList().ForEach(x => choice.Items.Add(x));

    return complexType;
  }

  private XmlSchemaAny DeriveWildcardType()
  {
    return new XmlSchemaAny()
    {
      MinOccurs = 0,
      MaxOccursString = "unbounded",
      ProcessContents = XmlSchemaContentProcessing.Lax,
    };
  }

  private IEnumerable<XmlSchemaAttribute> DeriveAttributesOfType()
  {
    var attributeList = new List<XmlSchemaAttribute>();

    // Only Defs have Name and ParentName attributes
    if (typeof(Def).IsAssignableFrom(assemblyType))
    {
      attributeList.Add(new XmlSchemaAttribute() { Name = "Name", SchemaTypeName = SchemaCommonValues.stringType });
      attributeList.Add(
        new XmlSchemaAttribute() { Name = "ParentName", SchemaTypeName = SchemaCommonValues.stringType }
      );
      attributeList.Add(
        new XmlSchemaAttribute()
        {
          Name = "Abstract",
          SchemaTypeName = new XmlQualifiedName("StrBoolean", SchemaCommonValues.targetNamespace),
        }
      );
    }

    // All type have the attribute Class
    attributeList.Add(new XmlSchemaAttribute() { Name = "Class", SchemaTypeName = SchemaCommonValues.stringType });
    attributeList.Add(new XmlSchemaAttribute() { Name = "MayRequire", SchemaTypeName = SchemaCommonValues.stringType });
    attributeList.Add(
      new XmlSchemaAttribute() { Name = "MayRequireAnyOf", SchemaTypeName = SchemaCommonValues.stringType }
    );

    return attributeList;
  }

  private IEnumerable<XmlSchemaElement> DeriveFieldsOfType()
  {
    var fieldList = new List<XmlSchemaElement>();
    var fields = PatchUsedPrivateFields(assemblyType.GetFields()).ToList().FindAll(x => x != null);

    foreach (var field in fields)
    {
      if (DeriveField(field) is var f and not null)
        fieldList.Add(f);
    }
    return fieldList;
  }

  private FieldInfo[] PatchUsedPrivateFields(FieldInfo[] baseFields)
  {
    FieldInfo[] addedFields = [];

    if (assemblyType.Name == "RulePack")
    {
      addedFields =
      [
        assemblyType.GetField("rulesStrings", BindingFlags.NonPublic | BindingFlags.Instance)!,
        assemblyType.GetField("rulesFiles", BindingFlags.NonPublic | BindingFlags.Instance)!,
      ];
    }
    if (assemblyType.BaseType?.Name == "StyleItemDef")
    {
      addedFields = [assemblyType.BaseType.GetField("category", BindingFlags.NonPublic | BindingFlags.Instance)!];
    }
    if (assemblyType.Name == "BiomeDef")
    {
      addedFields =
      [
        assemblyType.GetField("wildAnimals", BindingFlags.NonPublic | BindingFlags.Instance)!,
        assemblyType.GetField("pollutionWildAnimals", BindingFlags.NonPublic | BindingFlags.Instance)!,
        assemblyType.GetField("coastalWildAnimals", BindingFlags.NonPublic | BindingFlags.Instance)!,
        assemblyType.GetField("diseases", BindingFlags.NonPublic | BindingFlags.Instance)!,
        assemblyType.GetField("allowedPackAnimals", BindingFlags.NonPublic | BindingFlags.Instance)!,
      ];
    }

    return [.. baseFields, .. addedFields];
  }

  private XmlSchemaElement? DeriveField(FieldInfo field)
  {
    if (IsFieldIgnored(field))
      return null;

    // Boolean are a special case... The XML does not respect the standard xs:boolean XML data type.
    if (field.FieldType == typeof(bool))
    {
      return new XmlSchemaElement()
      {
        Name = field.Name,
        SchemaTypeName = new XmlQualifiedName("StrBoolean", SchemaCommonValues.targetNamespace),
      };
    }

    // If the field name is a known type, the element will be of that type.
    if (knownTypes.TryGetValue(field.FieldType, out string? value))
    {
      return PatchKnownTypeElement(field, value);
    }

    // Is not a generic type
    if (!field.FieldType.IsGenericType)
      return DeriveNonGenericType(field);

    return DeriveGenericType(field);
  }

  private XmlSchemaElement? PatchKnownTypeElement(FieldInfo field, string value)
  {
    string[] toPatch = ["rareCatchesSetMaker"];

    if (toPatch.Any(x => x == field.Name))
    {
      var knownElement = new XmlSchemaElement() { Name = field.Name };
      var innerComplexType = new XmlSchemaComplexType();
      knownElement.SchemaType = innerComplexType;
      var innerContent = new XmlSchemaSimpleContent();
      var extension = new XmlSchemaSimpleContentExtension
      {
        BaseTypeName = new XmlQualifiedName(value, SchemaCommonValues.xsdSchema),
      };
      innerComplexType.ContentModel = innerContent;
      innerContent.Content = extension;
      extension.Attributes.Add(
        new XmlSchemaAttribute() { Name = "MayRequire", SchemaTypeName = SchemaCommonValues.stringType }
      );
      extension.Attributes.Add(
        new XmlSchemaAttribute() { Name = "MayRequireAnyOf", SchemaTypeName = SchemaCommonValues.stringType }
      );
      return knownElement;
    }

    return new XmlSchemaElement()
    {
      Name = field.Name,
      SchemaTypeName = new XmlQualifiedName(value, SchemaCommonValues.xsdSchema),
    };
  }

  private bool IsFieldIgnored(FieldInfo field)
  {
    // Ignoring IExposable fields
    if (field.FieldType.GetInterfaces().Contains(typeof(IExposable)))
      return true;

    // Ignoring Literal or Static fields.
    if (field.IsLiteral || field.IsStatic)
      return true;

    // Ignoring fields with custom attributes.
    if (Attribute.GetCustomAttribute(field, typeof(UnsavedAttribute)) != null)
      return true;

    return false;
  }
}
