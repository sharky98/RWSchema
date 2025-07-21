using System.Xml.Schema;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ExtractSchemas;

partial class SchemaTypeExtractor(Type assemblyType, Dictionary<Type, string> knownTypes)
{
  public XmlSchemaType? XmlSchemaType { get; set; }
  private readonly Type assemblyType = assemblyType;
  private readonly Dictionary<Type, string> knownTypes = knownTypes;
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
}
