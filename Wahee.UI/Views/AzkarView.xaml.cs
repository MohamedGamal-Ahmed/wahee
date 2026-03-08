using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wahee.UI.Views
{
    public partial class AzkarView : UserControl
    {
        private List<Zikr> _currentAzkar = new();
        private int _currentIndex = 0;
        private int _currentCount = 0;
        private string _currentType = "morning";

        public AzkarView()
        {
            InitializeComponent();
            Loaded += AzkarView_Loaded;
        }

        private async void AzkarView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAzkarAsync("morning");
        }

        private async Task LoadAzkarAsync(string type)
        {
            try
            {
                LoadingBar.Visibility = Visibility.Visible;
                _currentType = type;

                // Load from embedded resource or file
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data", "azkar_data.json");
                
                if (File.Exists(jsonPath))
                {
                    string json = await File.ReadAllTextAsync(jsonPath);
                    var data = JsonSerializer.Deserialize<AzkarData>(json, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    _currentAzkar = type == "morning" ? data?.Morning ?? new() : data?.Evening ?? new();
                }
                else
                {
                    // Fallback data if file not found
                    _currentAzkar = new List<Zikr>
                    {
                        new Zikr { Id = 1, Text = "سُبْحَانَ اللهِ وَبِحَمْدِهِ", Count = 33, Reference = "مسلم" },
                        new Zikr { Id = 2, Text = "الْحَمْدُ لِلَّهِ", Count = 33, Reference = "مسلم" },
                        new Zikr { Id = 3, Text = "اللهُ أَكْبَرُ", Count = 34, Reference = "مسلم" }
                    };
                }

                _currentIndex = 0;
                _currentCount = 0;
                DisplayCurrentZikr();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading azkar: {ex.Message}");
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void DisplayCurrentZikr()
        {
            if (_currentAzkar.Count == 0) return;

            var zikr = _currentAzkar[_currentIndex];
            ZikrTextBlock.Text = zikr.Text;
            CounterTxt.Text = $"{_currentCount} / {zikr.Count}";
            ReferenceTxt.Text = zikr.Reference;

            // Update progress
            double progress = (double)(_currentIndex + 1) / _currentAzkar.Count * 100;
            ProgressBar.Value = progress;
            ProgressTxt.Text = $"{_currentIndex + 1} من {_currentAzkar.Count}";
        }

        private void ZikrCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_currentAzkar.Count == 0) return;

            var zikr = _currentAzkar[_currentIndex];
            _currentCount++;

            if (_currentCount >= zikr.Count)
            {
                // Move to next zikr
                if (_currentIndex < _currentAzkar.Count - 1)
                {
                    _currentIndex++;
                    _currentCount = 0;
                }
                else
                {
                    // Completed all azkar
                    MessageBox.Show("تقبل الله منك! أتممت جميع الأذكار 🤲", "وحي - الأذكار", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _currentIndex = 0;
                    _currentCount = 0;
                }
            }

            DisplayCurrentZikr();
        }

        private void TypeTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton tab)
            {
                string type = tab.Name == "MorningTab" ? "morning" : "evening";
                _ = LoadAzkarAsync(type);
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                _currentCount = 0;
                DisplayCurrentZikr();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _currentAzkar.Count - 1)
            {
                _currentIndex++;
                _currentCount = 0;
                DisplayCurrentZikr();
            }
        }

        // Data models
        private class AzkarData
        {
            public List<Zikr> Morning { get; set; } = new();
            public List<Zikr> Evening { get; set; } = new();
        }

        private class Zikr
        {
            public int Id { get; set; }
            public string Text { get; set; } = string.Empty;
            public int Count { get; set; }
            public string Reference { get; set; } = string.Empty;
        }
    }
}
