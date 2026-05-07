using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;

namespace PasswordGenerator;

public partial class MainWindow : Window
{
    private readonly string _settingsPath;

    public MainWindow()
    {
        InitializeComponent();

        string appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PasswordGenerator");

        Directory.CreateDirectory(appDir);
        _settingsPath = Path.Combine(appDir, "settings.json");

        LoadSettings();
        PasswordBox.Text = "";
    }
    private void About_Click(object sender, RoutedEventArgs e)
    {
        AboutWindow win = new();
        win.Owner = this;
        win.ShowDialog();
    }
    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        GeneratePassword();
        Clipboard.SetText(PasswordBox.Text);
        SaveSettings();
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(PasswordBox.Text))
            Clipboard.SetText(PasswordBox.Text);

        SaveSettings();
    }

    private void GeneratePassword()
    {
        if (!int.TryParse(LengthBox.Text, out int length))
            length = 29;

        if (length < 4)
            length = 4;

        if (length > 512)
            length = 512;

        LengthBox.Text = length.ToString();

        string upper = UpperBox.IsChecked == true ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
        string lower = LowerBox.IsChecked == true ? "abcdefghijklmnopqrstuvwxyz" : "";
        string digits = DigitBox.IsChecked == true ? "0123456789" : "";
        string symbols = SymbolBox.IsChecked == true ? SymbolTextBox.Text : "";

        string allChars = upper + lower + digits + symbols;

        if (ExcludeBox.IsChecked == true)
        {
            foreach (char c in ExcludeTextBox.Text)
                allChars = allChars.Replace(c.ToString(), "");
        }

        if (allChars.Length == 0)
        {
            MessageBox.Show("Select at least one character group.", "Password Generator");
            return;
        }

        string letterChars = upper + lower;

        if (ExcludeBox.IsChecked == true)
        {
            foreach (char c in ExcludeTextBox.Text)
                letterChars = letterChars.Replace(c.ToString(), "");
        }

        letterChars = new string(letterChars.Distinct().ToArray());

        bool protectEnds = NoNumberOrSymbolEndsBox.IsChecked == true;

        if (protectEnds && letterChars.Length == 0)
        {
            MessageBox.Show("To prevent numbers or symbols at the start/end, you must allow at least one letter.", "Password Generator");
            return;
        }

        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            string source = allChars;

            if (protectEnds && (i == 0 || i == length - 1))
                source = letterChars;

            int index = RandomNumberGenerator.GetInt32(source.Length);
            result[i] = source[index];
        }

        PasswordBox.Text = new string(result);
    }

    private void LoadSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            LengthBox.Text = "29";
            UpperBox.IsChecked = true;
            LowerBox.IsChecked = true;
            DigitBox.IsChecked = true;
            SymbolBox.IsChecked = true;
            SymbolTextBox.Text = "!@#&";
            ExcludeBox.IsChecked = true;
            ExcludeTextBox.Text = "0oOiIlL1";
            NoNumberOrSymbolEndsBox.IsChecked = false;

            return;
        }

        try
        {
            string json = File.ReadAllText(_settingsPath);
            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (settings == null)
                return;

            LengthBox.Text = settings.Length.ToString();
            UpperBox.IsChecked = settings.UseUpper;
            LowerBox.IsChecked = settings.UseLower;
            DigitBox.IsChecked = settings.UseDigits;
            SymbolBox.IsChecked = settings.UseSymbols;
            SymbolTextBox.Text = settings.Symbols;
            ExcludeBox.IsChecked = settings.UseExclude;
            ExcludeTextBox.Text = settings.Exclude;
            NoNumberOrSymbolEndsBox.IsChecked = settings.NoNumberOrSymbolEnds;
        }
        catch
        {
            LengthBox.Text = "29";
        }
    }

    private void SaveSettings()
    {
        if (!int.TryParse(LengthBox.Text, out int length))
            length = 29;

        AppSettings settings = new()
        {
            Length = length,
            UseUpper = UpperBox.IsChecked == true,
            UseLower = LowerBox.IsChecked == true,
            UseDigits = DigitBox.IsChecked == true,
            UseSymbols = SymbolBox.IsChecked == true,
            Symbols = SymbolTextBox.Text,
            UseExclude = ExcludeBox.IsChecked == true,
            Exclude = ExcludeTextBox.Text,
            NoNumberOrSymbolEnds = NoNumberOrSymbolEndsBox.IsChecked == true
        };

        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsPath, json);
    }
    private void LengthUp_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(LengthBox.Text, out int val))
            val = 29;

        val++;
        if (val > 512) val = 512;

        LengthBox.Text = val.ToString();
    }

    private void LengthDown_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(LengthBox.Text, out int val))
            val = 29;

        val--;
        if (val < 4) val = 4;

        LengthBox.Text = val.ToString();
    }
}

public class AppSettings
{
    public int Length { get; set; } = 29;
    public bool UseUpper { get; set; } = true;
    public bool UseLower { get; set; } = true;
    public bool UseDigits { get; set; } = true;
    public bool UseSymbols { get; set; } = true;
    public string Symbols { get; set; } = "!@#&";
    public bool UseExclude { get; set; } = true;
    public string Exclude { get; set; } = "OoOiLl1";
    public bool NoNumberOrSymbolEnds { get; set; } = false;
}