using DormitoryApp.Core.Models;

namespace DormitoryApp.Core.Strategies;

/// <summary>
/// Strategy pattern context for executing XML search queries.
/// </summary>
public class XmlSearchContext
{
    private IXmlSearchStrategy _strategy;

    /// <summary>
    /// Gets the currently active search strategy.
    /// </summary>
    public IXmlSearchStrategy CurrentStrategy => _strategy;

    /// <summary>
    /// Initializes a new context instance with the given strategy.
    /// </summary>
    public XmlSearchContext(IXmlSearchStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Switches the active search strategy at runtime.
    /// </summary>
    public void SetStrategy(IXmlSearchStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Delegates search execution to the currently selected strategy.
    /// </summary>
    public IReadOnlyList<DormitoryStudent> ExecuteSearch(string xmlPath, FilterCriteria criteria)
    {
        return _strategy.Search(xmlPath, criteria);
    }
}
