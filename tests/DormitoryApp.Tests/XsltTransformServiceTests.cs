using DormitoryApp.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DormitoryApp.Tests;

[TestClass]
public class XsltTransformServiceTests
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

    private readonly XsltTransformService _service = new();

    [TestMethod]
    public void Transform_GeneratesValidHtmlFile()
    {
        var tempHtml = Path.Combine(Path.GetTempPath(), $"test_report_{Guid.NewGuid():N}.html");

        try
        {
            _service.Transform(TestXmlPath, TestXslPath, tempHtml);

            Assert.IsTrue(File.Exists(tempHtml), "HTML файл має бути створено");
            var content = File.ReadAllText(tempHtml);

            Assert.IsTrue(content.Contains("<html"), "Файл повинен містити тег html");
            Assert.IsTrue(content.Contains("Остапенко Роман"), "Файл повинен містити дані студента");
            Assert.IsTrue(content.Contains("Всього студентів"), "Файл повинен містити блок статистики");
        }
        finally
        {
            if (File.Exists(tempHtml))
            {
                File.Delete(tempHtml);
            }
        }
    }

    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void Transform_ThrowsFileNotFound_WhenXmlMissing()
    {
        _service.Transform("non_existent_file.xml", TestXslPath, "out.html");
    }
}
