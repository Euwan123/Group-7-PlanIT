using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace PlanIT2.TabTypes
{
    public class ScheduleTabManager
    {
        private Action MarkUnsaved;
        private bool isDarkMode;
        private MonthCalendar scheduleCalendar;
        private DataGridView currentDgv;
        private ContextMenuStrip timePickerMenu;
        private string connectionString = "Server=localhost;Database=planit;Uid=root;Pwd=;";

        public ScheduleTabManager(Action markUnsaved, bool darkMode)
        {
            MarkUnsaved = markUnsaved;
            isDarkMode = darkMode;
        }

        public TabPage CreateScheduleTab(string name, string content = null)
        {
            TabPage tab = new TabPage(name);
            tab.SetRoundedCorners(15);
            tab.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(245, 245, 245);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(15)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var topPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50
            };

            var addButton = new Button
            {
                Text = "+ Add Schedule",
                Width = 130,
                Height = 38,
                Location = new Point(5, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            addButton.FlatAppearance.BorderSize = 0;

            var sortButton = new Button
            {
                Text = "Sort by Date",
                Width = 110,
                Height = 38,
                Location = new Point(145, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            sortButton.FlatAppearance.BorderSize = 0;

            topPanel.Controls.Add(addButton);
            topPanel.Controls.Add(sortButton);
            mainPanel.Controls.Add(topPanel, 0, 0);

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                ColumnHeadersVisible = true,
                EnableHeadersVisualStyles = false,
                BackgroundColor = isDarkMode ? Color.FromArgb(40, 40, 40) : Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                RowHeadersVisible = true,
                RowHeadersWidth = 50,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToResizeRows = false,
                RowTemplate = { Height = 40 },
                Font = new Font("Segoe UI", 9.5F),
                GridColor = Color.FromArgb(220, 220, 220)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 45;

            dgv.DefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White;
            dgv.DefaultCellStyle.ForeColor = isDarkMode ? Color.FromArgb(220, 220, 220) : Color.FromArgb(40, 40, 40);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 130, 180);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            var idColumn = new DataGridViewTextBoxColumn
            {
                Name = "ID",
                HeaderText = "ID",
                Width = 60,
                ReadOnly = true,
                Visible = false
            };
            dgv.Columns.Add(idColumn);

            var statusColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Status",
                HeaderText = "Done",
                Width = 70,
                FalseValue = false,
                TrueValue = true
            };
            dgv.Columns.Add(statusColumn);

            var dateColumn = new DataGridViewTextBoxColumn
            {
                Name = "Date",
                HeaderText = "Date",
                Width = 120,
                ReadOnly = true
            };
            dgv.Columns.Add(dateColumn);

            var timeColumn = new DataGridViewTextBoxColumn
            {
                Name = "Time",
                HeaderText = "Time",
                Width = 100
            };
            dgv.Columns.Add(timeColumn);

            var priorityColumn = new DataGridViewComboBoxColumn
            {
                Name = "Priority",
                HeaderText = "Priority",
                Width = 100
            };
            priorityColumn.Items.AddRange("High", "Medium", "Low");
            dgv.Columns.Add(priorityColumn);

            var categoryColumn = new DataGridViewTextBoxColumn
            {
                Name = "Category",
                HeaderText = "Category",
                Width = 130
            };
            dgv.Columns.Add(categoryColumn);

            var notesColumn = new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "Notes",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dgv.Columns.Add(notesColumn);

            var dateButtonColumn = new DataGridViewButtonColumn
            {
                Name = "PickDate",
                HeaderText = "Pick",
                Text = "📅",
                UseColumnTextForButtonValue = true,
                Width = 60
            };
            dgv.Columns.Add(dateButtonColumn);

            currentDgv = dgv;

            addButton.Click += AddButton_Click;
            sortButton.Click += SortButton_Click;
            dgv.CellContentClick += Dgv_CellContentClick;
            dgv.CellValueChanged += Dgv_CellValueChanged;
            dgv.UserDeletedRow += Dgv_UserDeletedRow;

            mainPanel.Controls.Add(dgv, 0, 1);
            tab.Controls.Add(mainPanel);

            LoadSchedulesFromDatabase(dgv);

            return tab;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var form = new Form
            {
                Text = "Add Schedule",
                Width = 400,
                Height = 400,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblDate = new Label { Text = "Date:", Location = new Point(20, 20), Width = 80 };
            var txtDate = new TextBox { Location = new Point(110, 20), Width = 150, ReadOnly = true };
            var btnPickDate = new Button { Text = "Pick Date", Location = new Point(270, 18), Width = 90 };

            var lblTime = new Label { Text = "Time (HH:MM):", Location = new Point(20, 60), Width = 80 };
            var txtTime = new TextBox { Location = new Point(110, 60), Width = 150, Text = "00:00" };

            var lblPriority = new Label { Text = "Priority:", Location = new Point(20, 100), Width = 80 };
            var cmbPriority = new ComboBox { Location = new Point(110, 100), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "High", "Medium", "Low" });
            cmbPriority.SelectedIndex = 1;

            var lblCategory = new Label { Text = "Category:", Location = new Point(20, 140), Width = 80 };
            var txtCategory = new TextBox { Location = new Point(110, 140), Width = 250 };

            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 180), Width = 80 };
            var txtNotes = new TextBox { Location = new Point(110, 180), Width = 250, Height = 80, Multiline = true };

            var btnSave = new Button { Text = "Save", Location = new Point(110, 280), Width = 100, BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(220, 280), Width = 100 };

            btnPickDate.Click += (sd, ed) =>
            {
                var cal = new MonthCalendar { Location = new Point(110, 45) };
                form.Controls.Add(cal);
                cal.BringToFront();
                cal.DateSelected += (cs, ce) =>
                {
                    txtDate.Text = cal.SelectionStart.ToShortDateString();
                    form.Controls.Remove(cal);
                    cal.Dispose();
                };
            };

            btnSave.Click += (ss, es) =>
            {
                if (string.IsNullOrEmpty(txtDate.Text))
                {
                    MessageBox.Show("Please select a date", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"INSERT INTO schedules (status, date, time, priority, category, notes) 
                                       VALUES (@status, @date, @time, @priority, @category, @notes)";
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@status", false);
                            cmd.Parameters.AddWithValue("@date", txtDate.Text);
                            cmd.Parameters.AddWithValue("@time", txtTime.Text);
                            cmd.Parameters.AddWithValue("@priority", cmbPriority.Text);
                            cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                            cmd.Parameters.AddWithValue("@notes", txtNotes.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadSchedulesFromDatabase(currentDgv);
                    form.Close();
                    MarkUnsaved();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (sc, ec) => form.Close();

            form.Controls.AddRange(new Control[] { lblDate, txtDate, btnPickDate, lblTime, txtTime, lblPriority, cmbPriority, lblCategory, txtCategory, lblNotes, txtNotes, btnSave, btnCancel });
            form.ShowDialog();
        }

        private void SortButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM schedules ORDER BY STR_TO_DATE(date, '%m/%d/%Y')";
                    using (var adapter = new MySqlDataAdapter(query, conn))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        currentDgv.Rows.Clear();
                        foreach (DataRow row in dt.Rows)
                        {
                            currentDgv.Rows.Add(row["id"], row["status"], row["date"], row["time"], row["priority"], row["category"], row["notes"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sorting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv.Columns[e.ColumnIndex].Name == "PickDate" && e.RowIndex >= 0)
            {
                scheduleCalendar = new MonthCalendar
                {
                    Visible = true,
                    Location = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Location
                };
                dgv.Parent.Controls.Add(scheduleCalendar);
                scheduleCalendar.BringToFront();

                int rowIdx = e.RowIndex;
                scheduleCalendar.DateSelected += (calSender, calE) =>
                {
                    if (rowIdx < dgv.Rows.Count)
                    {
                        dgv.Rows[rowIdx].Cells["Date"].Value = scheduleCalendar.SelectionStart.ToShortDateString();
                        UpdateRecordInDatabase(dgv, rowIdx);
                    }
                    scheduleCalendar.Dispose();
                    scheduleCalendar = null;
                };
            }
        }

        private void Dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (e.RowIndex >= 0 && !dgv.Rows[e.RowIndex].IsNewRow)
            {
                UpdateRecordInDatabase(dgv, e.RowIndex);
            }
        }

        private void Dgv_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            var id = e.Row.Cells["ID"].Value;
            if (id != null)
            {
                DeleteRecordFromDatabase(Convert.ToInt32(id));
            }
        }

        private void LoadSchedulesFromDatabase(DataGridView dgv)
        {
            try
            {
                dgv.Rows.Clear();
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM schedules ORDER BY id DESC";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgv.Rows.Add(
                                reader["id"],
                                Convert.ToBoolean(reader["status"]),
                                reader["date"],
                                reader["time"],
                                reader["priority"],
                                reader["category"],
                                reader["notes"]
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRecordInDatabase(DataGridView dgv, int rowIndex)
        {
            try
            {
                var row = dgv.Rows[rowIndex];
                var id = row.Cells["ID"].Value;
                if (id == null) return;

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE schedules SET status=@status, date=@date, time=@time, 
                                   priority=@priority, category=@category, notes=@notes WHERE id=@id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@status", row.Cells["Status"].Value ?? false);
                        cmd.Parameters.AddWithValue("@date", row.Cells["Date"].Value?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@time", row.Cells["Time"].Value?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@priority", row.Cells["Priority"].Value?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@category", row.Cells["Category"].Value?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@notes", row.Cells["Notes"].Value?.ToString() ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
                MarkUnsaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRecordFromDatabase(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM schedules WHERE id=@id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                MarkUnsaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string GetContent(TabPage tab)
        {
            return "database_mode";
        }
    }
}