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

        public void Save()
        {
            SettingsHelper.UpdateApplicationSetting(
                nameof(Settings.Default.KeyBind),
                SelectedKey);
            
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

        public override string DisplayName
        {
            get => "";
            set => throw new NotImplementedException();
        }
    }
}