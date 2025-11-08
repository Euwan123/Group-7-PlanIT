using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class AppSettings
    {
        public bool AutoSave { get; set; } = true;
        public int AutoSaveIntervalMinutes { get; set; } = 5;
        public string DefaultTabType { get; set; } = "text";
        public bool ShowWelcomeMessage { get; set; } = true;
        public Color AccentColor { get; set; } = Color.FromArgb(220, 70, 0);
        public string FontFamily { get; set; } = "Segoe UI";
        public int FontSize { get; set; } = 11;
        public bool EnableAnimations { get; set; } = true;
        public bool ShowLineNumbers { get; set; } = false;
        public bool WordWrap { get; set; } = true;
        public int MaxRecentWorkspaces { get; set; } = 10;
        public bool CheckForUpdates { get; set; } = true;
        public string LastOpenedWorkspace { get; set; } = "";
        public bool ConfirmTabClose { get; set; } = true;
        public bool EnableSpellCheck { get; set; } = false;
        public int UndoLevels { get; set; } = 50;
        public bool SaveWindowPosition { get; set; } = true;
        public Point WindowPosition { get; set; } = new Point(100, 100);
        public Size WindowSize { get; set; } = new Size(1400, 800);
    }

    public class SettingsManager
    {
        private static string settingsPath = Path.Combine(Application.StartupPath, "settings.json");
        private AppSettings settings;

        public AppSettings Settings => settings;

        public SettingsManager()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    settings = new AppSettings();
                }
            }
            catch
            {
                settings = new AppSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ResetToDefaults()
        {
            settings = new AppSettings();
            SaveSettings();
        }

        public void ShowSettingsDialog(Form owner)
        {
            using (SettingsDialog dialog = new SettingsDialog(this))
            {
                if (dialog.ShowDialog(owner) == DialogResult.OK)
                {
                    SaveSettings();
                }
            }
        }
    }

    public class SettingsDialog : Form
    {
        private SettingsManager manager;
        private TabControl tabControl;

        // General settings
        private CheckBox chkAutoSave;
        private NumericUpDown numAutoSaveInterval;
        private ComboBox cmbDefaultTabType;
        private CheckBox chkShowWelcome;
        private CheckBox chkEnableAnimations;
        private CheckBox chkConfirmClose;

        // Editor settings
        private ComboBox cmbFontFamily;
        private NumericUpDown numFontSize;
        private CheckBox chkWordWrap;
        private CheckBox chkLineNumbers;
        private NumericUpDown numUndoLevels;

        // Appearance
        private Button btnAccentColor;
        private Panel pnlColorPreview;

        public SettingsDialog(SettingsManager settingsManager)
        {
            manager = settingsManager;
            InitializeComponents();
            LoadCurrentSettings();
        }

        private void InitializeComponents()
        {
            this.Text = "Settings";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // General Tab
            TabPage generalTab = new TabPage("General");
            CreateGeneralSettings(generalTab);
            tabControl.TabPages.Add(generalTab);

            // Editor Tab
            TabPage editorTab = new TabPage("Editor");
            CreateEditorSettings(editorTab);
            tabControl.TabPages.Add(editorTab);

            // Appearance Tab
            TabPage appearanceTab = new TabPage("Appearance");
            CreateAppearanceSettings(appearanceTab);
            tabControl.TabPages.Add(appearanceTab);

            this.Controls.Add(tabControl);

            // Buttons
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.WhiteSmoke
            };

            Button btnOk = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 35),
                Location = new Point(280, 12),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnOk.Click += BtnOk_Click;

            Button btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 35),
                Location = new Point(390, 12),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };

            Button btnReset = new Button
            {
                Text = "Reset to Defaults",
                Size = new Size(140, 35),
                Location = new Point(20, 12),
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            btnReset.Click += (s, e) =>
            {
                var result = MessageBox.Show("Reset all settings to defaults?", "Confirm Reset",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    manager.ResetToDefaults();
                    LoadCurrentSettings();
                }
            };

            buttonPanel.Controls.AddRange(new Control[] { btnOk, btnCancel, btnReset });
            this.Controls.Add(buttonPanel);
        }

        private void CreateGeneralSettings(TabPage tab)
        {
            int y = 20;

            chkAutoSave = new CheckBox
            {
                Text = "Enable Auto-Save",
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(chkAutoSave);
            y += 35;

            Label lblInterval = new Label
            {
                Text = "Auto-Save Interval (minutes):",
                Location = new Point(40, y),
                AutoSize = true
            };
            numAutoSaveInterval = new NumericUpDown
            {
                Location = new Point(250, y - 3),
                Width = 80,
                Minimum = 1,
                Maximum = 60,
                Value = 5
            };
            tab.Controls.AddRange(new Control[] { lblInterval, numAutoSaveInterval });
            y += 40;

            Label lblDefaultTab = new Label
            {
                Text = "Default Tab Type:",
                Location = new Point(20, y),
                AutoSize = true
            };
            cmbDefaultTabType = new ComboBox
            {
                Location = new Point(250, y - 3),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDefaultTabType.Items.AddRange(new string[] { "text", "checklist", "schedule", "drawing" });
            tab.Controls.AddRange(new Control[] { lblDefaultTab, cmbDefaultTabType });
            y += 40;

            chkShowWelcome = new CheckBox
            {
                Text = "Show Welcome Message",
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(chkShowWelcome);
            y += 35;

            chkEnableAnimations = new CheckBox
            {
                Text = "Enable Animations",
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(chkEnableAnimations);
            y += 35;

            chkConfirmClose = new CheckBox
            {
                Text = "Confirm Before Closing Tabs",
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(chkConfirmClose);
        }

        private void CreateEditorSettings(TabPage tab)
        {
            int y = 20;

            Label lblFont = new Label
            {
                Text = "Font Family:",
                Location = new Point(20, y),
                AutoSize = true
            };
            cmbFontFamily = new ComboBox
            {
                Location = new Point(250, y - 3),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (FontFamily font in FontFamily.Families)
            {
                cmbFontFamily.Items.Add(font.Name);
            }
            tab.Controls.AddRange(new Control[] { lblFont, cmbFontFamily });
            y += 40;

            Label lblSize = new Label
            {
                Text = "Font Size:",
                Location = new Point(20, y),
                AutoSize = true
            };
            numFontSize = new NumericUpDown
            {
                Location = new Point(250, y - 3),
                Width = 80,
                Minimum = 8,
                Maximum = 24,
                Value = 11
            };
            tab.Controls.AddRange(new Control[] { lblSize, numFontSize });
            y += 40;

            chkWordWrap = new CheckBox
            {
                Text = "Enable Word Wrap",
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(chkWordWrap);
            y += 35;

            chkLineNumbers = new CheckBox
            {
                Text = "Show Line Numbers",
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(chkLineNumbers);
            y += 35;

            Label lblUndo = new Label
            {
                Text = "Undo Levels:",
                Location = new Point(20, y),
                AutoSize = true
            };
            numUndoLevels = new NumericUpDown
            {
                Location = new Point(250, y - 3),
                Width = 80,
                Minimum = 10,
                Maximum = 100,
                Value = 50
            };
            tab.Controls.AddRange(new Control[] { lblUndo, numUndoLevels });
        }

        private void CreateAppearanceSettings(TabPage tab)
        {
            int y = 20;

            Label lblAccent = new Label
            {
                Text = "Accent Color:",
                Location = new Point(20, y),
                AutoSize = true
            };

            pnlColorPreview = new Panel
            {
                Location = new Point(250, y - 3),
                Size = new Size(100, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = manager.Settings.AccentColor
            };

            btnAccentColor = new Button
            {
                Text = "Choose Color",
                Location = new Point(360, y - 3),
                Size = new Size(120, 30),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAccentColor.Click += (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.Color = pnlColorPreview.BackColor;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        pnlColorPreview.BackColor = cd.Color;
                    }
                }
            };

            tab.Controls.AddRange(new Control[] { lblAccent, pnlColorPreview, btnAccentColor });
        }

        private void LoadCurrentSettings()
        {
            chkAutoSave.Checked = manager.Settings.AutoSave;
            numAutoSaveInterval.Value = manager.Settings.AutoSaveIntervalMinutes;
            cmbDefaultTabType.SelectedItem = manager.Settings.DefaultTabType;
            chkShowWelcome.Checked = manager.Settings.ShowWelcomeMessage;
            chkEnableAnimations.Checked = manager.Settings.EnableAnimations;
            chkConfirmClose.Checked = manager.Settings.ConfirmTabClose;

            cmbFontFamily.SelectedItem = manager.Settings.FontFamily;
            numFontSize.Value = manager.Settings.FontSize;
            chkWordWrap.Checked = manager.Settings.WordWrap;
            chkLineNumbers.Checked = manager.Settings.ShowLineNumbers;
            numUndoLevels.Value = manager.Settings.UndoLevels;

            pnlColorPreview.BackColor = manager.Settings.AccentColor;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            manager.Settings.AutoSave = chkAutoSave.Checked;
            manager.Settings.AutoSaveIntervalMinutes = (int)numAutoSaveInterval.Value;
            manager.Settings.DefaultTabType = cmbDefaultTabType.SelectedItem?.ToString() ?? "text";
            manager.Settings.ShowWelcomeMessage = chkShowWelcome.Checked;
            manager.Settings.EnableAnimations = chkEnableAnimations.Checked;
            manager.Settings.ConfirmTabClose = chkConfirmClose.Checked;

            manager.Settings.FontFamily = cmbFontFamily.SelectedItem?.ToString() ?? "Segoe UI";
            manager.Settings.FontSize = (int)numFontSize.Value;
            manager.Settings.WordWrap = chkWordWrap.Checked;
            manager.Settings.ShowLineNumbers = chkLineNumbers.Checked;
            manager.Settings.UndoLevels = (int)numUndoLevels.Value;

            manager.Settings.AccentColor = pnlColorPreview.BackColor;
        }
    }
}