using EventGenerator.Common;
using EventGenerator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace EventGenerator.Modules
{
    internal class Publisher
    {
        #region private members

        #endregion

        #region private controls

        private Dialog? _dialog = null;
        private Dialog? _dialogBgProc = null;
        private Button? _btnCancel = null;
        private Button? _btnPublish = null;
        private Label? _lblEndpoint = null;
        private TextField? _txtAzureFunctionEndpoint = null;
        private CheckBox? _chkRememberAzureFunctionEndpoint = null;
        private Label? _lblDelayMs = null;
        private TextField? _txtAzureFunctionDelayMs = null;

        #endregion

        #region constructor

        public Publisher()
        {
        }

        #endregion

        #region public methods

        public void DisplayDialog(string scenario)
        {
            CreateDialog(scenario);
        }

        #endregion

        #region private methods

        private void CreateDialog(string scenario)
        {
            var buttons = new List<Button>();

            _btnCancel = new Button("_Cancel", false);
            _btnCancel.Visible = true;
            _btnCancel.TextAlignment = TextAlignment.Centered;
            _btnCancel.Clicked += _btnCancel_Clicked;
            buttons.Add(_btnCancel);

            _btnPublish = new Button("_Publish", true);
            _btnPublish.Visible = true;
            _btnPublish.TextAlignment = TextAlignment.Centered;
            _btnPublish.Clicked += _btnPublish_Clicked;
            buttons.Add(_btnPublish);

            _dialog = new Dialog(string.Empty, buttons.ToArray());

            switch (scenario)
            {
                case "function":
                    _dialog.Title = "Publish to Azure Function";

                    _lblEndpoint = new Label("Endpoint:")
                    {
                        X = 1,
                        Y = 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 15,
                        Height = 1,
                        Visible = true
                    };
                    _dialog.Add(_lblEndpoint);

                    _txtAzureFunctionEndpoint = new TextField("")
                    {
                        X = 1,
                        Y = Pos.Bottom(_lblEndpoint) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 80,
                        Height = 1,
                        Visible = true
                    };
                    _dialog.Add(_txtAzureFunctionEndpoint);

                    _chkRememberAzureFunctionEndpoint = new CheckBox("Remember this endpoint", false)
                    {
                        X = 1,
                        Y = Pos.Bottom(_txtAzureFunctionEndpoint) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 50,
                        Height = 1,
                        Visible = true
                    };
                    _dialog.Add(_chkRememberAzureFunctionEndpoint);

                    _lblDelayMs = new Label("Delay between events (in milliseconds, 60000 ms max.):")
                    {
                        X = 1,
                        Y = Pos.Bottom(_chkRememberAzureFunctionEndpoint) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 15,
                        Height = 1,
                        Visible = true
                    };
                    _dialog.Add(_lblDelayMs);

                    _txtAzureFunctionDelayMs = new TextField("")
                    {
                        X = 1,
                        Y = Pos.Bottom(_lblDelayMs) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 10,
                        Height = 1,
                        Visible = true,
                        Text = "50",

                    };
                    _dialog.Add(_txtAzureFunctionDelayMs);

                    _txtAzureFunctionDelayMs.TextChanged += (e) =>
                    {
                        var strDelayMs = _txtAzureFunctionDelayMs.Text.ToString();
                        if (string.IsNullOrEmpty(strDelayMs))
                            return;

                        if (Regex.IsMatch(strDelayMs, "[^0-9]+"))
                        {
                            var cp = _txtAzureFunctionDelayMs.CursorPosition;
                            _txtAzureFunctionDelayMs.Text = e;
                            _txtAzureFunctionDelayMs.CursorPosition = Math.Min(cp, _txtAzureFunctionDelayMs.Text.RuneCount);

                            MessageBox.Query("Error", "Input value is not a number", "Ok");

                            return;
                        }

                        if (_txtAzureFunctionDelayMs.Text.Length > 5)
                        {
                            var cp = _txtAzureFunctionDelayMs.CursorPosition;
                            _txtAzureFunctionDelayMs.Text = e;
                            _txtAzureFunctionDelayMs.CursorPosition = Math.Min(cp, _txtAzureFunctionDelayMs.Text.RuneCount);

                            MessageBox.Query("Error", "Max length reached", "Ok");

                            return;
                        }

                        int delayMs = Convert.ToInt32(_txtAzureFunctionDelayMs.Text.ToString());
                        if (delayMs > 60000)
                        {
                            var cp = _txtAzureFunctionDelayMs.CursorPosition;
                            _txtAzureFunctionDelayMs.Text = e;
                            _txtAzureFunctionDelayMs.CursorPosition = Math.Min(cp, _txtAzureFunctionDelayMs.Text.RuneCount);

                            MessageBox.Query("Error", "Max value reached", "Ok");

                            return;
                        }
                    };

                    _txtAzureFunctionEndpoint.SetFocus();

                    break;
            }

            var settings = Common.Utils.GetSettings();

            if (settings == null)
                return;

            if (_txtAzureFunctionEndpoint == null)
                return;

            _txtAzureFunctionEndpoint.Text = (string.IsNullOrEmpty(settings.AzureFunctionEndpoint)) ? string.Empty : settings.AzureFunctionEndpoint;

            if (_chkRememberAzureFunctionEndpoint == null)
                return;

            _chkRememberAzureFunctionEndpoint.Checked = settings.RememberAzureFunctionEndpoint;

            Application.Run(_dialog);
        }

        private void _btnCancel_Clicked()
        {
            Application.RequestStop();
        }

        private void _btnPublish_Clicked()
        {
            if (_chkRememberAzureFunctionEndpoint == null)
                return;

            var endpoint = string.Empty;
            if (_txtAzureFunctionEndpoint != null)
                if (!string.IsNullOrEmpty(_txtAzureFunctionEndpoint.Text.ToString()))
                    endpoint = _txtAzureFunctionEndpoint.Text.ToString();

            if (string.IsNullOrEmpty(endpoint))
                return;

            if (_chkRememberAzureFunctionEndpoint.Checked)
            {
                var settings = Utils.GetSettings();

                if (settings == null)
                {
                    MessageBox.ErrorQuery("Error", $"There was an error reading the settings file.", "Ok");
                    return;
                }

                settings.AzureFunctionEndpoint = endpoint;
                settings.RememberAzureFunctionEndpoint = true;
                Utils.UpdateSettings(settings);
            }
            else
            {
                var settings = new Settings() { AzureFunctionEndpoint = string.Empty, RememberAzureFunctionEndpoint = false };
                Utils.UpdateSettings(settings);
            }

            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), PrepareBackgroundProcess);
            StartBackgroundProcessDialog();
        }

        bool PrepareBackgroundProcess(MainLoop _)
        {
            if (_dialogBgProc != null)
            {
                var endpoint = string.Empty;
                if (_txtAzureFunctionEndpoint != null)
                    if (!string.IsNullOrEmpty(_txtAzureFunctionEndpoint.Text.ToString()))
                        endpoint = _txtAzureFunctionEndpoint.Text.ToString();

                if (string.IsNullOrEmpty(endpoint))
                {
                    Application.RequestStop();
                    MessageBox.ErrorQuery("Error", "There is no endpoint registered.", "Ok");
                    return false;
                }

                var delayMs = 0;
                if (_txtAzureFunctionDelayMs != null)
                    Convert.ToInt32(_txtAzureFunctionDelayMs.Text.ToString());

                var content = string.Empty;
                if (Editor._textView != null)
                    if (!string.IsNullOrEmpty(Editor._textView.Text.ToString()))
                        content = Editor._textView.Text.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    Application.RequestStop();
                    MessageBox.ErrorQuery("Error", "There are no events in the editor.", "Ok");
                    return false;
                }

                StringBuilder sb = new StringBuilder();

                Application.MainLoop.Invoke(() =>
                {
                    try
                    {
                        var token = JToken.Parse(content);

                        if (token is JArray)
                        {
                            var events = token.ToObject<List<ExpandoObject>>();
                            if (events != null)
                                if (events.Count > 0)
                                    foreach (dynamic e in events)
                                    {
                                        string json = JsonConvert.SerializeObject(e, Formatting.Indented);
                                        var res = SendEvent(endpoint, json);
                                        sb.Append(res.ToString());
                                        Thread.Sleep(delayMs);
                                    }
                                else if (token is JObject)
                                {
                                    string json = JsonConvert.SerializeObject(token, Formatting.Indented);
                                    var res = SendEvent(endpoint, json);
                                    sb.Append(res.ToString());
                                }

                            Application.RequestStop();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        sb.Append(ex.ToString());
                    }

                    if (sb.Length > 0)
                    {
                        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                        var filePath = Path.Combine(directoryPath, $"{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.txt");
                        try
                        {
                            MessageBox.ErrorQuery("Error", $"There were some exceptions during the publication, check the log file for more details: {filePath}.", "Ok");
                            Directory.CreateDirectory(directoryPath);
                            File.WriteAllText(filePath, sb.ToString());
                        }
                        catch
                        {
                            MessageBox.ErrorQuery("Error", $"There was an error saving the file: {filePath}.", "Ok");
                        }
                        Application.RequestStop();
                    }
                });
            }

            return _dialogBgProc == null;
        }

        private void StartBackgroundProcessDialog()
        {
            _dialogBgProc = new Dialog("Notification");

            var _lblTitle = new Label("Please wait. Sending events.")
            {
                X = 1,
                Y = 1,
                TextAlignment = TextAlignment.Left,
                AutoSize = true,
                Visible = true
            };
            _dialogBgProc.Add(_lblTitle);

            var _lblSubtitle = new Label("This window will disappear once the request is completed.")
            {
                X = 1,
                Y = Pos.Bottom(_lblTitle),
                TextAlignment = TextAlignment.Left,
                AutoSize = true,
                Visible = true
            };
            _dialogBgProc.Add(_lblSubtitle);

            Application.Run(_dialogBgProc);
        }

        private async Task<StringBuilder> SendEvent(string endpoint, string json)
        {
            var httpClient = new HttpClient();

            StringBuilder sb = new StringBuilder();
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Add("aeg-event-type", "Notification");
            var httpResponseMessage = await httpClient.PostAsync(endpoint, requestContent);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                sb.AppendLine("event error:");
                sb.AppendLine(json);
                sb.AppendLine(httpResponseMessage.ReasonPhrase);
                sb.AppendLine(httpResponseMessage.StatusCode.ToString());
            }

            httpClient.Dispose();

            return sb;
        }
    }

    #endregion
}
