using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PlanIT2.Managers
{
    public class DictionaryManager
    {
        private Dictionary<string, string> dictionary;
        private string connectionString = "Server=localhost;Database=planit;Uid=root;Pwd=;";
        private Panel dictionaryPanel;
        private ListView listView;
        private Label countLabel;
        private TextBox searchBox;
        private bool isDictionaryVisible = false;
        private ComboBox filterComboBox;
        private string currentUsername; // Store current logged-in username
        private Button requestBtn;
        private Button manageRequestsBtn;

        public DictionaryManager(string username)
        {
            currentUsername = username;     
            LoadDictionaryFromDatabase();
        }
        private void LoadDictionaryFromDatabase()
        {
            // Use OrdinalIgnoreCase for case-insensitive lookup
            dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string selectQuery = "SELECT word, definition FROM dictionary ORDER BY word";
                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string word = reader["word"].ToString();
                            string definition = reader["definition"].ToString();
                            dictionary[word] = definition;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dictionary: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateDefaultWords(MySqlConnection conn)
        {
            var defaultWords = new Dictionary<string, string>
            {
                {"Plan", "A detailed proposal for doing or achieving something; a scheme or method of acting."},
                {"Organize", "To arrange systematically; to order and structure elements efficiently."},
                {"Schedule", "A plan for carrying out a process or procedure, giving lists of intended events and times."},
                {"Task", "A piece of work to be done or undertaken; an assignment or duty."},
                {"Goal", "The object of a person's ambition or effort; an aim or desired result."},
                {"Priority", "A thing that is regarded as more important than others; precedence in order or importance."},
                {"Deadline", "The latest time or date by which something should be completed."},
                {"Productivity", "The effectiveness of productive effort, especially measured in terms of output per unit."},
                {"Efficiency", "The state or quality of being efficient; achieving maximum output with minimum wasted effort."},
                {"Focus", "The center of interest or activity; to pay particular attention to something."},
                {"Strategy", "A plan of action designed to achieve a long-term or overall aim."},
                {"Objective", "A goal or target to be achieved; something aimed at or sought after."},
                {"Milestone", "A significant stage or event in the development of something."},
                {"Progress", "Forward or onward movement toward a destination or goal."},
                {"Achievement", "A thing done successfully with effort, skill, or courage."},
                {"Commitment", "The state or quality of being dedicated to a cause or activity."},
                {"Consistency", "Conformity in the application of something; steadfast adherence to principles."},
                {"Delegation", "The assignment of responsibility or authority to another person to carry out specific activities."},
                {"Execution", "The carrying out of a plan, order, or course of action."},
                {"Initiative", "The ability to assess and initiate things independently; taking action proactively."}
            };

            string insertQuery = "INSERT INTO dictionary (word, definition, category, added_by) VALUES (@word, @definition, @category, @added_by)";
            foreach (var entry in defaultWords)
            {
                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@word", entry.Key);
                    cmd.Parameters.AddWithValue("@definition", entry.Value);
                    cmd.Parameters.AddWithValue("@category", "Productivity");
                    cmd.Parameters.AddWithValue("@added_by", "System");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Panel CreateDictionaryPanel(int width, int height)
        {
            dictionaryPanel = new Panel
            {
                Size = new Size(width, height),
                BackColor = Color.FromArgb(245, 245, 250),
                Visible = false
            };

            // Header
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(65, 105, 225)
            };

            Label titleLabel = new Label
            {
                // CHANGE 1: New Title
                Text = "📖 English Word Dictionary",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);

            countLabel = new Label
            {
                Text = $"{dictionary.Count} words available",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(220, 230, 255),
                Location = new Point(25, 60),
                AutoSize = true
            };
            headerPanel.Controls.Add(countLabel);

            Button closeBtn = new Button
            {
                // CHANGE 2: "Back" button -> Arrow Only
                Text = "←",
                Size = new Size(60, 45), // Reduced size for arrow only
                Location = new Point(width - 85, 25), // Adjusted location
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 18, FontStyle.Bold), // Larger font for arrow
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            closeBtn.Click += (s, e) => HideDictionary();
            headerPanel.Controls.Add(closeBtn);

            // Search and Action Panel
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(25, 15, 25, 15)
            };

            // REMOVED: Search icon (Label searchIcon) per request

            searchBox = new TextBox
            {
                // ADJUSTED: Position to fill where the icon was
                Location = new Point(25, 23),
                Width = width - 650,
                Height = 35,
                Font = new Font("Segoe UI", 13),
                BorderStyle = BorderStyle.FixedSingle
            };
            // Search will be word-only now, handled in FilterListView
            searchBox.TextChanged += (s, e) => FilterListView();
            searchPanel.Controls.Add(searchBox);

            Label filterLabel = new Label
            {
                Text = "Filter:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(width - 560, 27),
                AutoSize = true
            };
            searchPanel.Controls.Add(filterLabel);

            filterComboBox = new ComboBox
            {
                Location = new Point(width - 510, 23),
                Width = 100,
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterComboBox.Items.AddRange(new object[] { "All", "A-E", "F-J", "K-O", "P-T", "U-Z" });
            filterComboBox.SelectedIndex = 0;
            filterComboBox.SelectedIndexChanged += (s, e) => FilterListView();
            searchPanel.Controls.Add(filterComboBox);

            // Button initialization and positioning
            int rightmostButtonX = width - 60; // Start position for the rightmost button (Refresh)
            int buttonSpacing = 10;
            int buttonWidthLarge = 160;
            int buttonWidthSmall = 145;
            int buttonWidthAdminAdd = 100; // New width for 'Add' button

            // Refresh Button (Rightmost)
            Button refreshBtn = new Button
            {
                Text = "🔄",
                Location = new Point(rightmostButtonX - 38, 20),
                Size = new Size(38, 38),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(33, 136, 56);
            refreshBtn.Click += (s, e) =>
            {
                LoadDictionaryFromDatabase();
                RefreshListView();
                MessageBox.Show("Dictionary refreshed!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            ToolTip refreshTooltip = new ToolTip();
            refreshTooltip.SetToolTip(refreshBtn, "Refresh Dictionary");
            searchPanel.Controls.Add(refreshBtn);

            // Calculate next button position to the left of Refresh
            int nextButtonX = refreshBtn.Location.X - buttonSpacing;

            if (IsAdmin())
            {
                // Admin: Manage Requests button (next to Refresh)
                manageRequestsBtn = new Button
                {
                    Text = "⚙️ Manage Requests",
                    Location = new Point(nextButtonX - buttonWidthLarge, 20),
                    Size = new Size(buttonWidthLarge, 38),
                    BackColor = Color.FromArgb(220, 70, 0),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                manageRequestsBtn.FlatAppearance.BorderSize = 0;
                manageRequestsBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 0);
                manageRequestsBtn.Click += (s, e) => ShowManageRequestsDialog();
                searchPanel.Controls.Add(manageRequestsBtn);

                nextButtonX = manageRequestsBtn.Location.X - buttonSpacing;

                // Admin: "Add" button (replaces Request Word)
                requestBtn = new Button
                {
                    // CHANGE 3: Admin gets "Add" (Renamed)
                    Text = "➕ Add",
                    Location = new Point(nextButtonX - buttonWidthAdminAdd, 20), // Use new smaller width
                    Size = new Size(buttonWidthAdminAdd, 38),
                    BackColor = Color.FromArgb(40, 167, 69), // Green for direct add
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                requestBtn.FlatAppearance.BorderSize = 0;
                requestBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(33, 136, 56);
                // CHANGE 4: Admin clicks show Add Word dialog
                requestBtn.Click += (s, e) => ShowAddWordDialog();
                searchPanel.Controls.Add(requestBtn);
            }
            else
            {
                // Regular User: Request Word button
                requestBtn = new Button
                {
                    Text = "📝 Request Word",
                    Location = new Point(nextButtonX - buttonWidthSmall, 20),
                    Size = new Size(buttonWidthSmall, 38),
                    BackColor = Color.FromArgb(255, 193, 7),
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                requestBtn.FlatAppearance.BorderSize = 0;
                requestBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 173, 0);
                requestBtn.Click += (s, e) => ShowRequestWordDialog();
                searchPanel.Controls.Add(requestBtn);
            }

            // ListView
            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // ADJUSTED: Column widths for better display
            int wordColumnWidth = 200;
            listView.Columns.Add("Word", wordColumnWidth);
            listView.Columns.Add("Definition", width - wordColumnWidth - 50); // Use remaining space

            listView.OwnerDraw = true;
            listView.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(65, 105, 225)), e.Bounds);
                e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI", 11, FontStyle.Bold),
                    Brushes.White, e.Bounds.Left + 5, e.Bounds.Top + 5);
            };

            listView.DrawItem += (s, e) => { e.DrawDefault = true; };

            listView.DrawSubItem += (s, e) =>
            {
                if (e.ItemIndex % 2 == 0)
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(248, 249, 250)), e.Bounds);
                else
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds);

                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, listView.Font, e.Bounds, Color.Black,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            };

            listView.DoubleClick += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    string word = listView.SelectedItems[0].Text;
                    // Using ToLowerInvariant to get the key regardless of casing displayed in the list
                    string dictionaryKey = dictionary.Keys.FirstOrDefault(k => k.Equals(word, StringComparison.OrdinalIgnoreCase));

                    if (dictionaryKey != null)
                    {
                        string definition = dictionary[dictionaryKey];
                        ShowWordDetailDialog(word, definition);
                    }
                }
            };

            // Bottom Info Panel
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            Label infoLabel = new Label
            {
                Text = IsAdmin() ?
                    "👑 Admin Mode | 💡 Double-click word to view | ➕ Click 'Add' to instantly add new words" :
                    "💡 Double-click word to view full definition | 📝 Click 'Request Word' to suggest new words",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(25, 15),
                AutoSize = true
            };
            bottomPanel.Controls.Add(infoLabel);

            dictionaryPanel.Controls.Add(listView);
            dictionaryPanel.Controls.Add(bottomPanel); // Add bottom panel before search and header for z-index (DockStyle.Bottom)
            dictionaryPanel.Controls.Add(searchPanel);
            dictionaryPanel.Controls.Add(headerPanel);

            RefreshListView();

            return dictionaryPanel;
        }

        private bool IsAdmin()
        {
            return currentUsername.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        // NEW: Admin 'Add Word' Dialog (immediate addition)
        // ------------------------------------------------------------------
        private void ShowAddWordDialog()
        {
            Form dialog = new Form
            {
                Text = "Add New Word - Admin",
                Size = new Size(550, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(40, 167, 69) // Green for immediate add
            };

            Label titleLabel = new Label
            {
                Text = "➕ Add New Word",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);
            dialog.Controls.Add(headerPanel);

            // ... (Input controls are the same as request dialog)
            Label lblWord = new Label { Text = "Word:", Location = new Point(30, 90), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            TextBox txtWord = new TextBox { Location = new Point(30, 115), Width = 480, Font = new Font("Segoe UI", 12) };
            Label lblDefinition = new Label { Text = "Definition:", Location = new Point(30, 155), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            TextBox txtDefinition = new TextBox { Location = new Point(30, 180), Width = 480, Height = 130, Font = new Font("Segoe UI", 11), Multiline = true, ScrollBars = ScrollBars.Vertical };
            Label lblCategory = new Label { Text = "Category:", Location = new Point(30, 325), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            ComboBox cmbCategory = new ComboBox { Location = new Point(30, 350), Width = 200, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "General", "Productivity", "Business", "Technology", "Other" });
            cmbCategory.SelectedIndex = 0;

            dialog.Controls.AddRange(new Control[] { lblWord, txtWord, lblDefinition, txtDefinition, lblCategory, cmbCategory });

            Button btnAdd = new Button
            {
                Text = "Add to Dictionary",
                Location = new Point(280, 345),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(65, 105, 225),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtWord.Text) || string.IsNullOrWhiteSpace(txtDefinition.Text))
                {
                    MessageBox.Show("Please enter both a word and a definition", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check for existence before adding
                if (dictionary.ContainsKey(txtWord.Text.Trim()))
                {
                    MessageBox.Show($"The word '{txtWord.Text.Trim()}' already exists in the dictionary.",
                                "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Call immediate addition logic
                AddWordToDictionary(txtWord.Text.Trim(), txtDefinition.Text.Trim(), cmbCategory.SelectedItem.ToString());
                dialog.Close();
                LoadDictionaryFromDatabase(); // Reload after addition
                RefreshListView(); // Update the main list
            };
            dialog.Controls.Add(btnAdd);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(450, 345),
                Size = new Size(60, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => dialog.Close();
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }

        private void AddWordToDictionary(string word, string definition, string category)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string insertQuery = @"INSERT INTO dictionary (word, definition, category, added_by) 
                                         VALUES (@word, @definition, @category, @added_by)";
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@word", word);
                        cmd.Parameters.AddWithValue("@definition", definition);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@added_by", currentUsername); // Will be "Admin"
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Word '{word}' has been successfully added to the dictionary!",
                        "Word Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry error
            {
                MessageBox.Show($"The word '{word}' already exists in the dictionary.",
                               "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding word: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ------------------------------------------------------------------
        // END NEW: Admin 'Add Word' Dialog
        // ------------------------------------------------------------------


        private void ShowRequestWordDialog()
        {
            Form dialog = new Form
            {
                Text = "Request New Word",
                Size = new Size(550, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(255, 193, 7)
            };

            Label titleLabel = new Label
            {
                Text = "📝 Request New Word",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(20, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);
            dialog.Controls.Add(headerPanel);

            Label lblWord = new Label
            {
                Text = "Word:",
                Location = new Point(30, 90),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            dialog.Controls.Add(lblWord);

            TextBox txtWord = new TextBox
            {
                Location = new Point(30, 115),
                Width = 480,
                Font = new Font("Segoe UI", 12)
            };
            dialog.Controls.Add(txtWord);

            Label lblDefinition = new Label
            {
                Text = "Definition:",
                Location = new Point(30, 155),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            dialog.Controls.Add(lblDefinition);

            TextBox txtDefinition = new TextBox
            {
                Location = new Point(30, 180),
                Width = 480,
                Height = 130,
                Font = new Font("Segoe UI", 11),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            dialog.Controls.Add(txtDefinition);

            Label lblCategory = new Label
            {
                Text = "Category:",
                Location = new Point(30, 325),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            dialog.Controls.Add(lblCategory);

            ComboBox cmbCategory = new ComboBox
            {
                Location = new Point(30, 350),
                Width = 200,
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.AddRange(new object[] { "General", "Productivity", "Business", "Technology", "Other" });
            cmbCategory.SelectedIndex = 0;
            dialog.Controls.Add(cmbCategory);

            Button btnSubmit = new Button
            {
                Text = "Submit Request",
                Location = new Point(280, 345),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtWord.Text))
                {
                    MessageBox.Show("Please enter a word", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtDefinition.Text))
                {
                    MessageBox.Show("Please enter a definition", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SubmitWordRequest(txtWord.Text.Trim(), txtDefinition.Text.Trim(), cmbCategory.SelectedItem.ToString());
                dialog.Close();
            };
            dialog.Controls.Add(btnSubmit);

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(410, 345),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => dialog.Close();
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }

        private void SubmitWordRequest(string word, string definition, string category)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if word already exists in dictionary (case-insensitive check)
                    string checkQuery = "SELECT COUNT(*) FROM dictionary WHERE word = @word COLLATE utf8mb4_unicode_ci";
                    using (MySqlCommand cmd = new MySqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@word", word);
                        int exists = Convert.ToInt32(cmd.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show($"The word '{word}' already exists in the dictionary.",
                                "Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    // Check if request already pending
                    string checkPendingQuery = "SELECT COUNT(*) FROM dictionary_requests WHERE word = @word AND status = 'pending'";
                    using (MySqlCommand cmd = new MySqlCommand(checkPendingQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@word", word);
                        int pending = Convert.ToInt32(cmd.ExecuteScalar());
                        if (pending > 0)
                        {
                            MessageBox.Show($"A request for '{word}' is already pending approval.",
                                "Request Pending", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO dictionary_requests 
                        (word, definition, category, requested_by, status) 
                        VALUES (@word, @definition, @category, @requested_by, 'pending')";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@word", word);
                        cmd.Parameters.AddWithValue("@definition", definition);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@requested_by", currentUsername);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Word request submitted successfully!\n\nWord: {word}\n\nYour request will be reviewed by the administrator.",
                        "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting request: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowManageRequestsDialog()
        {
            Form dialog = new Form
            {
                Text = "Manage Word Requests - Admin Panel",
                Size = new Size(1000, 700),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                BackColor = Color.White
            };

            // FIX: Reduced header height from 80 to 50 for better list visibility
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(220, 70, 0)
            };

            Label titleLabel = new Label
            {
                Text = "⚙️ Manage Word Requests",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);

            // FIX: Moved countRequestsLabel to the right side of the header
            Label countRequestsLabel = new Label
            {
                Text = "Loading requests...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(255, 220, 220),
                Location = new Point(dialog.Width - 250, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true
            };
            headerPanel.Controls.Add(countRequestsLabel);

            dialog.Controls.Add(headerPanel);

            // Requests ListView
            ListView requestsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            requestsListView.Columns.Add("ID", 50);
            requestsListView.Columns.Add("Word", 150);
            requestsListView.Columns.Add("Definition", 350);
            requestsListView.Columns.Add("Category", 100);
            requestsListView.Columns.Add("Requested By", 120);
            requestsListView.Columns.Add("Date", 150);

            requestsListView.OwnerDraw = true;
            requestsListView.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 70, 0)), e.Bounds);
                e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI", 10, FontStyle.Bold),
                    Brushes.White, e.Bounds.Left + 5, e.Bounds.Top + 5);
            };

            requestsListView.DrawItem += (s, e) => { e.DrawDefault = true; };
            requestsListView.DrawSubItem += (s, e) =>
            {
                if (e.ItemIndex % 2 == 0)
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(248, 249, 250)), e.Bounds);
                else
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds);

                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, requestsListView.Font, e.Bounds, Color.Black,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };

            dialog.Controls.Add(requestsListView);

            // Bottom Action Panel
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            Button approveBtn = new Button
            {
                Text = "✅ Approve",
                Location = new Point(20, 15),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            approveBtn.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(approveBtn);

            Button rejectBtn = new Button
            {
                Text = "❌ Reject",
                Location = new Point(150, 15),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            rejectBtn.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(rejectBtn);

            Button viewBtn = new Button
            {
                Text = "👁️ View Details",
                Location = new Point(280, 15),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(65, 105, 225),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            viewBtn.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(viewBtn);

            Button refreshRequestsBtn = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(430, 15),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            refreshRequestsBtn.FlatAppearance.BorderSize = 0;
            bottomPanel.Controls.Add(refreshRequestsBtn);

            Button closeDialogBtn = new Button
            {
                Text = "Close",
                // Adjusted position for better spacing in the bottom panel
                Location = new Point(dialog.Size.Width - 140, 15),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            closeDialogBtn.FlatAppearance.BorderSize = 0;
            closeDialogBtn.Click += (s, e) => dialog.Close();
            bottomPanel.Controls.Add(closeDialogBtn);

            dialog.Controls.Add(bottomPanel);

            // Load pending requests
            Action loadRequests = () =>
            {
                requestsListView.Items.Clear();
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"SELECT id, word, definition, category, requested_by, requested_at 
                                       FROM dictionary_requests 
                                       WHERE status = 'pending' 
                                       ORDER BY requested_at DESC";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem(reader["id"].ToString());
                                item.SubItems.Add(reader["word"].ToString());
                                item.SubItems.Add(reader["definition"].ToString());
                                item.SubItems.Add(reader["category"].ToString());
                                item.SubItems.Add(reader["requested_by"].ToString());
                                item.SubItems.Add(Convert.ToDateTime(reader["requested_at"]).ToString("yyyy-MM-dd HH:mm"));
                                requestsListView.Items.Add(item);
                            }
                        }

                        countRequestsLabel.Text = $"{requestsListView.Items.Count} pending requests";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading requests: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            loadRequests();

            // Enable/disable buttons based on selection
            requestsListView.SelectedIndexChanged += (s, e) =>
            {
                bool hasSelection = requestsListView.SelectedItems.Count > 0;
                approveBtn.Enabled = hasSelection;
                rejectBtn.Enabled = hasSelection;
                viewBtn.Enabled = hasSelection;
            };

            // Approve button action
            approveBtn.Click += (s, e) =>
            {
                if (requestsListView.SelectedItems.Count == 0) return;

                var item = requestsListView.SelectedItems[0];
                int requestId = int.Parse(item.SubItems[0].Text);
                string word = item.SubItems[1].Text;
                string definition = item.SubItems[2].Text;
                string category = item.SubItems[3].Text;

                var result = MessageBox.Show(
                    $"Approve this word request?\n\nWord: {word}\nDefinition: {definition}\n\nThis will add the word to the dictionary.",
                    "Confirm Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ApproveRequest(requestId, word, definition, category);
                    loadRequests();
                    LoadDictionaryFromDatabase();
                    RefreshListView();
                }
            };

            // Reject button action
            rejectBtn.Click += (s, e) =>
            {
                if (requestsListView.SelectedItems.Count == 0) return;

                var item = requestsListView.SelectedItems[0];
                int requestId = int.Parse(item.SubItems[0].Text);
                string word = item.SubItems[1].Text;

                var result = MessageBox.Show(
                    $"Reject this word request?\n\nWord: {word}\n\nThis action cannot be undone.",
                    "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    RejectRequest(requestId);
                    loadRequests();
                }
            };

            // View details button action
            viewBtn.Click += (s, e) =>
            {
                if (requestsListView.SelectedItems.Count == 0) return;

                var item = requestsListView.SelectedItems[0];
                string word = item.SubItems[1].Text;
                string definition = item.SubItems[2].Text;
                string category = item.SubItems[3].Text;
                string requestedBy = item.SubItems[4].Text;
                string requestedAt = item.SubItems[5].Text;

                ShowRequestDetailsDialog(word, definition, category, requestedBy, requestedAt);
            };

            // Refresh button action
            refreshRequestsBtn.Click += (s, e) => loadRequests();

            dialog.ShowDialog();
        }

        private void ApproveRequest(int requestId, string word, string definition, string category)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Start transaction
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check if word already exists in dictionary before adding to prevent transaction rollback error 
                            // on duplicate key, especially in a race condition.
                            string checkQuery = "SELECT COUNT(*) FROM dictionary WHERE word = @word COLLATE utf8mb4_unicode_ci";
                            using (MySqlCommand cmd = new MySqlCommand(checkQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@word", word);
                                int exists = Convert.ToInt32(cmd.ExecuteScalar());
                                if (exists == 0)
                                {
                                    // Add word to dictionary
                                    string insertQuery = @"INSERT INTO dictionary (word, definition, category, added_by) 
                                                         VALUES (@word, @definition, @category, @added_by)";
                                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@word", word);
                                        insertCmd.Parameters.AddWithValue("@definition", definition);
                                        insertCmd.Parameters.AddWithValue("@category", category);
                                        insertCmd.Parameters.AddWithValue("@added_by", currentUsername);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // If it somehow exists, still update the request status but inform the admin
                                    MessageBox.Show($"Warning: Word '{word}' already exists in the dictionary, but the request status will be updated to approved.",
                                        "Duplicate Word", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }


                                // Update request status
                                string updateQuery = @"UPDATE dictionary_requests 
                                                 SET status = 'approved', 
                                                     reviewed_by = @reviewed_by, 
                                                     reviewed_at = NOW() 
                                                 WHERE id = @id";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@reviewed_by", currentUsername);
                                    updateCmd.Parameters.AddWithValue("@id", requestId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                transaction.Commit();

                                MessageBox.Show($"Word '{word}' has been approved and added to the dictionary!",
                                    "Request Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error approving request: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RejectRequest(int requestId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = @"UPDATE dictionary_requests 
                                         SET status = 'rejected', 
                                             reviewed_by = @reviewed_by, 
                                             reviewed_at = NOW() 
                                         WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@reviewed_by", currentUsername);
                        cmd.Parameters.AddWithValue("@id", requestId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Request has been rejected.",
                        "Request Rejected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rejecting request: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRequestDetailsDialog(string word, string definition, string category,
            string requestedBy, string requestedAt)
        {
            Form detailDialog = new Form
            {
                Text = "Request Details",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(65, 105, 225)
            };

            Label titleLabel = new Label
            {
                Text = "📋 Request Details",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);
            detailDialog.Controls.Add(headerPanel);

            int yPos = 90;

            // Word
            Label lblWordTitle = new Label
            {
                Text = "Word:",
                Location = new Point(30, yPos),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblWordTitle);

            Label lblWord = new Label
            {
                Text = word,
                Location = new Point(30, yPos + 25),
                Width = 530,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(65, 105, 225)
            };
            detailDialog.Controls.Add(lblWord);

            yPos += 65;

            // Definition
            Label lblDefTitle = new Label
            {
                Text = "Definition:",
                Location = new Point(30, yPos),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblDefTitle);

            TextBox txtDefinition = new TextBox
            {
                Text = definition,
                Location = new Point(30, yPos + 25),
                Width = 530,
                Height = 120,
                Font = new Font("Segoe UI", 11),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            detailDialog.Controls.Add(txtDefinition);

            yPos += 160;

            // Category
            Label lblCategoryTitle = new Label
            {
                Text = "Category:",
                Location = new Point(30, yPos),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblCategoryTitle);

            Label lblCategory = new Label
            {
                Text = category,
                Location = new Point(120, yPos),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblCategory);

            yPos += 30;

            // Requested By
            Label lblReqByTitle = new Label
            {
                Text = "Requested By:",
                Location = new Point(30, yPos),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblReqByTitle);

            Label lblReqBy = new Label
            {
                Text = requestedBy,
                Location = new Point(150, yPos),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblReqBy);

            yPos += 30;

            // Requested At
            Label lblReqAtTitle = new Label
            {
                Text = "Requested At:",
                Location = new Point(30, yPos),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblReqAtTitle);

            Label lblReqAt = new Label
            {
                Text = requestedAt,
                Location = new Point(150, yPos),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            detailDialog.Controls.Add(lblReqAt);

            // Close button
            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(460, 410),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => detailDialog.Close();
            detailDialog.Controls.Add(btnClose);

            detailDialog.ShowDialog();
        }

        // NEW METHOD: Check if a word is favorited
        private bool IsWordFavorited(string word)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM favorites WHERE username = @username AND word = @word";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", currentUsername);
                        cmd.Parameters.AddWithValue("@word", word);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking favorite status: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // NEW METHOD: Toggle favorite status
        private void ToggleFavoriteWord(string word)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    if (IsWordFavorited(word))
                    {
                        // Remove favorite
                        string deleteQuery = "DELETE FROM favorites WHERE username = @username AND word = @word";
                        using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@username", currentUsername);
                            cmd.Parameters.AddWithValue("@word", word);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Word '{word}' removed from favorites.", "Favorite Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Add favorite
                        string insertQuery = "INSERT INTO favorites (username, word) VALUES (@username, @word)";
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@username", currentUsername);
                            cmd.Parameters.AddWithValue("@word", word);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Word '{word}' added to favorites!", "Favorite Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling favorite: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowDictionary(Form parent)
        {
            if (dictionaryPanel != null)
            {
                isDictionaryVisible = true;
                dictionaryPanel.Visible = true;
                dictionaryPanel.BringToFront();
                RefreshListView();
            }
        }

        public void HideDictionary()
        {
            if (dictionaryPanel != null)
            {
                isDictionaryVisible = false;
                dictionaryPanel.Visible = false;
            }
        }

        public void ToggleDictionary()
        {
            if (isDictionaryVisible)
                HideDictionary();
            else
                ShowDictionary(null);
        }

        private void RefreshListView()
        {
            if (listView == null) return;

            // Clear search box and filter when refreshing to show full list
            if (searchBox != null) searchBox.Text = "";
            if (filterComboBox != null) filterComboBox.SelectedIndex = 0;

            listView.Items.Clear();
            foreach (var entry in dictionary.OrderBy(x => x.Key))
            {
                ListViewItem item = new ListViewItem(entry.Key);
                item.SubItems.Add(entry.Value);
                item.Font = new Font("Segoe UI", 11);
                listView.Items.Add(item);
            }

            if (countLabel != null)
            {
                countLabel.Text = $"{dictionary.Count} words available";
            }
        }

        private void FilterListView()
        {
            if (listView == null) return;

            string searchTerm = searchBox?.Text?.Trim().ToLower() ?? "";
            string filter = filterComboBox?.SelectedItem?.ToString() ?? "All";

            listView.Items.Clear();

            var filteredDict = dictionary.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // FIX: Only search by WORD, not definition
                filteredDict = filteredDict.Where(x => x.Key.ToLower().Contains(searchTerm));
            }

            if (filter != "All")
            {
                switch (filter)
                {
                    case "A-E":
                        filteredDict = filteredDict.Where(x => x.Key.ToUpper()[0] >= 'A' && x.Key.ToUpper()[0] <= 'E');
                        break;
                    case "F-J":
                        filteredDict = filteredDict.Where(x => x.Key.ToUpper()[0] >= 'F' && x.Key.ToUpper()[0] <= 'J');
                        break;
                    case "K-O":
                        filteredDict = filteredDict.Where(x => x.Key.ToUpper()[0] >= 'K' && x.Key.ToUpper()[0] <= 'O');
                        break;
                    case "P-T":
                        filteredDict = filteredDict.Where(x => x.Key.ToUpper()[0] >= 'P' && x.Key.ToUpper()[0] <= 'T');
                        break;
                    case "U-Z":
                        filteredDict = filteredDict.Where(x => x.Key.ToUpper()[0] >= 'U' && x.Key.ToUpper()[0] <= 'Z');
                        break;
                }
            }

            foreach (var entry in filteredDict.OrderBy(x => x.Key))
            {
                ListViewItem item = new ListViewItem(entry.Key);
                item.SubItems.Add(entry.Value);
                listView.Items.Add(item);
            }

            if (countLabel != null)
            {
                countLabel.Text = $"{listView.Items.Count} words displayed";
            }
        }

        private void ShowWordDetailDialog(string word, string definition)
        {
            Form dialog = new Form
            {
                Text = "Word Details",
                Size = new Size(600, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(65, 105, 225)
            };

            Label wordLabel = new Label
            {
                Text = word,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(wordLabel);
            dialog.Controls.Add(headerPanel);

            Label defLabel = new Label
            {
                Text = "Definition:",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };
            dialog.Controls.Add(defLabel);

            TextBox txtDefinition = new TextBox
            {
                Location = new Point(20, 130),
                Width = 540,
                Height = 200,
                Font = new Font("Segoe UI", 12),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = definition,
                ReadOnly = true,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            dialog.Controls.Add(txtDefinition);

            // NEW: Favorite Button
            bool isFavorited = IsWordFavorited(word);

            Button btnFavorite = new Button
            {
                Text = isFavorited ? "⭐ Favorited" : "☆ Favorite",
                Location = new Point(20, 350),
                Size = new Size(130, 40),
                BackColor = isFavorited ? Color.FromArgb(255, 193, 7) : Color.FromArgb(108, 117, 125),
                ForeColor = isFavorited ? Color.Black : Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFavorite.FlatAppearance.BorderSize = 0;
            btnFavorite.Click += (s, e) =>
            {
                ToggleFavoriteWord(word);
                dialog.Close(); // Close and let user reopen to see updated status, or implement dynamic update logic here
            };
            dialog.Controls.Add(btnFavorite);


            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(460, 350),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => dialog.Close();
            dialog.Controls.Add(btnClose);

            dialog.ShowDialog();
        }

        public string LookupWord(string word)
        {
            if (dictionary.ContainsKey(word))
                return dictionary[word];
            return null;
        }
    }
}