using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Verse;

namespace ExtractSchemas;

class SchemaExtractor
{
  public Dictionary<Type, string> knownTypes = SchemaCommonValues.baseKnownTypes;

  public void Extract()
  {
    Assembly rimWorldAssembly = Assembly.Load("Assembly-CSharp");

    var assemblyTypes = rimWorldAssembly
      .GetExportedTypes()
      .Where(x => !x.IsAbstract)
      .Where(x => x.IsClass)
      .Where(x => typeof(Def).IsAssignableFrom(x));

    // Extract the list of all defs to get the known values.
    knownTypes.AddRange(assemblyTypes.ToDictionary(x => x, x => "string"));

    foreach (var assemblyType in assemblyTypes)
    {
      ExtractAssemblyTypeSchema(assemblyType);
    }
  }

  private void ExtractAssemblyTypeSchema(Type assemblyType)
  {
    // Each Assembly Type will have it's own XSD file.
    var schema = new XmlSchema
    {
      TargetNamespace = SchemaCommonValues.targetNamespace,
      ElementFormDefault = XmlSchemaForm.Qualified,
    };

    // Which mean, I need to include the common values (it'll be a LONG file)
    var include = new XmlSchemaInclude { SchemaLocation = "schema-common.xsd" };
    schema.Includes.Add(include);

    // I need to get the XML Schema Type (SimpleType or ComplexType)
    var typeExtractor = new SchemaTypeExtractor(assemblyType, knownTypes);
    typeExtractor.Derive();
    if (typeExtractor.XmlSchemaType == null)
    { // It should not be null, but just in case, we skip and go to the next type.
      Console.WriteLine($"The type {assemblyType.Name} returned a null XML schema type.");
      return;
    }
    schema.Items.Add(typeExtractor.XmlSchemaType);

    var compiledSchema = CompileXmlSchema(schema);
    WriteXmlSchemaToFile(compiledSchema, @$"{assemblyType.Name.ToStringSafe()}.xsd");
  }

  private XmlSchema CompileXmlSchema(XmlSchema schema)
  {
    XmlSchemaSet schemaSet = new XmlSchemaSet();
    schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
    schemaSet.Add(schema);
    schemaSet.Compile();

    XmlSchema? compiledSchema = null;

    foreach (XmlSchema schemaFromSet in schemaSet.Schemas())
    {
      compiledSchema = schemaFromSet;
    }

    return compiledSchema!;
  }

  private void WriteXmlSchemaToFile(XmlSchema schema, string filename)
  {
    if (!schema.IsCompiled)
    {
      Console.WriteLine($"The schema to be saved at {filename} was not compiled.");
      return;
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
}
