//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Windows.Forms;

//namespace PlanIT2
//{
//    public class NewTabDialog : Form
//    {
//        private TextBox nameBox;
//        private ComboBox typeBox;
//        private Button okButton;
//        private Button cancelButton;
//        private Button colorButton;
//        private TextBox tagsBox;
//        private ComboBox categoryBox;
//        private NumericUpDown priorityBox;

//        public string TabName { get; private set; }
//        public string TabType { get; private set; }
//        public Color TabColor { get; private set; }
//        public List<string> Tags { get; private set; }
//        public string Category { get; private set; }
//        public int Priority { get; private set; }

//        public NewTabDialog()
//        {
//            this.Text = "New Tab";
//            this.Size = new Size(400, 350);
//            this.StartPosition = FormStartPosition.CenterParent;
//            this.FormBorderStyle = FormBorderStyle.None;
//            this.SetRoundedCorners(15);
//            this.BackColor = Color.FromArgb(230, 230, 255);
//            this.TabColor = SystemColors.Control;

//            Label nameLabel = new Label()
//            {
//                Text = "Tab Name:",
//                Location = new Point(50, 40),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10)
//            };

//            nameBox = new TextBox()
//            {
//                Location = new Point(150, 40),
//                Width = 200,
//                Font = new Font("Segoe UI", 10),
//                BorderStyle = BorderStyle.FixedSingle
//            };

//            Label typeLabel = new Label()
//            {
//                Text = "Tab Type:",
//                Location = new Point(50, 80),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10)
//            };

//            typeBox = new ComboBox()
//            {
//                Location = new Point(150, 80),
//                Width = 200,
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Font = new Font("Segoe UI", 10),
//                Items = { "Text", "Checklist", "Drawing", "Schedule" }
//            };
//            typeBox.SelectedIndex = 0;

//            Label colorLabel = new Label()
//            {
//                Text = "Tab Color:",
//                Location = new Point(50, 120),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10)
//            };

//            colorButton = new Button()
//            {
//                Text = "Pick Color",
//                Location = new Point(150, 115),
//                Size = new Size(100, 30),
//                BackColor = TabColor,
//                FlatStyle = FlatStyle.Flat
//            };
//            colorButton.Click += ColorButton_Click;

//            Label tagsLabel = new Label()
//            {
//                Text = "Tags:",
//                Location = new Point(50, 160),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10)
//            };
//            tagsBox = new TextBox()
//            {
//                Location = new Point(150, 160),
//                Width = 200,
//                Font = new Font("Segoe UI", 10),
//                BorderStyle = BorderStyle.FixedSingle
//            };

//            Label categoryLabel = new Label()
//            {
//                Text = "Category:",
//                Location = new Point(50, 200),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10)
//            };
//            categoryBox = new ComboBox()
//            {
//                Location = new Point(150, 200),
//                Width = 200,
//                DropDownStyle = ComboBoxStyle.DropDown,
//                Font = new Font("Segoe UI", 10),
//                Items = { "Work", "Personal", "School", "Hobby" }
//            };

//            Label priorityLabel = new Label()
//            {
//                Text = "Priority (1-10):",
//                Location = new Point(50, 240),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10)
//            };
//            priorityBox = new NumericUpDown()
//            {
//                Location = new Point(150, 240),
//                Width = 50,
//                Font = new Font("Segoe UI", 10),
//                Minimum = 1,
//                Maximum = 10,
//                Value = 5
//            };

//            okButton = new Button()
//            {
//                Text = "OK",
//                Location = new Point(180, 280),
//                Size = new Size(80, 30),
//                DialogResult = DialogResult.OK,
//                BackColor = Color.SeaGreen,
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat,
//                Font = new Font("Segoe UI", 10, FontStyle.Bold)
//            };
//            okButton.FlatAppearance.BorderSize = 0;
//            okButton.SetRoundedCorners(10);
//            okButton.Click += OkButton_Click;

//            cancelButton = new Button()
//            {
//                Text = "Cancel",
//                Location = new Point(270, 280),
//                Size = new Size(80, 30),
//                DialogResult = DialogResult.Cancel,
//                BackColor = Color.Maroon,
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat,
//                Font = new Font("Segoe UI", 10, FontStyle.Bold)
//            };
//            cancelButton.FlatAppearance.BorderSize = 0;
//            cancelButton.SetRoundedCorners(10);

//            this.Controls.Add(nameLabel);
//            this.Controls.Add(nameBox);
//            this.Controls.Add(typeLabel);
//            this.Controls.Add(typeBox);
//            this.Controls.Add(colorLabel);
//            this.Controls.Add(colorButton);
//            this.Controls.Add(tagsLabel);
//            this.Controls.Add(tagsBox);
//            this.Controls.Add(categoryLabel);
//            this.Controls.Add(categoryBox);
//            this.Controls.Add(priorityLabel);
//            this.Controls.Add(priorityBox);
//            this.Controls.Add(okButton);
//            this.Controls.Add(cancelButton);
//        }

//        private void ColorButton_Click(object sender, EventArgs e)
//        {
//            using (ColorDialog colorDialog = new ColorDialog())
//            {
//                if (colorDialog.ShowDialog() == DialogResult.OK)
//                {
//                    TabColor = colorDialog.Color;
//                    colorButton.BackColor = TabColor;
//                }
//            }
//        }

//        private void OkButton_Click(object sender, EventArgs e)
//        {
//            TabName = nameBox.Text;
//            TabType = typeBox.SelectedItem?.ToString();
//            Tags = tagsBox.Text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
//            Category = categoryBox.Text;
//            Priority = (int)priorityBox.Value;
//        }
//    }
//}