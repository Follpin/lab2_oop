using System.Globalization;

namespace DormitoryApp.Core.Models;

/// <summary>
/// Dormitory room and residence details for a student (XML hierarchy level 2).
/// </summary>
public class RoomInfo
{
    public string Building { get; set; } = string.Empty;
    public int Floor { get; set; }
    public bool HasFloor { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string CheckInDate { get; set; } = string.Empty;
    public string? CheckOutDate { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool HasMonthlyFee { get; set; }

    /// <summary>
    /// Additional optional attributes on the Room XML node.
    /// </summary>
    public Dictionary<string, string> AdditionalAttributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Property for XAML DataBinding returning the formatted residence period.
    /// </summary>
    public string ResidencePeriod => FormatResidencePeriod();

    /// <summary>
    /// Populates the room property or additional attribute dictionary from an XML attribute.
    /// </summary>
    public void ApplyAttribute(string name, string value)
    {
        var trimmed = value.Trim();
        switch (name)
        {
            case "Building":
                Building = trimmed;
                break;
            case "Floor":
                if (int.TryParse(trimmed, out var floor))
                {
                    Floor = floor;
                    HasFloor = true;
                }
                break;
            case "RoomNumber":
                RoomNumber = trimmed;
                break;
            case "CheckInDate":
                CheckInDate = trimmed;
                break;
            case "CheckOutDate":
                CheckOutDate = trimmed;
                break;
            case "MonthlyFee":
                if (decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var fee))
                {
                    MonthlyFee = fee;
                    HasMonthlyFee = true;
                }
                break;
            default:
                AdditionalAttributes[name] = trimmed;
                break;
        }
    }

    /// <summary>
    /// Returns a formatted residence period string.
    /// </summary>
    public string FormatResidencePeriod()
    {
        if (string.IsNullOrWhiteSpace(CheckInDate))
        {
            return "Не вказано";
        }

        return string.IsNullOrWhiteSpace(CheckOutDate)
            ? $"{CheckInDate} - дотепер"
            : $"{CheckInDate} - {CheckOutDate}";
    }
}
