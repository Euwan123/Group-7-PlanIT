using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class KeyboardShortcut
    {
        public Keys Key { get; set; }
        public Keys Modifiers { get; set; }
        public Action Action { get; set; }
        public string Description { get; set; }

        public string GetDisplayText()
        {
            string text = "";
            if ((Modifiers & Keys.Control) == Keys.Control)
                text += "Ctrl+";
            if ((Modifiers & Keys.Shift) == Keys.Shift)
                text += "Shift+";
            if ((Modifiers & Keys.Alt) == Keys.Alt)
                text += "Alt+";

            text += Key.ToString();
            return text;
        }
    }

    public class KeyboardShortcutManager
    {
        private Form parentForm;
        private List<KeyboardShortcut> shortcuts = new List<KeyboardShortcut>();

        public KeyboardShortcutManager(Form form)
        {
            parentForm = form;
            parentForm.KeyPreview = true;
            parentForm.KeyDown += ParentForm_KeyDown;
        }

        public void RegisterShortcut(Keys key, Keys modifiers, Action action, string description)
        {
            shortcuts.Add(new KeyboardShortcut
            {
                Key = key,
                Modifiers = modifiers,
                Action = action,
                Description = description
            });
        }

        private void ParentForm_KeyDown(object sender, KeyEventArgs e)
        {
            Keys pressedKey = e.KeyCode;
            Keys modifiers = e.Modifiers;

            foreach (var shortcut in shortcuts)
            {
                if (shortcut.Key == pressedKey && shortcut.Modifiers == modifiers)
                {
                    shortcut.Action?.Invoke();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }
        }

        public void ShowShortcutDialog()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Keyboard Shortcuts";
                dialog.Size = new Size(600, 500);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.White;

                Label title = new Label
                {
                    Text = "⌨️ Keyboard Shortcuts",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 70, 0),
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                dialog.Controls.Add(title);

                ListView listView = new ListView
                {
                    Location = new Point(20, 70),
                    Size = new Size(540, 350),
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    Font = new Font("Segoe UI", 10)
                };

                listView.Columns.Add("Shortcut", 180);
                listView.Columns.Add("Action", 340);

                foreach (var shortcut in shortcuts)
                {
                    var item = new ListViewItem(shortcut.GetDisplayText());
                    item.SubItems.Add(shortcut.Description);
                    listView.Items.Add(item);
                }

                dialog.Controls.Add(listView);

                Button btnClose = new Button
                {
                    Text = "Close",
                    Location = new Point(470, 430),
                    Size = new Size(90, 35),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10),
                    DialogResult = DialogResult.OK
                };
                btnClose.FlatAppearance.BorderSize = 0;
                dialog.Controls.Add(btnClose);

                dialog.ShowDialog(parentForm);
            }
        }
    }
}