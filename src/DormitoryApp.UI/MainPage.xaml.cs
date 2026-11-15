using DormitoryApp.Core.Models;
using DormitoryApp.Core.Services;

namespace DormitoryApp.UI;

/// <summary>
/// Main window view (UI Layer).
/// Handles user input and rendering only; delegates all XML/XSLT work to DormitoryManager.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly DormitoryManager _dormitoryManager;
    private string? _currentXmlPath;
    private string? _currentXslPath;

    public MainPage()
    {
        InitializeComponent();
        _dormitoryManager = new DormitoryManager();

        InitializeStrategyPicker();
        InitializeDefaultXslPath();
    }

    private void InitializeStrategyPicker()
    {
        StrategyPicker.Items.Clear();
        StrategyPicker.Items.Add("LINQ to XML (XDocument)");
        StrategyPicker.Items.Add("DOM API (XmlDocument + XPath)");
        StrategyPicker.Items.Add("SAX API (XmlReader)");
        StrategyPicker.SelectedIndex = 0; // Default to LINQ
    }

    private void InitializeDefaultXslPath()
    {
        // Locate default stylesheet data/dormitory.xsl
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            var candidate = Path.Combine(dir, "data", "dormitory.xsl");
            if (File.Exists(candidate))
            {
                _currentXslPath = candidate;
                XslFileLabel.Text = $"XSL: {Path.GetFileName(candidate)}";
                return;
            }
            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }
    }

    #region UI Event Handlers

    private async void OnLoadXmlClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Оберіть XML-файл бази даних гуртожитку",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xml" } }
                })
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                _currentXmlPath = result.FullPath;
                XmlFileLabel.Text = $"XML: {Path.GetFileName(_currentXmlPath)}";
                XmlFileLabel.TextColor = Color.FromArgb("#1E3C72");

                // Dynamically extract all attributes via controller
                var attributes = _dormitoryManager.LoadAndExtractAttributes(_currentXmlPath);
                PopulateAttributePickers(attributes);
                ClearSearchResults("Файл завантажено. Налаштуйте фільтри та натисніть «Пошук».");

                await DisplayAlert("Успіх", $"XML-файл успішно завантажено!\nВиявлено атрибутів: {attributes.Count}", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка завантаження XML", ex.Message, "ОК");
        }
    }

    private async void OnLoadXslClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Оберіть XSLT-файл таблиці стилів",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xsl", ".xslt" } }
                })
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                _currentXslPath = result.FullPath;
                XslFileLabel.Text = $"XSL: {Path.GetFileName(_currentXslPath)}";
                await DisplayAlert("Успіх", "XSL-шаблон успішно обрано!", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка завантаження XSL", ex.Message, "ОК");
        }
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentXmlPath) || !File.Exists(_currentXmlPath))
        {
            await DisplayAlert("Увага", "Будь ласка, спочатку завантажте XML-файл.", "ОК");
            return;
        }

        if (CustomAttributePicker.SelectedIndex > 0 && string.IsNullOrWhiteSpace(CustomAttributeValueEntry.Text))
        {
            await DisplayAlert("Увага", "Введіть значення для обраного додаткового атрибута або оберіть «Не обрано».", "ОК");
            return;
        }

        if (CustomAttributePicker.SelectedIndex <= 0 && !string.IsNullOrWhiteSpace(CustomAttributeValueEntry.Text))
        {
            await DisplayAlert("Увага", "Оберіть назву додаткового атрибута зі списку або очистіть поле значення.", "ОК");
            return;
        }

        try
        {
            var criteria = BuildFilterCriteria();
            var strategyType = GetSelectedStrategyType();

            // Delegate search execution to facade controller
            var results = _dormitoryManager.SearchStudents(_currentXmlPath, criteria, strategyType);

            DisplayResults(results);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка під час пошуку", ex.Message, "ОК");
        }
    }

    private async void OnTransformToHtmlClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentXmlPath) || !File.Exists(_currentXmlPath))
        {
            await DisplayAlert("Увага", "Будь ласка, спочатку завантажте XML-файл.", "ОК");
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentXslPath) || !File.Exists(_currentXslPath))
        {
            await DisplayAlert("Увага", "XSL-шаблон не знайдено. Оберіть файл стилів вручну.", "ОК");
            return;
        }

        try
        {
            var outputFolder = Path.GetDirectoryName(_currentXmlPath) ?? AppContext.BaseDirectory;
            var outputHtmlPath = Path.Combine(outputFolder, "dormitory_report.html");

            _dormitoryManager.TransformXmlToHtml(_currentXmlPath, _currentXslPath, outputHtmlPath);

            var open = await DisplayAlert("Трансформація успішна",
                $"HTML-звіт збережено:\n{outputHtmlPath}\n\nБажаєте відкрити його у браузері?",
                "Так, відкрити", "Ні");

            if (open)
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(outputHtmlPath)
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка XSLT-трансформації", ex.Message, "ОК");
        }
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        var message = string.IsNullOrWhiteSpace(_currentXmlPath)
            ? "Оберіть XML-файл бази даних для початку роботи."
            : "Параметри та результати пошуку очищено. Налаштуйте нові фільтри.";
        ClearSearchResults(message);
    }

    private void ClearSearchResults(string emptyMessage)
    {
        ResetFilterControls();
        StudentsCollectionView.ItemsSource = null;
        StudentsCollectionView.IsVisible = false;
        EmptyStateView.IsVisible = true;
        EmptyStateMessageLabel.Text = emptyMessage;
        ResultsCountBadge.Text = "Знайдено: 0";
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        const string helpText =
            "Лабораторна робота №2 з ООП\n" +
            "Тема: \"РОБОТА З ФАЙЛАМИ XML\"\n\n" +
            "Виконав: Остапенко Роман, група К-24\n" +
            "Варіант 3: \"Гуртожиток\"\n\n" +
            "Програма дозволяє завантажити XML-файл бази даних, обрати спосіб парсингу (LINQ, DOM або SAX), виконати пошук за параметрами та зберегти HTML-звіт через XSL-шаблон.";

        await DisplayAlert("Про програму", helpText, "ОК");
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Підтвердження виходу",
            "Чи дійсно ви хочете завершити роботу з програмою?",
            "Так",
            "Ні"
        );

        if (confirm)
        {
            Application.Current?.Quit();
        }
    }

    #endregion

    #region UI Helpers

    private void PopulateAttributePickers(Dictionary<string, List<string>> attributes)
    {
        PopulateSinglePicker(FacultyPicker, attributes, "Faculty", "Усі факультети");
        PopulateSinglePicker(DepartmentPicker, attributes, "Department", "Усі кафедри");
        PopulateSinglePicker(CoursePicker, attributes, "Course", "Усі курси");
        PopulateSinglePicker(PrivilegePicker, attributes, "Privilege", "Усі пільги");
        PopulateSinglePicker(BuildingPicker, attributes, "Building", "Усі корпуси");
        PopulateSinglePicker(RoomNumberPicker, attributes, "RoomNumber", "Усі кімнати");

        // Populate custom attribute selector
        CustomAttributePicker.Items.Clear();
        CustomAttributePicker.Items.Add("Не обрано");
        foreach (var key in attributes.Keys.OrderBy(k => k))
        {
            CustomAttributePicker.Items.Add(key);
        }
        CustomAttributePicker.SelectedIndex = 0;
    }

    private static void PopulateSinglePicker(
        Picker picker,
        Dictionary<string, List<string>> attributes,
        string key,
        string defaultLabel)
    {
        picker.Items.Clear();
        picker.Items.Add(defaultLabel);

        if (attributes.TryGetValue(key, out var values))
        {
            foreach (var val in values)
            {
                picker.Items.Add(val);
            }
        }

        picker.SelectedIndex = 0;
    }

    private FilterCriteria BuildFilterCriteria()
    {
        var criteria = new FilterCriteria();

        if (!string.IsNullOrWhiteSpace(KeywordEntry.Text))
            criteria.FullNameKeyword = KeywordEntry.Text.Trim();

        if (FacultyPicker.SelectedIndex > 0)
            criteria.Faculty = FacultyPicker.SelectedItem?.ToString();

        if (DepartmentPicker.SelectedIndex > 0)
            criteria.Department = DepartmentPicker.SelectedItem?.ToString();

        if (CoursePicker.SelectedIndex > 0 && int.TryParse(CoursePicker.SelectedItem?.ToString(), out var course))
            criteria.Course = course;

        if (PrivilegePicker.SelectedIndex > 0)
            criteria.Privilege = PrivilegePicker.SelectedItem?.ToString();

        if (BuildingPicker.SelectedIndex > 0)
            criteria.Building = BuildingPicker.SelectedItem?.ToString();

        if (RoomNumberPicker.SelectedIndex > 0)
            criteria.RoomNumber = RoomNumberPicker.SelectedItem?.ToString();

        if (CustomAttributePicker.SelectedIndex > 0 && !string.IsNullOrWhiteSpace(CustomAttributeValueEntry.Text))
        {
            criteria.CustomAttributeName = CustomAttributePicker.SelectedItem?.ToString();
            criteria.CustomAttributeValue = CustomAttributeValueEntry.Text.Trim();
        }

        return criteria;
    }

    private SearchStrategyType GetSelectedStrategyType()
    {
        return StrategyPicker.SelectedIndex switch
        {
            1 => SearchStrategyType.Dom,
            2 => SearchStrategyType.Sax,
            _ => SearchStrategyType.Linq
        };
    }

    private void DisplayResults(IReadOnlyList<DormitoryStudent> students)
    {
        ResultsCountBadge.Text = $"Знайдено: {students.Count}";

        if (students.Count == 0)
        {
            StudentsCollectionView.ItemsSource = null;
            StudentsCollectionView.IsVisible = false;
            EmptyStateView.IsVisible = true;
            EmptyStateMessageLabel.Text = "За вказаними критеріями студентів не знайдено.";
        }
        else
        {
            EmptyStateView.IsVisible = false;
            StudentsCollectionView.ItemsSource = students;
            StudentsCollectionView.IsVisible = true;
        }
    }

    private void ResetFilterControls()
    {
        KeywordEntry.Text = string.Empty;
        if (FacultyPicker.Items.Count > 0) FacultyPicker.SelectedIndex = 0;
        if (DepartmentPicker.Items.Count > 0) DepartmentPicker.SelectedIndex = 0;
        if (CoursePicker.Items.Count > 0) CoursePicker.SelectedIndex = 0;
        if (PrivilegePicker.Items.Count > 0) PrivilegePicker.SelectedIndex = 0;
        if (BuildingPicker.Items.Count > 0) BuildingPicker.SelectedIndex = 0;
        if (RoomNumberPicker.Items.Count > 0) RoomNumberPicker.SelectedIndex = 0;
        if (CustomAttributePicker.Items.Count > 0) CustomAttributePicker.SelectedIndex = 0;
        CustomAttributeValueEntry.Text = string.Empty;
    }

    #endregion
}
