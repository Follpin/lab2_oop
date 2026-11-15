using DormitoryApp.Core.Models;
using DormitoryApp.Core.Strategies;

namespace DormitoryApp.Core.Services;

/// <summary>
/// Application facade controller coordinating UI requests with search strategies,
/// attribute extraction, and XSLT transformation services.
/// </summary>
public class DormitoryManager
{
    private readonly XmlSearchContext _searchContext;
    private readonly XmlAttributeExtractor _attributeExtractor;
    private readonly XsltTransformService _xsltService;

    /// <summary>
    /// Initializes a new controller instance using LINQ to XML as the default strategy.
    /// </summary>
    public DormitoryManager()
    {
        _attributeExtractor = new XmlAttributeExtractor();
        _xsltService = new XsltTransformService();
        _searchContext = new XmlSearchContext(new LinqXmlSearchStrategy());
    }

    /// <summary>
    /// Constructor with explicit dependency injection for testing and extensibility.
    /// </summary>
    public DormitoryManager(
        XmlSearchContext searchContext,
        XmlAttributeExtractor attributeExtractor,
        XsltTransformService xsltService)
    {
        _searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
        _attributeExtractor = attributeExtractor ?? throw new ArgumentNullException(nameof(attributeExtractor));
        _xsltService = xsltService ?? throw new ArgumentNullException(nameof(xsltService));
    }

    /// <summary>
    /// Loads the XML file and extracts all unique attribute values for UI filter pickers.
    /// </summary>
    public Dictionary<string, List<string>> LoadAndExtractAttributes(string xmlPath)
    {
        return _attributeExtractor.ExtractUniqueAttributeValues(xmlPath);
    }

    /// <summary>
    /// Searches students using the specified criteria and parsing strategy.
    /// </summary>
    public IReadOnlyList<DormitoryStudent> SearchStudents(
        string xmlPath,
        FilterCriteria criteria,
        SearchStrategyType strategyType)
    {
        var strategy = CreateStrategy(strategyType);
        _searchContext.SetStrategy(strategy);
        return _searchContext.ExecuteSearch(xmlPath, criteria);
    }

    /// <summary>
    /// Transforms the XML file to an HTML report using the given XSLT stylesheet.
    /// </summary>
    public string TransformXmlToHtml(string xmlPath, string xslPath, string outputHtmlPath)
    {
        _xsltService.Transform(xmlPath, xslPath, outputHtmlPath);
        return outputHtmlPath;
    }

    /// <summary>
    /// Factory method creating a concrete XML search strategy for the requested type.
    /// </summary>
    public IXmlSearchStrategy CreateStrategy(SearchStrategyType type)
    {
        return type switch
        {
            SearchStrategyType.Sax => new SaxXmlSearchStrategy(),
            SearchStrategyType.Dom => new DomXmlSearchStrategy(),
            SearchStrategyType.Linq => new LinqXmlSearchStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Непідтримуваний тип стратегії: {type}")
        };
    }
}
