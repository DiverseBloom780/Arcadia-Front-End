using System;
using System.Xml;
using System.Xml.Schema;

namespace Arcadia.TeknoParrot {
    public static class TPValidator {
        public static bool Validate(string xmlPath, string schemaPath) {
            XmlSchemaSet schemas = new();
            schemas.Add(null, schemaPath);

            XmlDocument doc = new();
            doc.Load(xmlPath);

            bool isValid = true;
            doc.Validate((_, e) => {
                Console.WriteLine($"Validation error: {e.Message}");
                isValid = false;
            }, schemas);

            return isValid;
        }
    }
}
