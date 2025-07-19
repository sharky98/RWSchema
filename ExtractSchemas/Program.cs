using System;

namespace ExtractSchemas
{
  internal class Program
  {
    static void Main(string[] args)
    {
      // Create an instance of SchemaExtractor
      var extractor = new SchemaExtractor();

      // Call the Extract method to perform schema extraction
      extractor.Extract();

      // Indicate completion
      Console.WriteLine("Schema extraction completed.");
    }
  }
}
