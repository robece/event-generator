using System;
using System.Collections;
using EventGenerator.Handlers;
using Terminal.Gui;

namespace EventGenerator.Modules
{
    public class Editor
    {
        #region private members

        private string _fileName = string.Empty;
        private bool _saved = false;
        private byte[] _originalText = new byte[0];
        private string _textToFind = string.Empty;
        private string _textToReplace = string.Empty;
        private bool _matchCase = false;
        private bool _matchWholeWord = false;

        #endregion

        #region private controls

        private Window? _editorWindow = null;
        private ScrollBarView? _scrollBarView = null;
        private Window? _findReplaceWindow = null;
        private TextView? _textView = null;

        #endregion

        #region constructor

        public Editor()
        {
        }

        #endregion

        #region public methods

        public void DisplayEditorWindow()
        {
            CreateEditorWindow();
        }
        
        #endregion

        #region private methods

        private void CreateEditorWindow()
        {
            _editorWindow = new Window(_fileName ?? "Untitled")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            _editorWindow.ColorScheme = new ColorScheme()
            {
                Disabled = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black),
                HotFocus = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black),
                Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
                Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Black),
                HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black)
            };
            //_editorWindow.ColorScheme = Colors.Base;

            Application.Top.Add(_editorWindow);

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

            _editorWindow.Add(_textView);

            var generator = new Generator();

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem []
                {
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
                    new MenuItem ("_Generate system source events", "", async () => await generator.DisplayDialogAsync()),
                    new MenuItem ("_Send events to Azure Event Grid", "", () => new NotImplementedException()),
                }),
                new MenuBarItem ("Forma_t", new MenuItem [] {
                    CreateWrapChecked()
                })
            }); ;

            Application.Top.Add(menu);


            var statusBar = new StatusBar(new StatusItem[] {
                siCursorPosition,
                new StatusItem(Key.F2, "~F2~ Open", () => Open()),
                new StatusItem(Key.F3, "~F3~ Save", () => Save()),
                new StatusItem(Key.F4, "~F4~ Save As", () => SaveAs()),
                new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Quit()),
                new StatusItem(Key.Null, $"OS Clipboard IsSupported : {Clipboard.IsSupported}", null),
                new StatusItem(Key.Null, $"Version : {Program.Version}", null)
            });
            Application.Top.Add(statusBar);

            _scrollBarView = new ScrollBarView(_textView, true);

            _scrollBarView.ChangedPosition += () => {
                _textView.TopRow = _scrollBarView.Position;
                if (_textView.TopRow != _scrollBarView.Position)
                {
                    _scrollBarView.Position = _textView.TopRow;
                }
                _textView.SetNeedsDisplay();
            };

            _scrollBarView.OtherScrollBarView.ChangedPosition += () => {
                _textView.LeftColumn = _scrollBarView.OtherScrollBarView.Position;
                if (_textView.LeftColumn != _scrollBarView.OtherScrollBarView.Position)
                {
                    _scrollBarView.OtherScrollBarView.Position = _textView.LeftColumn;
                }
                _textView.SetNeedsDisplay();
            };

            _scrollBarView.VisibleChanged += () => {
                if (_scrollBarView.Visible && _textView.RightOffset == 0)
                {
                    _textView.RightOffset = 1;
                }
                else if (!_scrollBarView.Visible && _textView.RightOffset == 1)
                {
                    _textView.RightOffset = 0;
                }
            };

            _scrollBarView.OtherScrollBarView.VisibleChanged += () => {
                if (_scrollBarView.OtherScrollBarView.Visible && _textView.BottomOffset == 0)
                {
                    _textView.BottomOffset = 1;
                }
                else if (!_scrollBarView.OtherScrollBarView.Visible && _textView.BottomOffset == 1)
                {
                    _textView.BottomOffset = 0;
                }
            };

            _textView.DrawContent += (e) => {
                _scrollBarView.Size = _textView.Lines;
                _scrollBarView.Position = _textView.TopRow;
                if (_scrollBarView.OtherScrollBarView != null)
                {
                    _scrollBarView.OtherScrollBarView.Size = _textView.Maxlength;
                    _scrollBarView.OtherScrollBarView.Position = _textView.LeftColumn;
                }
                _scrollBarView.LayoutSubviews();
                _scrollBarView.Refresh();
            };

            _editorWindow.KeyPress += (e) => {
                var keys = ShortcutHelper.GetModifiersKey(e.KeyEvent);
                if (_findReplaceWindow != null && (e.KeyEvent.Key == Key.Esc
                    || e.KeyEvent.Key == (Key.Q | Key.CtrlMask)))
                {
                    DisposeWinDialog();
                }
                else if (e.KeyEvent.Key == (Key.Q | Key.CtrlMask))
                {
                    Quit();
                    e.Handled = true;
                }
            };
        }

        private void InitFile()
        {
            if (_editorWindow == null || _textView == null)
                return;

            _editorWindow.Title = "Untitled.txt";
            _fileName = string.Empty;
            _originalText = new System.IO.MemoryStream().ToArray();
            _textView.Text = _originalText;
        }

        private void New(bool checkChanges = true)
        {
            if (_editorWindow == null || _textView == null)
                return;

            if (checkChanges && !CanCloseFile())
            {
                return;
            }

            _editorWindow.Title = "Untitled.txt";
            _fileName = string.Empty;
            _originalText = new System.IO.MemoryStream().ToArray();
            _textView.Text = _originalText;
        }

        private void Open()
        {
            if (!CanCloseFile())
            {
                return;
            }
            var aTypes = new List<string>() { ".txt", ".json", ".*" };
            var d = new OpenDialog("Open", "Choose the path where to open the file.", aTypes) { AllowsMultipleSelection = false };
            Application.Run(d);

            if (!d.Canceled && d.FilePaths.Count > 0)
            {
                _fileName = d.FilePaths[0];
                LoadFile();
            }
        }

        private bool Save()
        {
            if (_editorWindow == null)
                return false;

            var title = _editorWindow.Title.ToString();

            if (string.IsNullOrEmpty(title))
                return false;

            if (!string.IsNullOrEmpty(_fileName))
            {
                return SaveFile(title, _fileName);
            }
            else
            {
                return SaveAs();
            }
        }

        private bool SaveAs()
        {
            var aTypes = new List<string>() { ".txt", ".json", ".*" };
            var sd = new SaveDialog("Save file", "Choose the path where to save the file.", aTypes);
            sd.FilePath = System.IO.Path.Combine(sd.FilePath.ToString(), _editorWindow.Title.ToString());
            Application.Run(sd);

            if (!sd.Canceled)
            {
                if (System.IO.File.Exists(sd.FilePath.ToString()))
                {
                    if (MessageBox.Query("Save File",
                        "File already exists. Overwrite any way?", "No", "Yes") == 1)
                    {
                        return SaveFile(sd.FileName.ToString(), sd.FilePath.ToString());
                    }
                    else
                    {
                        _saved = false;
                        return _saved;
                    }
                }
                else
                {
                    return SaveFile(sd.FileName.ToString(), sd.FilePath.ToString());
                }
            }
            else
            {
                _saved = false;
                return _saved;
            }
        }

        private void CloseFile()
        {
            if (_textView == null)
                return;

            if (!CanCloseFile())
            {
                return;
            }

            try
            {
                if (_saved)
                    _textView.CloseFile();
                New(false);
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", ex.Message, "Ok");
            }
        }

        private void Quit()
        {
            if (!CanCloseFile())
            {
                return;
            }

            Application.RequestStop();
        }

        private bool SaveFile(string title, string file)
        {
            if (_editorWindow == null || _textView == null)
                return false;

            try
            {
                _editorWindow.Title = title;
                _fileName = file;
                System.IO.File.WriteAllText(_fileName, _textView.Text.ToString());
                _originalText = _textView.Text.ToByteArray();
                _saved = true;
                _textView.ClearHistoryChanges();
                MessageBox.Query("Save File", "File was successfully saved.", "Ok");

                LoadFile();

            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", ex.Message, "Ok");
                return false;
            }

            return true;
        }

        private void LoadFile()
        {
            if (_editorWindow == null || _textView == null)
                return;

            if (_fileName != null)
            {
                _textView.LoadFile(_fileName);
                //_textView.Text = System.IO.File.ReadAllText (_fileName);
                _originalText = _textView.Text.ToByteArray();
                _editorWindow.Title = _fileName;
                _saved = false;
            }
        }

        private bool CanCloseFile()
        {
            if (_editorWindow == null || _textView == null)
                return false;

            if (_textView.Text == _originalText)
            {
                //System.Diagnostics.Debug.Assert (!_textView.IsDirty);
                return true;
            }

            //System.Diagnostics.Debug.Assert(_textView.IsDirty);

            var r = MessageBox.ErrorQuery("Save File",
                $"Do you want save changes in {_editorWindow.Title}?", "Yes", "No", "Cancel");
            if (r == 0)
            {
                return Save();
            }
            else if (r == 1)
            {
                return true;
            }

            return false;
        }

        private void Copy()
        {
            if (_textView != null)
            {
                _textView.Copy();
            }
        }

        private void Cut()
        {
            if (_textView != null)
            {
                _textView.Cut();
            }
        }

        private void Paste()
        {
            if (_textView != null)
            {
                _textView.Paste();
            }
        }

        private void Find()
        {
            DisplayFindWindow();
        }

        private void FindNext()
        {
            ContinueFind();
        }

        private void FindPrevious()
        {
            ContinueFind(false);
        }

        private void ContinueFind(bool next = true, bool replace = false)
        {
            if (_textView == null)
                return;

            if (!replace && string.IsNullOrEmpty(_textToFind))
            {
                Find();
                return;
            }
            else if (replace && (string.IsNullOrEmpty(_textToFind)
                || (_findReplaceWindow == null && string.IsNullOrEmpty(_textToReplace))))
            {
                Replace();
                return;
            }

            bool found;
            bool gaveFullTurn;

            if (next)
            {
                if (!replace)
                {
                    found = _textView.FindNextText(_textToFind, out gaveFullTurn, _matchCase, _matchWholeWord);
                }
                else
                {
                    found = _textView.FindNextText(_textToFind, out gaveFullTurn, _matchCase, _matchWholeWord,
                        _textToReplace, true);
                }
            }
            else
            {
                if (!replace)
                {
                    found = _textView.FindPreviousText(_textToFind, out gaveFullTurn, _matchCase, _matchWholeWord);
                }
                else
                {
                    found = _textView.FindPreviousText(_textToFind, out gaveFullTurn, _matchCase, _matchWholeWord,
                        _textToReplace, true);
                }
            }
            if (!found)
            {
                MessageBox.Query("Find", $"The following specified text was not found: '{_textToFind}'", "Ok");
            }
            else if (gaveFullTurn)
            {
                MessageBox.Query("Find", $"No more occurrences were found for the following specified text: '{_textToFind}'", "Ok");
            }
        }

        private void Replace()
        {
            CreateReplaceWindow();
        }

        private void ReplaceNext()
        {
            ContinueFind(true, true);
        }

        private void ReplacePrevious()
        {
            ContinueFind(false, true);
        }

        private void ReplaceAll()
        {
            if (_textView == null)
                return;

            if (string.IsNullOrEmpty(_textToFind) || (string.IsNullOrEmpty(_textToReplace) && _findReplaceWindow == null))
            {
                Replace();
                return;
            }

            if (_textView.ReplaceAllText(_textToFind, _matchCase, _matchWholeWord, _textToReplace))
            {
                MessageBox.Query("Replace All", $"All occurrences were replaced for the following specified text: '{_textToReplace}'", "Ok");
            }
            else
            {
                MessageBox.Query("Replace All", $"None of the following specified text was found: '{_textToFind}'", "Ok");
            }
        }

        private void SelectAll()
        {
            if (_textView == null)
                return;

            _textView.SelectAll();
        }

        private MenuItem CreateWrapChecked()
        {
            var item = new MenuItem
            {
                Title = "Word Wrap"
            };
            item.CheckType |= MenuItemCheckStyle.Checked;
            item.Checked = true;
            item.Action += () => {

                if (_textView == null || _scrollBarView == null)
                    return;

                _textView.WordWrap = item.Checked = !item.Checked;
                if (_textView.WordWrap)
                {
                    _scrollBarView.OtherScrollBarView.ShowScrollIndicator = false;
                    _textView.BottomOffset = 0;
                }
                else
                {
                    _textView.BottomOffset = 1;
                }
            };

            return item;
        }

        private void SetFindText()
        {
            _textToFind = string.IsNullOrEmpty(_textToFind) ? "" : _textToFind;
        }

        private void SetReplaceText()
        {
            _textToReplace = string.IsNullOrEmpty(_textToReplace) ? "" : _textToReplace;
        }

        private void DisplayFindWindow()
        {
            if (_editorWindow == null)
                return;

            if (_findReplaceWindow != null)
            {
                _findReplaceWindow.SetFocus();
                return;
            }

            _findReplaceWindow = new Window("Find")
            {
                X = _editorWindow.Bounds.Width / 2 - 30,
                Y = _editorWindow.Bounds.Height / 2 - 10,
                Width = 66,
                Height = 11
            };
            _findReplaceWindow.ColorScheme = Colors.Dialog;
            _findReplaceWindow.Border.Effect3D = true;
            _findReplaceWindow.Add(CreateFindView());
            _editorWindow.Add(_findReplaceWindow);

            _findReplaceWindow.SuperView.BringSubviewToFront(_findReplaceWindow);
            _findReplaceWindow.SetFocus();
        }

        private View CreateFindView()
        {
            var d = new View();
            d.DrawContent += (e) =>
            {
                foreach (var v in d.Subviews)
                {
                    v.SetNeedsDisplay();
                }
            };

            var findText = "Find:";
            var lblFind = new Label(findText)
            {
                X = 1,
                Y = 1,
                Width = findText.Length,
                TextAlignment = TextAlignment.Right,
                AutoSize = false
            };
            d.Add(lblFind);

            SetFindText();
            var txtToFind = new TextField(_textToFind)
            {
                X = 1,
                Y = Pos.Bottom(lblFind),
                Width = 20
            };
            txtToFind.Enter += (_) => txtToFind.Text = _textToFind;
            d.Add(txtToFind);

            var ckbMatchCase = new CheckBox("Match c_ase")
            {
                X = 1,
                Y = Pos.Top(txtToFind) + 2,
                Checked = _matchCase
            };
            ckbMatchCase.Toggled += (e) => _matchCase = ckbMatchCase.Checked;
            d.Add(ckbMatchCase);

            var ckbMatchWholeWord = new CheckBox("Match _whole word")
            {
                X = 1,
                Y = Pos.Top(ckbMatchCase) + 1,
                Checked = _matchWholeWord
            };
            ckbMatchWholeWord.Toggled += (e) => _matchWholeWord = ckbMatchWholeWord.Checked;
            d.Add(ckbMatchWholeWord);

            var btnFindNext = new Button("Find _Next")
            {
                X = 1,
                Y = Pos.Bottom(ckbMatchCase) + 2,
                Width = 20,
                Enabled = !txtToFind.Text.IsEmpty,
                TextAlignment = TextAlignment.Centered,
                IsDefault = true,
                AutoSize = false
            };
            btnFindNext.Clicked += () => FindNext();
            d.Add(btnFindNext);

            var btnFindPrevious = new Button("Find _Previous")
            {
                X = Pos.Right(btnFindNext) + 1,
                Y = Pos.Bottom(ckbMatchCase) + 2,
                Width = 20,
                Enabled = !txtToFind.Text.IsEmpty,
                TextAlignment = TextAlignment.Centered,
                AutoSize = false
            };
            btnFindPrevious.Clicked += () => FindPrevious();
            d.Add(btnFindPrevious);

            txtToFind.TextChanged += (e) =>
            {
                if (_textView == null)
                    return;

                var strTextToFind = txtToFind.Text.ToString();
                if (string.IsNullOrEmpty(strTextToFind))
                    return;

                _textToFind = strTextToFind;
                _textView.FindTextChanged();
                btnFindNext.Enabled = !txtToFind.Text.IsEmpty;
                btnFindPrevious.Enabled = !txtToFind.Text.IsEmpty;
            };

            var btnCancel = new Button("Cancel")
            {
                X = Pos.Right(btnFindPrevious) + 1,
                Y = Pos.Bottom(ckbMatchCase) + 2,
                Width = 20,
                TextAlignment = TextAlignment.Centered,
                AutoSize = false
            };

            btnCancel.Clicked += () =>
            {
                DisposeWinDialog();
            };
            d.Add(btnCancel);

            d.Width = 100;
            d.Height = 50;

            return d;
        }

        private void CreateReplaceWindow()
        {
            if (_editorWindow == null)
                return;

            if (_findReplaceWindow != null)
            {
                _findReplaceWindow.SetFocus();
                return;
            }

            _findReplaceWindow = new Window("Replace")
            {
                X = _editorWindow.Bounds.Width / 2 - 30,
                Y = _editorWindow.Bounds.Height / 2 - 10,
                Width = 66,
                Height = 15,
            };
            _findReplaceWindow.ColorScheme = Colors.Dialog;
            _findReplaceWindow.Border.Effect3D = true;
            _findReplaceWindow.Add(ReplaceView());
            _editorWindow.Add(_findReplaceWindow);

            _findReplaceWindow.SuperView.BringSubviewToFront(_findReplaceWindow);
            _findReplaceWindow.SetFocus();
        }

        private View ReplaceView()
        {
            var d = new View();
            d.DrawContent += (e) =>
            {
                foreach (var v in d.Subviews)
                {
                    v.SetNeedsDisplay();
                }
            };

            var findText = "Find:";
            var lblFind = new Label(findText)
            {
                X = 1,
                Y = 1,
                Width = findText.Length,
                TextAlignment = TextAlignment.Right,
                AutoSize = false
            };
            d.Add(lblFind);

            SetFindText();
            var txtToFind = new TextField(_textToFind)
            {
                X = 1,
                Y = Pos.Bottom(lblFind),
                Width = 20
            };
            txtToFind.Enter += (_) => txtToFind.Text = _textToFind;
            d.Add(txtToFind);

            var replaceText = "Replace:";

            var lblReplace = new Label(replaceText)
            {
                X = 1,
                Y = Pos.Bottom(txtToFind),
                Width = replaceText.Length,
                TextAlignment = TextAlignment.Right,
                AutoSize = false
            };
            d.Add(lblReplace);

            SetReplaceText();
            var txtToReplace = new TextField(_textToReplace)
            {
                X = 1,
                Y = Pos.Bottom(lblReplace),
                Width = 20
            };
            txtToReplace.TextChanged += (e) =>
            {
                var strTextToReplace = txtToReplace.Text.ToString();
                if (string.IsNullOrEmpty(strTextToReplace))
                    return;
                _textToReplace = strTextToReplace;
            };
            d.Add(txtToReplace);

            var ckbMatchCase = new CheckBox("Match c_ase")
            {
                X = 1,
                Y = Pos.Top(txtToReplace) + 2,
                Checked = _matchCase
            };
            ckbMatchCase.Toggled += (e) => _matchCase = ckbMatchCase.Checked;
            d.Add(ckbMatchCase);

            var ckbMatchWholeWord = new CheckBox("Match _whole word")
            {
                X = 1,
                Y = Pos.Top(ckbMatchCase) + 1,
                Checked = _matchWholeWord
            };
            ckbMatchWholeWord.Toggled += (e) => _matchWholeWord = ckbMatchWholeWord.Checked;
            d.Add(ckbMatchWholeWord);

            var btnReplaceNext = new Button("Replace _Next")
            {
                X = 1,
                Y = Pos.Bottom(ckbMatchCase) + 2,
                Width = 20,
                Enabled = !txtToFind.Text.IsEmpty,
                TextAlignment = TextAlignment.Centered,
                IsDefault = true,
                AutoSize = false
            };
            btnReplaceNext.Clicked += () => ReplaceNext();
            d.Add(btnReplaceNext);

            var btnReplacePrevious = new Button("Replace _Previous")
            {
                X = Pos.Right(btnReplaceNext) + 1,
                Y = Pos.Bottom(ckbMatchCase) + 2,
                Width = 20,
                Enabled = !txtToFind.Text.IsEmpty,
                TextAlignment = TextAlignment.Centered,
                AutoSize = false
            };
            btnReplacePrevious.Clicked += () => ReplacePrevious();
            d.Add(btnReplacePrevious);

            var btnReplaceAll = new Button("Replace _All")
            {
                X = Pos.Right(btnReplacePrevious) + 1,
                Y = Pos.Bottom(ckbMatchCase) + 2,
                Width = 20,
                Enabled = !txtToFind.Text.IsEmpty,
                TextAlignment = TextAlignment.Centered,
                AutoSize = false
            };
            btnReplaceAll.Clicked += () => ReplaceAll();
            d.Add(btnReplaceAll);

            txtToFind.TextChanged += (e) =>
            {
                if (_textView == null)
                    return;

                var strTextToFind = txtToFind.Text.ToString();
                if (string.IsNullOrEmpty(strTextToFind))
                    return;

                _textToFind = strTextToFind;
                _textView.FindTextChanged();
                btnReplaceNext.Enabled = !txtToFind.Text.IsEmpty;
                btnReplacePrevious.Enabled = !txtToFind.Text.IsEmpty;
                btnReplaceAll.Enabled = !txtToFind.Text.IsEmpty;
            };

            var btnCancel = new Button("Cancel")
            {
                X = Pos.Left(btnReplaceAll),
                Y = Pos.Bottom(btnReplaceAll) + 1,
                Width = 20,
                TextAlignment = TextAlignment.Centered,
                AutoSize = false
            };

            btnCancel.Clicked += () =>
            {
                DisposeWinDialog();
            };
            d.Add(btnCancel);

            d.Width = 100;
            d.Height = 50;

            return d;
        }

        private void DisposeWinDialog()
        {
            if (_editorWindow == null || _findReplaceWindow == null)
                return;

            _findReplaceWindow.Dispose();
            _editorWindow.Remove(_findReplaceWindow);
            _findReplaceWindow = null;
        }

        #endregion
    }
}
