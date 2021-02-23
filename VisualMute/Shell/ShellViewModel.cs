using System;
using System.Windows;
using System.Windows.Media;
using Accord;
using Caliburn.Micro;
using ScottPlot;
using VisualMute.Workers;
using Style = ScottPlot.Style;

namespace VisualMute.Shell
{
    public class ShellViewModel : Screen, IDisposable
    {
        private const string _plotName = "MicrophonePlot";
        private readonly Context _context;

        private readonly GraphPlotter _graphPlotter;
        private bool _isMuted;

        public ShellViewModel(GraphPlotter graphPlotter, Context context)
        {
            _graphPlotter = graphPlotter;

            _context = context;
            _context.MicStatusUpdatedEvent += OnMicStatusUpdatedEvent;
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted == value)
                    return;
                _isMuted = value;
                NotifyOfPropertyChange();
            }
        }

        public string KeyBind => $"KeyBind: {Context.KeyBind.GetDescription()}";

        public string PrimaryDevice => _context.PrimaryDevice.FriendlyName;

        public Brush ForegroundColor =>
            IsMuted ? Brushes.Green : Brushes.Red;

        public string Text => IsMuted ? "Muted" : "Unmuted";

        public void Dispose()
        {
            _context.MicStatusUpdatedEvent -= OnMicStatusUpdatedEvent;
        }

        protected override void OnInitialize()
        {
            NotifyOfPropertyChange(nameof(PrimaryDevice));
            base.OnInitialize();
        }

        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var shellView = sender as ShellView;
            var wpfPlot = shellView.FindChild<WpfPlot>(_plotName);

            wpfPlot.Plot.Title("Microphone FFT Data");
            wpfPlot.Plot.YLabel("Power (raw)");
            wpfPlot.Plot.XLabel("Frequency (Hz)");
            wpfPlot.Plot.Style(Style.Black);

            _graphPlotter.Initialize(wpfPlot);
        }

        private void OnMicStatusUpdatedEvent(bool isMuted)
        {
            IsMuted = isMuted;

            NotifyOfPropertyChange(nameof(Text));
            NotifyOfPropertyChange(nameof(ForegroundColor));
        }
    }
}