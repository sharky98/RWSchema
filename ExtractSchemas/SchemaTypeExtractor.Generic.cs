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
    var baseType = GetSchemaTypeName(fieldInfo);
    if (baseType.Name == "StatModifier")
    {
      // I need to do something else... probably a simple xs:any here...
      var anyComplexType = new XmlSchemaComplexType();
      var choice = new XmlSchemaChoice() { MinOccurs = 1, MaxOccursString = "unbounded" };
      anyComplexType.Particle = choice;
      choice.Items.Add(DeriveWildcardType());
      return new XmlSchemaElement() { Name = fieldInfo.Name.ToCamelCase(), SchemaType = anyComplexType };
    }
    var listElements = new XmlSchemaElement()
    {
      Name = "li",
      MinOccurs = 0,
      MaxOccursString = "unbounded",
    };

    if (baseType.Namespace != SchemaCommonValues.targetNamespace)
    {
      var innerComplexType = new XmlSchemaComplexType();
      listElements.SchemaType = innerComplexType;
      var innerContent = new XmlSchemaSimpleContent();
      var extension = new XmlSchemaSimpleContentExtension { BaseTypeName = baseType };
      innerComplexType.ContentModel = innerContent;
      innerContent.Content = extension;
      extension.Attributes.Add(
        new XmlSchemaAttribute() { Name = "MayRequire", SchemaTypeName = SchemaCommonValues.stringType }
      );
      extension.Attributes.Add(
        new XmlSchemaAttribute() { Name = "MayRequireAnyOf", SchemaTypeName = SchemaCommonValues.stringType }
      );
    }
    else
    {
      listElements.SchemaTypeName = baseType;
    }

    // Sequence that contains the elements
    var list = new XmlSchemaSequence();
    list.Items.Add(listElements);

    // Complex Type that contain the sequence
    var complexType = new XmlSchemaComplexType { Particle = list };
    complexType.Attributes.Add(
      new XmlSchemaAttribute() { Name = "MayRequire", SchemaTypeName = SchemaCommonValues.stringType }
    );
    complexType.Attributes.Add(
      new XmlSchemaAttribute() { Name = "MayRequireAnyOf", SchemaTypeName = SchemaCommonValues.stringType }
    );

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
    keyValuePairType.Attributes.Add(
      new XmlSchemaAttribute() { Name = "MayRequire", SchemaTypeName = SchemaCommonValues.stringType }
    );
    keyValuePairType.Attributes.Add(
      new XmlSchemaAttribute() { Name = "MayRequireAnyOf", SchemaTypeName = SchemaCommonValues.stringType }
    );

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
    complexType.Attributes.Add(
      new XmlSchemaAttribute() { Name = "MayRequire", SchemaTypeName = SchemaCommonValues.stringType }
    );
    complexType.Attributes.Add(
      new XmlSchemaAttribute() { Name = "MayRequireAnyOf", SchemaTypeName = SchemaCommonValues.stringType }
    );

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
