namespace DormitoryApp.Core.Models;

/// <summary>
/// Supported XML parsing and search strategies.
/// </summary>
public enum SearchStrategyType
{
    /// <summary>
    /// Sequential streaming parsing (SAX API via XmlReader).
    /// </summary>
    Sax,

    /// <summary>
    /// Tree-based object parsing with XPath queries (DOM API via XmlDocument).
    /// </summary>
    Dom,

    /// <summary>
    /// Declarative functional parsing via LINQ to XML (XDocument).
    /// </summary>
    Linq
}
