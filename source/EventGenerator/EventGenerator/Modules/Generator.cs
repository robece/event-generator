using System;
using System.Collections;
using System.Text.RegularExpressions;
using EventGenerator.Handlers;
using Terminal.Gui;

namespace EventGenerator.Modules
{
    public class Generator
    {
        #region private members

        private List<KeyValuePair<string, string>> _repositoryTree = new List<KeyValuePair<string, string>>();
        private string _stage = "sourceName"; // _stage: 1. sourceName, 2. versionType, 3. version, 4. eventType, 5. review
        private string _selectedSourceName = string.Empty;
        private int _selectedSourceNameIdx = -1;
        private string _selectedVersionType = string.Empty;
        private int _selectedVersionTypeIdx = -1;
        private string _selectedVersion = string.Empty;
        private int _selectedVersionIdx = -1;
        private string _selectedEventType = string.Empty;
        private int _selectedEventTypeIdx = -1;
        
        #endregion

        #region private controls

        private Dialog? _dialog = null;
        private Button? _btnCancel = null;
        private Button? _btnBack = null;
        private Button? _btnNext = null;
        private Label? _lblTitle = null;
        private ListView? _lvDetails = null;
        private ScrollBarView? _scrollBarView = null;
        private Label? _lblSystemSource = null;
        private Label? _lblSystemSourceVersionType = null;
        private Label? _lblSystemSourceVersion = null;
        private Label? _lblSystemSourceEventType = null;
        private Label? _lblNumberOfEvents = null;
        private TextField? _txtNumberOfEvents = null;

        #endregion

        #region constructor

        public Generator()
        {
            
        }

        #endregion

        #region public methods

        public async Task DisplayDialogAsync()
        {
            var dictRepositoryTree = await GitHubTreeHandler.GetRepositoryTree();
            _repositoryTree = dictRepositoryTree.ToList<KeyValuePair<string, string>>();

            await CreateDialogAsync();
        }

        #endregion

        #region private methods

