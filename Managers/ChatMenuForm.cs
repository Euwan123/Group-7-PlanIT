using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using PlanIT2.TabTypes;
using PlanIT2.Managers;

namespace PlanIT2
{
    public partial class ChatMenuForm : Form
    {
        private string filePath;
        private bool isSaved = true;
        private bool isSaving = false;
        private bool isDarkMode = false;
        private AIManager aiManager;
        private ChecklistTabManager checklistTabManager;
        private ScheduleTabManager scheduleTabManager;
        private DrawingTabManager drawingTabManager;
        private ThemeManager themeManager;
        private SettingsManager settingsManager;
        private EnhancedTextTabManager enhancedTextTabManager;
        private KeyboardShortcutManager shortcutManager;
        private ExportManager exportManager;
        private BackupManager backupManager;
        private DictionaryManager dictionaryManager;
        private string currentUsername;

        private Panel topBar, bottomBar, sidePanel, mainPanel;
        private TabControl mainTabControl;
        private RichTextBox aiResponseBox;
        private TextBox aiInputBox;
        private Button sendAiButton, clearAiButton;
        private Label statusLabel, wordCountLabel, tabCountLabel, autoSaveTimerLabel;
        private Button btnBack, btnSave, btnAddTab, btnDictionary;
        private TextBox searchTextBox;
        private Button btnSearch, btnClearSearch;
        private System.Windows.Forms.Timer autoSaveTimer;
        private int autoSaveCountdown = 60;

        public ChatMenuForm(string path, string username)
        {
            filePath = path;
            currentUsername = username;
            settingsManager = new SettingsManager();
            themeManager = new ThemeManager(isDarkMode);
            aiManager = new AIManager();

            enhancedTextTabManager = new EnhancedTextTabManager(MarkUnsaved, isDarkMode, settingsManager.Settings.UndoLevels);
            checklistTabManager = new ChecklistTabManager(MarkUnsaved, isDarkMode);
            scheduleTabManager = new ScheduleTabManager(MarkUnsaved, isDarkMode);
            drawingTabManager = new DrawingTabManager(MarkUnsaved, isDarkMode);
            dictionaryManager = new DictionaryManager(currentUsername); 

            InitializeComponent();
            InitializeKeyboardShortcuts();
            InitializeAutoSaveTimer();

            exportManager = new ExportManager(mainTabControl);
            backupManager = new BackupManager(filePath, () => SaveWorkspace(true));

            if (settingsManager.Settings.AutoSave)
            {
                backupManager.StartAutoSave(settingsManager.Settings.AutoSaveIntervalMinutes);
            }

            LoadWorkspace();

            if (settingsManager.Settings.SaveWindowPosition)
            {
                this.Location = settingsManager.Settings.WindowPosition;
                this.Size = settingsManager.Settings.WindowSize;
            }
        }

        private void InitializeAutoSaveTimer()
        {
            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 1000;
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Start();
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            autoSaveCountdown--;
            if (autoSaveCountdown <= 0)
            {
                if (!isSaved)
                {
                    SaveWorkspace(true);
                }
                autoSaveCountdown = 60;
            }
            UpdateAutoSaveDisplay();
        }

        private void UpdateAutoSaveDisplay()
        {
            if (autoSaveTimerLabel != null)
            {
                if (isSaving)
                {
                    autoSaveTimerLabel.Text = "🟠 Saving...";
                    autoSaveTimerLabel.ForeColor = Color.Orange;
                }
                else if (isSaved)
                {
                    autoSaveTimerLabel.Text = $"🟢 Auto-save: {autoSaveCountdown}s";
                    autoSaveTimerLabel.ForeColor = Color.LightGreen;
                }
                else
                {
                    autoSaveTimerLabel.Text = $"🔴 Unsaved ({autoSaveCountdown}s)";
                    autoSaveTimerLabel.ForeColor = Color.FromArgb(255, 100, 100);
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "PlanIT";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.DoubleBuffered = true;
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 35, 35));
            this.FormClosing += ChatMenuForm_FormClosing;

            topBar = new Panel
            {
                Size = new Size(this.Width, 70),
                Location = new Point(0, 0),
                BackColor = Color.Black
            };
            this.Controls.Add(topBar);
            topBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) DragMove(this.Handle); };

