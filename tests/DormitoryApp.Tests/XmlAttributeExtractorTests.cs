using DormitoryApp.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DormitoryApp.Tests;

[TestClass]
public class XmlAttributeExtractorTests
{
    private static string TestXmlPath
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                var candidate = Path.Combine(dir, "data", "dormitory.xml");
                if (File.Exists(candidate))
                    return candidate;
                var parent = Directory.GetParent(dir);
                dir = parent?.FullName;
            }
            throw new FileNotFoundException("Cannot find data/dormitory.xml in parent directories.");
        }
    }

    private readonly XmlAttributeExtractor _extractor = new();

    [TestMethod]
    public void ExtractUniqueAttributeValues_ReturnsExpectedAttributes()
    {
        var dict = _extractor.ExtractUniqueAttributeValues(TestXmlPath);

        Assert.IsNotNull(dict);
        Assert.IsTrue(dict.ContainsKey("Faculty"), "Повинен містити Faculty");
        Assert.IsTrue(dict.ContainsKey("Course"), "Повинен містити Course");
        Assert.IsTrue(dict.ContainsKey("Department"), "Повинен містити Department");
        Assert.IsTrue(dict.ContainsKey("Building"), "Повинен містити Building");
        Assert.IsTrue(dict.ContainsKey("Privilege"), "Повинен містити варіативний атрибут Privilege");
        Assert.IsTrue(dict.ContainsKey("Phone"), "Повинен містити варіативний атрибут Phone");
        Assert.IsTrue(dict.ContainsKey("HasDebt"), "Повинен містити варіативний атрибут HasDebt");
        Assert.IsTrue(dict.ContainsKey("Notes"), "Повинен містити варіативний атрибут Notes");

        // Verify extracted values
        var faculties = dict["Faculty"];
        Assert.IsTrue(faculties.Contains("ФКНК"));
        Assert.IsTrue(faculties.Contains("ФІТ"));
        Assert.IsTrue(faculties.Contains("ФРЕКС"));
        Assert.IsTrue(faculties.Contains("МехМат"));

        var privileges = dict["Privilege"];
        Assert.IsTrue(privileges.Contains("ВПО"));
        Assert.IsTrue(privileges.Contains("УБД"));
        Assert.IsTrue(privileges.Contains("Сирота"));
        Assert.IsTrue(privileges.Contains("Чорнобилець"));
    }

    [TestMethod]
    public void GetAvailableStudentAttributes_IncludesMandatoryAndOptionalAttributes()
    {
        var attrs = _extractor.GetAvailableStudentAttributes(TestXmlPath);

        CollectionAssert.Contains(attrs.ToList(), "Id");
        CollectionAssert.Contains(attrs.ToList(), "FullName");
        CollectionAssert.Contains(attrs.ToList(), "Faculty");
        CollectionAssert.Contains(attrs.ToList(), "Department");
        CollectionAssert.Contains(attrs.ToList(), "Course");
        CollectionAssert.Contains(attrs.ToList(), "Privilege");
        CollectionAssert.Contains(attrs.ToList(), "Notes");
    }
}
