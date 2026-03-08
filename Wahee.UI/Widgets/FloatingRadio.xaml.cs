using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;

namespace Wahee.UI.Widgets
{
    public partial class FloatingRadio : WaheeWidgetBase
    {
        private readonly MediaPlayer _mediaPlayer = new();
        private bool _isPlaying = false;
        private const string CAIRO_RADIO_URL = "http://n0d.radiojar.com/8s5u5tpdtwzuv?rj-ttl=5&rj-tok=AAABm-TMdc4AG7WxYhPU44i9TA";

        public FloatingRadio()
        {
            InitializeComponent();
            _mediaPlayer.MediaOpened += (s, e) => StatusTxt.Text = "جاري البث المباشر...";
            _mediaPlayer.MediaFailed += (s, e) => StatusTxt.Text = "تعذر الاتصال";
            this.IsVisibleChanged += FloatingRadio_IsVisibleChanged;
        }

        private void FloatingRadio_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                _mediaPlayer.Stop();
                _isPlaying = false;
                PlayPauseIcon.Data = (Geometry)FindResource("PlayIconGeometry");
                StatusTxt.Text = "متوقف";
            }
        }

        protected override void InitializeWidget()
        {
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = 20; // Top of the screen
        }

        protected override void LoadWidgetData()
        {
            // Cairo radio is hardcoded, no loading needed
            StationNameTxt.Text = "إذاعة القرآن الكريم من القاهرة";
            StatusTxt.Text = "متوقف";
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _mediaPlayer.Pause();
                _isPlaying = false;
                PlayPauseIcon.Data = (Geometry)FindResource("PlayIconGeometry");
                StatusTxt.Text = "متوقف مؤقتاً";
            }
            else
            {
                _isPlaying = true;
                PlayPauseIcon.Data = (Geometry)FindResource("PauseIconGeometry");
                StatusTxt.Text = "جاري التحميل...";
                _mediaPlayer.Open(new Uri(CAIRO_RADIO_URL));
                _mediaPlayer.Play();
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaPlayer.Volume = e.NewValue;
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Resources["FadeFull"] is Storyboard sb) sb.Begin(this);
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Resources["FadeDim"] is Storyboard sb) sb.Begin(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            this.Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}

