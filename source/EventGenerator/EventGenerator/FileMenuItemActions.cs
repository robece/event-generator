using System;
using Terminal.Gui;

namespace EventGenerator
{
	internal partial class Program
    {
        private static void LoadFile()
        {
            if (_fileName != null)
            {
                // FIXED: BUGBUG: #452 TextView.LoadFile keeps file open and provides no way of closing it
                _textView.LoadFile(_fileName);
                //_textView.Text = System.IO.File.ReadAllText (_fileName);
                _originalText = _textView.Text.ToByteArray();
                _win.Title = _fileName;
                _saved = false;
            }
        }

        private static bool CanCloseFile()
        {
            if (_textView.Text == _originalText)
            {
                //System.Diagnostics.Debug.Assert (!_textView.IsDirty);
                return true;
            }

            System.Diagnostics.Debug.Assert(_textView.IsDirty);

            var r = MessageBox.ErrorQuery("Save File",
                $"Do you want save changes in {_win.Title}?", "Yes", "No", "Cancel");
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

        private static void New(bool checkChanges = true)
        {
            if (checkChanges && !CanCloseFile())
            {
                return;
            }

            _win.Title = "Untitled.txt";
            _fileName = null;
            _originalText = new System.IO.MemoryStream().ToArray();
            _textView.Text = _originalText;
        }

        private static void Open()
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

        private static bool Save()
        {
            if (_fileName != null)
            {
                // FIXED: BUGBUG: #279 TextView does not know how to deal with \r\n, only \r 
                // As a result files saved on Windows and then read back will show invalid chars.
                return SaveFile(_win.Title.ToString(), _fileName);
            }
            else
            {
                return SaveAs();
            }
        }

        private static bool SaveAs()
        {
            var aTypes = new List<string>() { ".txt", ".json", ".*" };
            var sd = new SaveDialog("Save file", "Choose the path where to save the file.", aTypes);
            sd.FilePath = System.IO.Path.Combine(sd.FilePath.ToString(), _win.Title.ToString());
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

        private static bool SaveFile(string title, string file)
        {
            try
            {
                _win.Title = title;
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

        private static void CloseFile()
        {
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

        private static void Quit()
        {
            if (!CanCloseFile())
            {
                return;
            }

            Application.RequestStop();
        }
    }
}

