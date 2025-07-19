namespace ExtractSchemas
{
  static class StringExtension
  {
    public static string ToCamelCase(this string str)
    {
      return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
  }
}
