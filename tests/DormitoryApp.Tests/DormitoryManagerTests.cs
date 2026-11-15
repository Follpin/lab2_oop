using DormitoryApp.Core.Models;
using DormitoryApp.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DormitoryApp.Tests;

[TestClass]
public class DormitoryManagerTests
{
    private static string TestXmlPath => FindFile("dormitory.xml");
    private static string TestXslPath => FindFile("dormitory.xsl");

    private static string FindFile(string filename)
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            var candidate = Path.Combine(dir, "data", filename);
            if (File.Exists(candidate))
                return candidate;
            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }
        throw new FileNotFoundException($"Cannot find data/{filename} in parent directories.");
    }

    private readonly DormitoryManager _manager = new();

    [TestMethod]
    public void LoadAndExtractAttributes_ReturnsNonEmptyDictionary()
    {
        var attrs = _manager.LoadAndExtractAttributes(TestXmlPath);

        Assert.IsNotNull(attrs);
        Assert.IsTrue(attrs.Count >= 5);
        Assert.IsTrue(attrs.ContainsKey("Faculty"));
    }

    [TestMethod]
    public void SearchStudents_WorksWithAllThreeStrategyTypes()
    {
        var criteria = new FilterCriteria { Course = 2 };

        var saxResults = _manager.SearchStudents(TestXmlPath, criteria, SearchStrategyType.Sax);
        var domResults = _manager.SearchStudents(TestXmlPath, criteria, SearchStrategyType.Dom);
        var linqResults = _manager.SearchStudents(TestXmlPath, criteria, SearchStrategyType.Linq);

        Assert.IsTrue(saxResults.Count > 0);
        Assert.AreEqual(saxResults.Count, domResults.Count);
        Assert.AreEqual(saxResults.Count, linqResults.Count);
    }

    [TestMethod]
    public void TransformXmlToHtml_SuccessfullyCreatesHtmlReport()
    {
        var tempHtml = Path.Combine(Path.GetTempPath(), $"manager_report_{Guid.NewGuid():N}.html");

        try
        {
            var resultPath = _manager.TransformXmlToHtml(TestXmlPath, TestXslPath, tempHtml);

            Assert.AreEqual(tempHtml, resultPath);
            Assert.IsTrue(File.Exists(resultPath));
        }
        finally
        {
            if (File.Exists(tempHtml))
                File.Delete(tempHtml);
        }
    }

    [TestMethod]
    public void CreateStrategy_ReturnsCorrectStrategyInstance()
    {
        var sax = _manager.CreateStrategy(SearchStrategyType.Sax);
        var dom = _manager.CreateStrategy(SearchStrategyType.Dom);
        var linq = _manager.CreateStrategy(SearchStrategyType.Linq);

        Assert.IsInstanceOfType(sax, typeof(DormitoryApp.Core.Strategies.SaxXmlSearchStrategy));
        Assert.IsInstanceOfType(dom, typeof(DormitoryApp.Core.Strategies.DomXmlSearchStrategy));
        Assert.IsInstanceOfType(linq, typeof(DormitoryApp.Core.Strategies.LinqXmlSearchStrategy));
    }
}
