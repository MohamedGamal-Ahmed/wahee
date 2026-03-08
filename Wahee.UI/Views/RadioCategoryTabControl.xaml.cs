using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Wahee.UI.Views
{
    public partial class RadioCategoryTabControl : UserControl
    {
        private readonly IQuranDataService? _quranDataService;
        private readonly IFavoritesService? _favoritesService;
        private readonly System.Windows.Media.MediaPlayer _mediaPlayer = new();
        private List<Radio> _allRadios = new();
        private HashSet<int> _favoriteIds = new();
        private Radio? _currentPlayingRadio;
        private string _currentCategory = "Favorites";

        // Category keywords for filtering
        private static readonly Dictionary<string, string[]> CategoryKeywords = new()
        {
            { "Quran", new[] { "قرآن", "quran", "كريم", "تلاوة", "مصحف", "ترتيل", "تجويد" } },
            { "Tafsir", new[] { "تفسير", "شرح", "علم", "فقه", "حديث", "سيرة", "دروس" } },
            { "Azkar", new[] { "أذكار", "دعاء", "ذكر", "تسبيح", "استغفار", "صلاة على النبي" } },
            { "Radios", Array.Empty<string>() } // Default category for everything else
        };

        public RadioCategoryTabControl()
        {
            InitializeComponent();
            
            // Get services from DI
            _quranDataService = App.ServiceProvider?.GetService<IQuranDataService>();
            _favoritesService = App.ServiceProvider?.GetService<IFavoritesService>();
            
            _mediaPlayer.MediaOpened += (s, e) => 
            {
                NowPlayingCategory.Text = "جاري البث...";
            };
            
            _mediaPlayer.MediaFailed += (s, e) => 
            {
                NowPlayingTxt.Text = "فشل الاتصال";
                NowPlayingCategory.Text = "تحقق من اتصال الإنترنت";
            };

            Loaded += RadioCategoryTabControl_Loaded;
        }

        private async void RadioCategoryTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRadiosAsync();
            await LoadFavoritesAsync();
            FilterByCategory("Favorites");
        }

        private async Task LoadFavoritesAsync()
        {
            if (_favoritesService == null) return;
            var ids = await _favoritesService.GetFavoriteRadioIdsAsync();
            _favoriteIds = new HashSet<int>(ids);
        }

        private async Task LoadRadiosAsync()
        {
            if (_quranDataService == null) return;

            try
            {
                LoadingBar.Visibility = Visibility.Visible;
                var radios = await _quranDataService.GetRadiosAsync();
                _allRadios = radios.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading radios: {ex.Message}");
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CategoryTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton tab)
            {
                string category = tab.Name switch
                {
                    "FavoritesTab" => "Favorites",
                    "QuranTab" => "Quran",
                    "RadiosTab" => "Radios",
                    "TafsirTab" => "Tafsir",
                    "AzkarTab" => "Azkar",
                    _ => "Radios"
                };
                FilterByCategory(category);
            }
        }

        private void PlayCairoRadio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cairoRadio = new Radio
                {
                    Id = -1,
                    Name = "إذاعة القرآن الكريم من القاهرة",
                    Url = "http://n0d.radiojar.com/8s5u5tpdtwzuv?rj-ttl=5&rj-tok=AAABm-TMdc4AG7WxYhPU44i9TA"
                };
                PlayRadio(cairoRadio);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing Cairo radio: {ex.Message}");
            }
        }

        private void FilterByCategory(string category)
        {
            _currentCategory = category;
            List<Radio> filtered;

            if (category == "Favorites")
            {
                // Show actual favorites if any, otherwise show default Quran stations
                if (_favoriteIds.Any())
                {
                    filtered = _allRadios.Where(r => _favoriteIds.Contains(r.Id)).ToList();
                }
                else
                {
                    // Show featured Quran recitation stations as defaults when no favorites
                    var quranKeywords = CategoryKeywords["Quran"];
                    filtered = _allRadios
                        .Where(r => quranKeywords.Any(k => r.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        .Take(8)
                        .ToList();
                }
            }
            else if (category == "Radios")
            {
                // Show all radios that don't match other categories
                var otherKeywords = CategoryKeywords
                    .Where(kv => kv.Key != "Radios")
                    .SelectMany(kv => kv.Value)
                    .ToArray();

                filtered = _allRadios
                    .Where(r => !otherKeywords.Any(k => r.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            else if (CategoryKeywords.TryGetValue(category, out var keywords) && keywords.Length > 0)
            {
                filtered = _allRadios
                    .Where(r => keywords.Any(k => r.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            else
            {
                filtered = _allRadios;
            }

            RadiosItemsControl.ItemsSource = filtered;
            EmptyState.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            // Update empty state message for favorites
            if (category == "Favorites" && filtered.Count == 0)
            {
                // The empty state is already handled in XAML
            }
        }

        private void RadioCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Radio radio)
            {
                PlayRadio(radio);
            }
        }

        private void PlayRadio(Radio radio)
        {
            try
            {
                _currentPlayingRadio = radio;
                _mediaPlayer.Open(new Uri(radio.Url));
                _mediaPlayer.Volume = VolumeSlider.Value;
                _mediaPlayer.Play();

                NowPlayingTxt.Text = radio.Name;
                NowPlayingCategory.Text = "جاري الاتصال...";
            }
            catch (Exception ex)
            {
                NowPlayingTxt.Text = "خطأ في التشغيل";
                NowPlayingCategory.Text = ex.Message;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            _currentPlayingRadio = null;
            NowPlayingTxt.Text = "اختر إذاعة للاستماع";
            NowPlayingCategory.Text = "لم يتم التشغيل بعد";
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaPlayer.Volume = e.NewValue;
        }

        private async void FavoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Stop event from bubbling to play the radio
            e.Handled = true;
            
            if (sender is Button btn && btn.Tag is Radio radio && _favoritesService != null)
            {
                var isFavorite = _favoriteIds.Contains(radio.Id);
                
                await _favoritesService.ToggleFavoriteRadioAsync(radio.Id);
                
                if (isFavorite)
                {
                    _favoriteIds.Remove(radio.Id);
                }
                else
                {
                    _favoriteIds.Add(radio.Id);
                }
                
                // Update the heart icon
                if (btn.Content is System.Windows.Shapes.Path path)
                {
                    path.Fill = _favoriteIds.Contains(radio.Id) 
                        ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)) // Red for favorite
                        : (SolidColorBrush)FindResource("TextMutedBrush");
                }
                
                // Refresh the view if we're on favorites tab
                if (_currentCategory == "Favorites")
                {
                    FilterByCategory("Favorites");
                }
            }
        }
    }
}
