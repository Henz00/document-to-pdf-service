using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;

using var archive = ZipFile.OpenRead("C:\\Users\\Henzo\\source\\repos\\ODTDOCXtoPDFConverter\\TestDocuments\\mtest.docx");

foreach (var entrys in archive.Entries)
{
    Console.WriteLine(entrys.FullName);
}


//string json = await File.ReadAllTextAsync(@"C:\Users\Henzo\source\repos\ODTDOCXtoPDFConverter\TestDocuments\variables.json");
//Dictionary<string, string>? jsonVariables = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

//var entryBody = archive.GetEntry("word/document.xml");
//var entryHeader = archive.GetEntry("word/header1.xml");
//var entryFooter = archive.GetEntry("word/footer1.xml");


//List<ZipArchiveEntry> entries = new List<ZipArchiveEntry>();
//entries.Add(entryFooter);
//entries.Add(entryHeader);
//entries.Add(entryBody);

//foreach(var entry in entries)
//{
//    using var stream = entry!.Open();

//    var document = XDocument.Load(stream);
//    XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";



//    foreach (var textElement in document.Descendants(w + "t"))
//    {
//        foreach (var variable in jsonVariables)
//        {
//            textElement.Value = textElement.Value.Replace(
//                $"{{{variable.Key}}}",
//                variable.Value);
//        }
//    }
//    Console.WriteLine(document);
//}


