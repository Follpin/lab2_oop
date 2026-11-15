using System.Globalization;

namespace DormitoryApp.Core.Models;

/// <summary>
/// Represents a student residing in the dormitory.
/// Supports required attributes and a dynamic dictionary of optional XML attributes.
/// </summary>
public class DormitoryStudent
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Course { get; set; }
    public bool HasCourse { get; set; }
    public string? Privilege { get; set; }
    public string? Phone { get; set; }
    public RoomInfo Room { get; set; } = new();

    /// <summary>
    /// Additional optional attributes on the Student XML node (e.g. HasDebt, Scholarship, Notes).
    /// </summary>
    public Dictionary<string, string> AdditionalAttributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Populates the student property or additional attribute dictionary from an XML attribute.
    /// </summary>
    public void ApplyAttribute(string name, string value)
    {
        var trimmed = value.Trim();
        switch (name)
        {
            case "Id":
                Id = trimmed;
                break;
            case "FullName":
                FullName = trimmed;
                break;
            case "Faculty":
                Faculty = trimmed;
                break;
            case "Department":
                Department = trimmed;
                break;
            case "Course":
                if (int.TryParse(trimmed, out var course))
                {
                    Course = course;
                    HasCourse = true;
                }
                break;
            case "Privilege":
                Privilege = trimmed;
                break;
            case "Phone":
                Phone = trimmed;
                break;
            default:
                AdditionalAttributes[name] = trimmed;
                break;
        }
    }

    /// <summary>
    /// Checks whether the student or nested room has the specified attribute matching the expected value.
    /// </summary>
    public bool HasAttribute(string attributeName, string expectedValue)
    {
        if (string.IsNullOrWhiteSpace(attributeName) || string.IsNullOrWhiteSpace(expectedValue))
        {
            return true;
        }

        var actualVal = GetAttributeValue(attributeName);
        return actualVal != null &&
               actualVal.Contains(expectedValue, StringComparison.OrdinalIgnoreCase);
    }

    private string? GetAttributeValue(string attributeName)
    {
        if (attributeName.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            return Id;
        }
        if (attributeName.Equals("FullName", StringComparison.OrdinalIgnoreCase))
        {
            return FullName;
        }
        if (attributeName.Equals("Faculty", StringComparison.OrdinalIgnoreCase))
        {
            return Faculty;
        }
        if (attributeName.Equals("Department", StringComparison.OrdinalIgnoreCase))
        {
            return Department;
        }
        if (attributeName.Equals("Course", StringComparison.OrdinalIgnoreCase))
        {
            return (HasCourse || Course != 0) ? Course.ToString(CultureInfo.InvariantCulture) : null;
        }
        if (attributeName.Equals("Privilege", StringComparison.OrdinalIgnoreCase))
        {
            return Privilege;
        }
        if (attributeName.Equals("Phone", StringComparison.OrdinalIgnoreCase))
        {
            return Phone;
        }
        if (attributeName.Equals("Building", StringComparison.OrdinalIgnoreCase))
        {
            return Room.Building;
        }
        if (attributeName.Equals("Floor", StringComparison.OrdinalIgnoreCase))
        {
            return (Room.HasFloor || Room.Floor != 0) ? Room.Floor.ToString(CultureInfo.InvariantCulture) : null;
        }
        if (attributeName.Equals("RoomNumber", StringComparison.OrdinalIgnoreCase))
        {
            return Room.RoomNumber;
        }
        if (attributeName.Equals("CheckInDate", StringComparison.OrdinalIgnoreCase))
        {
            return Room.CheckInDate;
        }
        if (attributeName.Equals("CheckOutDate", StringComparison.OrdinalIgnoreCase))
        {
            return Room.CheckOutDate;
        }
        if (attributeName.Equals("MonthlyFee", StringComparison.OrdinalIgnoreCase))
        {
            return (Room.HasMonthlyFee || Room.MonthlyFee != 0m) ? Room.MonthlyFee.ToString(CultureInfo.InvariantCulture) : null;
        }

        if (AdditionalAttributes.TryGetValue(attributeName, out var studentVal))
        {
            return studentVal;
        }

        if (Room.AdditionalAttributes.TryGetValue(attributeName, out var roomVal))
        {
            return roomVal;
        }

        return null;
    }

    /// <summary>
    /// Property for XAML DataBinding returning only optional/additional student details.
    /// </summary>
    public string ExtraDetails
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Privilege))
            {
                parts.Add($"Пільга: {Privilege}");
            }
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                parts.Add($"Тел: {Phone}");
            }
            if (AdditionalAttributes.Count > 0)
            {
                parts.Add("Додатково: " + string.Join(", ", AdditionalAttributes.Select(kv => $"{kv.Key}={kv.Value}")));
            }
            return string.Join(" | ", parts);
        }
    }

    /// <summary>
    /// Indicates whether the student has any optional or additional attributes to show.
    /// </summary>
    public bool HasExtraDetails => !string.IsNullOrWhiteSpace(ExtraDetails);

    /// <summary>
    /// Property for XAML DataBinding returning the full formatted student summary.
    /// </summary>
    public string DisplayBlock => FormatDisplayBlock();

    /// <summary>
    /// Formats a single-line summary of the student record.
    /// </summary>
    public string FormatDisplayBlock()
    {
        var extra = ExtraDetails;
        var baseBlock = $"[{Id}] {FullName} | {Faculty}, {Department}, {Course} курс | {Room.Building}, кімн. {Room.RoomNumber} ({Room.FormatResidencePeriod()}) | Оплата: {Room.MonthlyFee} грн";
        return string.IsNullOrWhiteSpace(extra) ? baseBlock : $"{baseBlock} | {extra}";
    }
}
