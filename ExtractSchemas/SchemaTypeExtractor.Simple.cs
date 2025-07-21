using System.Xml.Schema;
using UnityEngine;
using Verse;

namespace ExtractSchemas;

partial class SchemaTypeExtractor
{
  private XmlSchemaSimpleType DeriveSimpleType()
  {
    if (assemblyType.BaseType == typeof(Enum))
      return DeriveTypeEnum();
    if (assemblyType == typeof(IntVec2))
      return DeriveTypeIntVec2();
    if (assemblyType == typeof(IntVec3))
      return DeriveTypeIntVec3();
    if (assemblyType == typeof(Vector2))
      return DeriveTypeVector2();
    if (assemblyType == typeof(Vector3))
      return DeriveTypeVector3();
    if (assemblyType == typeof(Color))
      return DeriveTypeColor();

    throw new Exception("You should not have came here!");
  }

  private XmlSchemaSimpleType DeriveTypeEnum()
  {
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };
    foreach (var name in Enum.GetNames(assemblyType))
      restriction.Facets.Add(new XmlSchemaEnumerationFacet() { Value = name });
    return new XmlSchemaSimpleType() { Content = restriction };
  }

  private XmlSchemaSimpleType DeriveTypeIntVec2()
  {
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };
    restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rIntVec2 });
    return new XmlSchemaSimpleType() { Content = restriction };
  }

  private XmlSchemaSimpleType DeriveTypeIntVec3()
  {
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };
    restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rIntVec3 });
    return new XmlSchemaSimpleType() { Content = restriction };
  }

  private XmlSchemaSimpleType DeriveTypeVector2()
  {
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };
    restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rVector2 });
    return new XmlSchemaSimpleType() { Content = restriction };
  }

  private XmlSchemaSimpleType DeriveTypeVector3()
  {
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };
    restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rVector3 });
    return new XmlSchemaSimpleType() { Content = restriction };
  }

  private XmlSchemaSimpleType DeriveTypeColor()
  {
    var restriction = new XmlSchemaSimpleTypeRestriction() { BaseTypeName = SchemaCommonValues.stringType };
    restriction.Facets.Add(new XmlSchemaPatternFacet() { Value = SchemaCommonValues.rColor });
    return new XmlSchemaSimpleType() { Content = restriction };
  }
}
