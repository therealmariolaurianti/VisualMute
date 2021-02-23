using System;
using System.Windows.Forms;

namespace VisualMute.Workers
{
    public class HotkeyManager : NativeWindow, IDisposable
    {
        private readonly Context context;
        private bool _isMuted;
        private int _hotKeyConstant = 786;

        public HotkeyManager(Context context)
        {
            this.context = context;
            CreateHandle(new CreateParams());
        }

        public void Dispose()
        {
            DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        { 
            if (m.Msg == _hotKeyConstant)
            {
                if (_isMuted)
                {
                    context.unmuteMic();
                    _isMuted = false;
                }
                else
                {
                    context.muteMic();
                    _isMuted = true;
                }
            }

            base.WndProc(ref m);
        }
    }
}