using DormitoryApp.Core.Models;

namespace DormitoryApp.Core.Strategies;

/// <summary>
/// Strategy pattern interface for querying the XML database.
/// </summary>
public interface IXmlSearchStrategy
{
    /// <summary>
    /// Display name of the XML search strategy.
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// Searches students in the XML file matching the given filter criteria.
    /// </summary>
    IReadOnlyList<DormitoryStudent> Search(string xmlPath, FilterCriteria criteria);
}
