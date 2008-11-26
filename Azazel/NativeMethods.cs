// Copyright (c) 2007 Joel Bennett

// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.

// *****************************************************************************
// NOTE: YOU MAY *ALSO* DISTRIBUTE THIS FILE UNDER ANY OF THE FOLLOWING LICENSES:
// BSD:	 http://www.opensource.org/licenses/bsd-license.php
// MIT:   http://www.opensource.org/licenses/mit-license.html
// Ms-PL: http://www.microsoft.com/resources/sharedsource/licensingbasics/permissivelicense.mspx
// Ms-CL: http://www.microsoft.com/resources/sharedsource/licensingbasics/communitylicense.mspx
// GPL 2: http://www.gnu.org/copyleft/gpl.html  

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Azazel {
    /// <summary>
    /// Wrapper for P/INVOKE methods
    /// </summary>
    public sealed partial class NativeMethods {
        /// <summary>Enum*Windows Callback Delegate</summary>
        /// <param name="hWnd"></param>
        /// <param name="param"></param>
        /// <returns>True if the window was processed - to continue enumeration</returns>
        public delegate bool EnumWindowsCallback(IntPtr hWnd, int param);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Gets the window position as a Rectangle.
        /// This is a wrapper for the Win32 API call GetWindowRect which doesn't return a proper .NET Rectangle
        /// </summary>
        /// <param name="hWnd">The Window Handle.</param>
        /// <returns>The Rectangle occupied by the Window on success, or the default Rectangle otherwise</returns>
        internal static Rectangle GetWindowRectangle(IntPtr hWnd) {
            RECT rect;
            if (GetWindowRect(hWnd, out rect))
                return rect.ToRectangle();
            return new Rectangle();
        }

        // This static method is required because legacy OSes do not support
        // GetWindowLongPtr
        public static IntPtr GetWindowLongish(IntPtr hWnd, GWL nIndex) {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr(hWnd, nIndex);
            else
                return new IntPtr(GetWindowLong(hWnd, nIndex));
        }

        //public static IntPtr SetWindowLongish(IntPtr hWnd, GWL nIndex, IntPtr newLong)
        //{
        //    if (IntPtr.Size == 8)
        //        return SetWindowLongPtr(hWnd, nIndex, newLong);
        //    else
        //        return new IntPtr(SetWindowLong(hWnd, nIndex, (uint)newLong.ToInt32()));
        //}

        [DllImport("User32")]
        public static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsCallback callback, int param);

        [DllImport("User32")]
        public static extern bool EnumThreadWindows(int threadId, EnumWindowsCallback callback, int param);

        [DllImport("User32")]
        public static extern bool EnumWindows(EnumWindowsCallback callback, int param);

        [DllImport("User32")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32")]
        public static extern int GetWindowThreadProcessId(IntPtr window, ref int processId);

        [DllImport("User32")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("User32")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32")]
        public static extern bool IsChild(IntPtr parent, IntPtr window);

        [DllImport("User32")]
        public static extern bool IsIconic(IntPtr window);

        [DllImport("User32")]
        public static extern bool IsZoomed(IntPtr window);

        [DllImport("User32")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("User32")]
        public static extern bool SetWindowText(IntPtr window, [MarshalAs(UnmanagedType.LPTStr)] string text);

        [DllImport("User32")]
        public static extern int GetWindowTextLength(IntPtr window);

        [DllImport("User32")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        /// <summary>
        /// Gets the text (caption) of the specified window.
        /// This is a wrapper for API GetWindowText so you don't have to mess with pointers (StringBuilder) and lengths
        /// </summary>
        /// <param name="hWnd">The Window Handle</param>
        /// <returns>The Window's Text</returns>
        public static string GetWindowText(IntPtr hWnd) {
            var sb = new StringBuilder(GetWindowTextLength(hWnd) + 1);
            return (GetWindowText(hWnd, sb, sb.Capacity) > 0) ? sb.ToString() : null;
            //if (sb.length == 0) SendMessageTimeout( ... WM_GETTEXT ... );
        }

        /// <summary>
        /// Get the ClassName of the specified Window.
        /// This is a wrapper for API GetClassName so you don't have to mess with pointers (StringBuilder) and lengths
        /// </summary>
        /// <param name="hWnd">The Window Handle</param>
        /// <returns>The Window's ClassName</returns>
        public static string GetClassName(IntPtr hWnd) {
            var sb = new StringBuilder(100);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        //// SendMessage
        //[DllImport("User32")]
        //public static extern IntPtr SendMessage(IntPtr hWnd, IntPtr Msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("User32", SetLastError = true, CharSet = CharSet.Auto)]
        //public static extern IntPtr SendMessageTimeout(IntPtr windowHandle,
        //    uint Msg,
        //    IntPtr wParam,
        //    IntPtr lParam,
        //    SMTO flags,
        //    uint timeout,
        //    out IntPtr result);

        //// Overload for string lParam (e.g. WM_GETTEXT)
        //[DllImport("User32")]
        //public static extern IntPtr SendMessage(IntPtr hWnd, IntPtr Msg, IntPtr wParam, System.Text.StringBuilder lParam);

        //[return: MarshalAs(UnmanagedType.Bool)]
        //[DllImport("User32", SetLastError = true)]
        //public static extern bool PostMessage(IntPtr hWnd, int Msg, SC wParam, IntPtr lParam);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, IntPtr Msg, IntPtr wParam, IntPtr lParam);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("User32")]
        public static extern IntPtr GetParent(IntPtr window);

        [DllImport("User32")]
        public static extern IntPtr GetLastActivePopup(IntPtr window);

        [DllImport("User32")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("User32")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        //[DllImport("User32")]
        //public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //[DllImport("User32" /*, SetLastError = true */ )]
        //public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, uint dwNewLong);

        //[DllImport("User32", EntryPoint = "SetWindowLongPtr")]
        //public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);

        [DllImport("User32", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("User32", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex);

        [DllImport("User32")]
        public static extern IntPtr GetWindow(IntPtr hWnd, GW uCmd);

        [DllImport("User32")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, SWP flags);
    }

    public struct SC {
        internal const int Close = 0xf060;
        internal const int Maximize = 0xf030;
        internal const int Minimize = 0xf020;
        internal const int Restore = 0xf120;

        //Size = 0xf000,
        //Move = 0xf010,
        //NextWindow = 0xf040,
        //PrevWindow   =0xf050,
        //VScroll      =0xf070,
        //HScroll      =0xf080,
        //MouseMenu    =0xf090,
        //KeyMenu      =0xf100,
        //Arrange      =0xf110,
        //TaskList     =0xf130,
        //ScreenSave   =0xf140,
        //hotkey       =0xf150,
        //default      =0xf160,
        //monitorpower =0xf170,
        //contexthelp  =0xf180,
        //separator    =0xf00f
    }

    /// <summary>SetWindowPos parameter enumeration</summary>
    [Flags]
    public enum SWP : uint {
        /// <summary>Retains the current size (ignores width and height parameters).</summary>
        NOSIZE = 0x0001,
        /// <summary>Retains the current positions (ignores the x and y parameters).</summary>
        NOMOVE = 0x0002,
        /// <summary>Does not change the owner window's position in the Z order.</summary>
        NOZORDER = 0x0004,
        /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs.</summary>
        NOREDRAW = 0x0008,
        /// <summary>Does not activate the window.</summary>
        NOACTIVATE = 0x0010,
        /// <summary>Applies new frame styles set using the SetWindowLong function. 
        /// Sends a WM_NCCALCSIZE message to the window, 
        /// even if the window's size is not being changed.
        /// </summary>
        FRAMECHANGED = 0x0020,
        /// <summary>Displays the window.</summary>
        SHOWWINDOW = 0x0040,
        /// <summary>Hides the window.</summary>
        HIDEWINDOW = 0x0080,
        /// <summary>Discards the entire contents of the client area. 
        /// If this flag is not specified, the valid contents of the client area 
        /// are saved and copied back into the client area 
        /// after the window is sized or repositioned.
        /// </summary>
        NOCOPYBITS = 0x0100,
        /// <summary>Does not change the owner window's position in the Z order.</summary>
        NOOWNERZORDER = 0x0200,
        /// <summary>
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        NOSENDCHANGING = 0x0400,
        //      , DRAWFRAME      =FRAMECHANGED
        //      , NOREPOSITION   =NOOWNERZORDER
        /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
        DEFERERASE = 0x2000,
        /// <summary>If the calling thread and the thread that owns the window 
        /// are attached to different input queues, the system posts the request
        /// to the thread that owns the window. Prevents blocking thread execution.
        /// </summary>
        ASYNCWINDOWPOS = 0x4000
    }

    /// <summary>WindowMessages</summary>
    public partial struct WM {
        internal const int Activate = 0x6;
        internal const int ActivateApp = 0x1C;
        internal const int SysCommand = 0x112;
    }

    /// <summary>Indices for GetWindowLong and SetWindowLong</summary>
    public enum GWL {
        WndProc = (-4),
        hInstance = (-6),
        hWndParent = (-8),
        Style = (-16),
        ExStyle = (-20),
        UserData = (-21),
        Id = (-12)
    }

    /// <summary>GetWindow parameters</summary>
    public enum GW {
        hWndFirst = 0,
        hWndLast = 1,
        hWndNext = 2,
        hWndPrev = 3,
        Owner = 4,
        Child = 5,
        EnabledPopup = 6
    }

    [Flags]
    public enum WS : uint {
        OVERLAPPED = 0x00000000,
        POPUP = 0x80000000,
        CHILD = 0x40000000,
        MINIMIZE = 0x20000000,
        VISIBLE = 0x10000000,
        DISABLED = 0x08000000,
        CLIPSIBLINGS = 0x04000000,
        CLIPCHILDREN = 0x02000000,
        MAXIMIZE = 0x01000000,
        BORDER = 0x00800000,
        DLGFRAME = 0x00400000,
        VSCROLL = 0x00200000,
        HSCROLL = 0x00100000,
        SYSMENU = 0x00080000,
        THICKFRAME = 0x00040000,
        GROUP = 0x00020000,
        TABSTOP = 0x00010000,

        MINIMIZEBOX = 0x00020000,
        MAXIMIZEBOX = 0x00010000,

        CAPTION = BORDER | DLGFRAME,
        TILED = OVERLAPPED,
        ICONIC = MINIMIZE,
        SIZEBOX = THICKFRAME,
        TILEDWINDOW = OVERLAPPEDWINDOW,

        OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX,
        POPUPWINDOW = POPUP | BORDER | SYSMENU,
        CHILDWINDOW = CHILD
    }

    [Flags]
    public enum WS_EX : uint {
        /// <summary>
        /// Specifies that a window created with this style accepts drag-drop files.
        /// </summary>
        ACCEPTFILES = 0x00000010,
        /// <summary>
        /// Forces a top-level window onto the taskbar when the window is visible.
        /// </summary>
        APPWINDOW = 0x00040000,
        /// <summary>
        /// Specifies that a window has a border with a sunken edge.
        /// </summary>
        CLIENTEDGE = 0x00000200,
        /// <summary>
        /// Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// </summary>
        COMPOSITED = 0x02000000,
        /// <summary>
        /// Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
        /// CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        /// </summary>
        CONTEXTHELP = 0x00000400,
        /// <summary>
        /// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// </summary>
        CONTROLPARENT = 0x00010000,
        /// <summary>
        /// Creates a window that has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
        /// </summary>
        DLGMODALFRAME = 0x00000001,
        /// <summary>
        /// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// </summary>
        LAYERED = 0x00080000,
        /// <summary>
        /// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left.
        /// </summary>
        LAYOUTRTL = 0x00400000,
        /// <summary>
        /// Creates a window that has generic left-aligned properties. This is the default.
        /// </summary>
        LEFT = 0x00000000,
        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
        /// </summary>
        LEFTSCROLLBAR = 0x00004000,
        /// <summary>
        /// The window text is displayed using left-to-right reading-order properties. This is the default.
        /// </summary>
        LTRREADING = 0x00000000,
        /// <summary>
        /// Creates a multiple-document interface (MDI) child window.
        /// </summary>
        MDICHILD = 0x00000040,
        /// <summary>
        /// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the APPWINDOW style.
        /// </summary>
        NOACTIVATE = 0x08000000,
        /// <summary>
        /// Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
        /// </summary>
        NOINHERITLAYOUT = 0x00100000,
        /// <summary>
        /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
        /// </summary>
        NOPARENTNOTIFY = 0x00000004,
        /// <summary>
        /// Combines the CLIENTEDGE and WINDOWEDGE styles.
        /// </summary>
        OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,
        /// <summary>
        /// Combines the WINDOWEDGE, TOOLWINDOW, and TOPMOST styles.
        /// </summary>
        PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,
        /// <summary>
        /// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
        /// Using the RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
        /// </summary>
        RIGHT = 0x00001000,
        /// <summary>
        /// Vertical scroll bar (if present) is to the right of the client area. This is the default.
        /// </summary>
        RIGHTSCROLLBAR = 0x00000000,
        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
        /// </summary>
        RTLREADING = 0x00002000,
        /// <summary>
        /// Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
        /// </summary>
        STATICEDGE = 0x00020000,
        /// <summary>
        /// Creates a tool window; that is, a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
        /// </summary>
        TOOLWINDOW = 0x00000080,
        /// <summary>
        /// Specifies that a window created with this style should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
        /// </summary>
        TOPMOST = 0x00000008,
        /// <summary>
        /// Specifies that a window created with this style should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// </summary>
        TRANSPARENT = 0x00000020,
        /// <summary>
        /// Specifies that a window has a border with a raised edge.
        /// </summary>
        WINDOWEDGE = 0x00000100
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left_, int top_, int right_, int bottom_) {
            Left = left_;
            Top = top_;
            Right = right_;
            Bottom = bottom_;
        }

        public int Height {
            get { return (Bottom - Top) + 1; }
            set { Bottom = (Top + value) - 1; }
        }

        public int Width {
            get { return (Right - Left) + 1; }
            set { Right = (Left + value) - 1; }
        }

        public Size Size {
            get { return new Size(Width, Height); }
        }

        public Point Location {
            get { return new Point(Left, Top); }
        }

        // Handy method for converting to a System.Drawing.Rectangle
        public Rectangle ToRectangle() {
            return Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        public static RECT FromRectangle(Rectangle rectangle) {
            return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public override int GetHashCode() {
            return Left ^ ((Top << 13) | (Top >> 0x13)) ^ ((Width << 0x1a) | (Width >> 6)) ^ ((Height << 7) | (Height >> 0x19));
        }

        //public static implicit operator System.Windows.Shapes.Rectangle(RECT rect)
        //{
        //    System.Windows.Shapes.Rectangle sRectangle = new System.Windows.Shapes.Rectangle();
        //    sRectangle.Height = rect.Height;
        //    sRectangle.Width = rect.Width;
        //}

        public static implicit operator Rectangle(RECT rect) {
            return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static implicit operator RECT(Rectangle rect) {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}