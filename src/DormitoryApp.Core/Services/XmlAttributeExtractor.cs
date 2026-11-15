using System.Xml.Linq;

namespace DormitoryApp.Core.Services;

/// <summary>
/// Extracts attribute names and unique values from an XML file
/// to dynamically populate UI filter pickers.
/// </summary>
public class XmlAttributeExtractor
{
    /// <summary>
    /// Scans the XML document and returns a dictionary mapping each attribute name
    /// to a sorted list of its distinct values.
    /// </summary>
    public Dictionary<string, List<string>> ExtractUniqueAttributeValues(string xmlPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"XML-файл не знайдено: {xmlPath}");
        }

        var doc = XDocument.Load(xmlPath);
        var attributeValues = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        // Scan all <Student> nodes
        foreach (var studentEl in doc.Descendants("Student"))
        {
            foreach (var attr in studentEl.Attributes())
            {
                var attrName = attr.Name.LocalName;
                var attrVal = attr.Value.Trim();

                if (string.IsNullOrWhiteSpace(attrVal))
                    continue;

                if (!attributeValues.TryGetValue(attrName, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    attributeValues[attrName] = set;
                }
                set.Add(attrVal);
            }

            // Scan child <Room> elements
            var roomEl = studentEl.Element("Room");
            if (roomEl != null)
            {
                foreach (var attr in roomEl.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    var attrVal = attr.Value.Trim();

                    if (string.IsNullOrWhiteSpace(attrVal))
                        continue;

                    if (!attributeValues.TryGetValue(attrName, out var set))
                    {
                        set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        attributeValues[attrName] = set;
                    }
                    set.Add(attrVal);
                }
            }
        }

        // Convert sets to sorted lists
        return attributeValues.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase).ToList(),
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <summary>
    /// Returns all attribute names present on Student nodes across the file.
    /// </summary>
    public IReadOnlyList<string> GetAvailableStudentAttributes(string xmlPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"XML-файл не знайдено: {xmlPath}");
        }

        var doc = XDocument.Load(xmlPath);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var studentEl in doc.Descendants("Student"))
        {
            foreach (var attr in studentEl.Attributes())
            {
                names.Add(attr.Name.LocalName);
            }
        }

        return names.OrderBy(n => n).ToList();
    }
}
