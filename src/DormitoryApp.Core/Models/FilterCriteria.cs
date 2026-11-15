namespace DormitoryApp.Core.Models;

/// <summary>
/// Query object holding search and filter parameters for querying the student database.
/// </summary>
public class FilterCriteria
{
    public string? FullNameKeyword { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public int? Course { get; set; }
    public string? Building { get; set; }
    public string? RoomNumber { get; set; }
    public string? Privilege { get; set; }
    public string? CustomAttributeName { get; set; }
    public string? CustomAttributeValue { get; set; }

    /// <summary>
    /// Returns true if no filter criteria are specified.
    /// </summary>
    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(FullNameKeyword) &&
               string.IsNullOrWhiteSpace(Faculty) &&
               string.IsNullOrWhiteSpace(Department) &&
               !Course.HasValue &&
               string.IsNullOrWhiteSpace(Building) &&
               string.IsNullOrWhiteSpace(RoomNumber) &&
               string.IsNullOrWhiteSpace(Privilege) &&
               string.IsNullOrWhiteSpace(CustomAttributeName) &&
               string.IsNullOrWhiteSpace(CustomAttributeValue);
    }

    /// <summary>
    /// Resets all filter criteria to their default empty state.
    /// </summary>
    public void Clear()
    {
        FullNameKeyword = null;
        Faculty = null;
        Department = null;
        Course = null;
        Building = null;
        RoomNumber = null;
        Privilege = null;
        CustomAttributeName = null;
        CustomAttributeValue = null;
    }
}