            var brandLabel = new Label
            {
                Text = "PlanIT",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 18),
                AutoSize = true
            };
            topBar.Controls.Add(brandLabel);

            int buttonStartX = 200;

            Button btnExportTop = new Button
            {
                Text = "📤",
                Size = new Size(40, 40),
                Location = new Point(buttonStartX, 15),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btnExportTop.FlatAppearance.BorderSize = 0;
            btnExportTop.Click += (s, e) => exportManager.ShowExportDialog(this);
            ToolTip exportTooltip = new ToolTip();
            exportTooltip.SetToolTip(btnExportTop, "Export (Ctrl+Shift+E)");
            topBar.Controls.Add(btnExportTop);

            Button btnBackupTop = new Button
            {
                Text = "💾",
                Size = new Size(40, 40),
                Location = new Point(buttonStartX + 50, 15),
                BackColor = Color.FromArgb(139, 0, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btnBackupTop.FlatAppearance.BorderSize = 0;
            btnBackupTop.Click += (s, e) => backupManager.ShowBackupDialog(this);
            ToolTip backupTooltip = new ToolTip();
            backupTooltip.SetToolTip(btnBackupTop, "Backup Manager (Ctrl+Shift+B)");
            topBar.Controls.Add(btnBackupTop);

            Button btnSettingsTop = new Button
            {
                Text = "⚙️",
                Size = new Size(40, 40),
                Location = new Point(buttonStartX + 100, 15),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            btnSettingsTop.FlatAppearance.BorderSize = 0;
            btnSettingsTop.Click += (s, e) => settingsManager.ShowSettingsDialog(this);
            ToolTip settingsTooltip = new ToolTip();
            settingsTooltip.SetToolTip(btnSettingsTop, "Settings (Ctrl+,)");
            topBar.Controls.Add(btnSettingsTop);

            var minimizeButton = new Button
            {
                Text = "─",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 110, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            topBar.Controls.Add(minimizeButton);

            var closeButton = new Button
            {
                Text = "✕",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 55, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();
            topBar.Controls.Add(closeButton);

            bottomBar = new Panel
            {
                Size = new Size(this.Width, 60),
                Location = new Point(0, this.Height - 60),
                BackColor = Color.Black
            };
            this.Controls.Add(bottomBar);

            bottomBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("⚙️ Settings", null, (_, __) => settingsManager.ShowSettingsDialog(this));
                    menu.Items.Add("📤 Export", null, (_, __) => exportManager.ShowExportDialog(this));
                    menu.Items.Add("💾 Backup Manager", null, (_, __) => backupManager.ShowBackupDialog(this));
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("⌨️ Keyboard Shortcuts", null, (_, __) => shortcutManager.ShowShortcutDialog());
                    menu.Items.Add("🔍 Command Palette", null, (_, __) => ShowCommandPalette());
                    menu.Items.Add("📚 Dictionary", null, (_, __) => dictionaryManager.ShowDictionary(this));
                    menu.Show(bottomBar, e.Location);
                }
            };

            statusLabel = new Label
            {
                Text = "● Saved",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                Location = new Point(30, 20),
                AutoSize = true
            };
            bottomBar.Controls.Add(statusLabel);

            autoSaveTimerLabel = new Label
            {
                Text = "🟢 Auto-save: 60s",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGreen,
                Location = new Point(150, 22),
                AutoSize = true
            };
            bottomBar.Controls.Add(autoSaveTimerLabel);

            wordCountLabel = new Label
            {
                Text = "Words: 0 | Characters: 0",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(320, 22),
                AutoSize = true
            };
            bottomBar.Controls.Add(wordCountLabel);

            tabCountLabel = new Label
            {
                Text = "Tabs: 0",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(550, 22),
                AutoSize = true
            };
            bottomBar.Controls.Add(tabCountLabel);

            var footerLabel = new Label
            {
                Text = "© 2025 PlanIT - Plan Smarter Together",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(this.Width - 350, 20),
                AutoSize = true
            };
            bottomBar.Controls.Add(footerLabel);

            sidePanel = new Panel
            {
                Size = new Size(380, this.Height - 130),
                Location = new Point(this.Width - 380, 70),
                BackColor = Color.FromArgb(30, 30, 40)
            };
            this.Controls.Add(sidePanel);

            var aiHeaderPanel = new Panel
            {
                Size = new Size(340, 60),
                Location = new Point(20, 20),
                BackColor = Color.FromArgb(45, 45, 60)
            };
            aiHeaderPanel.SetRoundedCorners(15);
            sidePanel.Controls.Add(aiHeaderPanel);

            var aiLabel = new Label
            {
                Text = "🤖 AI Assistant",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 17),
                AutoSize = true
            };
            aiHeaderPanel.Controls.Add(aiLabel);

            aiResponseBox = new RichTextBox
            {
                Size = new Size(340, 380),
                Location = new Point(20, 100),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                Text = "👋 Welcome to PlanIT AI!\n\nAsk me anything about your workspace, notes, or general questions.\n\nExamples:\n• What is in my notes?\n• Calculate 25 * 4\n• Tell me about planning"
            };
            sidePanel.Controls.Add(aiResponseBox);

            aiInputBox = new TextBox
            {
                Size = new Size(340, 100),
                Location = new Point(20, 500),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 11),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Type your question here..."
            };
            aiInputBox.GotFocus += (s, e) => { if (aiInputBox.Text == "Type your question here...") { aiInputBox.Text = ""; aiInputBox.ForeColor = Color.White; } };
            aiInputBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(aiInputBox.Text)) { aiInputBox.Text = "Type your question here..."; aiInputBox.ForeColor = Color.Gray; } };
            aiInputBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && e.Control) { SendAiButton_Click(null, null); e.SuppressKeyPress = true; } };
            sidePanel.Controls.Add(aiInputBox);

            sendAiButton = new Button
            {
                Text = "📤 Send",
                Size = new Size(160, 45),
                Location = new Point(20, 620),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            sendAiButton.FlatAppearance.BorderSize = 0;
            sendAiButton.SetRoundedCorners(10);
            sendAiButton.Click += SendAiButton_Click;
            sidePanel.Controls.Add(sendAiButton);

            clearAiButton = new Button
            {
                Text = "🗑️ Clear",
                Size = new Size(160, 45),
                Location = new Point(200, 620),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            clearAiButton.FlatAppearance.BorderSize = 0;
            clearAiButton.SetRoundedCorners(10);
            clearAiButton.Click += (s, e) =>
            {
                aiResponseBox.Text = "👋 Welcome to PlanIT AI!\n\nAsk me anything about your workspace, notes, or general questions.";
                aiInputBox.Text = "Type your question here...";
                aiInputBox.ForeColor = Color.Gray;
            };
            sidePanel.Controls.Add(clearAiButton);

            mainPanel = new Panel
            {
                Size = new Size(this.Width - 380, this.Height - 130),
                Location = new Point(0, 70),
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(mainPanel);

            // Create dictionary panel (hidden by default)
            Panel dictPanel = dictionaryManager.CreateDictionaryPanel(this.Width - 380, this.Height - 130);
            dictPanel.Location = new Point(0, 70);
            this.Controls.Add(dictPanel);

            var toolbarPanel = new Panel
            {
                Size = new Size(mainPanel.Width - 40, 70),
                Location = new Point(20, 20),
                BackColor = Color.White
            };
            toolbarPanel.SetRoundedCorners(15);
            mainPanel.Controls.Add(toolbarPanel);

            btnBack = new Button
            {
                Text = "🔙 Back",
                Size = new Size(100, 45),
                Location = new Point(15, 12),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.SetRoundedCorners(10);
            btnBack.Click += BtnBack_Click;
            toolbarPanel.Controls.Add(btnBack);

            btnSave = new Button
            {
                Text = "💾 Save",
                Size = new Size(100, 45),
                Location = new Point(125, 12),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.SetRoundedCorners(10);
            btnSave.Click += BtnSave_Click;
            toolbarPanel.Controls.Add(btnSave);

            btnAddTab = new Button
            {
                Text = "➕ Add Tab",
                Size = new Size(120, 45),
                Location = new Point(235, 12),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddTab.FlatAppearance.BorderSize = 0;
            btnAddTab.SetRoundedCorners(10);
            btnAddTab.Click += BtnAddTab_Click;
            toolbarPanel.Controls.Add(btnAddTab);

            // NEW: Dictionary Button
            btnDictionary = new Button
            {
                Text = "📚 Dictionary",
                Size = new Size(130, 45),
                Location = new Point(365, 12),
                BackColor = Color.FromArgb(75, 0, 130),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDictionary.FlatAppearance.BorderSize = 0;
            btnDictionary.SetRoundedCorners(10);
            btnDictionary.Click += (s, e) => dictionaryManager.ShowDictionary(this);
            ToolTip dictTooltip = new ToolTip();
            dictTooltip.SetToolTip(btnDictionary, "Open Dictionary (Ctrl+D)");
            toolbarPanel.Controls.Add(btnDictionary);

            searchTextBox = new TextBox
            {
                Size = new Size(180, 45),
                Location = new Point(515, 16),
                Font = new Font("Segoe UI", 11),
                Text = "🔍 Search words...",
                BorderStyle = BorderStyle.FixedSingle
            };
            searchTextBox.GotFocus += (s, e) => { if (searchTextBox.Text == "🔍 Search words...") searchTextBox.Text = ""; };
            searchTextBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(searchTextBox.Text)) searchTextBox.Text = "🔍 Search words..."; };
            toolbarPanel.Controls.Add(searchTextBox);

            btnSearch = new Button
            {
                Text = "Search",
                Size = new Size(90, 45),
                Location = new Point(705, 12),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.SetRoundedCorners(10);
            btnSearch.Click += BtnSearch_Click;
            toolbarPanel.Controls.Add(btnSearch);

            btnClearSearch = new Button
            {
                Text = "Clear",
                Size = new Size(90, 45),
                Location = new Point(805, 12),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClearSearch.FlatAppearance.BorderSize = 0;
            btnClearSearch.SetRoundedCorners(10);
            btnClearSearch.Click += (s, e) => { searchTextBox.Text = "🔍 Search words..."; ClearHighlights(); };
            toolbarPanel.Controls.Add(btnClearSearch);

            mainTabControl = new TabControl
            {
                Size = new Size(mainPanel.Width - 40, mainPanel.Height - 110),
                Location = new Point(20, 100),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                DrawMode = TabDrawMode.OwnerDrawFixed,
                Padding = new Point(20, 5)
            };
            mainTabControl.DrawItem += MainTabControl_DrawItem;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            mainTabControl.MouseDown += MainTabControl_MouseDown;
            mainPanel.Controls.Add(mainTabControl);

            ShowEmptyState();
        }

        private void InitializeKeyboardShortcuts()
        {
            shortcutManager = new KeyboardShortcutManager(this);
            shortcutManager.RegisterShortcut(Keys.S, Keys.Control, () => SaveWorkspace(false), "Save workspace");
            shortcutManager.RegisterShortcut(Keys.N, Keys.Control, () => BtnAddTab_Click(null, null), "New tab");
            shortcutManager.RegisterShortcut(Keys.W, Keys.Control, CloseCurrentTab, "Close tab");
            shortcutManager.RegisterShortcut(Keys.Q, Keys.Control, () => this.Close(), "Quit");
            shortcutManager.RegisterShortcut(Keys.Oemcomma, Keys.Control, () => settingsManager.ShowSettingsDialog(this), "Open settings");
            shortcutManager.RegisterShortcut(Keys.E, Keys.Control | Keys.Shift, () => exportManager.ShowExportDialog(this), "Export");
            shortcutManager.RegisterShortcut(Keys.B, Keys.Control | Keys.Shift, () => backupManager.ShowBackupDialog(this), "Backup manager");
            shortcutManager.RegisterShortcut(Keys.F1, Keys.None, () => shortcutManager.ShowShortcutDialog(), "Show shortcuts");
            shortcutManager.RegisterShortcut(Keys.Tab, Keys.Control, NextTab, "Next tab");
            shortcutManager.RegisterShortcut(Keys.Tab, Keys.Control | Keys.Shift, PreviousTab, "Previous tab");
            shortcutManager.RegisterShortcut(Keys.P, Keys.Control, ShowCommandPalette, "Command palette");
            shortcutManager.RegisterShortcut(Keys.D, Keys.Control, () => dictionaryManager.ShowDictionary(this), "Dictionary");
        }

        private void CloseCurrentTab()
        {
            if (mainTabControl.SelectedIndex >= 0) CloseTab(mainTabControl.SelectedIndex);
        }

        private void NextTab()
        {
            if (mainTabControl.TabCount > 0)
            {
                int nextIndex = (mainTabControl.SelectedIndex + 1) % mainTabControl.TabCount;
                mainTabControl.SelectedIndex = nextIndex;
            }
        }

        private void PreviousTab()
        {
            if (mainTabControl.TabCount > 0)
            {
                int prevIndex = mainTabControl.SelectedIndex - 1;
                if (prevIndex < 0) prevIndex = mainTabControl.TabCount - 1;
                mainTabControl.SelectedIndex = prevIndex;
            }
        }

        private void ShowCommandPalette()
        {
            var commands = new Dictionary<string, Action>
            {
                ["Save Workspace"] = () => SaveWorkspace(false),
                ["New Text Tab"] = () => CreateQuickTab("text"),
                ["New Checklist Tab"] = () => CreateQuickTab("checklist"),
                ["New Schedule Tab"] = () => CreateQuickTab("schedule"),
                ["New Drawing Tab"] = () => CreateQuickTab("drawing"),
                ["Export Current Tab"] = () => exportManager.ShowExportDialog(this),
                ["Open Settings"] = () => settingsManager.ShowSettingsDialog(this),
                ["Show Keyboard Shortcuts"] = () => shortcutManager.ShowShortcutDialog(),
                ["Backup Manager"] = () => backupManager.ShowBackupDialog(this),
                ["Open Dictionary"] = () => dictionaryManager.ShowDictionary(this),
                ["Close Current Tab"] = CloseCurrentTab,
                ["Close All Tabs"] = CloseAllTabs,
                ["Search in Tab"] = () => searchTextBox.Focus()
            };
            using (CommandPalette palette = new CommandPalette(commands))
            {
                palette.ShowDialog(this);
            }
        }

        private void CreateQuickTab(string type)
        {
            string name = $"New {type} {DateTime.Now:HHmmss}";
            TabPage newTab = null;
            switch (type)
            {
                case "text": newTab = enhancedTextTabManager.CreateTextTab(name); break;
                case "checklist": newTab = checklistTabManager.CreateChecklistTab(name); break;
                case "schedule": newTab = scheduleTabManager.CreateScheduleTab(name); break;
                case "drawing": newTab = drawingTabManager.CreateDrawingTab(name); break;
            }
            if (newTab != null)
            {
                newTab.Tag = type;
                mainTabControl.TabPages.Add(newTab);
                mainTabControl.SelectedTab = newTab;
                ShowEmptyState();
                UpdateTabCount();
                MarkUnsaved();
                HookTextChangeEvents(newTab);
            }
        }

        private void HookTextChangeEvents(TabPage tab)
        {
            string tabType = tab.Tag?.ToString() ?? "text";
            if (tabType == "text")
            {
                foreach (Control c in tab.Controls)
                {
                    if (c is Panel panel)
                    {
                        foreach (Control innerC in panel.Controls)
                        {
                            if (innerC is RichTextBox rtb)
                            {
                                rtb.TextChanged += (s, e) => UpdateWordCount();
                            }
                        }
                    }
                }
            }
        }

        private void CloseAllTabs()
        {
            var result = MessageBox.Show("Close all tabs?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                mainTabControl.TabPages.Clear();
                ShowEmptyState();
                UpdateTabCount();
                MarkUnsaved();
            }
        }

        private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= mainTabControl.TabCount) return;
            TabPage tab = mainTabControl.TabPages[e.Index];
            Rectangle bounds = e.Bounds;
            bool isSelected = (e.Index == mainTabControl.SelectedIndex);
            Color backColor = isSelected ? Color.FromArgb(220, 70, 0) : Color.FromArgb(200, 200, 200);
            Color foreColor = isSelected ? Color.White : Color.Black;
            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }
            string icon = "📄";
            string tabType = tab.Tag?.ToString() ?? "text";
            switch (tabType)
            {
                case "text": icon = "📝"; break;
                case "checklist": icon = "☑️"; break;
                case "schedule": icon = "📅"; break;
                case "drawing": icon = "🎨"; break;
            }
            string displayText = $"{icon} {tab.Text}";
            using (SolidBrush textBrush = new SolidBrush(foreColor))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(displayText, e.Font, textBrush, bounds, sf);
            }
        }

        private void ShowEmptyState()
        {
            var existingEmpty = mainPanel.Controls.Find("EmptyStatePanel", false).FirstOrDefault();
            if (existingEmpty != null) mainPanel.Controls.Remove(existingEmpty);

            if (mainTabControl.TabCount == 0)
            {
                Panel emptyPanel = new Panel
                {
                    Size = new Size(600, 300),
                    BackColor = Color.White,
                    Name = "EmptyStatePanel"
                };
                emptyPanel.SetRoundedCorners(20);
                emptyPanel.Location = new Point(
                    (mainPanel.Width - emptyPanel.Width) / 2,
                    (mainPanel.Height - emptyPanel.Height) / 2 - 30
                );

                Label emptyIcon = new Label
                {
                    Text = "📋",
                    Font = new Font("Segoe UI Emoji", 72),
                    AutoSize = true
                };
                emptyIcon.Location = new Point(
                    (emptyPanel.Width - emptyIcon.PreferredWidth) / 2,
                    30
                );

                Label emptyTitle = new Label
                {
                    Text = "No Tabs Yet",
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    AutoSize = true
                };
                emptyTitle.Location = new Point(
                    (emptyPanel.Width - emptyTitle.PreferredWidth) / 2,
                    150
                );

                Label emptySubtitle = new Label
                {
                    Text = "Click '➕ Add Tab' to create your first tab!",
                    Font = new Font("Segoe UI", 14),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    AutoSize = true
                };
                emptySubtitle.Location = new Point(
                    (emptyPanel.Width - emptySubtitle.PreferredWidth) / 2,
                    210
                );

                emptyPanel.Controls.AddRange(new Control[] { emptyIcon, emptyTitle, emptySubtitle });
                mainPanel.Controls.Add(emptyPanel);
                emptyPanel.BringToFront();
            }
        }


        private void MainTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < mainTabControl.TabCount; i++)
                {
                    Rectangle tabRect = mainTabControl.GetTabRect(i);
                    if (tabRect.Contains(e.Location))
                    {
                        ShowTabContextMenu(i, e.Location);
                        break;
                    }
                }
            }
        }

        private void ShowTabContextMenu(int tabIndex, Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(50, 50, 50);
            menu.ForeColor = Color.White;
            ToolStripMenuItem renameItem = new ToolStripMenuItem("✏️ Rename Tab");
            renameItem.Click += (s, e) => RenameTab(tabIndex);
            menu.Items.Add(renameItem);
            ToolStripMenuItem colorItem = new ToolStripMenuItem("🎨 Change Color");
            colorItem.Click += (s, e) => ChangeTabColor(tabIndex);
            menu.Items.Add(colorItem);
            ToolStripMenuItem duplicateItem = new ToolStripMenuItem("📋 Duplicate Tab");
            duplicateItem.Click += (s, e) => DuplicateTab(tabIndex);
            menu.Items.Add(duplicateItem);
            menu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem closeItem = new ToolStripMenuItem("❌ Close Tab");
            closeItem.Click += (s, e) => CloseTab(tabIndex);
            menu.Items.Add(closeItem);
            menu.Show(mainTabControl, location);
        }

        private void RenameTab(int index)
        {
            if (index < 0 || index >= mainTabControl.TabCount) return;
            string currentName = mainTabControl.TabPages[index].Text;
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter new tab name:", "Rename Tab", currentName);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                mainTabControl.TabPages[index].Text = newName;
                MarkUnsaved();
            }
        }

        private void ChangeTabColor(int index)
        {
            if (index < 0 || index >= mainTabControl.TabCount) return;
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    mainTabControl.TabPages[index].BackColor = cd.Color;
                    MarkUnsaved();
                }
            }
        }

        private void DuplicateTab(int index)
        {
            if (index < 0 || index >= mainTabControl.TabCount) return;
            TabPage originalTab = mainTabControl.TabPages[index];
            string content = GetTabContent(originalTab);
            string tabType = originalTab.Tag?.ToString() ?? "text";
            TabPage newTab = null;
            switch (tabType)
            {
                case "text": newTab = enhancedTextTabManager.CreateTextTab($"{originalTab.Text} (Copy)", content); break;
                case "checklist": newTab = checklistTabManager.CreateChecklistTab($"{originalTab.Text} (Copy)", content); break;
                case "schedule": newTab = scheduleTabManager.CreateScheduleTab($"{originalTab.Text} (Copy)", content); break;
                case "drawing": newTab = drawingTabManager.CreateDrawingTab($"{originalTab.Text} (Copy)", content); break;
            }
            if (newTab != null)
            {
                newTab.Tag = tabType;
                mainTabControl.TabPages.Add(newTab);
                ShowEmptyState();
                UpdateTabCount();
                MarkUnsaved();
            }
        }

        private void CloseTab(int index)
        {
            if (index < 0 || index >= mainTabControl.TabCount) return;
            var result = MessageBox.Show($"Are you sure you want to close '{mainTabControl.TabPages[index].Text}'?", "Close Tab", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                mainTabControl.TabPages.RemoveAt(index);
                ShowEmptyState();
                UpdateTabCount();
                MarkUnsaved();
            }
        }

        private void BtnAddTab_Click(object sender, EventArgs e)
        {
            using (Form tabDialog = new Form())
            {
                tabDialog.Text = "Add New Tab";
                tabDialog.Size = new Size(400, 280);
                tabDialog.StartPosition = FormStartPosition.CenterParent;
                tabDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                tabDialog.MaximizeBox = false;
                tabDialog.MinimizeBox = false;
                tabDialog.BackColor = Color.White;
                Label lblName = new Label { Text = "Tab Name:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 11) };
                TextBox txtName = new TextBox { Location = new Point(20, 50), Width = 340, Font = new Font("Segoe UI", 11) };
                Label lblType = new Label { Text = "Tab Type:", Location = new Point(20, 90), AutoSize = true, Font = new Font("Segoe UI", 11) };
                ComboBox cmbType = new ComboBox { Location = new Point(20, 120), Width = 340, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
                cmbType.Items.AddRange(new string[] { "📝 Text", "☑️ Checklist", "📅 Schedule", "🎨 Drawing" });
                cmbType.SelectedIndex = 0;
                Button btnOk = new Button { Text = "Create", Location = new Point(180, 180), Size = new Size(90, 40), BackColor = Color.FromArgb(220, 70, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), DialogResult = DialogResult.OK };
                btnOk.FlatAppearance.BorderSize = 0;
                Button btnCancel = new Button { Text = "Cancel", Location = new Point(280, 180), Size = new Size(80, 40), BackColor = Color.FromArgb(128, 128, 128), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), DialogResult = DialogResult.Cancel };
                btnCancel.FlatAppearance.BorderSize = 0;
                tabDialog.Controls.AddRange(new Control[] { lblName, txtName, lblType, cmbType, btnOk, btnCancel });
                tabDialog.AcceptButton = btnOk;
                if (tabDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    TabPage newTab = null;
                    string tabType = "";
                    switch (cmbType.SelectedIndex)
                    {
                        case 0: newTab = enhancedTextTabManager.CreateTextTab(txtName.Text); tabType = "text"; break;
                        case 1: newTab = checklistTabManager.CreateChecklistTab(txtName.Text); tabType = "checklist"; break;
                        case 2: newTab = scheduleTabManager.CreateScheduleTab(txtName.Text); tabType = "schedule"; break;
                        case 3: newTab = drawingTabManager.CreateDrawingTab(txtName.Text); tabType = "drawing"; break;
                    }
                    if (newTab != null)
                    {
                        newTab.Tag = tabType;
                        mainTabControl.TabPages.Add(newTab);
                        mainTabControl.SelectedTab = newTab;
                        ShowEmptyState();
                        UpdateTabCount();
                        UpdateWordCount();
                        MarkUnsaved();
                        HookTextChangeEvents(newTab);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveWorkspace(false);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (!isSaved)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save before exiting?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    SaveWorkspace(false);
                    this.Close();
                }
                else if (result == DialogResult.No)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (searchTextBox.Text != "🔍 Search words..." && !string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                SearchAndHighlight(searchTextBox.Text);
            }
        }

        private void SearchAndHighlight(string searchText)
        {
            ClearHighlights();
            int foundCount = 0;
            foreach (TabPage tab in mainTabControl.TabPages)
            {
                string tabType = tab.Tag?.ToString() ?? "text";
                if (tabType == "text")
                {
                    foreach (Control c in tab.Controls)
                    {
                        if (c is Panel panel)
                        {
                            foreach (Control innerC in panel.Controls)
                            {
                                if (innerC is RichTextBox rtb)
                                {
                                    string text = rtb.Text;
                                    int index = 0;
                                    while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
                                    {
                                        rtb.Select(index, searchText.Length);
                                        rtb.SelectionBackColor = Color.Yellow;
                                        foundCount++;
                                        index += searchText.Length;
                                    }
                                    rtb.Select(0, 0);
                                }
                            }
                        }
                    }
                }
            }
            MessageBox.Show($"Found and highlighted {foundCount} occurrence(s) of '{searchText}'", "Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearHighlights()
        {
            foreach (TabPage tab in mainTabControl.TabPages)
            {
                string tabType = tab.Tag?.ToString() ?? "text";
                if (tabType == "text")
                {
                    foreach (Control c in tab.Controls)
                    {
                        if (c is Panel panel)
                        {
                            foreach (Control innerC in panel.Controls)
                            {
                                if (innerC is RichTextBox rtb)
                                {
                                    rtb.SelectAll();
                                    rtb.SelectionBackColor = rtb.BackColor;
                                    rtb.Select(0, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWordCount();
        }

        private void SendAiButton_Click(object sender, EventArgs e)
        {
            string userInput = aiInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput) || userInput == "Type your question here...") return;
            aiResponseBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            aiResponseBox.SelectionColor = Color.LightBlue;
            aiResponseBox.AppendText($"\n\n👤 You: ");
            aiResponseBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
            aiResponseBox.SelectionColor = Color.White;
            aiResponseBox.AppendText($"{userInput}\n");
            try
            {
                string context = GetCurrentTabContent();
                string response = aiManager.GetAiResponse(userInput, context);
                aiResponseBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
                aiResponseBox.SelectionColor = Color.LightGreen;
                aiResponseBox.AppendText($"🤖 AI: ");
                aiResponseBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
                aiResponseBox.SelectionColor = Color.White;
                aiResponseBox.AppendText($"{response}\n");
            }
            catch (Exception ex)
            {
                aiResponseBox.SelectionColor = Color.Red;
                aiResponseBox.AppendText($"❌ Error: {ex.Message}\n");
            }
            finally
            {
                aiInputBox.Text = "Type your question here...";
                aiInputBox.ForeColor = Color.Gray;
                aiResponseBox.ScrollToCaret();
            }
        }

        private string GetCurrentTabContent()
        {
            if (mainTabControl.SelectedTab != null)
            {
                return GetTabContent(mainTabControl.SelectedTab);
            }
            return "";
        }

        private string GetTabContent(TabPage tab)
        {
            string tabType = tab.Tag?.ToString() ?? "text";
            switch (tabType)
            {
                case "text": return enhancedTextTabManager.GetContent(tab);
                case "checklist": return checklistTabManager.GetContent(tab);
                case "schedule": return scheduleTabManager.GetContent(tab);
                case "drawing": return drawingTabManager.GetContent(tab);
                default: return "";
            }
        }

        private void SaveWorkspace(bool silent = false)
        {
            try
            {
                isSaving = true;
                UpdateAutoSaveDisplay();

                var tabDataList = new List<TabData>();
                foreach (TabPage tab in mainTabControl.TabPages)
                {
                    string tabType = tab.Tag?.ToString() ?? "text";
                    string content = GetTabContent(tab);
                    tabDataList.Add(new TabData { Title = tab.Text, Type = tabType, Content = content, TabColor = tab.BackColor });
                }
                string json = JsonConvert.SerializeObject(tabDataList, Formatting.Indented);
                File.WriteAllText(filePath, json);

                isSaved = true;
                isSaving = false;
                UpdateStatusDisplay();
                UpdateAutoSaveDisplay();
                autoSaveCountdown = 60;

                if (!silent)
                {
                    MessageBox.Show("Workspace saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                isSaving = false;
                UpdateAutoSaveDisplay();
                if (!silent)
                {
                    MessageBox.Show($"Error saving workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadWorkspace()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var tabDataList = JsonConvert.DeserializeObject<List<TabData>>(json);
                    if (tabDataList != null)
                    {
                        foreach (var tabData in tabDataList)
                        {
                            TabPage tab = null;
                            switch (tabData.Type)
                            {
                                case "text": tab = enhancedTextTabManager.CreateTextTab(tabData.Title, tabData.Content); break;
                                case "checklist": tab = checklistTabManager.CreateChecklistTab(tabData.Title, tabData.Content); break;
                                case "schedule": tab = scheduleTabManager.CreateScheduleTab(tabData.Title, tabData.Content); break;
                                case "drawing": tab = drawingTabManager.CreateDrawingTab(tabData.Title, tabData.Content); break;
                            }
                            if (tab != null)
                            {
                                tab.Tag = tabData.Type;
                                tab.BackColor = tabData.TabColor;
                                mainTabControl.TabPages.Add(tab);
                                HookTextChangeEvents(tab);
                            }
                        }
                    }
                }
                isSaved = true;
                UpdateStatusDisplay();
                ShowEmptyState();
                UpdateTabCount();
                UpdateWordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MarkUnsaved()
        {
            isSaved = false;
            UpdateStatusDisplay();
            UpdateAutoSaveDisplay();
        }

        private void UpdateStatusDisplay()
        {
            if (isSaved)
            {
                statusLabel.Text = "● Saved";
                statusLabel.ForeColor = Color.LightGreen;
            }
            else
            {
                statusLabel.Text = "● Unsaved";
                statusLabel.ForeColor = Color.LightCoral;
            }
        }

        private void UpdateWordCount()
        {
            if (mainTabControl.SelectedTab == null)
            {
                wordCountLabel.Text = "Words: 0 | Characters: 0";
                return;
            }
            string tabType = mainTabControl.SelectedTab.Tag?.ToString() ?? "text";
            if (tabType == "text")
            {
                // FIX: Get plain text content only, not RTF formatting
                string plainText = "";
                foreach (Control c in mainTabControl.SelectedTab.Controls)
                {
                    if (c is Panel panel)
                    {
                        foreach (Control innerC in panel.Controls)
                        {
                            if (innerC is RichTextBox rtb)
                            {
                                plainText = rtb.Text; // Use .Text instead of GetContent() which returns RTF
                                break;
                            }
                        }
                    }
                }

                int wordCount = string.IsNullOrWhiteSpace(plainText) ? 0 :
                    plainText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                int charCount = plainText.Length;
                wordCountLabel.Text = $"Words: {wordCount} | Characters: {charCount}";
            }
            else
            {
                wordCountLabel.Text = "Words: N/A | Characters: N/A";
            }
        }

        private void UpdateTabCount()
        {
            tabCountLabel.Text = $"Tabs: {mainTabControl.TabCount}";
        }

        private void ChatMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (autoSaveTimer != null)
            {
                autoSaveTimer.Stop();
                autoSaveTimer.Dispose();
            }
            if (settingsManager.Settings.SaveWindowPosition)
            {
                settingsManager.Settings.WindowPosition = this.Location;
                settingsManager.Settings.WindowSize = this.Size;
                settingsManager.SaveSettings();
            }
            backupManager?.StopAutoSave();
            if (!isSaved)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save before closing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    SaveWorkspace(false);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            backupManager?.Dispose();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void DragMove(IntPtr handle)
        {
            ReleaseCapture();
            SendMessage(handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
    }
}