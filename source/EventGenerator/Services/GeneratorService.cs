﻿using EventGenerator.Common;
using EventGenerator.Helpers;
using EventGenerator.Models;
using EventGenerator.Services.Interfaces;
using Jint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Polly;
using System.Collections;
using System.Dynamic;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace EventGenerator.Services
{
    internal class GeneratorService : IGeneratorService
    {
        #region private members

        private List<KeyValuePair<string, string>> _repositoryTree = new List<KeyValuePair<string, string>>();
        private string _stage = "sourceName"; // _stage: 1. sourceName, 2. versionType, 3. version, 4. eventSchema, 6. eventType, 7. review
        private string _selectedSourceName = string.Empty;
        private int _selectedSourceNameIdx = -1;
        private string _selectedVersionType = string.Empty;
        private int _selectedVersionTypeIdx = -1;
        private string _selectedVersion = string.Empty;
        private int _selectedVersionIdx = -1;
        private string _selectedEventSchema = string.Empty;
        private int _selectedEventSchemaIdx = -1;
        private string _selectedEventType = string.Empty;
        private int _selectedEventTypeIdx = -1;

        private IHost? _host = null;
        private readonly IHttpClientFactory? _httpClientFactory = null;

        #endregion

        #region private controls

        private Dialog? _dialog = null;
        private Dialog? _dialogBgProc = null;
        private Button? _btnCancel = null;
        private Button? _btnBack = null;
        private Button? _btnNext = null;
        private Label? _lblTitle = null;
        private ListView? _lvDetails = null;
        private ScrollBarView? _scrollBarView = null;
        private Label? _lblSelectedSourceName = null;
        private Label? _lblSelectedVersionType = null;
        private Label? _lblSelectedVersion = null;
        private Label? _lblSelectedEventSchema = null;
        private Label? _lblSelectedEventType = null;
        private Label? _lblNumberOfEvents = null;
        private TextField? _txtNumberOfEvents = null;
        private Label? _lblAPIKey = null;
        private TextField? _txtAPIKey = null;
        private CheckBox? _chkRememberAPIKey = null;

        #endregion

        #region constructor

        public GeneratorService(IHost host, IHttpClientFactory httpClientFactory)
        {
            _host = host;
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region public methods

        public async Task DisplayDialogAsync()
        {
            try
            {
                if (_host == null)
                    return;

                var gitHubTreeService = _host.Services.GetRequiredService<IGitHubTreeService>();
                var dictRepositoryTree = await gitHubTreeService.GetRepositoryTreeAsync();
                _repositoryTree = dictRepositoryTree.ToList<KeyValuePair<string, string>>();

                await CreateDialogAsync();
            }
            catch (Exception ex)
            {
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                var filePath = Path.Combine(directoryPath, $"{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.txt");
                try
                {
                    MessageBox.ErrorQuery("Error", $"There was an error, check the log file for more details: {filePath}.", "Ok");
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(filePath, ex.ToString());
                }
                catch
                {
                    MessageBox.ErrorQuery("Error", $"There was an error saving the file: {filePath}.", "Ok");
                }
                Application.RequestStop();
            }
        }

        #endregion

        #region private methods

        private async Task CreateDialogAsync()
        {
            var buttons = new List<Button>();

            _btnCancel = new Button("_Cancel", false);
            _btnCancel.Visible = true;
            _btnCancel.TextAlignment = TextAlignment.Centered;
            _btnCancel.Clicked += _btnCancel_Clicked;
            buttons.Add(_btnCancel);

            _btnBack = new Button("_Back", false);
            _btnCancel.Visible = true;
            _btnBack.Enabled = false;
            _btnBack.TextAlignment = TextAlignment.Centered;
            _btnBack.Clicked += _btnBack_Clicked;
            buttons.Add(_btnBack);

            _btnNext = new Button("_Next", true);
            _btnNext.Visible = true;
            _btnNext.TextAlignment = TextAlignment.Centered;
            _btnNext.Clicked += _btnNext_Clicked;
            buttons.Add(_btnNext);

            _dialog = new Dialog("Generate system source events", buttons.ToArray());

            var systemSourceText = "Select a system source:";
            _lblTitle = new Label(systemSourceText)
            {
                X = 1,
                Y = 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = true
            };
            _dialog.Add(_lblTitle);

            _lvDetails = new ListView()
            {
                X = 1,
                Y = Pos.Bottom(_lblTitle) + 1,
                Width = Dim.Fill() - 1,
                Height = Dim.Fill() - 2,
                Visible = true
            };

            if (_repositoryTree == null)
                return;

            var sources = EventSourceHelper.GetSources(_repositoryTree);
            await _lvDetails.SetSourceAsync((IList)sources);

            _dialog.Add(_lvDetails);

            _lblSelectedSourceName = new Label("System source:")
            {
                X = 1,
                Y = Pos.Bottom(_lblTitle) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSelectedSourceName);

            _lblSelectedVersionType = new Label("Version type:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSelectedSourceName) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSelectedVersionType);

            _lblSelectedVersion = new Label("Version:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSelectedVersionType) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSelectedVersion);

            _lblSelectedEventSchema = new Label("Event schema:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSelectedVersion) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSelectedEventSchema);

            _lblSelectedEventType = new Label("Event type:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSelectedEventSchema) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSelectedEventType);

            _lblNumberOfEvents = new Label("Number of events (Up to 500 events):")
            {
                X = 1,
                Y = Pos.Bottom(_lblSelectedEventType) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblNumberOfEvents);

            _txtNumberOfEvents = new TextField("")
            {
                X = 1,
                Y = Pos.Bottom(_lblNumberOfEvents) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 10,
                Height = 1,
                Visible = false,
                Text = "100"
            };
            _dialog.Add(_txtNumberOfEvents);

            _txtNumberOfEvents.TextChanged += (e) =>
            {
                var strNumberOfEvents = _txtNumberOfEvents.Text.ToString();
                if (string.IsNullOrEmpty(strNumberOfEvents))
                    return;

                if (Regex.IsMatch(strNumberOfEvents, "[^0-9]+"))
                {
                    var cp = _txtNumberOfEvents.CursorPosition;
                    _txtNumberOfEvents.Text = e;
                    _txtNumberOfEvents.CursorPosition = Math.Min(cp, _txtNumberOfEvents.Text.RuneCount);

                    MessageBox.Query("Error", "Input value is not a number.", "Ok");

                    return;
                }

                if (_txtNumberOfEvents.Text.Length > 3)
                {
                    var cp = _txtNumberOfEvents.CursorPosition;
                    _txtNumberOfEvents.Text = e;
                    _txtNumberOfEvents.CursorPosition = Math.Min(cp, _txtNumberOfEvents.Text.RuneCount);

                    MessageBox.Query("Error", "Max length reached.", "Ok");

                    return;
                }

                int numberOfEvents = Convert.ToInt16(_txtNumberOfEvents.Text.ToString());
                if (numberOfEvents > 500)
                {
                    var cp = _txtNumberOfEvents.CursorPosition;
                    _txtNumberOfEvents.Text = e;
                    _txtNumberOfEvents.CursorPosition = Math.Min(cp, _txtNumberOfEvents.Text.RuneCount);

                    MessageBox.Query("Error", "Max value reached.", "Ok");

                    return;
                }
            };

            _lblAPIKey = new Label("OpenAI API Key:")
            {
                X = 1,
                Y = Pos.Bottom(_txtNumberOfEvents) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblAPIKey);

            _txtAPIKey = new TextField("")
            {
                X = 1,
                Y = Pos.Bottom(_lblAPIKey) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 80,
                Height = 1,
                Visible = false,
                Text = string.Empty
            };
            _dialog.Add(_txtAPIKey);

            _txtAPIKey.TextChanged += (e) =>
            {
                if (_txtAPIKey.Text.Length > 80)
                {
                    var cp = _txtAPIKey.CursorPosition;
                    _txtAPIKey.Text = e;
                    _txtAPIKey.CursorPosition = Math.Min(cp, _txtAPIKey.Text.RuneCount);

                    MessageBox.Query("Error", "Max length reached.", "Ok");

                    return;
                }
            };

            _chkRememberAPIKey = new CheckBox("Remember this key", false)
            {
                X = 1,
                Y = Pos.Bottom(_txtAPIKey) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 50,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_chkRememberAPIKey);

            _scrollBarView = new ScrollBarView(_lvDetails, true);

            _scrollBarView.ChangedPosition += () =>
            {
                _lvDetails.TopItem = _scrollBarView.Position;
                if (_lvDetails.TopItem != _scrollBarView.Position)
                {
                    _scrollBarView.Position = _lvDetails.TopItem;
                }
                _lvDetails.SetNeedsDisplay();
            };

            _scrollBarView.OtherScrollBarView.ChangedPosition += () =>
            {
                _lvDetails.LeftItem = _scrollBarView.OtherScrollBarView.Position;
                if (_lvDetails.LeftItem != _scrollBarView.OtherScrollBarView.Position)
                {
                    _scrollBarView.OtherScrollBarView.Position = _lvDetails.LeftItem;
                }
                _lvDetails.SetNeedsDisplay();
            };

            _lvDetails.DrawContent += LvDetails_DrawContent;

            _btnNext.SetFocus();

            var settings = Common.Utils.GetSettings();

            if (settings == null)
                return;

            if (_txtAPIKey == null)
                return;

            _txtAPIKey.Text = (string.IsNullOrEmpty(settings.OpenAIAPIKey)) ? string.Empty : settings.OpenAIAPIKey;

            if (_chkRememberAPIKey == null)
                return;

            _chkRememberAPIKey.Checked = settings.RememberOpenAIAPIKey;

            Application.Run(_dialog);
        }

        private void LvDetails_DrawContent(Rect obj)
        {
            if (_scrollBarView == null || _lvDetails == null)
                return;

            _scrollBarView.Size = _lvDetails.Source.Count;
            _scrollBarView.Position = _lvDetails.TopItem;
            _scrollBarView.OtherScrollBarView.Size = _lvDetails.Maxlength;
            _scrollBarView.OtherScrollBarView.Position = _lvDetails.LeftItem;
            _scrollBarView.Refresh();
        }

        private void _btnCancel_Clicked()
        {
            Reset();
            Application.RequestStop();
        }

        private void Reset()
        {
            _stage = "sourceName";
            _selectedSourceName = string.Empty;
            _selectedSourceNameIdx = -1;
            _selectedVersionType = string.Empty;
            _selectedVersionTypeIdx = -1;
            _selectedVersion = string.Empty;
            _selectedVersionIdx = -1;
            _selectedEventSchema = string.Empty;
            _selectedEventSchemaIdx = -1;
            _selectedEventType = string.Empty;
            _selectedEventTypeIdx = -1;

            _lblSelectedSourceName = null;
            _lblSelectedVersionType = null;
            _lblSelectedVersion = null;
            _lblSelectedEventSchema = null;
            _lblSelectedEventType = null;
            _txtNumberOfEvents = null;
            _lblAPIKey = null;
            _txtAPIKey = null;
            _chkRememberAPIKey = null;
        }

        private async void _btnBack_Clicked()
        {
            if (_btnBack == null || _btnNext == null ||
                _lblTitle == null || _lvDetails == null ||
                _lblSelectedSourceName == null || _lblSelectedVersionType == null ||
                _lblSelectedVersion == null || _lblSelectedEventSchema == null || _lblSelectedEventType == null ||
                _lblNumberOfEvents == null || _txtNumberOfEvents == null || _lblAPIKey == null || _txtAPIKey == null || _chkRememberAPIKey == null)
                return;

            switch (_stage)
            {
                case "versionType":
                    _stage = "sourceName";
                    _btnBack.Enabled = false;
                    _btnNext.SetFocus();

                    _lblTitle.Text = "Select a system source:";
                    var sources = EventSourceHelper.GetSources(_repositoryTree);
                    await _lvDetails.SetSourceAsync((IList)sources);
                    _lvDetails.SelectedItem = _selectedSourceNameIdx;

                    _btnNext.Enabled = (sources.Count > 0) ? true : false;

                    break;

                case "version":
                    _stage = "versionType";
                    _btnBack.SetFocus();

                    _lblTitle.Text = "Select a version type:";
                    var versionTypes = EventSourceHelper.GetVersionTypes(_repositoryTree, _selectedSourceName);
                    await _lvDetails.SetSourceAsync((IList)versionTypes);
                    _lvDetails.SelectedItem = _selectedVersionTypeIdx;

                    _btnNext.Enabled = (versionTypes.Count > 0) ? true : false;

                    break;

                case "eventSchema":
                    _stage = "version";
                    _btnBack.SetFocus();

                    _lblTitle.Text = "Select a version:";
                    var versions = EventSourceHelper.GetVersions(_repositoryTree, _selectedSourceName, _selectedVersionType);
                    await _lvDetails.SetSourceAsync((IList)versions);
                    _lvDetails.SelectedItem = _selectedVersionIdx;

                    _btnNext.Enabled = (versions.Count > 0) ? true : false;

                    break;

                case "eventType":
                    _stage = "eventSchema";
                    _btnBack.SetFocus();

                    _lblTitle.Text = "Select an event schema:";
                    var eventSchemas = EventSourceHelper.GetEventSchemas(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion);
                    await _lvDetails.SetSourceAsync((IList)eventSchemas);
                    _lvDetails.SelectedItem = _selectedEventSchemaIdx;

                    _btnNext.Enabled = (eventSchemas.Count > 0) ? true : false;

                    break;

                case "review":
                    _stage = "eventType";
                    _btnBack.SetFocus();

                    _btnNext.Text = "_Next";
                    _lblTitle.Visible = true;
                    _lvDetails.Visible = true;
                    _lblSelectedSourceName.Visible = false;
                    _lblSelectedVersionType.Visible = false;
                    _lblSelectedVersion.Visible = false;
                    _lblSelectedEventSchema.Visible = false;
                    _lblSelectedEventType.Visible = false;
                    _lblNumberOfEvents.Visible = false;
                    _txtNumberOfEvents.Visible = false;
                    _lblAPIKey.Visible = false;
                    _txtAPIKey.Visible = false;
                    _chkRememberAPIKey.Visible = false;

                    _lblTitle.Text = "Select an event type:";
                    var eventTypes = EventSourceHelper.GetEventTypes(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion, _selectedEventSchema);
                    await _lvDetails.SetSourceAsync((IList)eventTypes);
                    _lvDetails.SelectedItem = _selectedEventTypeIdx;

                    _btnNext.Enabled = (eventTypes.Count > 0) ? true : false;

                    break;
            }
        }

        private async void _btnNext_Clicked()
        {
            if (_btnBack == null || _btnNext == null ||
            _lblTitle == null || _lvDetails == null ||
            _lblSelectedSourceName == null || _lblSelectedVersionType == null ||
            _lblSelectedVersion == null || _lblSelectedEventSchema == null || _lblSelectedEventType == null ||
            _lblNumberOfEvents == null || _txtNumberOfEvents == null || _lblAPIKey == null || _txtAPIKey == null || _chkRememberAPIKey == null)
                return;

            switch (_stage)
            {
                case "sourceName":
                    _stage = "versionType";
                    _btnBack.Enabled = true;
                    _btnNext.SetFocus();

                    _selectedSourceNameIdx = _lvDetails.SelectedItem;

                    var sources = EventSourceHelper.GetSources(_repositoryTree);
                    if (sources.Count > 0)
                        _selectedSourceName = sources[_selectedSourceNameIdx];


                    _lblTitle.Text = "Select a version type:";
                    var versionTypes = EventSourceHelper.GetVersionTypes(_repositoryTree, _selectedSourceName);
                    await _lvDetails.SetSourceAsync((IList)versionTypes);

                    _btnNext.Enabled = (versionTypes.Count > 0) ? true : false;

                    break;
                case "versionType":
                    _stage = "version";
                    _btnNext.SetFocus();

                    _selectedVersionTypeIdx = _lvDetails.SelectedItem;

                    versionTypes = EventSourceHelper.GetVersionTypes(_repositoryTree, _selectedSourceName);
                    if (versionTypes.Count > 0)
                        _selectedVersionType = versionTypes[_selectedVersionTypeIdx];

                    _lblTitle.Text = "Select a version:";
                    var versions = EventSourceHelper.GetVersions(_repositoryTree, _selectedSourceName, _selectedVersionType);
                    await _lvDetails.SetSourceAsync((IList)versions);

                    _btnNext.Enabled = (versions.Count > 0) ? true : false;

                    break;
                case "version":
                    _stage = "eventSchema";
                    _btnNext.SetFocus();

                    _selectedVersionIdx = _lvDetails.SelectedItem;

                    versions = EventSourceHelper.GetVersions(_repositoryTree, _selectedSourceName, _selectedVersionType);
                    if (versions.Count > 0)
                        _selectedVersion = versions[_selectedVersionIdx];

                    _lblTitle.Text = "Select an event schema:";
                    var eventSchemas = EventSourceHelper.GetEventSchemas(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion);
                    await _lvDetails.SetSourceAsync((IList)eventSchemas);

                    _btnNext.Enabled = (eventSchemas.Count > 0) ? true : false;

                    break;
                case "eventSchema":
                    _stage = "eventType";
                    _btnNext.SetFocus();

                    _selectedEventSchemaIdx = _lvDetails.SelectedItem;

                    eventSchemas = EventSourceHelper.GetEventSchemas(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion);
                    if (eventSchemas.Count > 0)
                        _selectedEventSchema = eventSchemas[_selectedEventSchemaIdx];

                    _lblTitle.Text = "Select an event type:";
                    var eventTypes = EventSourceHelper.GetEventTypes(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion, _selectedEventSchema);
                    await _lvDetails.SetSourceAsync((IList)eventTypes);

                    _btnNext.Enabled = (eventTypes.Count > 0) ? true : false;

                    break;
                case "eventType":
                    _stage = "review";
                    _selectedEventTypeIdx = _lvDetails.SelectedItem;

                    eventTypes = EventSourceHelper.GetEventTypes(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion, _selectedEventSchema);
                    _selectedEventType = eventTypes[_selectedEventTypeIdx];

                    _btnNext.Text = "_Generate";
                    _lblTitle.Text = "> Review your generation request <";
                    _lvDetails.Visible = false;
                    _lblSelectedSourceName.Visible = true;
                    _lblSelectedVersionType.Visible = true;
                    _lblSelectedVersion.Visible = true;
                    _lblSelectedEventSchema.Visible = true;
                    _lblSelectedEventType.Visible = true;
                    _lblNumberOfEvents.Visible = true;
                    _txtNumberOfEvents.Visible = true;
                    _lblAPIKey.Visible = true;
                    _txtAPIKey.Visible = true;
                    _chkRememberAPIKey.Visible = true;

                    _lblSelectedSourceName.Text = $"- System source: {_selectedSourceName}";
                    _lblSelectedVersionType.Text = $"- Version type: {_selectedVersionType}";
                    _lblSelectedVersion.Text = $"- Version: {_selectedVersion}";
                    _lblSelectedEventSchema.Text = $"- Event schema: {_selectedEventSchema}";
                    _lblSelectedEventType.Text = $"- Event type: {_selectedEventType}";
                    _btnNext.SetFocus();

                    break;
                case "review":

                    if (_chkRememberAPIKey == null)
                        return;

                    var apiKey = string.Empty;
                    if (_txtAPIKey != null)
                        if (!string.IsNullOrEmpty(_txtAPIKey.Text.ToString()))
                            apiKey = _txtAPIKey.Text.ToString();

                    if (string.IsNullOrEmpty(apiKey))
                    {
                        MessageBox.ErrorQuery("Error", $"OpenAI API key required.", "Ok");
                        return;
                    }

                    if (_chkRememberAPIKey.Checked)
                    {
                        var settings = Utils.GetSettings();

                        if (settings == null)
                        {
                            MessageBox.ErrorQuery("Error", $"There was an error reading the settings file.", "Ok");
                            return;
                        }

                        settings.OpenAIAPIKey = apiKey;
                        settings.RememberOpenAIAPIKey = true;
                        Utils.UpdateSettings(settings);
                    }
                    else
                    {
                        var settings = new Settings() { OpenAIAPIKey = string.Empty, RememberOpenAIAPIKey = false };
                        Utils.UpdateSettings(settings);
                    }

                    Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), PrepareBackgroundProcess);
                    StartBackgroundProcessDialog();

                    break;
            }
        }

        bool PrepareBackgroundProcess(MainLoop _)
        {
            if (_dialogBgProc != null)
            {
                Application.MainLoop.Invoke(async () =>
                {
                    if (_host == null || _txtNumberOfEvents == null || _txtAPIKey == null || EditorService._textView == null || _dialog == null)
                        return;

                    try
                    {
                        int numberOfEvents = Convert.ToInt16(_txtNumberOfEvents.Text.ToString());

                        var apiKey = string.Empty;
                        if (_txtAPIKey != null)
                            if (!string.IsNullOrEmpty(_txtAPIKey.Text.ToString()))
                                apiKey = _txtAPIKey.Text.ToString();

                        if (string.IsNullOrEmpty(apiKey))
                            return;

                        var result = await SendToGPTAndProcessResponse(apiKey, _selectedSourceName, _selectedVersionType, _selectedVersion, _selectedEventSchema, _selectedEventType, numberOfEvents);

                        Application.RequestStop();

                        _dialog.RequestStop();
                        EditorService._textView.Text = result;
                        Reset();
                    }
                    catch (Exception ex)
                    {
                        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                        var filePath = Path.Combine(directoryPath, $"{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.txt");
                        try
                        {
                            MessageBox.ErrorQuery("Error", $"There was an error, check the log file for more details: {filePath}.", "Ok");
                            Directory.CreateDirectory(directoryPath);
                            File.WriteAllText(filePath, ex.ToString());
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

        private async Task<string> SendToGPTAndProcessResponse(string openAIKey, string sourceName, string versionType, string version, string eventSchema, string eventType, int numberOfEvents)
        {
            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(3,
              retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
              (exception, calculatedWaitDuration) =>
              {
                  // Console.WriteLine($"Exception: {exception.Message}");
              });

            string response = string.Empty;

            if (string.IsNullOrEmpty(sourceName) || string.IsNullOrEmpty(versionType) || string.IsNullOrEmpty(version) ||
             string.IsNullOrEmpty(eventSchema) || string.IsNullOrEmpty(eventType) || numberOfEvents == 0)
                throw new Exception("There was an error with the request parameters");

            if (_httpClientFactory == null)
                throw new Exception("Http client initialization error");

            var httpClient = _httpClientFactory.CreateClient();
            var contentUrl = $"https://raw.githubusercontent.com/robece/event-generator-specs/main/data-plane/{sourceName}/{versionType}/{version}/gpt-prompts/{eventSchema}/{eventType}.prompt";
            var content = await httpClient.GetStringAsync(contentUrl);

            if (string.IsNullOrEmpty(content))
                throw new Exception("There was an error in the content response");

            var methodsUrl = $"https://raw.githubusercontent.com/robece/event-generator-specs/main/data-plane/{sourceName}/{versionType}/{version}/gpt-prompts/common/Methods.prompt";
            var methodsContent = await httpClient.GetStringAsync(methodsUrl);

            if (string.IsNullOrEmpty(methodsContent))
                throw new Exception("There was an error in the content response");

            content = content.Replace("{{methods}}", $"{methodsContent}");
            content = content.Replace("{{numberOfEvents}}", $"{numberOfEvents}");

            var completionRequest = CreateCompletionCreateRequest(content);

            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = openAIKey
            });

            await policy.ExecuteAsync(async () =>
            {
                var completionResult = await openAiService.Completions.CreateCompletion(completionRequest);
                if (completionResult.Successful)
                {
                    response = completionResult.Choices.First().Text;
                    response = response.Replace("console.log", "log");

                    var engine = new Engine().SetValue("log", new Action<string?>((b) =>
                    {
                        if (!string.IsNullOrEmpty(b))
                            response = b.ToString();
                    }));

                    var engineResponse = engine.Execute(response);

                    try
                    {
                        var token = JToken.Parse(response);
                        if (token is not JArray)
                            throw new Exception("Try again because is not an array");

                        var events = token.ToObject<List<ExpandoObject>>();
                        if (events != null)
                            if (events.Count != numberOfEvents)
                                throw new Exception("Try again because array of elements is not correct");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Try again because content is not valid: {ex.Message}");
                    }
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        throw new Exception("Try again because there was an unknown error");
                    }
                    throw new Exception($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                }
            });

            return response;
        }

        private CompletionCreateRequest CreateCompletionCreateRequest(string content)
        {
            var completionRequest = new CompletionCreateRequest();
            completionRequest.Model = OpenAI.GPT3.ObjectModels.Models.TextDavinciV3;
            completionRequest.Temperature = 0.7f;
            completionRequest.MaxTokens = 3000;
            completionRequest.TopP = 1;
            completionRequest.FrequencyPenalty = 0;
            completionRequest.PresencePenalty = 0;
            completionRequest.Prompt = content;
            return completionRequest;
        }

        #endregion
    }
}
