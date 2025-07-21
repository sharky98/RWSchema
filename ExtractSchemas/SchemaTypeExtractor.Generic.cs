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
  private XmlSchemaParticle? DeriveGenericType(FieldInfo fieldInfo)
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

    Console.WriteLine($"Unable to process the generic type of {fieldInfo.Name} is {assemblyType.Name}.");
    return null;
  }

  private XmlSchemaParticle? DeriveFieldListGenericType(FieldInfo fieldInfo)
  {
    // As a list, there is only one generic argument
    var innerFieldType = fieldInfo.FieldType.GetGenericArguments()[0];
    // TODO: If it's not already a known type or common type, extract it

    // Then create the list
    var list = new XmlSchemaSequence();
    var schemaTypeName = knownTypes.TryGetValue(innerFieldType, out string? value)
      ? new XmlQualifiedName(value, SchemaCommonValues.xsdSchema)
      : new XmlQualifiedName(innerFieldType.Name.ToCamelCase(), SchemaCommonValues.targetNamespace);
    list.Items.Add(
      new XmlSchemaElement()
      {
        Name = "li",
        SchemaTypeName = schemaTypeName,
        MinOccurs = 0,
        MaxOccursString = "unbounded",
      }
    );

    return list;
  }

  private XmlSchemaParticle? DeriveFieldDictionaryGenericType(FieldInfo fieldInfo)
  {
    Console.WriteLine($"{fieldInfo.Name} is a dictionary");
    return null;
  }

  private XmlSchemaParticle? DeriveFieldNullableGenericType(FieldInfo fieldInfo)
  {
    Console.WriteLine($"{fieldInfo.Name} is nullable");
    return null;
  }
}
