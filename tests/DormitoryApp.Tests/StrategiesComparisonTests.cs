using DormitoryApp.Core.Models;
using DormitoryApp.Core.Strategies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DormitoryApp.Tests;

[TestClass]
public class StrategiesComparisonTests
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

    private readonly IXmlSearchStrategy _sax = new SaxXmlSearchStrategy();
    private readonly IXmlSearchStrategy _dom = new DomXmlSearchStrategy();
    private readonly IXmlSearchStrategy _linq = new LinqXmlSearchStrategy();

    [TestMethod]
    public void TestXmlFileExists()
    {
        Assert.IsTrue(File.Exists(TestXmlPath), $"XML-файл не знайдено за шляхом: {TestXmlPath}");
    }

    [TestMethod]
    public void AllStrategies_ReturnAllStudents_WhenCriteriaIsEmpty()
    {
        var criteria = new FilterCriteria();

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(10, saxResults.Count, "SAX повинен повернути 10 студентів");
        Assert.AreEqual(10, domResults.Count, "DOM повинен повернути 10 студентів");
        Assert.AreEqual(10, linqResults.Count, "LINQ повинен повернути 10 студентів");

        CollectionAssert.AreEqual(
            saxResults.Select(s => s.Id).ToList(),
            domResults.Select(s => s.Id).ToList(),
            "SAX та DOM повинні повернути однакові ID"
        );

        CollectionAssert.AreEqual(
            saxResults.Select(s => s.Id).ToList(),
            linqResults.Select(s => s.Id).ToList(),
            "SAX та LINQ повинні повернути однакові ID"
        );
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByFaculty()
    {
        var criteria = new FilterCriteria { Faculty = "ФКНК" };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(3, saxResults.Count, "На ФКНК має бути 3 студенти");
        CollectionAssert.AreEqual(
            saxResults.Select(s => s.Id).OrderBy(id => id).ToList(),
            domResults.Select(s => s.Id).OrderBy(id => id).ToList()
        );
        CollectionAssert.AreEqual(
            saxResults.Select(s => s.Id).OrderBy(id => id).ToList(),
            linqResults.Select(s => s.Id).OrderBy(id => id).ToList()
        );
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByFacultyAndCourse()
    {
        var criteria = new FilterCriteria
        {
            Faculty = "ФКНК",
            Course = 2
        };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(1, saxResults.Count);
        Assert.AreEqual("ST-001", saxResults[0].Id);
        Assert.AreEqual("Остапенко Роман Ігорович", saxResults[0].FullName);

        Assert.AreEqual(saxResults[0].Id, domResults[0].Id);
        Assert.AreEqual(saxResults[0].Id, linqResults[0].Id);
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByBuildingAndRoom()
    {
        var criteria = new FilterCriteria
        {
            Building = "Корпус 1",
            RoomNumber = "412"
        };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(2, saxResults.Count, "У кімнаті 412 проживає 2 студенти");
        var expectedIds = new List<string> { "ST-001", "ST-009" };

        CollectionAssert.AreEquivalent(expectedIds, saxResults.Select(s => s.Id).ToList());
        CollectionAssert.AreEquivalent(expectedIds, domResults.Select(s => s.Id).ToList());
        CollectionAssert.AreEquivalent(expectedIds, linqResults.Select(s => s.Id).ToList());
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByPrivilege()
    {
        var criteria = new FilterCriteria { Privilege = "ВПО" };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(1, saxResults.Count);
        Assert.AreEqual("ST-001", saxResults[0].Id);
        Assert.AreEqual(saxResults[0].Id, domResults[0].Id);
        Assert.AreEqual(saxResults[0].Id, linqResults[0].Id);
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenKeywordSearchApplied()
    {
        var criteria = new FilterCriteria { FullNameKeyword = "роман" }; // Verify case-insensitivity

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(1, saxResults.Count);
        Assert.AreEqual("ST-001", saxResults[0].Id);
        Assert.AreEqual(saxResults[0].Id, domResults[0].Id);
        Assert.AreEqual(saxResults[0].Id, linqResults[0].Id);
    }

    [TestMethod]
    public void AllStrategies_ReturnEmpty_WhenCriteriaMatchesNothing()
    {
        var criteria = new FilterCriteria
        {
            Faculty = "НеіснуючийФакультет",
            Course = 99
        };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(0, saxResults.Count);
        Assert.AreEqual(0, domResults.Count);
        Assert.AreEqual(0, linqResults.Count);
    }

    [TestMethod]
    public void XmlSearchContext_CorrectlySwitchesStrategies()
    {
        var context = new XmlSearchContext(_sax);
        Assert.AreEqual("SAX (XmlReader)", context.CurrentStrategy.StrategyName);

        var criteria = new FilterCriteria { Course = 1 };
        var saxRes = context.ExecuteSearch(TestXmlPath, criteria);
        Assert.AreEqual(2, saxRes.Count);

        context.SetStrategy(_dom);
        Assert.AreEqual("DOM (XmlDocument + XPath)", context.CurrentStrategy.StrategyName);
        var domRes = context.ExecuteSearch(TestXmlPath, criteria);
        Assert.AreEqual(2, domRes.Count);

        context.SetStrategy(_linq);
        Assert.AreEqual("LINQ to XML (XDocument)", context.CurrentStrategy.StrategyName);
        var linqRes = context.ExecuteSearch(TestXmlPath, criteria);
        Assert.AreEqual(2, linqRes.Count);

        CollectionAssert.AreEqual(
            saxRes.Select(s => s.Id).ToList(),
            domRes.Select(s => s.Id).ToList()
        );
        CollectionAssert.AreEqual(
            saxRes.Select(s => s.Id).ToList(),
            linqRes.Select(s => s.Id).ToList()
        );
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenValueContainsApostrophe()
    {
        var criteria = new FilterCriteria { Department = "Комп'ютерні науки" };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.IsTrue(saxResults.Count > 0, "У базі мають бути студенти кафедри Комп'ютерні науки");
        CollectionAssert.AreEqual(
            saxResults.Select(s => s.Id).OrderBy(id => id).ToList(),
            domResults.Select(s => s.Id).OrderBy(id => id).ToList()
        );
        CollectionAssert.AreEqual(
            saxResults.Select(s => s.Id).OrderBy(id => id).ToList(),
            linqResults.Select(s => s.Id).OrderBy(id => id).ToList()
        );
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByRoomCustomAttribute()
    {
        var criteria = new FilterCriteria
        {
            CustomAttributeName = "Floor",
            CustomAttributeValue = "4"
        };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.IsTrue(linqResults.Count > 0, "На 4 поверсі мають бути студенти");
        CollectionAssert.AreEqual(
            linqResults.Select(s => s.Id).OrderBy(id => id).ToList(),
            saxResults.Select(s => s.Id).OrderBy(id => id).ToList()
        );
        CollectionAssert.AreEqual(
            linqResults.Select(s => s.Id).OrderBy(id => id).ToList(),
            domResults.Select(s => s.Id).OrderBy(id => id).ToList()
        );
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByCustomAttributeSubstringCaseInsensitive()
    {
        var criteria = new FilterCriteria
        {
            CustomAttributeName = "Notes",
            CustomAttributeValue = "староста"
        };

        var saxResults = _sax.Search(TestXmlPath, criteria);
        var domResults = _dom.Search(TestXmlPath, criteria);
        var linqResults = _linq.Search(TestXmlPath, criteria);

        Assert.AreEqual(1, linqResults.Count);
        Assert.AreEqual("ST-004", linqResults[0].Id);
        CollectionAssert.AreEqual(
            linqResults.Select(s => s.Id).ToList(),
            saxResults.Select(s => s.Id).ToList()
        );
        CollectionAssert.AreEqual(
            linqResults.Select(s => s.Id).ToList(),
            domResults.Select(s => s.Id).ToList()
        );
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenFilteredByCheckInDateAndMonthlyFee()
    {
        var checkInCriteria = new FilterCriteria
        {
            CustomAttributeName = "CheckInDate",
            CustomAttributeValue = "2025-09"
        };

        var feeCriteria = new FilterCriteria
        {
            CustomAttributeName = "MonthlyFee",
            CustomAttributeValue = "1200"
        };

        var checkInLinq = _linq.Search(TestXmlPath, checkInCriteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var checkInSax = _sax.Search(TestXmlPath, checkInCriteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var checkInDom = _dom.Search(TestXmlPath, checkInCriteria).Select(s => s.Id).OrderBy(id => id).ToList();

        Assert.IsTrue(checkInLinq.Count > 0);
        CollectionAssert.AreEqual(checkInLinq, checkInSax);
        CollectionAssert.AreEqual(checkInLinq, checkInDom);

        var feeLinq = _linq.Search(TestXmlPath, feeCriteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var feeSax = _sax.Search(TestXmlPath, feeCriteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var feeDom = _dom.Search(TestXmlPath, feeCriteria).Select(s => s.Id).OrderBy(id => id).ToList();

        Assert.IsTrue(feeLinq.Count > 0);
        CollectionAssert.AreEqual(feeLinq, feeSax);
        CollectionAssert.AreEqual(feeLinq, feeDom);
    }

    [TestMethod]
    public void DomBuildXPathQuery_HandlesMixedQuotesAndInvalidAttributeNameSafely()
    {
        var mixedQuotesCriteria = new FilterCriteria { Faculty = "a'b\"c" };
        var xpath = DomXmlSearchStrategy.BuildXPathQuery(mixedQuotesCriteria);
        Assert.IsTrue(xpath.Contains("concat('a', \"'\", 'b\"c')"), $"Unexpected XPath: {xpath}");

        var invalidAttrCriteria = new FilterCriteria
        {
            CustomAttributeName = "Invalid Attr!",
            CustomAttributeValue = "test"
        };
        var results = _dom.Search(TestXmlPath, invalidAttrCriteria);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void AllStrategies_ReturnIdenticalResults_WhenMonthlyFeeIsZero()
    {
        var criteria = new FilterCriteria
        {
            CustomAttributeName = "MonthlyFee",
            CustomAttributeValue = "0.00"
        };

        var linqIds = _linq.Search(TestXmlPath, criteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var saxIds = _sax.Search(TestXmlPath, criteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var domIds = _dom.Search(TestXmlPath, criteria).Select(s => s.Id).OrderBy(id => id).ToList();

        CollectionAssert.Contains(linqIds, "ST-006");
        CollectionAssert.AreEqual(linqIds, saxIds);
        CollectionAssert.AreEqual(linqIds, domIds);
    }

    [TestMethod]
    public void AllStrategies_AreCaseInsensitive_ForStandardFiltersAndCustomAttributeName()
    {
        var facultyCriteria = new FilterCriteria { Faculty = "фкнк" };
        var facultyLinq = _linq.Search(TestXmlPath, facultyCriteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var facultySax = _sax.Search(TestXmlPath, facultyCriteria).Select(s => s.Id).OrderBy(id => id).ToList();
        var facultyDom = _dom.Search(TestXmlPath, facultyCriteria).Select(s => s.Id).OrderBy(id => id).ToList();

        Assert.AreEqual(3, facultyLinq.Count);
        CollectionAssert.AreEqual(facultyLinq, facultySax);
        CollectionAssert.AreEqual(facultyLinq, facultyDom);

        var customNameCriteria = new FilterCriteria
        {
            CustomAttributeName = "notes",
            CustomAttributeValue = "Староста"
        };
        var customLinq = _linq.Search(TestXmlPath, customNameCriteria).Select(s => s.Id).ToList();
        var customSax = _sax.Search(TestXmlPath, customNameCriteria).Select(s => s.Id).ToList();
        var customDom = _dom.Search(TestXmlPath, customNameCriteria).Select(s => s.Id).ToList();

        Assert.AreEqual(1, customLinq.Count);
        CollectionAssert.AreEqual(customLinq, customSax);
        CollectionAssert.AreEqual(customLinq, customDom);
    }

    [TestMethod]
    public void AllStrategies_HandleWhitespacePaddedCourseAndZeroFloorConsistently()
    {
        var tempXml = Path.Combine(Path.GetTempPath(), $"edge_cases_{Guid.NewGuid():N}.xml");
        File.WriteAllText(tempXml, """
            <?xml version="1.0" encoding="utf-8"?>
            <DormitoryDatabase>
              <Student Id="ST-100" FullName="Тест Юзер" Faculty=" ФКНК " Department="Кібернетика" Course="02">
                <Room Building="Корпус 1" Floor="0" RoomNumber="10" CheckInDate="2025-09-01" MonthlyFee="0" />
              </Student>
            </DormitoryDatabase>
            """);

        try
        {
            var criteria = new FilterCriteria
            {
                Faculty = "ФКНК",
                Course = 2,
                CustomAttributeName = "Floor",
                CustomAttributeValue = "0"
            };

            var linqRes = _linq.Search(tempXml, criteria);
            var saxRes = _sax.Search(tempXml, criteria);
            var domRes = _dom.Search(tempXml, criteria);

            Assert.AreEqual(1, linqRes.Count);
            Assert.AreEqual(1, saxRes.Count);
            Assert.AreEqual(1, domRes.Count);
            Assert.AreEqual("2025-09-01 - дотепер", linqRes[0].Room.FormatResidencePeriod());
        }
        finally
        {
            if (File.Exists(tempXml))
            {
                File.Delete(tempXml);
            }
        }
    }
}
