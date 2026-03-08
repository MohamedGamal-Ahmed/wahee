// Task Summary:
// - Phase 2 / Task 3: Wired SettingsView to SettingsViewModel with DataContext-based loading.
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wahee.UI.ViewModels;

namespace Wahee.UI.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider!.GetRequiredService<SettingsViewModel>();
            DataContext = _viewModel;
            Loaded += async (s, e) => await _viewModel.LoadSettingsAsync();
        }
    }
}
