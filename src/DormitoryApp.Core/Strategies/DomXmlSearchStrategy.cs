using System.Globalization;
using System.Text;
using System.Xml;
using DormitoryApp.Core.Models;

namespace DormitoryApp.Core.Strategies;

/// <summary>
/// XML search strategy using DOM API (XmlDocument with dynamic XPath query construction).
/// </summary>
public class DomXmlSearchStrategy : IXmlSearchStrategy
{
    private const string UpperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ";
    private const string LowerAlphabet = "abcdefghijklmnopqrstuvwxyzабвгґдеєжзиіїйклмнопрстуфхцчшщьюя";

    public string StrategyName => "DOM (XmlDocument + XPath)";

    public IReadOnlyList<DormitoryStudent> Search(string xmlPath, FilterCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"XML-файл не знайдено за вказаним шляхом: {xmlPath}");
        }

        criteria ??= new FilterCriteria();
        var results = new List<DormitoryStudent>();

        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var xpath = BuildXPathQuery(criteria);
        var nodes = doc.SelectNodes(xpath);

        if (nodes != null)
        {
            foreach (XmlNode node in nodes)
            {
                var student = MapXmlNodeToStudent(node);

                // Extra case-insensitive check for Cyrillic keyword matching
                if (MatchesAdditionalCriteria(student, criteria))
                {
                    results.Add(student);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Dynamically builds an XPath query based on the provided filter criteria.
    /// </summary>
    public static string BuildXPathQuery(FilterCriteria criteria)
    {
        var predicates = new List<string>();

        if (!string.IsNullOrWhiteSpace(criteria.Faculty))
        {
            predicates.Add(BuildCaseInsensitiveEquals("@Faculty", criteria.Faculty));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Department))
        {
            predicates.Add(BuildCaseInsensitiveEquals("@Department", criteria.Department));
        }

        if (criteria.Course.HasValue)
        {
            predicates.Add($"number(@Course)={criteria.Course.Value}");
        }

        if (!string.IsNullOrWhiteSpace(criteria.Privilege))
        {
            predicates.Add(BuildCaseInsensitiveEquals("@Privilege", criteria.Privilege));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Building))
        {
            predicates.Add(BuildCaseInsensitiveEquals("Room/@Building", criteria.Building));
        }

        if (!string.IsNullOrWhiteSpace(criteria.RoomNumber))
        {
            predicates.Add(BuildCaseInsensitiveEquals("Room/@RoomNumber", criteria.RoomNumber));
        }

        if (!string.IsNullOrWhiteSpace(criteria.CustomAttributeName) &&
            !string.IsNullOrWhiteSpace(criteria.CustomAttributeValue))
        {
            var attrName = criteria.CustomAttributeName.Trim();
            if (IsValidXmlAttributeName(attrName))
            {
                var lowerName = FormatXPathLiteral(attrName.ToLowerInvariant());
                predicates.Add($"(@*[translate(local-name(), '{UpperAlphabet}', '{LowerAlphabet}')={lowerName}] or Room/@*[translate(local-name(), '{UpperAlphabet}', '{LowerAlphabet}')={lowerName}])");
            }
            else
            {
                predicates.Add("false()");
            }
        }

        // FullNameKeyword and CustomAttribute substring matching are evaluated in MatchesAdditionalCriteria
        // to ensure case-insensitive Cyrillic and Room attribute parity across all strategies.

        if (predicates.Count == 0)
        {
            return "//Student";
        }

        return $"//Student[{string.Join(" and ", predicates)}]";
    }

    private static string BuildCaseInsensitiveEquals(string xpathExpr, string rawValue)
    {
        var lowerVal = rawValue.Trim().ToLowerInvariant();
        return $"translate(normalize-space({xpathExpr}), '{UpperAlphabet}', '{LowerAlphabet}')={FormatXPathLiteral(lowerVal)}";
    }

    private static bool IsValidXmlAttributeName(string name)
    {
        try
        {
            XmlConvert.VerifyNCName(name);
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    private static string FormatXPathLiteral(string value)
    {
        if (!value.Contains('\''))
        {
            return $"'{value}'";
        }

        if (!value.Contains('"'))
        {
            return $"\"{value}\"";
        }

        var parts = value.Split('\'');
        var concatArgs = new List<string>();
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                concatArgs.Add($"'{parts[i]}'");
            }

            if (i < parts.Length - 1)
            {
                concatArgs.Add("\"'\"");
            }
        }

        return $"concat({string.Join(", ", concatArgs)})";
    }

    private static DormitoryStudent MapXmlNodeToStudent(XmlNode node)
    {
        var student = new DormitoryStudent();

        if (node.Attributes != null)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                student.ApplyAttribute(attr.Name, attr.Value);
            }
        }

        var roomNode = node.SelectSingleNode("Room");
        if (roomNode?.Attributes != null)
        {
            var room = new RoomInfo();
            foreach (XmlAttribute attr in roomNode.Attributes)
            {
                room.ApplyAttribute(attr.Name, attr.Value);
            }
            student.Room = room;
        }

        return student;
    }

    private static bool MatchesAdditionalCriteria(DormitoryStudent student, FilterCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.FullNameKeyword) &&
            !student.FullName.Contains(criteria.FullNameKeyword.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.CustomAttributeName) &&
            !string.IsNullOrWhiteSpace(criteria.CustomAttributeValue) &&
            !student.HasAttribute(criteria.CustomAttributeName.Trim(), criteria.CustomAttributeValue.Trim()))
        {
            return false;
        }

        return true;
    }
}
