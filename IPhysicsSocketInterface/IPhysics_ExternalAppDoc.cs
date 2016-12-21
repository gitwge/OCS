using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace IPhysics
{
    public partial class ExternalAppDock : UserControl
    {
        private Process externalApp;
        private Control ControlParent;
        private System.Windows.Forms.Timer timer;

        public ExternalAppDock()
        {
            InitializeComponent();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += (sender, e) => ResizeFinished();
        }

        ~ExternalAppDock()
        {
            if (externalApp != null)
            {
                SetParent(externalApp.MainWindowHandle, new IntPtr(0));
            }
        }

        public Process ExternalApp { 
            get { 
                return externalApp; 
            }

            set { 
                DockExternalApp(value, this.ControlParent); 
            }
        }

        [DllImport("user32")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_KEYDOWN = 0x100;
        const uint WM_SYSCOMMAND = 0x018;
        const uint SC_CLOSE = 0x053;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public void DockExternalApp(Process app, Control DockParent)
        {
            externalApp = app;
            if (externalApp != null)
            {
                // Get Parent of this Control
                this.ControlParent = DockParent;

                // Set Parent of External App to this Control
                SetParent(externalApp.MainWindowHandle, this.Handle);
                SendMessage(externalApp.MainWindowHandle, WM_KEYDOWN, ((IntPtr)Keys.F4), (IntPtr)0);
                SetWindowPos(externalApp.MainWindowHandle, HWND_TOP, 0, 0, 0, 0, 64);
                ShowWindow(externalApp.MainWindowHandle, 3);
            }
        }

        public void ExternalAppResize(int x, int y)
        {
            if (externalApp == null)
                return;

            SetWindowPos(externalApp.MainWindowHandle, HWND_TOP, 0, 0, x, y, 64);
        }


        // Override OnSizeChanged Event to exectue custom code
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (timer == null)
                return;

            timer.Stop();
            timer.Start();
        }

        // Custom code to be Executed after the OnChangedEvent has finished
        private void ResizeFinished()
        {
            timer.Stop();

            // Custom code starts here
            if (this.ControlParent == null)
                return;

            ExternalAppResize(this.ControlParent.Width, this.ControlParent.Height);
        }
    }
}
