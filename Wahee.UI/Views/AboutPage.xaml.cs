using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wahee.UI.Services;

namespace Wahee.UI.Views
{
    public partial class AboutPage : UserControl
    {
        private const string RepoBaseUrl = "https://github.com/MohamedGamal-Ahmed/wahee";
        private const string BugReportUrl = RepoBaseUrl + "/issues/new?template=bug_report.yml";
        private const string FeatureRequestUrl = RepoBaseUrl + "/issues/new?template=feature_request.yml";
        private const string RateAppUrl = RepoBaseUrl + "/issues/new?title=%D8%AA%D9%82%D9%8A%D9%8A%D9%85%20%D8%A7%D9%84%D8%AA%D8%B7%D8%A8%D9%8A%D9%82&labels=feedback";

        public AboutPage()
        {
            InitializeComponent();
        }

        private void ReportBug_Click(object sender, RoutedEventArgs e) => OpenUrl(BugReportUrl);

        private void FeatureRequest_Click(object sender, RoutedEventArgs e) => OpenUrl(FeatureRequestUrl);

        private void RateApp_Click(object sender, RoutedEventArgs e) => OpenUrl(RateAppUrl);

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            var updateService = App.ServiceProvider?.GetService<IUpdateService>();
            if (updateService == null)
            {
                MessageBox.Show("خدمة التحديث غير متاحة الآن.", "وحي", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await updateService.CheckForUpdatesAsync(true);
        }

        private static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}

