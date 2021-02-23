using System;
using System.Windows.Forms;

namespace VisualMute.Workers
{
    public class HotkeyManager : NativeWindow, IDisposable
    {
        private const int _hotKeyConstant = 786;
        private readonly Context context;
        private bool _isMuted;

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
                    context.UnmuteMic();
                    _isMuted = false;
                }
                else
                {
                    context.MuteMic();
                    _isMuted = true;
                }
            }

            base.WndProc(ref m);
        }
    }
}