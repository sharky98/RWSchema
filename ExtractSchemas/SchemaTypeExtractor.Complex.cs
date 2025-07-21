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
    }

    // All type have the attribute Class
    attributeList.Add(new XmlSchemaAttribute() { Name = "Class", SchemaTypeName = SchemaCommonValues.stringType });

    return attributeList;
  }

  private IEnumerable<XmlSchemaElement> DeriveFieldsOfType()
  {
    var fieldList = new List<XmlSchemaElement>();
    foreach (var field in assemblyType.GetFields())
    {
      if (DeriveField(field) is var f and not null)
        fieldList.Add(f);
    }
    return fieldList;
  }

  private XmlSchemaElement? DeriveField(FieldInfo field)
  {
    if (IsFieldIgnored(field))
      return null;

    // If the field name is a known type, the element will be of that type.
    if (knownTypes.TryGetValue(field.FieldType, out string? value))
      return new XmlSchemaElement()
      {
        Name = field.Name,
        SchemaTypeName = new XmlQualifiedName(value, SchemaCommonValues.xsdSchema),
      };

    // Is not a generic type
    if (!field.FieldType.IsGenericType)
      return DeriveNonGenericType(field);

    return DeriveGenericType(field);
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
