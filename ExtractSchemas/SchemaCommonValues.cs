using System.Xml;

namespace ExtractSchemas;

static class SchemaCommonValues
{
  public static readonly string destination =
    "/home/bparesimard/Documents/21-development/21.06-rimworld/RWSchema/schemas/Defs";
  public static readonly string xsdSchema = "http://www.w3.org/2001/XMLSchema";
  public static readonly string targetNamespace = "https://github.com/sharky98/RWSchema/Defs";
  public static readonly XmlQualifiedName stringType = new("string", xsdSchema);

  public static readonly Dictionary<Type, string> baseKnownTypes = new()
  {
    [typeof(byte)] = "unsignedByte",
    [typeof(sbyte)] = "byte",
    [typeof(short)] = "short",
    [typeof(ushort)] = "unsignedShort",
    [typeof(int)] = "int",
    [typeof(uint)] = "unsignedInt",
    [typeof(long)] = "long",
    [typeof(ulong)] = "unsignedLong",
    [typeof(float)] = "float",
    [typeof(double)] = "double",
    [typeof(decimal)] = "decimal",
    // [typeof(bool)] = "boolean",
    [typeof(char)] = "string",
    [typeof(string)] = "string",
    [typeof(Type)] = "string",
    [typeof(Verse.BuildableDef)] = "string",
  };

  #region XML Regex for restrictions

  /// <summary>
  /// XML Regex for a integer (positive or negative, including any whitespace before or after)
  /// </summary>
  public static readonly string rI = @"\s*-?\d+\s*";

  /// <summary>
  /// XML Regex for a float (positive or negative, including any whitespace before or after)
  /// </summary>
  public static readonly string rF = @"\s*-?\d+(\.\d+)?\s*";

  /// <summary>
  /// XML Regex for a integer (positive only, including any whitespace before or after)
  /// </summary>
  public static readonly string rIP = @"\s*\d+\s*";

  /// <summary>
  /// XML Regex for a float (positive only, including any whitespace before or after)
  /// </summary>
  public static readonly string rFP = @"\s*\d+(\.\d+)?\s*";

  // IntVec2
  public static readonly string rIntVec2 = @$"\({rI},{rI}\)";

  // IntVec3
  public static readonly string rIntVec3 = @$"\({rI},{rI},{rI}\)";

  // Vector2
  public static readonly string rVector2 = @$"\({rF},{rF}\)";

  // Vector3
  public static readonly string rVector3 = @$"\({rF},{rF},{rF}\)";

  // Color (Positive IntVec4)
  public static readonly string rColor = @$"\({rFP},{rFP},{rFP}(,{rFP})?\)";

  // Int Range
  public static readonly string rIR = @$"{rI}(~{rI})?";

  // Float Range
  public static readonly string rFR = @$"{rF}(~{rF})?";

  #endregion XML Regex
}
