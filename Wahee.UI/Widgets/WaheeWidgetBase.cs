using System;
using System.Windows;
using System.Windows.Input;
using Wahee.UI.Helpers;

namespace Wahee.UI.Widgets
{
    public abstract class WaheeWidgetBase : Window
    {
        public WaheeWidgetBase()
        {
            // Apply base styling
            this.Style = (Style)FindResource("WaheeWidgetWindowStyle");
            this.ShowInTaskbar = false; // Hide from taskbar
            this.Loaded += (s, e) => SetupWidget();
            this.MouseDown += OnMouseDown;
            this.LocationChanged += OnLocationChanged;
            this.IsVisibleChanged += (s, e) => {
                if ((bool)e.NewValue) LoadWidgetData();
            };
        }

        private void OnLocationChanged(object? sender, EventArgs e)
        {
            SnapToEdges();
        }

        private void SnapToEdges()
        {
            const double SnapThreshold = 25;
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            if (this.Left < SnapThreshold) this.Left = 0;
            if (this.Top < SnapThreshold) this.Top = 0;
            if (this.Left + this.Width > screenWidth - SnapThreshold) this.Left = screenWidth - this.Width;
            if (this.Top + this.Height > screenHeight - SnapThreshold) this.Top = screenHeight - this.Height;
        }

        // Template Method
        private void SetupWidget()
        {
            WindowBlurHelper.ApplyBlur(this);
            InitializeWidget();
            LoadWidgetData();
        }

        // Steps to be implemented by concrete widgets
        protected abstract void InitializeWidget();
        protected abstract void LoadWidgetData();

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        
        protected void CloseWidget()
        {
            this.Hide();
        }
    }
}