        private async Task CreateDialogAsync()
        {
            var buttons = new List<Button>();

            _btnCancel = new Button("_Cancel", false);
            _btnCancel.Visible = true;
            _btnCancel.TextAlignment = TextAlignment.Centered;
            _btnCancel.Clicked += BtnCancel_Clicked;
            buttons.Add(_btnCancel);

            _btnBack = new Button("_Back", true);
            _btnCancel.Visible = true;
            _btnBack.Enabled = false;
            _btnBack.TextAlignment = TextAlignment.Centered;
            _btnBack.Clicked += BtnBack_Clicked;
            buttons.Add(_btnBack);

            _btnNext = new Button("_Next", true);
            _btnNext.Visible = true;
            _btnNext.TextAlignment = TextAlignment.Centered;
            _btnNext.Clicked += BtnNext_Clicked;
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

            var sources = EventSourceHandler.GetSources(_repositoryTree);
            await _lvDetails.SetSourceAsync((IList)sources);

            _dialog.Add(_lvDetails);

            _lblSystemSource = new Label("System source:")
            {
                X = 1,
                Y = Pos.Bottom(_lblTitle) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,               
                Visible = false
            };
            _dialog.Add(_lblSystemSource);

            _lblSystemSourceVersionType = new Label("Version type:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSystemSource) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSystemSourceVersionType);

            _lblSystemSourceVersion = new Label("Version:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSystemSourceVersionType) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSystemSourceVersion);

            _lblSystemSourceEventType = new Label("Event type:")
            {
                X = 1,
                Y = Pos.Bottom(_lblSystemSourceVersion) + 1,
                TextAlignment = TextAlignment.Left,
                Width = 25,
                Height = 1,
                Visible = false
            };
            _dialog.Add(_lblSystemSourceEventType);

            _lblNumberOfEvents = new Label("Number of events (Up to 50 events):")
            {
                X = 1,
                Y = Pos.Bottom(_lblSystemSourceEventType) + 1,
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
                Visible = false
            };
            _dialog.Add(_txtNumberOfEvents);

            _txtNumberOfEvents.TextChanged +=  (e) => 
            {
                var strTextToReplace = _txtNumberOfEvents.Text.ToString();
                if (string.IsNullOrEmpty(strTextToReplace))
                    return;

                if (Regex.IsMatch (strTextToReplace, "[^0-9]+"))
                {
                    var cp = _txtNumberOfEvents.CursorPosition;
                    _txtNumberOfEvents.Text = e;
                    _txtNumberOfEvents.CursorPosition = Math.Min (cp, _txtNumberOfEvents.Text.RuneCount);

                    MessageBox.Query("Error", "Input value is not a number", "Ok");

                    return;
                }

                if (_txtNumberOfEvents.Text.Length > 2)
                {
                    var cp = _txtNumberOfEvents.CursorPosition;
                    _txtNumberOfEvents.Text = e;
                    _txtNumberOfEvents.CursorPosition = Math.Min (cp, _txtNumberOfEvents.Text.RuneCount);

                    MessageBox.Query("Error", "Max length reached", "Ok");

                    return;
                }

                int numberOfEvents = Convert.ToInt16(_txtNumberOfEvents.Text.ToString());
                if (numberOfEvents > 50)
                {
                    var cp = _txtNumberOfEvents.CursorPosition;
                    _txtNumberOfEvents.Text = e;
                    _txtNumberOfEvents.CursorPosition = Math.Min (cp, _txtNumberOfEvents.Text.RuneCount);

                    MessageBox.Query("Error", "Max value reached", "Ok");

                    return;
                }
            };

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
            _dialog.Loaded += Dialog_Loaded;

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

        private void Dialog_Loaded()
        {
            if (_btnNext == null)
                return;

            _btnNext.SetFocus();
        }

        private void BtnCancel_Clicked()
        {
            _stage = "sourceName";
            _selectedSourceName = string.Empty;
            _selectedSourceNameIdx = -1;
            _selectedVersionType = string.Empty;
            _selectedVersionTypeIdx = -1;
            _selectedVersion = string.Empty;
            _selectedVersionIdx = -1;
            _selectedEventType = string.Empty;
            _selectedEventTypeIdx = -1;

            Application.RequestStop();
        }

        private async void BtnBack_Clicked()
        {
            if (_btnBack == null || _btnNext == null ||
                _lblTitle == null || _lvDetails == null ||
                _lblSystemSource == null || _lblSystemSourceVersionType == null ||
                _lblSystemSourceVersion == null || _lblSystemSourceEventType == null ||
                _lblNumberOfEvents == null || _txtNumberOfEvents == null)
                return;

            switch (_stage)
            {
                case "versionType":
                    _stage = "sourceName";
                    _btnBack.Enabled = false;
                    _btnNext.SetFocus();

                    _lblTitle.Text = "Select a system source:";
                    var sources = EventSourceHandler.GetSources(_repositoryTree);
                    await _lvDetails.SetSourceAsync((IList)sources);
                    _lvDetails.SelectedItem = _selectedSourceNameIdx;

                    _btnNext.Enabled = (sources.Count > 0) ? true : false;

                    break;

                case "version":
                    _stage = "versionType";
                    _btnBack.SetFocus();

                    _lblTitle.Text = "Select a version type:";
                    var versionTypes = EventSourceHandler.GetVersionTypes(_repositoryTree, _selectedSourceName);
                    await _lvDetails.SetSourceAsync((IList)versionTypes);
                    _lvDetails.SelectedItem = _selectedVersionTypeIdx;

                    _btnNext.Enabled = (versionTypes.Count > 0) ? true : false;

                    break;

                case "eventType":
                    _stage = "version";
                    _btnBack.SetFocus();

                    _lblTitle.Text = "Select a version:";
                    var versions = EventSourceHandler.GetVersions(_repositoryTree, _selectedSourceName, _selectedVersionType);
                    await _lvDetails.SetSourceAsync((IList)versions);
                    _lvDetails.SelectedItem = _selectedVersionIdx;

                    _btnNext.Enabled = (versions.Count > 0) ? true : false;

                    break;

                case "review":
                    _stage = "eventType";
                    _btnBack.SetFocus();

                    _btnNext.Text = "_Next";
                    _lblTitle.Visible = true;
                    _lvDetails.Visible = true;
                    _lblSystemSource.Visible = false;
                    _lblSystemSourceVersionType.Visible = false;
                    _lblSystemSourceVersion.Visible = false;
                    _lblSystemSourceEventType.Visible = false;
                    _lblNumberOfEvents.Visible = false;
                    _txtNumberOfEvents.Visible = false;

                    _lblTitle.Text = "Select an event type:";
                    var eventTypes = EventSourceHandler.GetEventTypes(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion);
                    await _lvDetails.SetSourceAsync((IList)eventTypes);
                    _lvDetails.SelectedItem = _selectedEventTypeIdx;

                    _btnNext.Enabled = (eventTypes.Count > 0) ? true : false;

                    break;
            }
        }

        private async void BtnNext_Clicked()
        {
            if (_btnBack == null || _btnNext == null ||
            _lblTitle == null || _lvDetails == null ||
            _lblSystemSource == null || _lblSystemSourceVersionType == null ||
            _lblSystemSourceVersion == null || _lblSystemSourceEventType == null ||
            _lblNumberOfEvents == null || _txtNumberOfEvents == null)
                return;

            switch (_stage)
            {
                case "sourceName":
                    _stage = "versionType";
                    _btnBack.Enabled = true;
                    _btnNext.SetFocus();

                    _selectedSourceNameIdx = _lvDetails.SelectedItem;

                    var sources = EventSourceHandler.GetSources(_repositoryTree);
                    if (sources.Count > 0)
                        _selectedSourceName = sources[_selectedSourceNameIdx];


                    _lblTitle.Text = "Select a version type:";
                    var versionTypes = EventSourceHandler.GetVersionTypes(_repositoryTree, _selectedSourceName);
                    await _lvDetails.SetSourceAsync((IList)versionTypes);

                    _btnNext.Enabled = (versionTypes.Count > 0) ? true : false;

                    break;
                case "versionType":
                    _stage = "version";
                    _btnNext.SetFocus();

                    _selectedVersionTypeIdx = _lvDetails.SelectedItem;

                    versionTypes = EventSourceHandler.GetVersionTypes(_repositoryTree, _selectedSourceName);
                    if (versionTypes.Count > 0)
                        _selectedVersionType = versionTypes[_selectedVersionTypeIdx];

                    _lblTitle.Text = "Select a version:";
                    var versions = EventSourceHandler.GetVersions(_repositoryTree, _selectedSourceName, _selectedVersionType);
                    await _lvDetails.SetSourceAsync((IList)versions);
                    
                    _btnNext.Enabled = (versions.Count > 0) ? true : false;

                    break;
                case "version":
                    _stage = "eventType";
                    _btnNext.SetFocus();

                    _selectedVersionIdx = _lvDetails.SelectedItem;

                    versions = EventSourceHandler.GetVersions(_repositoryTree, _selectedSourceName, _selectedVersionType);
                    if (versions.Count > 0)
                        _selectedVersion = versions[_selectedVersionIdx];

                    _lblTitle.Text = "Select an event type:";
                    var eventTypes = EventSourceHandler.GetEventTypes(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion);
                    await _lvDetails.SetSourceAsync((IList)eventTypes);

                    _btnNext.Enabled = (eventTypes.Count > 0) ? true : false;

                    break;

                case "eventType":
                    _stage = "review";
                    _selectedEventTypeIdx = _lvDetails.SelectedItem;

                    eventTypes = EventSourceHandler.GetEventTypes(_repositoryTree, _selectedSourceName, _selectedVersionType, _selectedVersion);
                    _selectedEventType = eventTypes[_selectedEventTypeIdx];

                    _btnNext.Text = "_Generate";
                    _lblTitle.Text = "> Review your generation request <";
                    _lvDetails.Visible = false;
                    _lblSystemSource.Visible = true;
                    _lblSystemSourceVersionType.Visible = true;
                    _lblSystemSourceVersion.Visible = true;
                    _lblSystemSourceEventType.Visible = true;
                    _lblNumberOfEvents.Visible = true;
                    _txtNumberOfEvents.Visible = true;
                    _lblSystemSource.Text = $"- System source: {_selectedSourceName}";
                    _lblSystemSourceVersionType.Text = $"- Version type: {_selectedVersionType}";
                    _lblSystemSourceVersion.Text = $"- Version: {_selectedVersion}";
                    _lblSystemSourceEventType.Text = $"- Event type: {_selectedEventType}";

                    _txtNumberOfEvents.SetFocus();

                    break;
            }

        }

        #endregion
    }
}
