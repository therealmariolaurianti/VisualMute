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
        public static int MUTE_CODE = 123;

        private static readonly double REFRESH_INTERVAL = 1000;
        private readonly Timer refreshTimer;
        private readonly NotifyIcon tbIcon;
        private HotkeyManager hkManager;

        public Action<bool> MicStatusUpdatedEvent;

        public Context()
        {
            tbIcon = createIcon();
            updateMicStatus();
            refreshTimer = startTimer();
            hkManager = registerHotkeys();
        }

        public MMDevice PrimaryDevice { get; private set; }

        public static Keys KeyBind => Keys.F13;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        private HotkeyManager registerHotkeys()
        {
            var manager = new HotkeyManager(this);

            RegisterHotKey(manager.Handle, MUTE_CODE, 0, (int) KeyBind);

            return manager;
        }

        private NotifyIcon createIcon()
        {
            var exitMenuItem = new MenuItem("Exit", handleExit);

            var icon = new NotifyIcon
            {
                Icon = Resources.mic_off,
                ContextMenu = new ContextMenu(new[] {exitMenuItem})
            };

            icon.DoubleClick += (source, e) => toggleMicStatus();
            icon.Visible = true;

            return icon;
        }

        private Timer startTimer()
        {
            var timer = new Timer(REFRESH_INTERVAL);
            timer.Elapsed += (source, e) => updateMicStatus();
            timer.Start();
            return timer;
        }

        public void setMicMuteStatus(bool doMute)
        {
            var device = getPrimaryMicDevice();

            if (device != null)
            {
                device.AudioEndpointVolume.Mute = doMute;
                updateMicStatus(device);
            }
            else
            {
                updateMicStatus(null);
            }
        }

        public void muteMic()
        {
            setMicMuteStatus(true);
            MicStatusUpdatedEvent?.Invoke(true);
        }

        public void unmuteMic()
        {
            setMicMuteStatus(false);
            MicStatusUpdatedEvent?.Invoke(false);
        }

        private void toggleMicStatus()
        {
            var device = getPrimaryMicDevice();

            if (device != null)
            {
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
                updateMicStatus(device);
            }
            else
            {
                updateMicStatus(null);
            }
        }

        private void updateMicStatus()
        {
            var device = getPrimaryMicDevice();
            updateMicStatus(device);
        }

        private void updateMicStatus(MMDevice device)
        {
            if (device == null || device.AudioEndpointVolume.Mute)
                tbIcon.Icon = Resources.mic_off;
            else
                tbIcon.Icon = Resources.mic_on;

            disposeDevice(device);
        }

        private MMDevice getPrimaryMicDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            var result = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            enumerator.Dispose();

            tbIcon.Text = result.DeviceFriendlyName;

            if (PrimaryDevice is null || PrimaryDevice != result)
                PrimaryDevice = result;

            return PrimaryDevice;
        }

        private void disposeDevice(MMDevice device)
        {
            if (device != null)
            {
                device.AudioEndpointVolume.Dispose();
                device.Dispose();
            }
        }

        private void handleExit(object sender, EventArgs e)
        {
            tbIcon.Visible = false;
            refreshTimer.Stop();
            Application.Current.Shutdown();
        }
    }
}