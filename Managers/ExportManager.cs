using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class ExportManager
    {
        private TabControl tabControl;

        public ExportManager(TabControl mainTabControl)
        {
            tabControl = mainTabControl;
        }

        public void ShowExportDialog(Form owner)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Export Options";
                dialog.Size = new Size(500, 350);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.White;

                Label title = new Label
                {
                    Text = "📤 Export Content",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 70, 0),
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                dialog.Controls.Add(title);

                Label lblScope = new Label
                {
                    Text = "Export:",
                    Location = new Point(20, 80),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11)
                };
                dialog.Controls.Add(lblScope);

                ComboBox cmbScope = new ComboBox
                {
                    Location = new Point(120, 77),
                    Width = 340,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 11)
                };
                cmbScope.Items.AddRange(new string[] { "Current Tab", "All Tabs" });
                cmbScope.SelectedIndex = 0;
                dialog.Controls.Add(cmbScope);

                Label lblFormat = new Label
                {
                    Text = "Format:",
                    Location = new Point(20, 130),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11)
                };
                dialog.Controls.Add(lblFormat);

                ComboBox cmbFormat = new ComboBox
                {
                    Location = new Point(120, 127),
                    Width = 340,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 11)
                };
                cmbFormat.Items.AddRange(new string[] { "Plain Text (.txt)", "Rich Text (.rtf)", "HTML (.html)", "Markdown (.md)" });
                cmbFormat.SelectedIndex = 0;
                dialog.Controls.Add(cmbFormat);

                Button btnExport = new Button
                {
                    Text = "Export",
                    Location = new Point(250, 250),
                    Size = new Size(100, 40),
                    BackColor = Color.FromArgb(34, 139, 34),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold)
                };
                btnExport.FlatAppearance.BorderSize = 0;
                btnExport.Click += (s, e) =>
                {
                    ExportContent(cmbScope.SelectedIndex == 0, cmbFormat.SelectedIndex);
                    dialog.Close();
                };
                dialog.Controls.Add(btnExport);

                Button btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(360, 250),
                    Size = new Size(100, 40),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11)
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (s, e) => dialog.Close();
                dialog.Controls.Add(btnCancel);

                dialog.ShowDialog(owner);
            }
        }

        private void ExportContent(bool currentTabOnly, int formatIndex)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            switch (formatIndex)
            {
                case 0: sfd.Filter = "Text Files (*.txt)|*.txt"; break;
                case 1: sfd.Filter = "Rich Text Files (*.rtf)|*.rtf"; break;
                case 2: sfd.Filter = "HTML Files (*.html)|*.html"; break;
                case 3: sfd.Filter = "Markdown Files (*.md)|*.md"; break;
            }

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder content = new StringBuilder();

                    if (currentTabOnly)
                    {
                        if (tabControl.SelectedTab != null)
                        {
                            content.AppendLine($"# {tabControl.SelectedTab.Text}");
                            content.AppendLine();
                            content.AppendLine(GetTabContent(tabControl.SelectedTab));
                        }
                    }
                    else
                    {
                        foreach (TabPage tab in tabControl.TabPages)
                        {
                            content.AppendLine($"# {tab.Text}");
                            content.AppendLine();
                            content.AppendLine(GetTabContent(tab));
                            content.AppendLine();
                            content.AppendLine("---");
                            content.AppendLine();
                        }
                    }

                    File.WriteAllText(sfd.FileName, content.ToString());

                    MessageBox.Show("Content exported successfully!", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetTabContent(TabPage tab)
        {
            foreach (Control c in tab.Controls)
            {
                if (c is RichTextBox rtb)
                    return rtb.Text;

                if (c is Panel panel)
                {
                    foreach (Control pc in panel.Controls)
                    {
                        if (pc is RichTextBox prtb)
                            return prtb.Text;
                    }
                }
            }
            return "";
        }
    }
}