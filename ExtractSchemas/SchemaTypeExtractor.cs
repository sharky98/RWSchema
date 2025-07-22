using System.Xml.Schema;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ExtractSchemas;

partial class SchemaTypeExtractor(
  Type assemblyType,
  Dictionary<Type, string> knownTypes,
  IList<XmlSchemaType> commonElements
)
{
  public XmlSchemaType? XmlSchemaType { get; set; }
  private readonly Type assemblyType = assemblyType;
  private readonly Dictionary<Type, string> knownTypes = knownTypes;
  public IList<XmlSchemaType> CommonElements = commonElements;
  private readonly IEnumerable<Type> commonTypes =
  [
    typeof(Enum),
    typeof(IntVec2),
    typeof(IntVec3),
    typeof(Vector2),
    typeof(Vector3),
    typeof(Color),
  ];
  private readonly IEnumerable<Type> wildcardTypes =
  [
    typeof(DefModExtension),
    typeof(CompProperties),
    typeof(IngestionOutcomeDoer),
    typeof(HediffGiver),
    typeof(ThinkNode),
    typeof(GenStep),
    typeof(ScattererValidator),
    typeof(SymbolResolver),
    typeof(ScenPart),
    typeof(AudioGrain),
    typeof(StatPart),
    typeof(SkillNeed),
    typeof(ColorGenerator),
    typeof(StockGenerator),
    typeof(WorldGenStep),
  ];

  public void Derive()
  {
    if (commonTypes.Any(x => x == assemblyType))
    {
      XmlSchemaType = DeriveSimpleType();
      XmlSchemaType.Name = assemblyType.Name;
      return;
    }

    XmlSchemaType = DeriveComplexType();
    XmlSchemaType.Name = assemblyType.Name;
    return;

    throw new Exception("not done yet");
  }

  private void MaybeAddToCommonElements(Type fieldType)
  {
    var fieldName = fieldType.Name.ToCamelCase();
    var isKnownType = knownTypes.ContainsKey(fieldType);
    var isCommonElement = CommonElements.Any(x => x.Name!.ToCamelCase() == fieldName);

    if (isKnownType || isCommonElement)
      return;
    if (fieldType == assemblyType)
      return;

    var xmlFieldType = new SchemaTypeExtractor(fieldType, knownTypes, CommonElements);
    xmlFieldType.Derive();
    if (xmlFieldType.XmlSchemaType == null)
    { // It should not be null, but just in case, we skip and go to the next type.
      Console.WriteLine($"The type {fieldName} returned a null XML schema type.");
      return;
    }
    CommonElements.Add(xmlFieldType.XmlSchemaType);
  }
}
