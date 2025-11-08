using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace PlanIT2.TabTypes
{
    public class ChecklistTabManager
    {
        private Action MarkUnsaved;
        private bool isDarkMode;
        private string connectionString = "Server=localhost;Database=planit;Uid=root;Pwd=;";
        private FlowLayoutPanel mainFlp;
        private ComboBox filterCombo;

        public ChecklistTabManager(Action markUnsaved, bool darkMode)
        {
            MarkUnsaved = markUnsaved;
            isDarkMode = darkMode;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string createTable = @"CREATE TABLE IF NOT EXISTS checklists (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        status BOOLEAN DEFAULT FALSE,
                        text VARCHAR(500),
                        priority VARCHAR(20) DEFAULT 'Medium',
                        category VARCHAR(100),
                        due_date DATE NULL,
                        notes TEXT,
                        item_order INT DEFAULT 0,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )";
                    using (var cmd = new MySqlCommand(createTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public TabPage CreateChecklistTab(string name, string content = null)
        {
            TabPage tab = new TabPage(name);
            tab.SetRoundedCorners(15);
            tab.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(248, 249, 250);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

            var headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 45
            };

            var clearCompletedBtn = new Button
            {
                Text = "Clear Completed",
                Location = new Point(5, 3),
                Width = 130,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            clearCompletedBtn.FlatAppearance.BorderSize = 0;
            clearCompletedBtn.Click += ClearCompleted_Click;

            filterCombo = new ComboBox
            {
                Location = new Point(145, 8),
                Width = 120,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            filterCombo.Items.AddRange(new object[] { "All", "Work", "Personal", "Shopping", "Health", "Study" });
            filterCombo.SelectedIndex = 0;
            filterCombo.SelectedIndexChanged += FilterCombo_Changed;

            headerPanel.Controls.AddRange(new Control[] { clearCompletedBtn, filterCombo });
            mainLayout.Controls.Add(headerPanel, 0, 0);

            mainFlp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                BackColor = isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(255, 255, 255),
                Padding = new Padding(10)
            };
            mainLayout.Controls.Add(mainFlp, 0, 1);

            var addPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 55,
                Padding = new Padding(0, 5, 0, 0)
            };

            var addButton = new Button
            {
                Text = "+ Add New Task",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => ShowAddTaskDialog();

            addPanel.Controls.Add(addButton);
            mainLayout.Controls.Add(addPanel, 0, 2);

            tab.Controls.Add(mainLayout);
            LoadChecklistsFromDatabase();

            return tab;
        }

        private void ShowAddTaskDialog(string prefilledText = "")
        {
            var form = new Form
            {
                Text = "Add New Task",
                Width = 450,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White
            };

            var lblTask = new Label { Text = "Task:", Location = new Point(20, 20), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtTask = new TextBox { Location = new Point(110, 20), Width = 300, Text = prefilledText, Font = new Font("Segoe UI", 10F) };

            var lblPriority = new Label { Text = "Priority:", Location = new Point(20, 60), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var cmbPriority = new ComboBox { Location = new Point(110, 60), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "High", "Medium", "Low" });
            cmbPriority.SelectedIndex = 1;

            var lblCategory = new Label { Text = "Category:", Location = new Point(20, 100), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtCategory = new TextBox { Location = new Point(110, 100), Width = 300, Font = new Font("Segoe UI", 10F) };

            var lblDueDate = new Label { Text = "Due Date:", Location = new Point(20, 140), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtDueDate = new TextBox { Location = new Point(110, 140), Width = 150, ReadOnly = true };
            var btnPickDate = new Button { Text = "Pick Date", Location = new Point(270, 138), Width = 90, BackColor = Color.SteelBlue, ForeColor = Color.White };
            var btnClearDate = new Button { Text = "Clear", Location = new Point(370, 138), Width = 60, BackColor = Color.Gray, ForeColor = Color.White };

            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 180), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtNotes = new TextBox { Location = new Point(110, 180), Width = 300, Height = 120, Multiline = true, ScrollBars = ScrollBars.Vertical };

            var btnSave = new Button { Text = "Save Task", Location = new Point(110, 320), Width = 140, Height = 40, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(260, 320), Width = 140, Height = 40, BackColor = Color.Gray, ForeColor = Color.White };

            btnPickDate.Click += (sd, ed) =>
            {
                var cal = new MonthCalendar { Location = new Point(110, 165) };
                form.Controls.Add(cal);
                cal.BringToFront();
                cal.DateSelected += (cs, ce) =>
                {
                    txtDueDate.Text = cal.SelectionStart.ToShortDateString();
                    form.Controls.Remove(cal);
                    cal.Dispose();
                };
            };

            btnClearDate.Click += (sc, ec) => txtDueDate.Clear();

            btnSave.Click += (ss, es) =>
            {
                if (string.IsNullOrWhiteSpace(txtTask.Text))
                {
                    MessageBox.Show("Please enter a task", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"INSERT INTO checklists (status, text, priority, category, due_date, notes, item_order) 
                                       VALUES (@status, @text, @priority, @category, @due_date, @notes, @order)";
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@status", false);
                            cmd.Parameters.AddWithValue("@text", txtTask.Text);
                            cmd.Parameters.AddWithValue("@priority", cmbPriority.Text);
                            cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                            cmd.Parameters.AddWithValue("@due_date", string.IsNullOrEmpty(txtDueDate.Text) ? (object)DBNull.Value : DateTime.Parse(txtDueDate.Text));
                            cmd.Parameters.AddWithValue("@notes", txtNotes.Text);
                            cmd.Parameters.AddWithValue("@order", 0);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadChecklistsFromDatabase();
                    form.Close();
                    MarkUnsaved();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (sc, ec) => form.Close();

            form.Controls.AddRange(new Control[] { lblTask, txtTask, lblPriority, cmbPriority, lblCategory, txtCategory, lblDueDate, txtDueDate, btnPickDate, btnClearDate, lblNotes, txtNotes, btnSave, btnCancel });
            form.ShowDialog();
        }

        private void LoadChecklistsFromDatabase()
        {
            try
            {
                mainFlp.Controls.Clear();
                string filter = filterCombo?.SelectedItem?.ToString() ?? "All";

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = filter == "All"
                        ? "SELECT * FROM checklists ORDER BY item_order, id DESC"
                        : "SELECT * FROM checklists WHERE category=@category ORDER BY item_order, id DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (filter != "All")
                            cmd.Parameters.AddWithValue("@category", filter);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime? dueDate = reader["due_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["due_date"]);
                                AddChecklistItemFromDb(
                                    Convert.ToInt32(reader["id"]),
                                    reader["text"].ToString(),
                                    Convert.ToBoolean(reader["status"]),
                                    reader["priority"].ToString(),
                                    reader["category"].ToString(),
                                    dueDate,
                                    reader["notes"].ToString()
                                );
                            }
                        }
                    }
                }
                UpdateChecklistProgress();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddChecklistItemFromDb(int id, string text, bool isChecked, string priority, string category, DateTime? dueDate, string notes)
        {
            var itemPanel = new Panel
            {
                Width = mainFlp.Width - 40,
                Height = 70,
                Margin = new Padding(5),
                BackColor = GetPriorityColor(priority, isChecked)
            };

            var checkBox = new CheckBox
            {
                Text = text,
                AutoSize = false,
                Width = itemPanel.Width - 200,
                Height = 20,
                Checked = isChecked,
                Font = new Font("Segoe UI", 10F, isChecked ? FontStyle.Strikeout : FontStyle.Regular),
                Location = new Point(10, 10),
                ForeColor = isDarkMode ? Color.White : Color.Black
            };

            var infoPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 35),
                Width = itemPanel.Width - 200,
                Height = 25,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false
            };

            var categoryLabel = new Label
            {
                Text = $"📁 {category}",
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Margin = new Padding(0, 0, 15, 0)
            };

            var dueDateLabel = new Label
            {
                Text = dueDate.HasValue ? $"📅 {dueDate.Value.ToShortDateString()}" : "",
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                ForeColor = dueDate.HasValue && dueDate.Value < DateTime.Now ? Color.Red : Color.FromArgb(100, 100, 100)
            };

            infoPanel.Controls.Add(categoryLabel);
            if (dueDate.HasValue)
                infoPanel.Controls.Add(dueDateLabel);

            var notesBtn = new Button
            {
                Text = "📝",
                Width = 40,
                Height = 40,
                Location = new Point(itemPanel.Width - 165, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F),
                Cursor = Cursors.Hand
            };
            notesBtn.FlatAppearance.BorderSize = 0;
            notesBtn.Click += (s, e) => ShowNotes(notes);

            var editBtn = new Button
            {
                Text = "✏️",
                Width = 40,
                Height = 40,
                Location = new Point(itemPanel.Width - 120, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F),
                Cursor = Cursors.Hand
            };
            editBtn.FlatAppearance.BorderSize = 0;
            editBtn.Click += (s, e) => EditTask(id);

            var removeBtn = new Button
            {
                Text = "✕",
                Width = 40,
                Height = 40,
                Location = new Point(itemPanel.Width - 65, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            removeBtn.FlatAppearance.BorderSize = 0;
            removeBtn.Click += (s, e) => DeleteTask(id);

            checkBox.CheckedChanged += (s, e) =>
            {
                checkBox.Font = new Font("Segoe UI", 10F, checkBox.Checked ? FontStyle.Strikeout : FontStyle.Regular);
                itemPanel.BackColor = GetPriorityColor(priority, checkBox.Checked);
                UpdateTaskStatus(id, checkBox.Checked);
            };

            itemPanel.Controls.AddRange(new Control[] { checkBox, infoPanel, notesBtn, editBtn, removeBtn });
            mainFlp.Controls.Add(itemPanel);
        }

        private Color GetPriorityColor(string priority, bool isChecked)
        {
            if (isChecked)
                return isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(230, 230, 230);

            switch (priority)
            {
                case "High":
                    return isDarkMode ? Color.FromArgb(60, 30, 30) : Color.FromArgb(255, 220, 220);
                case "Medium":
                    return isDarkMode ? Color.FromArgb(60, 50, 30) : Color.FromArgb(255, 250, 220);
                case "Low":
                    return isDarkMode ? Color.FromArgb(30, 60, 30) : Color.FromArgb(220, 255, 220);
                default:
                    return isDarkMode ? Color.FromArgb(50, 50, 50) : Color.White;
            }
        }

        private void ShowNotes(string notes)
        {
            MessageBox.Show(string.IsNullOrEmpty(notes) ? "No notes available" : notes, "Task Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditTask(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM checklists WHERE id=@id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DateTime? dueDate = reader["due_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["due_date"]);
                                ShowEditDialog(
                                    id,
                                    reader["text"].ToString(),
                                    reader["priority"].ToString(),
                                    reader["category"].ToString(),
                                    dueDate,
                                    reader["notes"].ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowEditDialog(int id, string text, string priority, string category, DateTime? dueDate, string notes)
        {
            var form = new Form
            {
                Text = "Edit Task",
                Width = 450,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White
            };

            var lblTask = new Label { Text = "Task:", Location = new Point(20, 20), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtTask = new TextBox { Location = new Point(110, 20), Width = 300, Text = text, Font = new Font("Segoe UI", 10F) };

            var lblPriority = new Label { Text = "Priority:", Location = new Point(20, 60), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var cmbPriority = new ComboBox { Location = new Point(110, 60), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "High", "Medium", "Low" });
            cmbPriority.SelectedItem = priority;

            var lblCategory = new Label { Text = "Category:", Location = new Point(20, 100), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtCategory = new TextBox { Location = new Point(110, 100), Width = 300, Font = new Font("Segoe UI", 10F), Text = category };

            var lblDueDate = new Label { Text = "Due Date:", Location = new Point(20, 140), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtDueDate = new TextBox { Location = new Point(110, 140), Width = 150, ReadOnly = true, Text = dueDate?.ToShortDateString() ?? "" };
            var btnPickDate = new Button { Text = "Pick Date", Location = new Point(270, 138), Width = 90, BackColor = Color.SteelBlue, ForeColor = Color.White };
            var btnClearDate = new Button { Text = "Clear", Location = new Point(370, 138), Width = 60, BackColor = Color.Gray, ForeColor = Color.White };

            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 180), Width = 80, ForeColor = isDarkMode ? Color.White : Color.Black };
            var txtNotes = new TextBox { Location = new Point(110, 180), Width = 300, Height = 120, Multiline = true, ScrollBars = ScrollBars.Vertical, Text = notes };

            var btnSave = new Button { Text = "Update Task", Location = new Point(110, 320), Width = 140, Height = 40, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(260, 320), Width = 140, Height = 40, BackColor = Color.Gray, ForeColor = Color.White };

            btnPickDate.Click += (sd, ed) =>
            {
                var cal = new MonthCalendar { Location = new Point(110, 165) };
                form.Controls.Add(cal);
                cal.BringToFront();
                cal.DateSelected += (cs, ce) =>
                {
                    txtDueDate.Text = cal.SelectionStart.ToShortDateString();
                    form.Controls.Remove(cal);
                    cal.Dispose();
                };
            };

            btnClearDate.Click += (sc, ec) => txtDueDate.Clear();

            btnSave.Click += (ss, es) =>
            {
                if (string.IsNullOrWhiteSpace(txtTask.Text))
                {
                    MessageBox.Show("Please enter a task", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"UPDATE checklists SET text=@text, priority=@priority, category=@category, 
                                       due_date=@due_date, notes=@notes WHERE id=@id";
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@text", txtTask.Text);
                            cmd.Parameters.AddWithValue("@priority", cmbPriority.Text);
                            cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                            cmd.Parameters.AddWithValue("@due_date", string.IsNullOrEmpty(txtDueDate.Text) ? (object)DBNull.Value : DateTime.Parse(txtDueDate.Text));
                            cmd.Parameters.AddWithValue("@notes", txtNotes.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadChecklistsFromDatabase();
                    form.Close();
                    MarkUnsaved();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (sc, ec) => form.Close();

            form.Controls.AddRange(new Control[] { lblTask, txtTask, lblPriority, cmbPriority, lblCategory, txtCategory, lblDueDate, txtDueDate, btnPickDate, btnClearDate, lblNotes, txtNotes, btnSave, btnCancel });
            form.ShowDialog();
        }

        private void UpdateTaskStatus(int id, bool status)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE checklists SET status=@status WHERE id=@id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.ExecuteNonQuery();
                    }
                }
                UpdateChecklistProgress();
                MarkUnsaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteTask(int id)
        {
            var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM checklists WHERE id=@id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadChecklistsFromDatabase();
                MarkUnsaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearCompleted_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Delete all completed tasks?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM checklists WHERE status=1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadChecklistsFromDatabase();
                MarkUnsaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterCombo_Changed(object sender, EventArgs e)
        {
            LoadChecklistsFromDatabase();
        }

        private void UpdateChecklistProgress()
        {

        }

        public string GetContent(TabPage tab)
        {
            return "database_mode";
        }
    }
}