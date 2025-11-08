using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class CommandPalette : Form
    {
        private TextBox searchBox;
        private ListBox commandList;
        private Dictionary<string, Action> commands;
        private List<KeyValuePair<string, Action>> filteredCommands;

        public CommandPalette(Dictionary<string, Action> availableCommands)
        {
            commands = availableCommands;
            filteredCommands = commands.ToList();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Command Palette";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 40);
            this.KeyPreview = true;

            Label title = new Label
            {
                Text = "🔍 Command Palette",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            this.Controls.Add(title);

            searchBox = new TextBox
            {
                Location = new Point(20, 50),
                Width = 540,
                Height = 35,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            searchBox.TextChanged += SearchBox_TextChanged;
            searchBox.KeyDown += SearchBox_KeyDown;
            this.Controls.Add(searchBox);

            commandList = new ListBox
            {
                Location = new Point(20, 100),
                Size = new Size(540, 250),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 35
            };
            commandList.DrawItem += CommandList_DrawItem;
            commandList.DoubleClick += CommandList_DoubleClick;
            this.Controls.Add(commandList);

            PopulateCommandList();
            searchBox.Focus();
        }

        private void PopulateCommandList()
        {
            commandList.Items.Clear();
            foreach (var cmd in filteredCommands)
            {
                commandList.Items.Add(cmd.Key);
            }

            if (commandList.Items.Count > 0)
                commandList.SelectedIndex = 0;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredCommands = commands.ToList();
            }
            else
            {
                filteredCommands = commands
                    .Where(cmd => cmd.Key.ToLower().Contains(searchText))
                    .ToList();
            }

            PopulateCommandList();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (commandList.SelectedIndex < commandList.Items.Count - 1)
                    commandList.SelectedIndex++;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (commandList.SelectedIndex > 0)
                    commandList.SelectedIndex--;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelectedCommand();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void CommandList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = isSelected ? Color.FromArgb(220, 70, 0) : Color.FromArgb(40, 40, 50);
            Color textColor = Color.White;

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string commandText = commandList.Items[e.Index].ToString();
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(commandText, e.Font, textBrush, e.Bounds, sf);
            }

            e.DrawFocusRectangle();
        }

        private void CommandList_DoubleClick(object sender, EventArgs e)
        {
            ExecuteSelectedCommand();
        }

        private void ExecuteSelectedCommand()
        {
            if (commandList.SelectedIndex >= 0)
            {
                string selectedCommand = commandList.Items[commandList.SelectedIndex].ToString();

                if (commands.ContainsKey(selectedCommand))
                {
                    this.Close();
                    commands[selectedCommand]?.Invoke();
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}