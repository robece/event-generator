using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace EventGenerator.Modules
{
    public class Publisher
    {
        #region private members

        #endregion

        #region private controls

        private Dialog? _dialog = null;
        private Dialog? _dialogBgProc = null;
        private Button? _btnCancel = null;
        private Button? _btnPublish = null;
        private Label? _lblEndpoint = null;
        private TextField? _txtEndpoint = null;
        private Label? _lblDelayMs = null;
        private TextField? _txtDelayMs = null;

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
            _btnCancel.Clicked += _btnCancel_Clicked; ;
            buttons.Add(_btnCancel);

            _btnPublish = new Button("_Publish", true);
            _btnPublish.Visible = true;
            _btnPublish.TextAlignment = TextAlignment.Centered;
            _btnPublish.Clicked += _btnPublish_Clicked; ;
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

                    _txtEndpoint = new TextField("")
                    {
                        X = 1,
                        Y = Pos.Bottom(_lblEndpoint) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 50,
                        Height = 1,
                        Visible = true
                    };
                    _dialog.Add(_txtEndpoint);

                    _lblDelayMs = new Label("Delay between events (in milliseconds):")
                    {
                        X = 1,
                        Y = Pos.Bottom(_txtEndpoint) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 15,
                        Height = 1,
                        Visible = true
                    };
                    _dialog.Add(_lblDelayMs);

                    _txtDelayMs = new TextField("")
                    {
                        X = 1,
                        Y = Pos.Bottom(_lblDelayMs) + 1,
                        TextAlignment = TextAlignment.Left,
                        Width = 5,
                        Height = 1,
                        Visible = true,
                        Text = "50",

                    };
                    _dialog.Add(_txtDelayMs);

                    _txtDelayMs.TextChanged += (e) =>
                    {
                        var strDelayMs = _txtDelayMs.Text.ToString();
                        if (string.IsNullOrEmpty(strDelayMs))
                            return;

                        if (Regex.IsMatch(strDelayMs, "[^0-9]+"))
                        {
                            var cp = _txtDelayMs.CursorPosition;
                            _txtDelayMs.Text = e;
                            _txtDelayMs.CursorPosition = Math.Min(cp, _txtDelayMs.Text.RuneCount);

                            MessageBox.Query("Error", "Input value is not a number", "Ok");

                            return;
                        }

                        if (_txtDelayMs.Text.Length > 5)
                        {
                            var cp = _txtDelayMs.CursorPosition;
                            _txtDelayMs.Text = e;
                            _txtDelayMs.CursorPosition = Math.Min(cp, _txtDelayMs.Text.RuneCount);

                            MessageBox.Query("Error", "Max length reached", "Ok");

                            return;
                        }

                        int delayMs = Convert.ToInt16(_txtDelayMs.Text.ToString());
                        if (delayMs > 99999)
                        {
                            var cp = _txtDelayMs.CursorPosition;
                            _txtDelayMs.Text = e;
                            _txtDelayMs.CursorPosition = Math.Min(cp, _txtDelayMs.Text.RuneCount);

                            MessageBox.Query("Error", "Max value reached", "Ok");

                            return;
                        }
                    };

                    _txtEndpoint.SetFocus();

                    break;
            }

            Application.Run(_dialog);
        }

        private void _btnCancel_Clicked()
        {
            Application.RequestStop();
        }

        private void _btnPublish_Clicked()
        {
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), PrepareBackgroundProcess);
            StartBackgroundProcessDialog();
        }

        bool PrepareBackgroundProcess(MainLoop _)
        {
            if (_dialogBgProc != null)
            {
                Application.MainLoop.Invoke(async () =>
                {
                    try
                    {
                        if (_txtDelayMs == null)
                            return;

                        var delayMs = Convert.ToInt16(_txtDelayMs.Text.ToString());

                        if (_txtEndpoint == null)
                            return;

                        var endpoint = _txtEndpoint.Text.ToString();

                        if (Editor._textView == null)
                            return;

                        var content = Editor._textView.Text.ToString();

                        if (string.IsNullOrEmpty(content))
                            return;

                        var httpClient = new HttpClient();
                        var token = JToken.Parse(content);

                        if (token is JArray)
                        {
                            var events = token.ToObject<List<ExpandoObject>>();
                            if (events != null)
                                if (events.Count > 0)
                                    foreach (dynamic e in events)
                                    {
                                        await Task.Delay(delayMs).ContinueWith(async (t) =>
                                        {
                                            var httpClient = new HttpClient();
                                            string json = JsonConvert.SerializeObject(e, Formatting.Indented);
                                            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                                            httpClient.DefaultRequestHeaders.Add("aeg-event-type", "Notification");
                                            var httpResponseMessage = await httpClient.PostAsync(endpoint, requestContent);
                                            httpResponseMessage.EnsureSuccessStatusCode();

                                            var res = await httpResponseMessage.Content.ReadAsStringAsync();
                                            httpClient = null;
                                        });
                                    }
                        }
                        else if (token is JObject)
                        {

                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    Application.RequestStop();
                });
            }
            return _dialogBgProc == null;
        }

        private void StartBackgroundProcessDialog()
        {
            _dialogBgProc = new Dialog("Notification");

            var _lblTitle = new Label("Please wait. Your request has been successfully submitted and is being processed.")
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
    }

    #endregion
}
