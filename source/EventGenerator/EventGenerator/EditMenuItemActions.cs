using System;
using Terminal.Gui;

namespace EventGenerator
{
    internal partial class Program
    {
        private static void Copy()
        {
            if (_textView != null)
            {
                _textView.Copy();
            }
        }

        private static void Cut()
        {
            if (_textView != null)
            {
                _textView.Cut();
            }
        }

        private static void Paste()
        {
            if (_textView != null)
            {
                _textView.Paste();
            }
        }

        private static void Find()
        {
            CreateFindWindow();
        }

        private static void FindNext()
        {
            ContinueFind();
        }

        private static void FindPrevious()
        {
            ContinueFind(false);
        }

        private static void ContinueFind(bool next = true, bool replace = false)
        {
            if (!replace && string.IsNullOrEmpty(_textToFind))
            {
                Find();
                return;
            }
            else if (replace && (string.IsNullOrEmpty(_textToFind)
                || (_winDialog == null && string.IsNullOrEmpty(_textToReplace))))
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

        private static void Replace()
        {
            CreateReplaceWindow();
        }

        private static void ReplaceNext()
        {
            ContinueFind(true, true);
        }

        private static void ReplacePrevious()
        {
            ContinueFind(false, true);
        }

        private static void ReplaceAll()
        {
            if (string.IsNullOrEmpty(_textToFind) || (string.IsNullOrEmpty(_textToReplace) && _winDialog == null))
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

        private static void SelectAll()
        {
            _textView.SelectAll();
        }

        private static MenuItem CreateWrapChecked()
        {
            var item = new MenuItem
            {
                Title = "Word Wrap"
            };
            item.CheckType |= MenuItemCheckStyle.Checked;
            item.Checked = _textView.WordWrap;
            item.Action += () => {
                _textView.WordWrap = item.Checked = !item.Checked;
                if (_textView.WordWrap)
                {
                    _scrollBar.OtherScrollBarView.ShowScrollIndicator = false;
                    _textView.BottomOffset = 0;
                }
                else
                {
                    _textView.BottomOffset = 1;
                }
            };

            return item;
        }

        private static void SetFindText()
        {
            _textToFind = string.IsNullOrEmpty(_textToFind) ? "" : _textToFind;
        }

        private static void SetReplaceText()
        {
            _textToReplace = string.IsNullOrEmpty(_textToReplace) ? "" : _textToReplace;
        }

        private static void CreateFindWindow()
        {
            if (_winDialog != null)
            {
                _winDialog.SetFocus();
                return;
            }

            _winDialog = new Window("Find")
            {
                X = _win.Bounds.Width / 2 - 30,
                Y = _win.Bounds.Height / 2 - 10,
                Width = 66,
                Height = 11,
                ColorScheme = Colors.Dialog
            };
            _winDialog.Border.Effect3D = true;
            _winDialog.Add(FindView());
            _win.Add(_winDialog);

            _winDialog.SuperView.BringSubviewToFront(_winDialog);
            _winDialog.SetFocus();
        }

        private static View FindView()
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
                _textToFind = txtToFind.Text.ToString();
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

        private static void CreateReplaceWindow()
        {
            if (_winDialog != null)
            {
                _winDialog.SetFocus();
                return;
            }

            _winDialog = new Window("Replace")
            {
                X = _win.Bounds.Width / 2 - 30,
                Y = _win.Bounds.Height / 2 - 10,
                Width = 66,
                Height = 15,
                ColorScheme = Colors.Dialog
            };
            _winDialog.Border.Effect3D = true;
            _winDialog.Add(ReplaceView());
            _win.Add(_winDialog);

            _winDialog.SuperView.BringSubviewToFront(_winDialog);
            _winDialog.SetFocus();
        }

        private static View ReplaceView()
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
            txtToReplace.TextChanged += (e) => _textToReplace = txtToReplace.Text.ToString();
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
                _textToFind = txtToFind.Text.ToString();
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
    }
}

