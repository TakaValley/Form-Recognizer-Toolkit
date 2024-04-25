﻿//#define RUN_AS_ENTRY_OF_TOP_LEVEL_STATEMENT
#if RUN_AS_ENTRY_OF_TOP_LEVEL_STATEMENT

/*
  This code sample shows Prebuilt US Tax 1099-INT operations with the Document Intelligence SDKs. 

  To learn more, please visit the documentation - Quickstart: Document Intelligence (formerly Form Recognizer) SDKs
  https://learn.microsoft.com/azure/ai-services/document-intelligence/quickstarts/get-started-sdks-rest-api?pivots=programming-language-csharp
*/

using Azure;
using Azure.AI.DocumentIntelligence;

/*
  Remember to remove the key from your code when you're done, and never post it publicly. For production, use
  secure methods to store and access your credentials. For more information, see 
  https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-security?tabs=command-line%2Ccsharp#environment-variables-and-application-configuration
*/
string endpoint = "<AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT>";
string key = "<AZURE_DOCUMENT_INTELLIGENCE_KEY>";

var client = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(key));

// sample document
Uri fileUri = new("<SAMPLE_DOCUMENT_URL>");

var content = new AnalyzeDocumentContent() { UrlSource = fileUri };

Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-tax.us.1099INT", content);

AnalyzeResult result = operation.Value;

#region function for extracting values from DocumentField
Action<string, DocumentField, bool>? extractDocumentField = null;
extractDocumentField = delegate (string key, DocumentField docField, bool isSubItem)
{
    if (extractDocumentField != null)
    {
        var valueStr = "";

        if (docField.Type == DocumentFieldType.String)
        {
            valueStr = docField.ValueString;
        }
        else if (docField.Type == DocumentFieldType.Address)
        {
            valueStr = $"{docField.ValueAddress.HouseNumber}, {docField.ValueAddress.Road}, {docField.ValueAddress.City}, " +
                       $"{docField.ValueAddress.State}, {docField.ValueAddress.PostalCode}";
        }
        else if (docField.Type == DocumentFieldType.Date)
        {
            valueStr = docField.ValueDate.ToString();
        }
        else if (docField.Type == DocumentFieldType.Object)
        {
            if (!isSubItem)
            {
                Console.WriteLine(key);
            }
            // if current DocumentField type is an Object, extract each property in Object.
            IReadOnlyDictionary<string, DocumentField> itemFields = docField.ValueObject;
            foreach (var item in itemFields)
            {
                extractDocumentField(item.Key, item.Value, true);
            }

            return;
        }
        else if (docField.Type == DocumentFieldType.Array)
        {
            Console.WriteLine(key);
            if (docField.ValueArray.Count > 0)
            {
                // if current DocumentField type is an array, extract each member in array.
                for (var i = 0; i < docField.ValueArray.Count; i++)
                {
                    Console.WriteLine($"    Index {i} :");

                    extractDocumentField(key, docField.ValueArray[i], true);
                }
            }

            return;
        }
        else
        {
            valueStr = docField.Content;
        }

        var keyStr = isSubItem ? $"        {key}" : key;
        Console.WriteLine($"{keyStr} : '{valueStr}', with confidence {docField.Confidence}");
    }
};
#endregion

// extracting AnalyzeResult.Documents
for (int i = 0; i < result.Documents.Count; i++)
{
    Console.WriteLine($"Document {i}:");

    AnalyzedDocument document = result.Documents[i];
    foreach (var item in document.Fields)
    {
        extractDocumentField(item.Key, item.Value, false);
    }
}

#endif