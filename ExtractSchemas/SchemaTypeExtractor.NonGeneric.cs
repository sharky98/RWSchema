using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Verse;

namespace ExtractSchemas;

partial class SchemaTypeExtractor
{
  private XmlSchemaElement? DeriveNonGenericType(FieldInfo field)
  {
    if (field.FieldType == typeof(IntRange) || field.FieldType == typeof(FloatRange))
      return DeriveRangeType(field);

    // If the field doesn't exist, we create it!
    var fieldTypeName = field.FieldType.Name.ToCamelCase();
    if (true)
    {
      Console.WriteLine($"The field type {fieldTypeName} should be added here.");
      // var fieldTypeExtractor = new SchemaTypeExtractor(field.FieldType, knownTypes, XmlSchemaCommonType);
      // fieldTypeExtractor.Derive();
      // if (fieldTypeExtractor.XmlSchemaType != null)
      // {
      //   XmlSchemaCommonType = fieldTypeExtractor.XmlSchemaCommonType;
      //   XmlSchemaCommonType.Add(fieldTypeName, fieldTypeExtractor.XmlSchemaType);
      // }
    }

    return new XmlSchemaElement()
    {
      Name = field.Name,
      SchemaTypeName = new XmlQualifiedName(field.FieldType.Name.ToCamelCase(), SchemaCommonValues.targetNamespace),
    };
  }

  private XmlSchemaElement DeriveRangeType(FieldInfo field)
  {
    // We have either float or int range.
    // According to the decompiled source, the parser only recognize the format [min]~[max] or [val].
    // So we have an element with a simple type restriction on string.
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };

    if (field.FieldType == typeof(IntRange))
      restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rIR });
    if (field.FieldType == typeof(FloatRange))
      restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rFR });

    return new XmlSchemaElement()
    {
      Name = field.Name,
      SchemaType = new XmlSchemaSimpleType() { Content = restriction },
    };
  }
}
