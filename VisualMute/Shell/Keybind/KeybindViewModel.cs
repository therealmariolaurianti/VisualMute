using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VisualMute.Properties;
using VisualMute.Workers;
using Screen = Caliburn.Micro.Screen;

namespace VisualMute.Shell.Keybind
{
    public class KeybindViewModel : Screen
    {
        private Keys _selectedKey;

        public KeybindViewModel(Keys keybind)
        {
            SelectedKey = keybind;
        }

        public Keys NewKeyBind { get; private set; }

        public List<Keys> Keys => Enum
            .GetValues(typeof(Keys))
            .Cast<Keys>()
            .ToList();

        public Keys SelectedKey
        {
            get => _selectedKey;
            set
            {
                if (value == _selectedKey) return;
                _selectedKey = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsUseControl { get; set; }
        public bool IsUseShift { get; set; }
        public bool IsUseAlt { get; set; }

        public override string DisplayName
        {
            get => "";
            set => throw new NotImplementedException();
        }

        public void Save()
        {
            // NewKeyBind = System.Windows.Forms.Keys.None;
            // if (IsUseControl)
            //     NewKeyBind |= System.Windows.Forms.Keys.Control;
            // if (IsUseShift)
            //     NewKeyBind |= System.Windows.Forms.Keys.Shift;
            // if (IsUseAlt)
            //     NewKeyBind |= System.Windows.Forms.Keys.Alt;

            NewKeyBind = SelectedKey;

            SettingsHelper.UpdateApplicationSetting(nameof(Settings.Default.KeyBind), NewKeyBind);

            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}