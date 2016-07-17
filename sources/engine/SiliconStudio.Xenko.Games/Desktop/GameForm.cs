﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimDX project.
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D || SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN) && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// Default Rendering Form on windows desktop.
    /// </summary>
    [DesignerCategory("Code")]
    public class GameForm : Form
    {
        private const int SIZE_RESTORED = 0;
        private const int SIZE_MINIMIZED = 1;
        private const int SIZE_MAXIMIZED = 2;
        private const int SIZE_MAXSHOW = 3;
        private const int SIZE_MAXHIDE = 4; 

        private const uint PBT_APMRESUMESUSPEND = 7;
        private const uint PBT_APMQUERYSUSPEND = 0;
        private const int SC_MONITORPOWER = 0xF170;
        private const int SC_SCREENSAVE = 0xF140;
        private const int MNC_CLOSE = 1;
        private const byte VK_RETURN = 0x0D;
        private System.Drawing.Size cachedSize;
        private FormWindowState previousWindowState;
        //private DisplayMonitor monitor;
        private bool isUserResizing;
        private bool isBackgroundFirstDraw;
        private bool isSizeChangedWithoutResizeBegin;

        /// Initializes a new instance of the <see cref="GameForm"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public GameForm()
        {
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(800, 600);

            ResizeRedraw = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // TODO: Provide proper access to icon and title through code and game studio
            Text = GameContext.ProductName;
            try
            {
                var productLocation = GameContext.ProductLocation;
                Icon = !string.IsNullOrEmpty(productLocation)
                    ? Icon.ExtractAssociatedIcon(productLocation)
                    : SystemIcons.Application;
            }
            catch
            {
                Icon = SystemIcons.Application;
            }

            previousWindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Occurs when [app activated].
        /// </summary>
        public event EventHandler<EventArgs> AppActivated;

        /// <summary>
        /// Occurs when [app deactivated].
        /// </summary>
        public event EventHandler<EventArgs> AppDeactivated;

        /// <summary>
        /// Occurs when [monitor changed].
        /// </summary>
        public event EventHandler<EventArgs> MonitorChanged;

        /// <summary>
        /// Occurs when [pause rendering].
        /// </summary>
        public event EventHandler<EventArgs> PauseRendering;

        /// <summary>
        /// Occurs when [resume rendering].
        /// </summary>
        public event EventHandler<EventArgs> ResumeRendering;

        /// <summary>
        /// Occurs when [screensaver].
        /// </summary>
        public event EventHandler<CancelEventArgs> Screensaver;

        /// <summary>
        /// Occurs when [system resume].
        /// </summary>
        public event EventHandler<EventArgs> SystemResume;

        /// <summary>
        /// Occurs when [system suspend].
        /// </summary>
        public event EventHandler<EventArgs> SystemSuspend;

        /// <summary>
        /// Occurs when [user resized].
        /// </summary>
        public event EventHandler<EventArgs> UserResized;

        /// <summary>
        /// Occurs when alt-enter key combination has been pressed.
        /// </summary>
        public event EventHandler<EventArgs> FullscreenToggle;

        protected bool EnableFullscreenToggle = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is processing keys. By default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if this instance is processing keys; otherwise, <c>false</c>.</value>
        public bool IsProcessingKeys { get; set; }

        internal bool IsFullScreen { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeBegin"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeBegin(EventArgs e)
        {
            isUserResizing = true;

            base.OnResizeBegin(e);
            cachedSize = Size;
            OnPauseRendering(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            if (isUserResizing && cachedSize != Size)
            {
                OnUserResized(e);
                // UpdateScreen();
            }

            isUserResizing = false;
            OnResumeRendering(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // UpdateScreen();
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!isBackgroundFirstDraw)
            {
                base.OnPaintBackground(e);
                isBackgroundFirstDraw = true;
            }
        }

        /// <summary>
        /// Raises the Pause Rendering event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnPauseRendering(EventArgs e)
        {
            if (PauseRendering != null)
                PauseRendering(this, e);
        }

        /// <summary>
        /// Raises the Resume Rendering event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnResumeRendering(EventArgs e)
        {
            if (ResumeRendering != null)
                ResumeRendering(this, e);
        }

        /// <summary>
        /// Raises the User resized event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnUserResized(EventArgs e)
        {
            if (UserResized != null)
                UserResized(this, e);
        }

        private void OnMonitorChanged(EventArgs e)
        {
            if (MonitorChanged != null)
                MonitorChanged(this, e);
        }

        /// <summary>
        /// Raises the On App Activated event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAppActivated(EventArgs e)
        {
            if (AppActivated != null)
                AppActivated(this, e);
        }

        /// <summary>
        /// Raises the App Deactivated event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAppDeactivated(EventArgs e)
        {
            if (AppDeactivated != null)
                AppDeactivated(this, e);
        }

        /// <summary>
        /// Raises the System Suspend event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSystemSuspend(EventArgs e)
        {
            if (SystemSuspend != null)
                SystemSuspend(this, e);
        }

        /// <summary>
        /// Raises the System Resume event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSystemResume(EventArgs e)
        {
            if (SystemResume != null)
                SystemResume(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Screensaver"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void OnScreensaver(CancelEventArgs e)
        {
            if (Screensaver != null)
                Screensaver(this, e);
        }

        /// <summary>
        /// Raises the FullScreenToggle event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnFullscreenToggle(EventArgs e)
        {
            if(FullscreenToggle != null)
                FullscreenToggle(this, e);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (!isUserResizing && (isSizeChangedWithoutResizeBegin || cachedSize != Size))
            {
                isSizeChangedWithoutResizeBegin = false;
                cachedSize = Size;
                OnUserResized(EventArgs.Empty);
                //UpdateScreen();
            }
        }

        /// <summary>
        /// Override windows message loop handling.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            long wparam = m.WParam.ToInt64();

            switch (m.Msg)
            {
                case Win32Native.WM_SIZE:
                    if (wparam == SIZE_MINIMIZED)
                    {
                        previousWindowState = FormWindowState.Minimized;
                        OnPauseRendering(EventArgs.Empty);
                    }
                    else
                    {
                        Rectangle rect;

                        GetClientRect(m.HWnd, out rect);
                        if (rect.Bottom - rect.Top == 0)
                        {
                            // Rapidly clicking the task bar to minimize and restore a window
                            // can cause a WM_SIZE message with SIZE_RESTORED when 
                            // the window has actually become minimized due to rapid change
                            // so just ignore this message
                        }
                        else if (wparam == SIZE_MAXIMIZED)
                        {
                            if (previousWindowState == FormWindowState.Minimized)
                                OnResumeRendering(EventArgs.Empty);

                            previousWindowState = FormWindowState.Maximized;

                            OnUserResized(EventArgs.Empty);
                            //UpdateScreen();
                            cachedSize = Size;
                        }
                        else if (wparam == SIZE_RESTORED)
                        {
                            if (previousWindowState == FormWindowState.Minimized)
                                OnResumeRendering(EventArgs.Empty);

                            if (!isUserResizing && (Size != cachedSize || previousWindowState == FormWindowState.Maximized))
                            {
                                previousWindowState = FormWindowState.Normal;

                                // Only update when cachedSize is != 0
                                if (cachedSize != Size.Empty)
                                {
                                    isSizeChangedWithoutResizeBegin = true;
                                }
                            }

                            previousWindowState = FormWindowState.Normal;
                        }
                    }
                    break;
                case Win32Native.WM_ACTIVATEAPP:
                    if (wparam != 0)
                    {
                        OnAppActivated(EventArgs.Empty);
                    }
                    else
                    {
                        OnAppDeactivated(EventArgs.Empty);
                    }
                    break;
                case Win32Native.WM_POWERBROADCAST:
                    if (wparam == PBT_APMQUERYSUSPEND)
                    {
                        OnSystemSuspend(EventArgs.Empty);
                        m.Result = new IntPtr(1);
                        return;
                    }
                    else if (wparam == PBT_APMRESUMESUSPEND)
                    {
                        OnSystemResume(EventArgs.Empty);
                        m.Result = new IntPtr(1);
                        return;
                    }
                    break;
                case Win32Native.WM_MENUCHAR:
                    //m.Result = new IntPtr(MNC_CLOSE << 16); // IntPtr(MAKELRESULT(0, MNC_CLOSE));
                    return;
                case Win32Native.WM_SYSCOMMAND:
                    wparam &= 0xFFF0;
                    if (wparam == SC_MONITORPOWER || wparam == SC_SCREENSAVE)
                    {
                        var e = new CancelEventArgs();
                        OnScreensaver(e);
                        if (e.Cancel)
                        {
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    break;
                case Win32Native.WM_SYSKEYDOWN: //alt is down
                    if(wparam == VK_RETURN)
                    {
                        if(!EnableFullscreenToggle) return;
                        OnFullscreenToggle(new EventArgs()); //we handle alt enter manually
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (IsSystemKeyToFilter(e))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            base.OnKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsSystemKeyToFilter(e))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        private static bool IsSystemKeyToFilter(KeyEventArgs e)
        {
            // Supress ALT and F10 keys from being processed
            return ((e.KeyValue == 0x12 || e.KeyValue == 0x79 || e.Alt) && !(e.Alt && e.KeyValue == 0x73));
        }

        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        private static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);
    }
}
#endif
