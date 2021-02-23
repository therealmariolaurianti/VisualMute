using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using VisualMute.Properties;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace VisualMute.Workers
{
    public class Context
    {
        private const int _muteCode = 123;
        private const double _refreshInterval = 1000;
        private HotkeyManager _hkManager;

        private NotifyIcon _icon;
        private Timer _refreshTimer;

        public Action<bool> MicStatusUpdatedEvent;

        public Context()
        {
            Initialize();
        }

        public MMDevice PrimaryDevice { get; private set; }

        public Keys KeyBind { get; private set; }

        private void Initialize()
        {
            CreateIcon();
            UpdateMicStatus();
            StartTimer();
            SetupHotkeys();
        }

        private void SetupHotkeys()
        {
            _hkManager = new HotkeyManager(this);
            KeyBind = Settings.Default.KeyBind;

            RegisterHotkeys(KeyBind);
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public void UpdateKeyBind(Keys keyBind)
        {
            UnregisterHotKey(_hkManager.Handle, _muteCode);
            RegisterHotkeys(keyBind);

            KeyBind = keyBind;
        }

        private void RegisterHotkeys(Keys keyBind)
        {
            RegisterHotKey(_hkManager.Handle, _muteCode, 0, (int) keyBind);
        }

        private void CreateIcon()
        {
            var exitMenuItem = new MenuItem("Exit", HandleExit);

            var icon = new NotifyIcon
            {
                Icon = Resources.mic_off,
                ContextMenu = new ContextMenu(new[] {exitMenuItem})
            };

            icon.DoubleClick += (source, e) => ToggleMicStatus();
            icon.Visible = true;

            _icon = icon;
        }

        private void StartTimer()
        {
            _refreshTimer = new Timer(_refreshInterval);
            _refreshTimer.Elapsed += (source, e) => UpdateMicStatus();
            _refreshTimer.Start();
        }

        private void SetMicMuteStatus(bool doMute)
        {
            var device = GetPrimaryMicDevice();

            if (device != null)
            {
                device.AudioEndpointVolume.Mute = doMute;
                UpdateMicStatus(device);
            }
            else
            {
                UpdateMicStatus(null);
            }
        }

        public void MuteMic()
        {
            SetMicMuteStatus(true);
            MicStatusUpdatedEvent?.Invoke(true);
        }

        public void UnmuteMic()
        {
            SetMicMuteStatus(false);
            MicStatusUpdatedEvent?.Invoke(false);
        }

        private void ToggleMicStatus()
        {
            var device = GetPrimaryMicDevice();

            if (device != null)
            {
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
                UpdateMicStatus(device);
            }
            else
            {
                UpdateMicStatus(null);
            }
        }

        private void UpdateMicStatus()
        {
            var device = GetPrimaryMicDevice();
            UpdateMicStatus(device);
        }

        private void UpdateMicStatus(MMDevice device)
        {
            if (device == null || device.AudioEndpointVolume.Mute)
                _icon.Icon = Resources.mic_off;
            else
                _icon.Icon = Resources.mic_on;

            DisposeDevice(device);
        }

        private MMDevice GetPrimaryMicDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            var result = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            enumerator.Dispose();

            _icon.Text = result.DeviceFriendlyName;

            if (PrimaryDevice is null || PrimaryDevice != result)
                PrimaryDevice = result;

            return PrimaryDevice;
        }

        private static void DisposeDevice(MMDevice device)
        {
            if (device == null)
                return;

            device.AudioEndpointVolume.Dispose();
            device.Dispose();
        }

        private void HandleExit(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _refreshTimer.Stop();
            Application.Current.Shutdown();
        }
    }
}