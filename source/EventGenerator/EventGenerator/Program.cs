using Terminal.Gui;
using System;
using System.Globalization;
using System.Net.Http.Headers;
using EventGenerator.Handlers;
using EventGenerator.Modules;

namespace EventGenerator
{
    internal partial class Program
    {
        private static string _version = "1.0-preview";
        private static string _fileName = string.Empty;
        private static TextView _textView;
        private static bool _saved = false;
        private static ScrollBarView _scrollBar;
        private static byte[] _originalText;
        private static string _textToFind;
        private static string _textToReplace;
        private static bool _matchCase;
        private static bool _matchWholeWord;
        private static Window _win;
        private static Window _winDialog;
        private static TabView _tabView;
        private static MenuItem _miForceMinimumPosToZero;
        private static bool _forceMinimumPosToZero = true;
        private static List<CultureInfo> _cultureInfos;

        private static List<KeyValuePair<string, string>> repositoryTree;

        static async Task Main(string[] args)
        {
            if (OperatingSystem.IsWindows())
            {
                Console.SetWindowSize(160, 80);
            }

            var dictRepositoryTree = await GitHubTreeHandler.GetRepositoryTree();
            repositoryTree = dictRepositoryTree.ToList<KeyValuePair<string, string>>();

            Application.Init();

            _cultureInfos = Application.SupportedCultures;

            _win = new Window(_fileName ?? "Untitled")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            //_win.ColorScheme = new ColorScheme()
            //{
            //    Disabled = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black),
            //    HotFocus = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black),
            //    Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            //    Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Black),
            //    HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black)
            //};
            _win.ColorScheme = Colors.Base;

            Application.Top.Add(_win);

            _textView = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BottomOffset = 1,
                RightOffset = 1
            };

            InitFile();

            var siCursorPosition = new StatusItem(Key.Null, "", null);

            _textView.UnwrappedCursorPosition += (e) => {
                siCursorPosition.Title = $"Ln {e.Y + 1}, Col {e.X + 1}";
            };

            _textView.WordWrap = true;

            _win.Add(_textView);

