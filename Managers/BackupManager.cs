using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class BackupManager : IDisposable
    {
        private string workspacePath;
        private Action saveCallback;
        private Timer autoSaveTimer;
        private string backupFolder;

        public BackupManager(string filePath, Action saveWorkspaceCallback)
        {
            workspacePath = filePath;
            saveCallback = saveWorkspaceCallback;
            backupFolder = Path.Combine(Path.GetDirectoryName(filePath), "Backups");

            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
        }

        public void StartAutoSave(int intervalMinutes)
        {
            StopAutoSave();

            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = intervalMinutes * 60 * 1000;
            autoSaveTimer.Tick += (s, e) => PerformAutoSave();
            autoSaveTimer.Start();
        }

        public void StopAutoSave()
        {
            if (autoSaveTimer != null)
            {
                autoSaveTimer.Stop();
                autoSaveTimer.Dispose();
                autoSaveTimer = null;
            }
        }

        private void PerformAutoSave()
        {
            try
            {
                saveCallback?.Invoke();
                CreateBackup();
            }
            catch
            {
                // Silent fail for auto-save
            }
        }

        public void CreateBackup()
        {
            try
            {
                if (!File.Exists(workspacePath))
                    return;

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{Path.GetFileNameWithoutExtension(workspacePath)}_backup_{timestamp}.json";
                string backupPath = Path.Combine(backupFolder, backupFileName);

                File.Copy(workspacePath, backupPath, true);

                // Keep only last 10 backups
                CleanOldBackups(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanOldBackups(int maxBackups)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupFolder, "*.json")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(maxBackups)
                    .ToList();

                foreach (var file in backupFiles)
                {
                    file.Delete();
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public void ShowBackupDialog(Form owner)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Backup Manager";
                dialog.Size = new Size(600, 500);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.White;

                Label title = new Label
                {
                    Text = "💾 Backup Manager",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 70, 0),
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                dialog.Controls.Add(title);

                ListView listView = new ListView
                {
                    Location = new Point(20, 70),
                    Size = new Size(540, 320),
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    Font = new Font("Segoe UI", 10)
                };

                listView.Columns.Add("Backup Name", 300);
                listView.Columns.Add("Date", 120);
                listView.Columns.Add("Size", 100);

                LoadBackupList(listView);

                dialog.Controls.Add(listView);

                Button btnCreate = new Button
                {
                    Text = "Create Backup",
                    Location = new Point(20, 410),
                    Size = new Size(130, 40),
                    BackColor = Color.FromArgb(34, 139, 34),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnCreate.FlatAppearance.BorderSize = 0;
                btnCreate.Click += (s, e) =>
                {
                    CreateBackup();
                    LoadBackupList(listView);
                    MessageBox.Show("Backup created successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                dialog.Controls.Add(btnCreate);

                Button btnRestore = new Button
                {
                    Text = "Restore",
                    Location = new Point(160, 410),
                    Size = new Size(100, 40),
                    BackColor = Color.FromArgb(70, 130, 180),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnRestore.FlatAppearance.BorderSize = 0;
                btnRestore.Click += (s, e) =>
                {
                    if (listView.SelectedItems.Count > 0)
                    {
                        var result = MessageBox.Show("This will replace your current workspace. Continue?",
                            "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            string backupPath = (string)listView.SelectedItems[0].Tag;
                            File.Copy(backupPath, workspacePath, true);
                            MessageBox.Show("Backup restored! Please restart the application.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            dialog.Close();
                        }
                    }
                };
                dialog.Controls.Add(btnRestore);

                Button btnDelete = new Button
                {
                    Text = "Delete",
                    Location = new Point(270, 410),
                    Size = new Size(100, 40),
                    BackColor = Color.FromArgb(139, 0, 0),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += (s, e) =>
                {
                    if (listView.SelectedItems.Count > 0)
                    {
                        var result = MessageBox.Show("Delete selected backup?",
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            string backupPath = (string)listView.SelectedItems[0].Tag;
                            File.Delete(backupPath);
                            LoadBackupList(listView);
                        }
                    }
                };
                dialog.Controls.Add(btnDelete);

                Button btnClose = new Button
                {
                    Text = "Close",
                    Location = new Point(460, 410),
                    Size = new Size(100, 40),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10)
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => dialog.Close();
                dialog.Controls.Add(btnClose);

                dialog.ShowDialog(owner);
            }
        }

        private void LoadBackupList(ListView listView)
        {
            listView.Items.Clear();

            if (!Directory.Exists(backupFolder))
                return;

            var backupFiles = Directory.GetFiles(backupFolder, "*.json")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            foreach (var file in backupFiles)
            {
                var item = new ListViewItem(file.Name);
                item.SubItems.Add(file.CreationTime.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add($"{file.Length / 1024} KB");
                item.Tag = file.FullName;
                listView.Items.Add(item);
            }
        }

        public void Dispose()
        {
            StopAutoSave();
        }
    }
}