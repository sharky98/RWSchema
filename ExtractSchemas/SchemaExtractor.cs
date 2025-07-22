using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Verse;

namespace ExtractSchemas;

class SchemaExtractor
{
  /// <summary>
  /// Contains the relation between a System.Type and it's XML type.
  /// When a `Def` is used in another one, they are linked by string, so all `Def` in the assembly are of Xml type string.
  /// </summary>
  public Dictionary<Type, string> knownTypes = SchemaCommonValues.baseKnownTypes;

  /// <summary>
  /// Gather in a single Xml Schema all complex and simple types common to all other schema.
  /// </summary>
  public IList<XmlSchemaType> commonElements = [];

  public XmlSchemaSet schemaSet = new();

  public void Extract()
  {
    schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);

    var assemblyTypes = GetAssemblyType("Assembly-CSharp");
    knownTypes.AddRange(assemblyTypes.ToDictionary(x => x, x => "string"));
    foreach (var assemblyType in assemblyTypes)
    {
      ExtractAssemblyTypeSchema(assemblyType);
    }

    var commonElementSchema = CreateNewSchema();
    schemaSet.Add(commonElementSchema);
    commonElements.ToList().ForEach(x => commonElementSchema.Items.Add(x));

    var schemas = CompileXmlSchemaSet();
    foreach (var schema in schemas)
    {
      XmlSchemaType schemaName = (XmlSchemaType)schema.Items[0];
      WriteXmlSchemaToFile(schema, $"{schemaName.Name}.xsd");
    }
  }

  private static IEnumerable<Type> GetAssemblyType(string assemblyName)
  {
    Assembly assembly = Assembly.Load(assemblyName);

    return assembly
      .GetExportedTypes()
      .Where(x => !x.IsAbstract)
      .Where(x => x.IsClass)
      .Where(x => typeof(Def).IsAssignableFrom(x));
  }

  private void ExtractAssemblyTypeSchema(Type assemblyType)
  {
    var schema = CreateNewSchema();
    schemaSet.Add(schema);

    var include = new XmlSchemaInclude { SchemaLocation = "CommonElements.xsd" };
    schema.Includes.Add(include);

    var typeExtractor = new SchemaTypeExtractor(assemblyType, knownTypes, commonElements);
    typeExtractor.Derive();
    if (typeExtractor.XmlSchemaType == null)
    { // It should not be null, but just in case, we skip and go to the next type.
      Console.WriteLine($"The type {assemblyType.Name} returned a null XML schema type.");
      return;
    }
    schema.Items.Add(typeExtractor.XmlSchemaType);
    commonElements = typeExtractor.CommonElements;
  }

  private IList<XmlSchema> CompileXmlSchemaSet()
  {
    schemaSet.Compile();

    IList<XmlSchema> compiledSchema = [];

    foreach (XmlSchema schemaFromSet in schemaSet.Schemas())
    {
      compiledSchema.Add(schemaFromSet);
    }

    return compiledSchema;
  }

  private void WriteXmlSchemaToFile(XmlSchema schema, string filename)
  {
    if (!schema.IsCompiled)
    {
      Console.WriteLine($"The schema to be saved at {filename} was not compiled.");
      return;
    }

    if (schema.Items.Count > 1)
    {
      filename = "CommonElements.xsd";
    }

    XmlNamespaceManager nsmgr = new(new NameTable());
    nsmgr.AddNamespace(string.Empty, SchemaCommonValues.targetNamespace);
    nsmgr.AddNamespace("xs", SchemaCommonValues.xsdSchema);

    var path = SchemaCommonValues.destination + "/" + filename;
    File.WriteAllText(path, string.Empty);
    using StreamWriter file = new(path);
    schema.Write(file, nsmgr);
  }

  private static void ValidationCallbackOne(object? sender, ValidationEventArgs args)
  {
    Console.WriteLine(args.Message);
  }

  private static XmlSchema CreateNewSchema()
  {
    return new XmlSchema
    {
      TargetNamespace = SchemaCommonValues.targetNamespace,
      ElementFormDefault = XmlSchemaForm.Qualified,
    };
  }
}