            Events events = new Events(repositoryTree);

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_New", "", () => New()),
                    new MenuItem ("_Open", "", () => Open()),
                    new MenuItem ("_Save", "", () => Save()),
                    new MenuItem ("_Save As", "", () => SaveAs()),
                    new MenuItem ("_Close", "", () => CloseFile()),
                    null,
                    new MenuItem ("_Quit", "", () => Quit()),
                }),
                new MenuBarItem ("_Edit", new MenuItem [] {
                    new MenuItem ("_Copy", "", () => Copy(),null,null, Key.CtrlMask | Key.C),
                    new MenuItem ("C_ut", "", () => Cut(),null,null, Key.CtrlMask | Key.W),
                    new MenuItem ("_Paste", "", () => Paste(),null,null, Key.CtrlMask | Key.Y),
                    null,
                    new MenuItem ("_Find", "", () => Find(),null,null, Key.CtrlMask | Key.S),
                    new MenuItem ("Find _Next", "", () => FindNext(),null,null, Key.CtrlMask | Key.ShiftMask | Key.S),
                    new MenuItem ("Find P_revious", "", () => FindPrevious(),null,null, Key.CtrlMask | Key.ShiftMask | Key.AltMask | Key.S),
                    new MenuItem ("_Replace", "", () => Replace(),null,null, Key.CtrlMask | Key.R),
                    new MenuItem ("Replace Ne_xt", "", () => ReplaceNext(),null,null, Key.CtrlMask | Key.ShiftMask | Key.R),
                    new MenuItem ("Replace _All", "", () => ReplaceAll(),null,null, Key.CtrlMask | Key.ShiftMask | Key.AltMask | Key.A),
                    null,
                    new MenuItem ("_Select All", "", () => SelectAll(),null,null, Key.CtrlMask | Key.T)
                }),
                new MenuBarItem("E_vents", new MenuItem []
                {
                    new MenuItem ("_Generate system source events", "", async () => await events.DisplayDialogAsync()),
                    new MenuItem ("_Send events to Azure Event Grid", "", () => new NotImplementedException()),
                }),
                new MenuBarItem ("Forma_t", new MenuItem [] {
                    CreateWrapChecked(),
                })
            });

            Application.Top.Add(menu);

            var statusBar = new StatusBar(new StatusItem[] {
                siCursorPosition,
                new StatusItem(Key.F2, "~F2~ Open", () => Open()),
                new StatusItem(Key.F3, "~F3~ Save", () => Save()),
                new StatusItem(Key.F4, "~F4~ Save As", () => SaveAs()),
                new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Quit()),
                new StatusItem(Key.Null, $"OS Clipboard IsSupported : {Clipboard.IsSupported}", null),
                new StatusItem(Key.Null, $"Version : {_version}", null)
            });
            Application.Top.Add(statusBar);

            _scrollBar = new ScrollBarView(_textView, true);

            _scrollBar.ChangedPosition += () => {
                _textView.TopRow = _scrollBar.Position;
                if (_textView.TopRow != _scrollBar.Position)
                {
                    _scrollBar.Position = _textView.TopRow;
                }
                _textView.SetNeedsDisplay();
            };

            _scrollBar.OtherScrollBarView.ChangedPosition += () => {
                _textView.LeftColumn = _scrollBar.OtherScrollBarView.Position;
                if (_textView.LeftColumn != _scrollBar.OtherScrollBarView.Position)
                {
                    _scrollBar.OtherScrollBarView.Position = _textView.LeftColumn;
                }
                _textView.SetNeedsDisplay();
            };

            _scrollBar.VisibleChanged += () => {
                if (_scrollBar.Visible && _textView.RightOffset == 0)
                {
                    _textView.RightOffset = 1;
                }
                else if (!_scrollBar.Visible && _textView.RightOffset == 1)
                {
                    _textView.RightOffset = 0;
                }
            };

            _scrollBar.OtherScrollBarView.VisibleChanged += () => {
                if (_scrollBar.OtherScrollBarView.Visible && _textView.BottomOffset == 0)
                {
                    _textView.BottomOffset = 1;
                }
                else if (!_scrollBar.OtherScrollBarView.Visible && _textView.BottomOffset == 1)
                {
                    _textView.BottomOffset = 0;
                }
            };

            _textView.DrawContent += (e) => {
                _scrollBar.Size = _textView.Lines;
                _scrollBar.Position = _textView.TopRow;
                if (_scrollBar.OtherScrollBarView != null)
                {
                    _scrollBar.OtherScrollBarView.Size = _textView.Maxlength;
                    _scrollBar.OtherScrollBarView.Position = _textView.LeftColumn;
                }
                _scrollBar.LayoutSubviews();
                _scrollBar.Refresh();
            };

            _win.KeyPress += (e) => {
                var keys = ShortcutHelper.GetModifiersKey(e.KeyEvent);
                if (_winDialog != null && (e.KeyEvent.Key == Key.Esc
                    || e.KeyEvent.Key == (Key.Q | Key.CtrlMask)))
                {
                    DisposeWinDialog();
                }
                else if (e.KeyEvent.Key == (Key.Q | Key.CtrlMask))
                {
                    Quit();
                    e.Handled = true;
                }
                else if (_winDialog != null && keys == (Key.Tab | Key.CtrlMask))
                {
                    if (_tabView.SelectedTab == _tabView.Tabs.ElementAt(_tabView.Tabs.Count - 1))
                    {
                        _tabView.SelectedTab = _tabView.Tabs.ElementAt(0);
                    }
                    else
                    {
                        _tabView.SwitchTabBy(1);
                    }
                    e.Handled = true;
                }
                else if (_winDialog != null && keys == (Key.Tab | Key.CtrlMask | Key.ShiftMask))
                {
                    if (_tabView.SelectedTab == _tabView.Tabs.ElementAt(0))
                    {
                        _tabView.SelectedTab = _tabView.Tabs.ElementAt(_tabView.Tabs.Count - 1);
                    }
                    else
                    {
                        _tabView.SwitchTabBy(-1);
                    }
                    e.Handled = true;
                }
            };

            Application.Top.Closed += (_) => Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Application.Run();
        }

        private static void InitFile()
        {
            _win.Title = "Untitled.txt";
            _fileName = null;
            _originalText = new System.IO.MemoryStream().ToArray();
            _textView.Text = _originalText;
        }

        private static void DisposeWinDialog()
        {
            _winDialog.Dispose();
            _win.Remove(_winDialog);
            _winDialog = null;
        }
    }
}
