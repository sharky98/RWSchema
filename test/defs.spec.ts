import assert from "node:assert";
import fs from "node:fs";
import path from "node:path";
import validator from "xsd-schema-validator";

const rootElementName = "Defs";
const schemaType = "Defs";

const schemaName = `RimWorld.Mod.${schemaType}.xsd`;
const schemaPath = path.resolve("schemas", schemaName);
const xmlDir = path.resolve(`test/fixtures/${schemaType.toLowerCase()}.spec`);

describe("XSD Validation against existing files", () => {
  describe(schemaName, () => {
    const files = fs.readdirSync(xmlDir, { recursive: true, encoding: "utf8" }).filter((file) => file.endsWith(".xml"));
    for (const file of files) {
      it(`should validate the reference ${file} files`, async () => {
        const result = await validator.validateXML(
          getContentWithSchema(file),
          schemaPath,
        );

        assert.ok(result.valid, `XML validation failed: ${result.messages.join(", ")}`);
      });
    }
  });
});

const getContentWithSchema = (file: string) => {
  const searchString = `<${rootElementName}>`;
  const replaceString = `<${rootElementName} xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns="https://github.com/sharky98/RWSchema/${schemaType}"
    xsi:schemaLocation="https://github.com/sharky98/RWSchema/${schemaType} ../../../../schemas/${schemaName}">`;

  const filePath = path.resolve(xmlDir, file);
  let content = fs.readFileSync(filePath, "utf-8");

  if (content.includes(searchString)) {
    return content.replaceAll(searchString, replaceString);
  }
  return content;
};
