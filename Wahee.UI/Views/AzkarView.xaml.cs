// Task Summary:
// - Phase 2 / Task 3: Wired AzkarView to AzkarViewModel and removed service-locator logic from UI flow.
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wahee.UI.ViewModels;

namespace Wahee.UI.Views
{
    public partial class AzkarView : UserControl
    {
        private readonly AzkarViewModel _viewModel;

        public AzkarView()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider!.GetRequiredService<AzkarViewModel>();
            DataContext = _viewModel;
            Loaded += async (s, e) => await _viewModel.LoadAllAzkarAsync();
        }
    }
}
