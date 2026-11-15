using System.Globalization;
using System.Xml;
using DormitoryApp.Core.Models;

namespace DormitoryApp.Core.Strategies;

/// <summary>
/// XML search strategy using SAX API (streaming sequential read via XmlReader).
/// </summary>
public class SaxXmlSearchStrategy : IXmlSearchStrategy
{
    public string StrategyName => "SAX (XmlReader)";

    public IReadOnlyList<DormitoryStudent> Search(string xmlPath, FilterCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"XML-файл не знайдено за вказаним шляхом: {xmlPath}");
        }

        criteria ??= new FilterCriteria();
        var results = new List<DormitoryStudent>();

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        using var reader = XmlReader.Create(xmlPath, settings);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "Student")
            {
                var student = ReadStudentNode(reader);
                if (MatchesCriteria(student, criteria))
                {
                    results.Add(student);
                }
            }
        }

        return results;
    }

    private static DormitoryStudent ReadStudentNode(XmlReader reader)
    {
        var student = new DormitoryStudent();

        // Read all attributes of the <Student> node
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                student.ApplyAttribute(reader.Name, reader.Value);
            }

            // Return to the Student element before reading child nodes
            reader.MoveToElement();
        }

        // Read child elements (including <Room>)
        if (!reader.IsEmptyElement)
        {
            var studentDepth = reader.Depth;
            while (reader.Read())
            {
                if (reader.Depth <= studentDepth && reader.NodeType == XmlNodeType.EndElement && reader.Name == "Student")
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Room")
                {
                    student.Room = ReadRoomNode(reader);
                }
            }
        }

        return student;
    }

    private static RoomInfo ReadRoomNode(XmlReader reader)
    {
        var room = new RoomInfo();

        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                room.ApplyAttribute(reader.Name, reader.Value);
            }

            reader.MoveToElement();
        }

        return room;
    }

    private static bool MatchesCriteria(DormitoryStudent student, FilterCriteria criteria)
    {
        if (criteria.IsEmpty())
            return true;

        if (!string.IsNullOrWhiteSpace(criteria.FullNameKeyword) &&
            !student.FullName.Contains(criteria.FullNameKeyword.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Faculty) &&
            !student.Faculty.Equals(criteria.Faculty.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Department) &&
            !student.Department.Equals(criteria.Department.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (criteria.Course.HasValue && student.Course != criteria.Course.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Building) &&
            !student.Room.Building.Equals(criteria.Building.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.RoomNumber) &&
            !student.Room.RoomNumber.Equals(criteria.RoomNumber.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Privilege) &&
            (student.Privilege == null || !student.Privilege.Equals(criteria.Privilege.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.CustomAttributeName) &&
            !string.IsNullOrWhiteSpace(criteria.CustomAttributeValue))
        {
            if (!student.HasAttribute(criteria.CustomAttributeName.Trim(), criteria.CustomAttributeValue.Trim()))
            {
                return false;
            }
        }

        return true;
    }
}
