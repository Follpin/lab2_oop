using System.Xml.Xsl;

namespace DormitoryApp.Core.Services;

/// <summary>
/// Service for transforming the XML database into an HTML report using XSLT.
/// </summary>
public class XsltTransformService
{
    /// <summary>
    /// Transforms the input XML file to HTML using the specified XSLT stylesheet.
    /// </summary>
    public void Transform(string xmlPath, string xslPath, string outputHtmlPath)
    {
        ValidateFiles(xmlPath, xslPath);

        if (string.IsNullOrWhiteSpace(outputHtmlPath))
        {
            throw new ArgumentException("Шлях до вихідного HTML-файлу не може бути порожнім.", nameof(outputHtmlPath));
        }

        var outputDir = Path.GetDirectoryName(outputHtmlPath);
        if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var transform = new XslCompiledTransform();
        transform.Load(xslPath, XsltSettings.Default, null);

        transform.Transform(xmlPath, outputHtmlPath);
    }

    /// <summary>
    /// Validates that both input XML and XSL files exist.
    /// </summary>
    public void ValidateFiles(string xmlPath, string xslPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"Вхідний XML-файл не знайдено: {xmlPath}");
        }

        if (string.IsNullOrWhiteSpace(xslPath) || !File.Exists(xslPath))
        {
            throw new FileNotFoundException($"XSLT-файл таблиці стилів не знайдено: {xslPath}");
        }
    }
}
