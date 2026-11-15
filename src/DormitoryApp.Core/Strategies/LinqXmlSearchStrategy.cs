using System.Globalization;
using System.Xml.Linq;
using DormitoryApp.Core.Models;

namespace DormitoryApp.Core.Strategies;

/// <summary>
/// XML search strategy using LINQ to XML (XDocument with declarative filtering).
/// </summary>
public class LinqXmlSearchStrategy : IXmlSearchStrategy
{
    public string StrategyName => "LINQ to XML (XDocument)";

    public IReadOnlyList<DormitoryStudent> Search(string xmlPath, FilterCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"XML-файл не знайдено за вказаним шляхом: {xmlPath}");
        }

        criteria ??= new FilterCriteria();

        var doc = XDocument.Load(xmlPath);

        var query = doc.Descendants("Student")
            .Where(el => MatchesElement(el, criteria))
            .Select(MapXElementToStudent);

        return query.ToList();
    }

    private static bool MatchesElement(XElement el, FilterCriteria criteria)
    {
        if (criteria.IsEmpty())
            return true;

        if (!string.IsNullOrWhiteSpace(criteria.FullNameKeyword))
        {
            var fullName = ((string?)el.Attribute("FullName"))?.Trim() ?? string.Empty;
            if (!fullName.Contains(criteria.FullNameKeyword.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Faculty))
        {
            var faculty = ((string?)el.Attribute("Faculty"))?.Trim();
            if (!string.Equals(faculty, criteria.Faculty.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Department))
        {
            var dept = ((string?)el.Attribute("Department"))?.Trim();
            if (!string.Equals(dept, criteria.Department.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (criteria.Course.HasValue)
        {
            var courseStr = ((string?)el.Attribute("Course"))?.Trim();
            if (!int.TryParse(courseStr, out var c) || c != criteria.Course.Value)
                return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Privilege))
        {
            var priv = ((string?)el.Attribute("Privilege"))?.Trim();
            if (!string.Equals(priv, criteria.Privilege.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        var roomEl = el.Element("Room");

        if (!string.IsNullOrWhiteSpace(criteria.Building))
        {
            var building = ((string?)roomEl?.Attribute("Building"))?.Trim();
            if (!string.Equals(building, criteria.Building.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.RoomNumber))
        {
            var roomNum = ((string?)roomEl?.Attribute("RoomNumber"))?.Trim();
            if (!string.Equals(roomNum, criteria.RoomNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.CustomAttributeName) &&
            !string.IsNullOrWhiteSpace(criteria.CustomAttributeValue))
        {
            var customVal = FindAttributeValue(el, criteria.CustomAttributeName.Trim()) ??
                            FindAttributeValue(roomEl, criteria.CustomAttributeName.Trim());

            if (customVal == null || !customVal.Contains(criteria.CustomAttributeValue.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static string? FindAttributeValue(XElement? element, string attributeName)
    {
        if (element == null)
        {
            return null;
        }

        return element.Attributes()
            .FirstOrDefault(a => a.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            ?.Value
            .Trim();
    }

    private static DormitoryStudent MapXElementToStudent(XElement el)
    {
        var student = new DormitoryStudent();

        foreach (var attr in el.Attributes())
        {
            student.ApplyAttribute(attr.Name.LocalName, attr.Value);
        }

        var roomEl = el.Element("Room");
        if (roomEl != null)
        {
            var room = new RoomInfo();
            foreach (var attr in roomEl.Attributes())
            {
                room.ApplyAttribute(attr.Name.LocalName, attr.Value);
            }
            student.Room = room;
        }

        return student;
    }
}
