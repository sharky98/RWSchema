using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using RimWorld;
using Verse;

namespace ExtractSchemas;

partial class SchemaTypeExtractor
{
  private XmlSchemaElement? DeriveGenericType(FieldInfo fieldInfo)
  {
    var fieldGenericTypeDefinition = fieldInfo.FieldType.GetGenericTypeDefinition();

    // At least one Generic Argument is GenericType.
    if (fieldInfo.FieldType.GetGenericArguments().Any(x => x.IsGenericType))
      return null;

    // Generic Type Def is List
    if (fieldGenericTypeDefinition.GetInterfaces().Any(x => x == typeof(IList)))
      return DeriveFieldListGenericType(fieldInfo);

    // Generic Type Def is Dictionary
    if (fieldGenericTypeDefinition.GetInterfaces().Any(x => x == typeof(IDictionary)))
      return DeriveFieldDictionaryGenericType(fieldInfo);

    // Generic Type Def is Nullable
    if (fieldGenericTypeDefinition == typeof(Nullable<>))
      return DeriveFieldNullableGenericType(fieldInfo);

    Console.WriteLine($"Unable to process the type `{fieldInfo.Name}` in `{assemblyType.FullName}`.");
    return null;
  }

  private XmlSchemaElement? DeriveFieldListGenericType(FieldInfo fieldInfo)
  {
    var listElements = new XmlSchemaElement()
    {
      Name = "li",
      SchemaTypeName = GetSchemaTypeName(fieldInfo),
      MinOccurs = 0,
      MaxOccursString = "unbounded",
    };

    // Sequence that contains the elements
    var list = new XmlSchemaSequence();
    list.Items.Add(listElements);

    // Complex Type that contain the sequence
    var complexType = new XmlSchemaComplexType { Particle = list };

    // Element that contain the complex type
    return new XmlSchemaElement() { Name = fieldInfo.Name.ToCamelCase(), SchemaType = complexType };
  }

  private XmlSchemaElement? DeriveFieldDictionaryGenericType(FieldInfo fieldInfo)
  {
    var keyElement = new XmlSchemaElement() { Name = "key", SchemaTypeName = GetSchemaTypeName(fieldInfo, 0) };
    var valueElement = new XmlSchemaElement() { Name = "value", SchemaTypeName = GetSchemaTypeName(fieldInfo, 1) };
    var keyValuePairElement = new XmlSchemaSequence();
    keyValuePairElement.Items.Add(keyElement);
    keyValuePairElement.Items.Add(valueElement);

    var keyValuePairType = new XmlSchemaComplexType() { Particle = keyValuePairElement };

    var listElements = new XmlSchemaElement()
    {
      Name = "li",
      SchemaType = keyValuePairType,
      MinOccurs = 0,
      MaxOccursString = "unbounded",
    };

    // Sequence that contains the elements
    var list = new XmlSchemaSequence();
    list.Items.Add(listElements);

    // Complex Type that contain the sequence
    var complexType = new XmlSchemaComplexType { Particle = list };

    // Element that contain the complex type
    return new XmlSchemaElement() { Name = fieldInfo.Name, SchemaType = complexType };
  }

  private XmlSchemaElement? DeriveFieldNullableGenericType(FieldInfo fieldInfo)
  {
    var nullableElement = new XmlSchemaElement()
    {
      Name = fieldInfo.Name.ToCamelCase(),
      SchemaTypeName = GetSchemaTypeName(fieldInfo),
    };
    return nullableElement;
  }

  private XmlQualifiedName GetSchemaTypeName(FieldInfo fieldInfo, int argIdx = 0)
  {
    var innerFieldType = fieldInfo.FieldType.GetGenericArguments()[argIdx];

    MaybeAddToCommonElements(innerFieldType);

    return knownTypes.TryGetValue(innerFieldType, out string? value)
      ? new XmlQualifiedName(value, SchemaCommonValues.xsdSchema)
      : new XmlQualifiedName(innerFieldType.Name, SchemaCommonValues.targetNamespace);
  }
}
