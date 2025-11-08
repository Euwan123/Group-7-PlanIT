using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PlanIT2
{
    public class WorkspaceData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsArchived { get; set; }
        public string Category { get; set; } = "Work";
        public string ColorTag { get; set; } = "#DC4600";
    }

    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string AvatarColor { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MainMenuForm : Form
    {
        private string loggedInUser;
        private int userId;
        private string userFolder;
        private ProfilePanel profilePanel;
        private Panel topBar, bottomBar, sidePanel, mainPanel, headerPanel;
        private Panel workspaceDisplayPanel;
        private Panel paginationPanel;
        private TextBox searchBox;
        private ComboBox sortComboBox;
        private Button btnNewWorkspace, btnGridView, btnListView, btnSettings, btnLogout;
        private Button btnDashboard, btnRecent, btnFavorites, btnArchive, btnProfile;
        private Label lblWelcome, lblWorkspaceCount, lblPageInfo;
        private bool isGridView = true;
        private string currentFilter = "all";
        private string currentSort = "updated_desc";
        private Timer searchDebounceTimer;
        private string currentView = "workspaces";
        private int currentPage = 1;
        private int itemsPerPage = 2;
        private List<WorkspaceData> currentWorkspaces = new List<WorkspaceData>();

        private static readonly string connectionString =
            "server=127.0.0.1;user id=root;password=;database=planit;";

        public MainMenuForm(string username)
        {
            loggedInUser = username;
            userId = GetUserId(username);

            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            userFolder = Path.Combine(docsPath, "PlanIT", loggedInUser);
            Directory.CreateDirectory(userFolder);

            searchDebounceTimer = new Timer();
            searchDebounceTimer.Interval = 300;
            searchDebounceTimer.Tick += (s, e) => { searchDebounceTimer.Stop(); FilterWorkspaces(); };

            InitializeComponent();
            LoadWorkspaces();
        }

        private int GetUserId(string username)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id FROM users WHERE username = @username";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting user ID: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.DoubleBuffered = true;
            this.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, this.Width, this.Height, 35, 35));

            topBar = new Panel
            {
                Size = new Size(this.Width, 70),
                Location = new Point(0, 0),
                BackColor = Color.Black
            };
            this.Controls.Add(topBar);
            topBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) NativeMethods.DragMove(this.Handle); };

            var brandLabel = new Label
            {
                Text = "PlanIT",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(brandLabel);

            var minimizeButton = new Button
            {
                Text = "─",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 110, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Font = new Font("Segoe UI", 16, FontStyle.Bold);
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
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            closeButton.Click += (s, e) => Application.Exit();
            closeButton.MouseEnter += (s, e) => closeButton.BackColor = Color.FromArgb(200, 0, 0);
            closeButton.MouseLeave += (s, e) => closeButton.BackColor = Color.Black;
            topBar.Controls.Add(closeButton);

            bottomBar = new Panel
            {
                Size = new Size(this.Width, 60),
                Location = new Point(0, this.Height - 60),
                BackColor = Color.Black
            };
            this.Controls.Add(bottomBar);

            var footerLabel = new Label
            {
                Text = "© 2025 PlanIT - Plan Smarter Together",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.White,
                Location = new Point(30, 20),
                AutoSize = true
            };
            bottomBar.Controls.Add(footerLabel);

            sidePanel = new Panel
            {
                Size = new Size(280, this.Height - 130),
                Location = new Point(0, 70),
                BackColor = Color.FromArgb(30, 30, 40)
            };
            this.Controls.Add(sidePanel);

            int sideY = 30;

            var userPanel = new Panel
            {
                Size = new Size(240, 100),
                Location = new Point(20, sideY),
                BackColor = Color.FromArgb(45, 45, 60),
                Cursor = Cursors.Hand
            };
            SetRoundedCorners(userPanel, 15);
            userPanel.Click += (s, e) => ShowProfile();
            sidePanel.Controls.Add(userPanel);

            var avatarLabel = new Label
            {
                Text = loggedInUser.Substring(0, 1).ToUpper(),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(60, 60),
                Location = new Point(15, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(220, 70, 0),
                Cursor = Cursors.Hand
            };
            SetRoundedCorners(avatarLabel, 30);
            avatarLabel.Click += (s, e) => ShowProfile();
            userPanel.Controls.Add(avatarLabel);

            var userNameLabel = new Label
            {
                Text = loggedInUser,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(85, 25),
                Size = new Size(140, 25),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            userNameLabel.Click += (s, e) => ShowProfile();
            userPanel.Controls.Add(userNameLabel);

            var userRoleLabel = new Label
            {
                Text = "View Profile →",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(85, 50),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            userRoleLabel.Click += (s, e) => ShowProfile();
            userPanel.Controls.Add(userRoleLabel);

            sideY += 130;

            btnNewWorkspace = CreateSideButton("➕ New Workspace", new Point(20, sideY), Color.FromArgb(220, 70, 0));
            btnNewWorkspace.Click += NewWorkspace_Click;
            sidePanel.Controls.Add(btnNewWorkspace);
            sideY += 70;

            btnDashboard = CreateSideButton("📊 Dashboard", new Point(20, sideY), Color.FromArgb(60, 60, 80));
            btnDashboard.Click += (s, e) => { currentFilter = "all"; HighlightActiveButton(btnDashboard); ShowWorkspaces(); };
            sidePanel.Controls.Add(btnDashboard);
            sideY += 60;

            btnRecent = CreateSideButton("🕒 Recent", new Point(20, sideY), Color.FromArgb(60, 60, 80));
            btnRecent.Click += (s, e) => { currentFilter = "recent"; HighlightActiveButton(btnRecent); ShowWorkspaces(); };
            sidePanel.Controls.Add(btnRecent);
            sideY += 60;

            btnFavorites = CreateSideButton("⭐ Favorites", new Point(20, sideY), Color.FromArgb(60, 60, 80));
            btnFavorites.Click += (s, e) => { currentFilter = "favorites"; HighlightActiveButton(btnFavorites); ShowWorkspaces(); };
            sidePanel.Controls.Add(btnFavorites);
            sideY += 60;

            btnArchive = CreateSideButton("📦 Archive", new Point(20, sideY), Color.FromArgb(60, 60, 80));
            btnArchive.Click += (s, e) => { currentFilter = "archive"; HighlightActiveButton(btnArchive); ShowWorkspaces(); };
            sidePanel.Controls.Add(btnArchive);
            sideY += 60;

            btnProfile = CreateSideButton("👤 Profile", new Point(20, sideY), Color.FromArgb(60, 60, 80));
            btnProfile.Click += (s, e) => { HighlightActiveButton(btnProfile); ShowProfile(); };
            sidePanel.Controls.Add(btnProfile);
            sideY += 80;

            btnSettings = CreateSideButton("⚙️ Settings", new Point(20, sideY), Color.FromArgb(60, 60, 80));
            btnSettings.Click += Settings_Click;
            sidePanel.Controls.Add(btnSettings);
            sideY += 60;

            btnLogout = CreateSideButton("🚪 Logout", new Point(20, sideY), Color.FromArgb(139, 0, 0));
            btnLogout.Click += (s, e) => this.Close();
            sidePanel.Controls.Add(btnLogout);

            mainPanel = new Panel
            {
                Size = new Size(this.Width - 280, this.Height - 130),
                Location = new Point(280, 70),
                BackColor = Color.FromArgb(245, 247, 250),
                AutoScroll = false
            };
            this.Controls.Add(mainPanel);

            CreateWorkspacePanel();

            profilePanel = new ProfilePanel(loggedInUser, userId, connectionString);
            profilePanel.Visible = false;
            //profilePanel.OnBackToWorkspaces += () => ShowWorkspaces();
            this.Controls.Add(profilePanel);

            HighlightActiveButton(btnDashboard);

            this.KeyPreview = true;
            this.KeyDown += MainMenuForm_KeyDown;
        }

        private void CreateWorkspacePanel()
        {
            int mainPanelWidth = this.Width - 280 - 40;
            headerPanel = new Panel
            {
                Size = new Size(mainPanelWidth, 200),
                Location = new Point(20, 20),
                BackColor = Color.White
            };
            SetRoundedCorners(headerPanel, 20);
            mainPanel.Controls.Add(headerPanel);

            lblWelcome = new Label
            {
                Text = $"Welcome back, {loggedInUser}!",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(30, 30),
                Size = new Size(headerPanel.Width - 60, 45),
                AutoEllipsis = true
            };
            headerPanel.Controls.Add(lblWelcome);

            var subtitleLabel = new Label
            {
                Text = "Manage your workspaces and stay organized",
                Font = new Font("Segoe UI", 13),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(30, 80),
                AutoSize = true
            };
            headerPanel.Controls.Add(subtitleLabel);

            lblWorkspaceCount = new Label
            {
                Text = "0 Workspaces",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 70, 0),
                Location = new Point(30, 110),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblWorkspaceCount);

            int controlsY = 150;
            int rightX = headerPanel.Width - 30;

            sortComboBox = new ComboBox
            {
                Size = new Size(160, 40),
                Location = new Point(rightX - 160, controlsY),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            sortComboBox.Items.AddRange(new object[] {
                "Recently Updated",
                "Recently Created",
                "Name (A-Z)",
                "Name (Z-A)",
                "Oldest First"
            });
            sortComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndexChanged += (s, e) => {
                switch (sortComboBox.SelectedIndex)
                {
                    case 0: currentSort = "updated_desc"; break;
                    case 1: currentSort = "created_desc"; break;
                    case 2: currentSort = "name_asc"; break;
                    case 3: currentSort = "name_desc"; break;
                    case 4: currentSort = "created_asc"; break;
                }
                LoadWorkspaces();
            };
            headerPanel.Controls.Add(sortComboBox);

            searchBox = new TextBox
            {
                Size = new Size(Math.Max(250, rightX - 300 - 30), 40),
                Location = new Point(30, controlsY),
                Font = new Font("Segoe UI", 12),
                Text = "🔍 Search workspaces...",
                ForeColor = Color.Gray
            };
            searchBox.GotFocus += (s, e) => {
                if (searchBox.Text == "🔍 Search workspaces...")
                {
                    searchBox.Text = "";
                    searchBox.ForeColor = Color.Black;
                }
            };
            searchBox.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "🔍 Search workspaces...";
                    searchBox.ForeColor = Color.Gray;
                }
            };
            searchBox.TextChanged += (s, e) => {
                searchDebounceTimer.Stop();
                searchDebounceTimer.Start();
            };
            headerPanel.Controls.Add(searchBox);

            var workspacesLabel = new Label
            {
                Text = "Your Workspaces",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(20, 240),
                AutoSize = true
            };
            mainPanel.Controls.Add(workspacesLabel);

            paginationPanel = new Panel
            {
                Location = new Point(250, 240),
                Size = new Size(300, 40),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(paginationPanel);

            workspaceDisplayPanel = new Panel
            {
                Location = new Point(20, 290),
                Size = new Size(mainPanelWidth, mainPanel.Height - 310),
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            mainPanel.Controls.Add(workspaceDisplayPanel);
        }

        private void CreatePaginationControls()
        {
            paginationPanel.Controls.Clear();

            int totalPages = (int)Math.Ceiling((double)currentWorkspaces.Count / itemsPerPage);
            if (totalPages <= 1) return;

            int buttonWidth = 35;
            int buttonSpacing = 8;

            var prevBtn = new Button
            {
                Text = "◀",
                Size = new Size(buttonWidth, 30),
                Location = new Point(0, 5),
                BackColor = currentPage > 1 ? Color.FromArgb(220, 70, 0) : Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = currentPage > 1 ? Cursors.Hand : Cursors.Default,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = currentPage > 1
            };
            prevBtn.FlatAppearance.BorderSize = 0;
            SetRoundedCorners(prevBtn, 6);
            prevBtn.Click += (s, e) => { if (currentPage > 1) { currentPage--; DisplayCurrentPage(); } };
            paginationPanel.Controls.Add(prevBtn);

            lblPageInfo = new Label
            {
                Text = $"Page {currentPage} of {totalPages}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(buttonWidth + buttonSpacing, 10)
            };
            paginationPanel.Controls.Add(lblPageInfo);

            int labelWidth = TextRenderer.MeasureText(lblPageInfo.Text, lblPageInfo.Font).Width;

            var nextBtn = new Button
            {
                Text = "▶",
                Size = new Size(buttonWidth, 30),
                Location = new Point(buttonWidth + buttonSpacing + labelWidth + buttonSpacing, 5),
                BackColor = currentPage < totalPages ? Color.FromArgb(220, 70, 0) : Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = currentPage < totalPages ? Cursors.Hand : Cursors.Default,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = currentPage < totalPages
            };
            nextBtn.FlatAppearance.BorderSize = 0;
            SetRoundedCorners(nextBtn, 6);
            nextBtn.Click += (s, e) => { if (currentPage < totalPages) { currentPage++; DisplayCurrentPage(); } };
            paginationPanel.Controls.Add(nextBtn);
        }

        private void DisplayCurrentPage()
        {
            workspaceDisplayPanel.Controls.Clear();

            int startIndex = (currentPage - 1) * itemsPerPage;
            int endIndex = Math.Min(startIndex + itemsPerPage, currentWorkspaces.Count);

            int panelWidth = workspaceDisplayPanel.Width;
            int panelHeight = workspaceDisplayPanel.Height;
            int cardMargin = 20;
            int cardGap = 20;

            int cardWidth = (panelWidth - (cardMargin * 2) - cardGap) / 2;
            int cardHeight = panelHeight - (cardMargin * 2);

            for (int i = startIndex; i < endIndex; i++)
            {
                int cardIndex = i - startIndex;
                int xPos = cardMargin + (cardIndex * (cardWidth + cardGap));

                var card = CreateWorkspaceCard(currentWorkspaces[i], cardWidth, cardHeight);
                card.Location = new Point(xPos + 100, cardMargin);
                card.Tag = new Point(xPos, cardMargin);
                workspaceDisplayPanel.Controls.Add(card);

                var slideTimer = new Timer { Interval = 10, Tag = card };
                slideTimer.Tick += (s, e) => {
                    var panel = (Panel)((Timer)s).Tag;
                    var targetPos = (Point)panel.Tag;
                    if (panel.Left > targetPos.X)
                    {
                        panel.Left -= 5;
                    }
                    else
                    {
                        panel.Left = targetPos.X;
                        ((Timer)s).Stop();
                        ((Timer)s).Dispose();
                    }
                };
                slideTimer.Start();
            }

            CreatePaginationControls();
        }

        private void ShowProfile()
        {
            currentView = "profile";
            mainPanel.Visible = false;
            profilePanel.Visible = true;
        }

        private void ShowWorkspaces()
        {
            currentView = "workspaces";
            profilePanel.Visible = false;
            mainPanel.Visible = true;
            LoadWorkspaces();
        }

        private void MainMenuForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N && currentView == "workspaces")
            {
                NewWorkspace_Click(null, null);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.F && currentView == "workspaces")
            {
                searchBox.Focus();
                e.Handled = true;
            }
        }

        private void HighlightActiveButton(Button activeBtn)
        {
            btnDashboard.BackColor = Color.FromArgb(60, 60, 80);
            btnRecent.BackColor = Color.FromArgb(60, 60, 80);
            btnFavorites.BackColor = Color.FromArgb(60, 60, 80);
            btnArchive.BackColor = Color.FromArgb(60, 60, 80);
            btnProfile.BackColor = Color.FromArgb(60, 60, 80);
            activeBtn.BackColor = Color.FromArgb(80, 80, 120);
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            using (SettingsDialog settings = new SettingsDialog(loggedInUser))
            {
                if (settings.ShowDialog() == DialogResult.OK)
                {
                    ApplyTheme(settings.SelectedTheme);
                }
            }
        }

        private void ApplyTheme(string theme)
        {
            switch (theme)
            {
                case "Default":
                    this.BackColor = Color.FromArgb(245, 247, 250);
                    mainPanel.BackColor = Color.FromArgb(245, 247, 250);
                    break;
                case "Dark":
                    this.BackColor = Color.FromArgb(30, 30, 40);
                    mainPanel.BackColor = Color.FromArgb(30, 30, 40);
                    break;
                case "Ocean":
                    this.BackColor = Color.FromArgb(230, 240, 250);
                    mainPanel.BackColor = Color.FromArgb(230, 240, 250);
                    break;
                case "Forest":
                    this.BackColor = Color.FromArgb(235, 245, 235);
                    mainPanel.BackColor = Color.FromArgb(235, 245, 235);
                    break;
            }
        }

        private Button CreateSideButton(string text, Point location, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(240, 50),
                Location = location,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            SetRoundedCorners(btn, 10);

            Color originalColor = backColor;
            btn.MouseEnter += (s, e) => {
                if (btn.BackColor == originalColor || btn.BackColor == Color.FromArgb(80, 80, 120))
                    btn.BackColor = Color.FromArgb(Math.Min(backColor.R + 20, 255),
                        Math.Min(backColor.G + 20, 255), Math.Min(backColor.B + 20, 255));
            };
            btn.MouseLeave += (s, e) => {
                if (btn == btnDashboard || btn == btnRecent || btn == btnFavorites || btn == btnArchive || btn == btnProfile)
                {
                    if ((currentFilter == "all" && btn == btnDashboard && currentView == "workspaces") ||
                        (currentFilter == "recent" && btn == btnRecent && currentView == "workspaces") ||
                        (currentFilter == "favorites" && btn == btnFavorites && currentView == "workspaces") ||
                        (currentFilter == "archive" && btn == btnArchive && currentView == "workspaces") ||
                        (btn == btnProfile && currentView == "profile"))
                    {
                        btn.BackColor = Color.FromArgb(80, 80, 120);
                    }
                    else
                    {
                        btn.BackColor = Color.FromArgb(60, 60, 80);
                    }
                }
                else
                {
                    btn.BackColor = backColor;
                }
            };
            return btn;
        }

        private void LoadWorkspaces()
        {
            currentWorkspaces = GetWorkspacesFromDatabase();
            currentPage = 1;

            lblWorkspaceCount.Text = $"{currentWorkspaces.Count} Workspace{(currentWorkspaces.Count != 1 ? "s" : "")}";

            if (currentWorkspaces.Count == 0)
            {
                workspaceDisplayPanel.Controls.Clear();
                paginationPanel.Controls.Clear();

                var emptyPanel = new Panel
                {
                    Size = new Size(400, 250),
                    BackColor = Color.White,
                    Location = new Point((workspaceDisplayPanel.Width - 400) / 2, (workspaceDisplayPanel.Height - 250) / 2)
                };
                SetRoundedCorners(emptyPanel, 15);

                var emptyIcon = new Label
                {
                    Text = "📁",
                    Font = new Font("Segoe UI", 60),
                    Location = new Point(170, 40),
                    AutoSize = true
                };
                emptyPanel.Controls.Add(emptyIcon);

                var emptyLabel = new Label
                {
                    Text = currentFilter == "all" ? "No workspaces yet" : $"No {currentFilter} workspaces",
                    Font = new Font("Segoe UI", 18, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Location = new Point(50, 140),
                    Size = new Size(300, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                emptyPanel.Controls.Add(emptyLabel);

                var emptySubLabel = new Label
                {
                    Text = "Click 'New Workspace' to get started!",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Location = new Point(50, 180),
                    Size = new Size(300, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                emptyPanel.Controls.Add(emptySubLabel);

                workspaceDisplayPanel.Controls.Add(emptyPanel);
                return;
            }

            DisplayCurrentPage();
        }

        private List<WorkspaceData> GetWorkspacesFromDatabase()
        {
            List<WorkspaceData> workspaces = new List<WorkspaceData>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string checkColumnQuery = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                       WHERE TABLE_SCHEMA = 'planit' 
                                       AND TABLE_NAME = 'workspaces' 
                                       AND COLUMN_NAME = 'category'";
                    bool hasCategory = false;
                    using (MySqlCommand checkCmd = new MySqlCommand(checkColumnQuery, conn))
                    {
                        hasCategory = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                    }

                    string checkColorQuery = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                       WHERE TABLE_SCHEMA = 'planit' 
                                       AND TABLE_NAME = 'workspaces' 
                                       AND COLUMN_NAME = 'color_tag'";
                    bool hasColorTag = false;
                    using (MySqlCommand checkCmd = new MySqlCommand(checkColorQuery, conn))
                    {
                        hasColorTag = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                    }

                    string query = @"SELECT id, workspace_name, file_path, created_at, updated_at, 
                            is_favorite, is_archived" +
                            (hasCategory ? ", category" : "") +
                            (hasColorTag ? ", color_tag" : "") + @" 
                            FROM workspaces 
                            WHERE user_id = @userId";

                    if (currentFilter == "favorites")
                        query += " AND is_favorite = 1";
                    else if (currentFilter == "archive")
                        query += " AND is_archived = 1";
                    else if (currentFilter == "all")
                        query += " AND is_archived = 0";

                    switch (currentSort)
                    {
                        case "updated_desc":
                            query += " ORDER BY updated_at DESC";
                            break;
                        case "created_desc":
                            query += " ORDER BY created_at DESC";
                            break;
                        case "created_asc":
                            query += " ORDER BY created_at ASC";
                            break;
                        case "name_asc":
                            query += " ORDER BY workspace_name ASC";
                            break;
                        case "name_desc":
                            query += " ORDER BY workspace_name DESC";
                            break;
                    }

                    if (currentFilter == "recent")
                        query += " LIMIT 10";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                workspaces.Add(new WorkspaceData
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("workspace_name"),
                                    FilePath = reader.GetString("file_path"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at"),
                                    IsFavorite = reader.GetBoolean("is_favorite"),
                                    IsArchived = reader.GetBoolean("is_archived"),
                                    Category = hasCategory && !reader.IsDBNull(reader.GetOrdinal("category"))
                                              ? reader.GetString("category")
                                              : "Work",
                                    ColorTag = hasColorTag && !reader.IsDBNull(reader.GetOrdinal("color_tag"))
                                              ? reader.GetString("color_tag")
                                              : "#DC4600"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading workspaces: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return workspaces;
        }

        private Panel CreateWorkspaceCard(WorkspaceData workspace, int cardWidth, int cardHeight)
        {
            var card = new Panel
            {
                Size = new Size(cardWidth, cardHeight),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = workspace
            };
            SetRoundedCorners(card, 15);

            var colorTag = new Panel
            {
                Size = new Size(cardWidth, 8),
                Location = new Point(0, 0),
                BackColor = ColorTranslator.FromHtml(workspace.ColorTag),
                Cursor = Cursors.Hand
            };
            card.Controls.Add(colorTag);

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 2))
                {
                    e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
                }
            };

            var iconLabel = new Label
            {
                Text = "📄",
                Font = new Font("Segoe UI", 48),
                Location = new Point(20, 25),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(iconLabel);

            if (workspace.IsFavorite)
            {
                var favStar = new Label
                {
                    Text = "⭐",
                    Font = new Font("Segoe UI", 22),
                    Location = new Point(cardWidth - 45, 270), // 45 20
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(favStar);
            }

            var categoryLabel = new Label
            {
                Text = $"📌 {workspace.Category}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml(workspace.ColorTag),
                Location = new Point(cardWidth - 110, 20), // 110 20
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(categoryLabel);

            var nameLabel = new Label
            {
                Text = workspace.Name,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(30, 125), // 20 95
                Size = new Size(cardWidth - 40, 32),
                AutoEllipsis = true
            };
            card.Controls.Add(nameLabel);

            var dateLabel = new Label
            {
                Text = $"Modified: {workspace.UpdatedAt:MMM dd, yyyy HH:mm}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(30, 162), // 20 132
                AutoSize = true
            };
            card.Controls.Add(dateLabel);

            var createdLabel = new Label
            {
                Text = $"Created: {workspace.CreatedAt:MMM dd, yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(30, 179), // 20 149
                AutoSize = true
            };
            card.Controls.Add(createdLabel);

            int btnY = cardHeight - 100;
            int btnX = 30;
            int btnWidth = 70;
            int btnHeight = 35;
            int btnSpacing = 10;

            var openBtn = CreateActionButton("Open", btnX, btnY, 100, btnHeight, Color.FromArgb(220, 70, 0));
            openBtn.Click += (s, e) => OpenWorkspace(workspace);
            card.Controls.Add(openBtn);
            btnX += 110;

            var favBtn = CreateActionButton(workspace.IsFavorite ? "💛" : "🤍", btnX, btnY, btnWidth, btnHeight, Color.FromArgb(255, 193, 7));
            favBtn.Click += (s, e) => ToggleFavorite(workspace);
            card.Controls.Add(favBtn);
            btnX += btnWidth + btnSpacing;

            var categoryBtn = CreateActionButton("📌", btnX, btnY, btnWidth, btnHeight, ColorTranslator.FromHtml(workspace.ColorTag));
            categoryBtn.Click += (s, e) => ShowCategoryPicker(workspace);
            card.Controls.Add(categoryBtn);
            btnX += btnWidth + btnSpacing;

            var duplicateBtn = CreateActionButton("📋", btnX, btnY, btnWidth, btnHeight, Color.FromArgb(76, 175, 80));
            duplicateBtn.Click += (s, e) => DuplicateWorkspace(workspace);
            card.Controls.Add(duplicateBtn);

            btnX = 30;
            btnY = cardHeight - 55;

            var renameBtn = CreateActionButton("✏️", btnX, btnY, btnWidth, btnHeight, Color.FromArgb(100, 100, 200));
            renameBtn.Click += (s, e) => RenameWorkspace(workspace);
            card.Controls.Add(renameBtn);
            btnX += btnWidth + btnSpacing;

            var archiveBtn = CreateActionButton(workspace.IsArchived ? "📂" : "📦", btnX, btnY, btnWidth, btnHeight, Color.FromArgb(128, 128, 128));
            archiveBtn.Click += (s, e) => ToggleArchive(workspace);
            card.Controls.Add(archiveBtn);
            btnX += btnWidth + btnSpacing;

            var deleteBtn = CreateActionButton("🗑️", btnX, btnY, btnWidth, btnHeight, Color.FromArgb(220, 50, 50));
            deleteBtn.Click += (s, e) => DeleteWorkspace(workspace);
            card.Controls.Add(deleteBtn);

            card.Click += (s, e) => {
                if (!(s is Button))
                    OpenWorkspace(workspace);
            };

            card.MouseEnter += (s, e) => {
                card.BackColor = Color.FromArgb(248, 250, 255);
                card.Invalidate();
            };
            card.MouseLeave += (s, e) => {
                card.BackColor = Color.White;
                card.Invalidate();
            };

            return card;
        }

        private void ShowCategoryPicker(WorkspaceData workspace)
        {
            using (CategoryPickerDialog dialog = new CategoryPickerDialog(workspace.Category, workspace.ColorTag))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();

                            string checkColumnQuery = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                                               WHERE TABLE_SCHEMA = 'planit' 
                                               AND TABLE_NAME = 'workspaces' 
                                               AND COLUMN_NAME = 'category'";
                            using (MySqlCommand checkCmd = new MySqlCommand(checkColumnQuery, conn))
                            {
                                int columnExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                                if (columnExists == 0)
                                {
                                    string addColumnQuery = "ALTER TABLE workspaces ADD COLUMN category VARCHAR(50) DEFAULT 'Work'";
                                    using (MySqlCommand addCmd = new MySqlCommand(addColumnQuery, conn))
                                    {
                                        addCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            string updateQuery = "UPDATE workspaces SET category = @category, color_tag = @color WHERE id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@category", dialog.SelectedCategory);
                                cmd.Parameters.AddWithValue("@color", dialog.SelectedColor);
                                cmd.Parameters.AddWithValue("@id", workspace.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        ShowNotification("✓ Category updated!", Color.FromArgb(76, 175, 80));
                        LoadWorkspaces();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating category: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private Button CreateActionButton(string text, int x, int y, int width, int height, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                Location = new Point(x, y),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", width > 70 ? 10 : 12, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            SetRoundedCorners(btn, 6);

            Color hoverColor = Color.FromArgb(
                Math.Min(backColor.R + 30, 255),
                Math.Min(backColor.G + 30, 255),
                Math.Min(backColor.B + 30, 255)
            );
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }

        private void NewWorkspace_Click(object sender, EventArgs e)
        {
            using (NewWorkspaceDialog dialog = new NewWorkspaceDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string workspaceName = dialog.WorkspaceName;
                    string newFileName = $"{workspaceName}.json";
                    string filePath = Path.Combine(userFolder, newFileName);

                    try
                    {
                        File.WriteAllText(filePath, "[]");

                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string query = @"INSERT INTO workspaces 
                                            (user_id, workspace_name, file_path, created_at, updated_at) 
                                            VALUES (@userId, @name, @path, @created, @updated)";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@userId", userId);
                                cmd.Parameters.AddWithValue("@name", workspaceName);
                                cmd.Parameters.AddWithValue("@path", filePath);
                                cmd.Parameters.AddWithValue("@created", DateTime.Now);
                                cmd.Parameters.AddWithValue("@updated", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        ShowNotification("✓ Workspace created successfully!", Color.FromArgb(76, 175, 80));
                        LoadWorkspaces();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error creating workspace: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OpenWorkspace(WorkspaceData workspace)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE workspaces SET updated_at = @updated WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@updated", DateTime.Now);
                        cmd.Parameters.AddWithValue("@id", workspace.Id);
                        cmd.ExecuteNonQuery();
                    }
                }

                this.Hide();
                ChatMenuForm chatForm = new ChatMenuForm(workspace.FilePath, loggedInUser);
                chatForm.FormClosed += (s, args) => { LoadWorkspaces(); this.Show(); };
                chatForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening workspace: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Show();
            }
        }

        private void ToggleFavorite(WorkspaceData workspace)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE workspaces SET is_favorite = @favorite WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@favorite", !workspace.IsFavorite);
                        cmd.Parameters.AddWithValue("@id", workspace.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                ShowNotification(workspace.IsFavorite ? "Removed from favorites" : "Added to favorites", Color.FromArgb(255, 193, 7));
                LoadWorkspaces();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating favorite: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleArchive(WorkspaceData workspace)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE workspaces SET is_archived = @archived WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@archived", !workspace.IsArchived);
                        cmd.Parameters.AddWithValue("@id", workspace.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                ShowNotification(workspace.IsArchived ? "Unarchived workspace" : "Archived workspace", Color.FromArgb(128, 128, 128));
                LoadWorkspaces();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating archive status: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DuplicateWorkspace(WorkspaceData workspace)
        {
            try
            {
                string newName = $"{workspace.Name}_Copy";
                int counter = 1;

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    while (true)
                    {
                        string checkQuery = "SELECT COUNT(*) FROM workspaces WHERE workspace_name = @name AND user_id = @userId";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@name", newName);
                            checkCmd.Parameters.AddWithValue("@userId", userId);
                            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (exists == 0) break;

                            newName = $"{workspace.Name}_Copy_{counter}";
                            counter++;
                        }
                    }

                    string newFileName = $"{newName}.json";
                    string newFilePath = Path.Combine(userFolder, newFileName);

                    if (File.Exists(workspace.FilePath))
                    {
                        File.Copy(workspace.FilePath, newFilePath);
                    }
                    else
                    {
                        File.WriteAllText(newFilePath, "[]");
                    }

                    string insertQuery = @"INSERT INTO workspaces 
                                          (user_id, workspace_name, file_path, created_at, updated_at) 
                                          VALUES (@userId, @name, @path, @created, @updated)";

                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@userId", userId);
                        insertCmd.Parameters.AddWithValue("@name", newName);
                        insertCmd.Parameters.AddWithValue("@path", newFilePath);
                        insertCmd.Parameters.AddWithValue("@created", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@updated", DateTime.Now);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                ShowNotification("✓ Workspace duplicated successfully!", Color.FromArgb(76, 175, 80));
                LoadWorkspaces();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error duplicating workspace: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenameWorkspace(WorkspaceData workspace)
        {
            using (RenameDialog dialog = new RenameDialog(workspace.Name))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string newName = dialog.NewFileName.Replace(".json", "").Trim();

                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        MessageBox.Show("Workspace name cannot be empty.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();

                            string checkQuery = "SELECT COUNT(*) FROM workspaces WHERE workspace_name = @name AND user_id = @userId AND id != @id";
                            using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@name", newName);
                                checkCmd.Parameters.AddWithValue("@userId", userId);
                                checkCmd.Parameters.AddWithValue("@id", workspace.Id);
                                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                                if (exists > 0)
                                {
                                    MessageBox.Show("A workspace with that name already exists.", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            string updateQuery = "UPDATE workspaces SET workspace_name = @name, updated_at = @updated WHERE id = @id";
                            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@name", newName);
                                updateCmd.Parameters.AddWithValue("@updated", DateTime.Now);
                                updateCmd.Parameters.AddWithValue("@id", workspace.Id);
                                updateCmd.ExecuteNonQuery();
                            }
                        }

                        ShowNotification("✓ Workspace renamed successfully!", Color.FromArgb(76, 175, 80));
                        LoadWorkspaces();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error renaming workspace: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DeleteWorkspace(WorkspaceData workspace)
        {
            var result = MessageBox.Show($"Are you sure you want to delete '{workspace.Name}'?\n\nThis action cannot be undone.",
                                         "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM workspaces WHERE id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", workspace.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (File.Exists(workspace.FilePath))
                    {
                        File.Delete(workspace.FilePath);
                    }

                    ShowNotification("✓ Workspace deleted", Color.FromArgb(220, 50, 50));
                    LoadWorkspaces();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting workspace: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FilterWorkspaces()
        {
            if (searchBox.Text == "🔍 Search workspaces..." || string.IsNullOrWhiteSpace(searchBox.Text))
            {
                LoadWorkspaces();
                return;
            }

            currentWorkspaces = GetWorkspacesFromDatabase()
                .Where(w => w.Name.ToLower().Contains(searchBox.Text.ToLower()))
                .ToList();

            currentPage = 1;
            lblWorkspaceCount.Text = $"{currentWorkspaces.Count} Workspace{(currentWorkspaces.Count != 1 ? "s" : "")} found";

            if (currentWorkspaces.Count == 0)
            {
                workspaceDisplayPanel.Controls.Clear();
                paginationPanel.Controls.Clear();

                var emptyPanel = new Panel
                {
                    Size = new Size(400, 200),
                    BackColor = Color.White,
                    Location = new Point((workspaceDisplayPanel.Width - 400) / 2, (workspaceDisplayPanel.Height - 200) / 2)
                };
                SetRoundedCorners(emptyPanel, 15);

                var emptyLabel = new Label
                {
                    Text = "No workspaces match your search.",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Location = new Point(50, 70),
                    Size = new Size(300, 60),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                emptyPanel.Controls.Add(emptyLabel);

                workspaceDisplayPanel.Controls.Add(emptyPanel);
                return;
            }

            DisplayCurrentPage();
        }

        private void ShowNotification(string message, Color backColor)
        {
            var notification = new Panel
            {
                Size = new Size(350, 60),
                Location = new Point(mainPanel.Width - 370, 20),
                BackColor = backColor
            };
            SetRoundedCorners(notification, 10);

            var messageLabel = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 20),
                AutoSize = true
            };
            notification.Controls.Add(messageLabel);

            if (currentView == "workspaces")
            {
                mainPanel.Controls.Add(notification);
                notification.BringToFront();
            }
            else
            {
                profilePanel.Controls.Add(notification);
                notification.BringToFront();
            }

            var fadeTimer = new Timer { Interval = 2000 };
            fadeTimer.Tick += (s, e) => {
                fadeTimer.Stop();
                if (currentView == "workspaces")
                    mainPanel.Controls.Remove(notification);
                else
                    profilePanel.Controls.Remove(notification);
                notification.Dispose();
            };
            fadeTimer.Start();
        }

        private void SetRoundedCorners(Control c, int radius)
        {
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius * 2, radius * 2), 180, 90);
            path.AddLine(radius, 0, c.Width - radius, 0);
            path.AddArc(new Rectangle(c.Width - radius * 2, 0, radius * 2, radius * 2), 270, 90);
            path.AddLine(c.Width, radius, c.Width, c.Height - radius);
            path.AddArc(new Rectangle(c.Width - radius * 2, c.Height - radius * 2, radius * 2, radius * 2), 0, 90);
            path.AddLine(c.Width - radius, c.Height, radius, c.Height);
            path.AddArc(new Rectangle(0, c.Height - radius * 2, radius * 2, radius * 2), 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        }

        private static class NativeMethods
        {
            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HTCAPTION = 0x2;

            [DllImport("user32.dll")]
            public static extern bool ReleaseCapture();

            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
                int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

            public static void DragMove(IntPtr handle)
            {
                ReleaseCapture();
                SendMessage(handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }

    public class NewWorkspaceDialog : Form
    {
        private TextBox inputBox;
        private Button okButton, cancelButton;
        public string WorkspaceName { get; private set; }

        public NewWorkspaceDialog()
        {
            this.Text = "Create New Workspace";
            this.Size = new Size(500, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var titleLabel = new Label
            {
                Text = "Create New Workspace",
                Location = new Point(30, 20),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            Label label = new Label
            {
                Text = "Enter workspace name:",
                Location = new Point(30, 65),
                Font = new Font("Segoe UI", 11),
                AutoSize = true
            };
            this.Controls.Add(label);

            inputBox = new TextBox
            {
                Text = $"Workspace_{DateTime.Now:yyyyMMdd_HHmmss}",
                Location = new Point(30, 95),
                Width = 430,
                Font = new Font("Segoe UI", 12)
            };
            inputBox.SelectAll();
            this.Controls.Add(inputBox);

            okButton = new Button
            {
                Text = "Create",
                Location = new Point(270, 145),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += (s, e) => { WorkspaceName = inputBox.Text.Trim(); };
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(370, 145),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(120, 120, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }

    public class RenameDialog : Form
    {
        private TextBox inputBox;
        private Button okButton, cancelButton;
        public string NewFileName { get; private set; }

        public RenameDialog(string currentName)
        {
            this.Text = "Rename Workspace";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label label = new Label
            {
                Text = "Enter new workspace name:",
                Location = new Point(30, 30),
                Font = new Font("Segoe UI", 11),
                AutoSize = true
            };
            this.Controls.Add(label);

            inputBox = new TextBox
            {
                Text = currentName,
                Location = new Point(30, 65),
                Width = 430,
                Font = new Font("Segoe UI", 12)
            };
            inputBox.SelectAll();
            this.Controls.Add(inputBox);

            okButton = new Button
            {
                Text = "Rename",
                Location = new Point(270, 120),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += (s, e) => { NewFileName = inputBox.Text.Trim(); };
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(370, 120),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(120, 120, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }

    public class SettingsDialog : Form
    {
        public string SelectedTheme { get; private set; } = "Default";
        private Button selectedThemeButton;

        public SettingsDialog(string username)
        {
            this.Text = "Settings";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var titleLabel = new Label
            {
                Text = "⚙️ Settings",
                Location = new Point(30, 25),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            var userLabel = new Label
            {
                Text = $"Logged in as: {username}",
                Location = new Point(30, 70),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true
            };
            this.Controls.Add(userLabel);

            var line1 = new Panel
            {
                Location = new Point(30, 110),
                Size = new Size(540, 2),
                BackColor = Color.FromArgb(220, 220, 220)
            };
            this.Controls.Add(line1);

            var appearanceLabel = new Label
            {
                Text = "Theme Selection",
                Location = new Point(30, 130),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(appearanceLabel);

            var themeSubLabel = new Label
            {
                Text = "Choose your preferred theme:",
                Location = new Point(50, 165),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true
            };
            this.Controls.Add(themeSubLabel);

            string[] themes = { "Default", "Dark", "Ocean", "Forest" };
            Color[] themeColors = {
                Color.FromArgb(220, 70, 0),
                Color.FromArgb(50, 50, 60),
                Color.FromArgb(30, 144, 255),
                Color.FromArgb(34, 139, 34)
            };

            int themeX = 50;
            int themeY = 200;

            for (int i = 0; i < themes.Length; i++)
            {
                var themeBtn = new Button
                {
                    Text = themes[i],
                    Location = new Point(themeX, themeY),
                    Size = new Size(120, 45),
                    BackColor = themeColors[i],
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = themes[i]
                };
                themeBtn.FlatAppearance.BorderSize = 0;

                if (themes[i] == "Default")
                {
                    selectedThemeButton = themeBtn;
                    themeBtn.FlatAppearance.BorderSize = 3;
                    themeBtn.FlatAppearance.BorderColor = Color.Black;
                }

                themeBtn.Click += (s, e) => {
                    if (selectedThemeButton != null)
                    {
                        selectedThemeButton.FlatAppearance.BorderSize = 0;
                    }
                    selectedThemeButton = themeBtn;
                    themeBtn.FlatAppearance.BorderSize = 3;
                    themeBtn.FlatAppearance.BorderColor = Color.Black;
                    SelectedTheme = (string)themeBtn.Tag;
                };

                this.Controls.Add(themeBtn);
                themeX += 135;
            }

            var line2 = new Panel
            {
                Location = new Point(30, 270),
                Size = new Size(540, 2),
                BackColor = Color.FromArgb(220, 220, 220)
            };
            this.Controls.Add(line2);

            var dataLabel = new Label
            {
                Text = "Data Management",
                Location = new Point(30, 290),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(dataLabel);

            var exportBtn = new Button
            {
                Text = "📤 Export All Workspaces",
                Location = new Point(50, 330),
                Size = new Size(220, 45),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            exportBtn.FlatAppearance.BorderSize = 0;
            exportBtn.Click += (s, e) => MessageBox.Show("Export feature coming soon!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Controls.Add(exportBtn);

            var backupBtn = new Button
            {
                Text = "💾 Backup Data",
                Location = new Point(290, 330),
                Size = new Size(220, 45),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            backupBtn.FlatAppearance.BorderSize = 0;
            backupBtn.Click += (s, e) => MessageBox.Show("Backup feature coming soon!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Controls.Add(backupBtn);

            var applyBtn = new Button
            {
                Text = "Apply Theme",
                Location = new Point(200, 460),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            applyBtn.FlatAppearance.BorderSize = 0;
            this.Controls.Add(applyBtn);

            var closeBtn = new Button
            {
                Text = "Close",
                Location = new Point(330, 460),
                Size = new Size(100, 45),
                BackColor = Color.FromArgb(120, 120, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            this.Controls.Add(closeBtn);
        }
    }

    public class CategoryPickerDialog : Form
    {
        public string SelectedCategory { get; private set; }
        public string SelectedColor { get; private set; }
        private Dictionary<string, string> categoryColors = new Dictionary<string, string>
        {
            { "Work", "#DC4600" },
            { "Business", "#2ECC71" },
            { "School", "#3498DB" },
            { "Personal", "#9B59B6" },
            { "Project", "#E74C3C" },
            { "Research", "#F39C12" },
            { "Meeting", "#1ABC9C" },
            { "Other", "#95A5A6" }
        };

        public CategoryPickerDialog(string currentCategory, string currentColor)
        {
            SelectedCategory = currentCategory;
            SelectedColor = currentColor;

            this.Text = "Choose Category";
            this.Size = new Size(550, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var titleLabel = new Label
            {
                Text = "📌 Choose a category for your workspace",
                Location = new Point(30, 20),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            var subtitleLabel = new Label
            {
                Text = "Categories help organize your workspaces by purpose",
                Location = new Point(30, 50),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(120, 120, 120),
                AutoSize = true
            };
            this.Controls.Add(subtitleLabel);

            int categorySize = 110;
            int spacing = 15;
            int startX = 30;
            int startY = 90;
            int categoriesPerRow = 4;

            int index = 0;
            foreach (var category in categoryColors)
            {
                int row = index / categoriesPerRow;
                int col = index % categoriesPerRow;

                var categoryPanel = new Panel
                {
                    Size = new Size(categorySize, categorySize),
                    Location = new Point(startX + col * (categorySize + spacing), startY + row * (categorySize + spacing)),
                    BackColor = ColorTranslator.FromHtml(category.Value),
                    Cursor = Cursors.Hand,
                    Tag = new KeyValuePair<string, string>(category.Key, category.Value)
                };

                var path = new GraphicsPath();
                path.AddArc(0, 0, 20, 20, 180, 90);
                path.AddArc(categorySize - 20, 0, 20, 20, 270, 90);
                path.AddArc(categorySize - 20, categorySize - 20, 20, 20, 0, 90);
                path.AddArc(0, categorySize - 20, 20, 20, 90, 90);
                path.CloseFigure();
                categoryPanel.Region = new Region(path);

                var iconLabel = new Label
                {
                    Text = GetCategoryIcon(category.Key),
                    Font = new Font("Segoe UI", 32),
                    ForeColor = Color.White,
                    Location = new Point(30, 20),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                categoryPanel.Controls.Add(iconLabel);

                var nameLabel = new Label
                {
                    Text = category.Key,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(5, categorySize - 30),
                    Size = new Size(categorySize - 10, 25),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                categoryPanel.Controls.Add(nameLabel);

                if (category.Key == currentCategory)
                {
                    categoryPanel.Paint += (s, e) => {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (Pen pen = new Pen(Color.Black, 4))
                        {
                            e.Graphics.DrawRectangle(pen, 2, 2, categorySize - 4, categorySize - 4);
                        }
                    };
                }

                EventHandler clickHandler = (s, e) => {
                    var kvp = (KeyValuePair<string, string>)categoryPanel.Tag;
                    SelectedCategory = kvp.Key;
                    SelectedColor = kvp.Value;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                categoryPanel.Click += clickHandler;
                iconLabel.Click += clickHandler;
                nameLabel.Click += clickHandler;

                this.Controls.Add(categoryPanel);
                index++;
            }

            var cancelBtn = new Button
            {
                Text = "Cancel",
                Location = new Point(215, 410),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(120, 120, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            cancelBtn.FlatAppearance.BorderSize = 0;
            this.Controls.Add(cancelBtn);
        }

        private string GetCategoryIcon(string category)
        {
            switch (category)
            {
                case "Work": return "💼";
                case "Business": return "📊";
                case "School": return "🎓";
                case "Personal": return "👤";
                case "Project": return "🚀";
                case "Research": return "🔬";
                case "Meeting": return "📅";
                case "Other": return "📌";
                default: return "📄";
            }
        }
    }
}