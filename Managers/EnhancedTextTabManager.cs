using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace PlanIT2.TabTypes
{
    public class EnhancedTextTabManager
    {
        private Action markUnsavedCallback;
        private bool isDarkMode;
        private int undoLevels;

        public EnhancedTextTabManager(Action markUnsaved, bool darkMode, int undoLevels)
        {
            this.markUnsavedCallback = markUnsaved;
            this.isDarkMode = darkMode;
            this.undoLevels = undoLevels;
        }

        public TabPage CreateTextTab(string name, string content = "")
        {
            TabPage tab = new TabPage(name);
            tab.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;

            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = tab.BackColor
            };

            // Top Toolbar for formatting
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 95,
                BackColor = isDarkMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(250, 250, 250),
                Padding = new Padding(10, 5, 10, 5)
            };

            // First Row
            int xPos = 10;
            int yPos = 8;

            Label lblFont = new Label { Text = "Font:", Location = new Point(xPos, yPos + 5), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            toolbarPanel.Controls.Add(lblFont);
            xPos += 45;

            ComboBox fontCombo = new ComboBox
            {
                Location = new Point(xPos, yPos),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            using (InstalledFontCollection fontsCollection = new InstalledFontCollection())
            {
                foreach (FontFamily font in fontsCollection.Families)
                {
                    fontCombo.Items.Add(font.Name);
                }
            }
            fontCombo.SelectedItem = "Segoe UI";
            toolbarPanel.Controls.Add(fontCombo);
            xPos += 190;

            Label lblSize = new Label { Text = "Size:", Location = new Point(xPos, yPos + 5), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            toolbarPanel.Controls.Add(lblSize);
            xPos += 40;

            ComboBox sizeCombo = new ComboBox
            {
                Location = new Point(xPos, yPos),
                Width = 70,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            sizeCombo.Items.AddRange(new object[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "32", "36", "48", "60", "72" });
            sizeCombo.SelectedItem = "11";
            toolbarPanel.Controls.Add(sizeCombo);
            xPos += 80;

            Button boldBtn = CreateToolbarButton("B", xPos, yPos, true);
            SetTooltip(boldBtn, "Bold (Ctrl+B)");
            toolbarPanel.Controls.Add(boldBtn);
            xPos += 35;

            Button italicBtn = CreateToolbarButton("I", xPos, yPos, false);
            italicBtn.Font = new Font(italicBtn.Font, FontStyle.Italic);
            SetTooltip(italicBtn, "Italic (Ctrl+I)");
            toolbarPanel.Controls.Add(italicBtn);
            xPos += 35;

            Button underlineBtn = CreateToolbarButton("U", xPos, yPos, false);
            underlineBtn.Font = new Font(underlineBtn.Font, FontStyle.Underline);
            SetTooltip(underlineBtn, "Underline (Ctrl+U)");
            toolbarPanel.Controls.Add(underlineBtn);
            xPos += 35;

            Button strikeBtn = CreateToolbarButton("S", xPos, yPos, false);
            strikeBtn.Font = new Font(strikeBtn.Font, FontStyle.Strikeout);
            SetTooltip(strikeBtn, "Strikethrough");
            toolbarPanel.Controls.Add(strikeBtn);
            xPos += 45;

            Button colorBtn = new Button
            {
                Text = "A",
                Location = new Point(xPos, yPos),
                Size = new Size(32, 28),
                BackColor = Color.Black,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            colorBtn.FlatAppearance.BorderSize = 1;
            SetTooltip(colorBtn, "Text Color");
            toolbarPanel.Controls.Add(colorBtn);
            xPos += 40;

            Button highlightBtn = new Button
            {
                Text = "■",
                Location = new Point(xPos, yPos),
                Size = new Size(32, 28),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            highlightBtn.FlatAppearance.BorderSize = 1;
            SetTooltip(highlightBtn, "Highlight");
            toolbarPanel.Controls.Add(highlightBtn);

            // Second Row
            xPos = 10;
            yPos = 50;

            Label lblTheme = new Label { Text = "Theme:", Location = new Point(xPos, yPos + 5), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            toolbarPanel.Controls.Add(lblTheme);
            xPos += 55;

            ComboBox themeCombo = new ComboBox
            {
                Location = new Point(xPos, yPos),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            themeCombo.Items.AddRange(new object[] {
                "🌟 Default",
                "🌙 Dark Mode",
                "🌊 Ocean Blue",
                "🌸 Soft Pink",
                "🌿 Forest Green",
                "🔥 Sunset Orange",
                "💜 Royal Purple",
                "☕ Coffee Brown",
                "❄️ Ice Blue"
            });
            themeCombo.SelectedIndex = 0;
            toolbarPanel.Controls.Add(themeCombo);
            xPos += 160;

            Button alignLeftBtn = CreateToolbarButton("≡", xPos, yPos, false);
            SetTooltip(alignLeftBtn, "Align Left");
            toolbarPanel.Controls.Add(alignLeftBtn);
            xPos += 35;

            Button alignCenterBtn = CreateToolbarButton("≣", xPos, yPos, false);
            SetTooltip(alignCenterBtn, "Align Center");
            toolbarPanel.Controls.Add(alignCenterBtn);
            xPos += 35;

            Button alignRightBtn = CreateToolbarButton("≡", xPos, yPos, false);
            SetTooltip(alignRightBtn, "Align Right");
            toolbarPanel.Controls.Add(alignRightBtn);
            xPos += 45;

            Button bulletBtn = CreateToolbarButton("•", xPos, yPos, false);
            SetTooltip(bulletBtn, "Bullet List");
            toolbarPanel.Controls.Add(bulletBtn);
            xPos += 35;

            Button numberedBtn = CreateToolbarButton("1.", xPos, yPos, false);
            SetTooltip(numberedBtn, "Numbered List");
            toolbarPanel.Controls.Add(numberedBtn);
            xPos += 45;

            Button increaseIndentBtn = CreateToolbarButton("→", xPos, yPos, false);
            SetTooltip(increaseIndentBtn, "Increase Indent");
            toolbarPanel.Controls.Add(increaseIndentBtn);
            xPos += 35;

            Button decreaseIndentBtn = CreateToolbarButton("←", xPos, yPos, false);
            SetTooltip(decreaseIndentBtn, "Decrease Indent");
            toolbarPanel.Controls.Add(decreaseIndentBtn);
            xPos += 45;

            Button clearFormatBtn = new Button
            {
                Text = "✖ Clear Format",
                Location = new Point(xPos, yPos),
                Size = new Size(110, 28),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            clearFormatBtn.FlatAppearance.BorderSize = 0;
            SetTooltip(clearFormatBtn, "Clear All Formatting");
            toolbarPanel.Controls.Add(clearFormatBtn);
            xPos += 120;

            // NEW: Find & Replace Button
            Button findReplaceBtn = new Button
            {
                Text = "🔍 Find",
                Location = new Point(xPos, yPos),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            findReplaceBtn.FlatAppearance.BorderSize = 0;
            SetTooltip(findReplaceBtn, "Find & Replace (Ctrl+F)");
            toolbarPanel.Controls.Add(findReplaceBtn);
            xPos += 80;

            // NEW: Word Count Button
            Button wordCountBtn = new Button
            {
                Text = "📊",
                Location = new Point(xPos, yPos),
                Size = new Size(35, 28),
                BackColor = Color.FromArgb(138, 43, 226),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            wordCountBtn.FlatAppearance.BorderSize = 0;
            SetTooltip(wordCountBtn, "Text Statistics");
            toolbarPanel.Controls.Add(wordCountBtn);
            xPos += 45;

            // NEW: Spell Check Button
            Button spellCheckBtn = new Button
            {
                Text = "✓",
                Location = new Point(xPos, yPos),
                Size = new Size(35, 28),
                BackColor = Color.FromArgb(50, 150, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            spellCheckBtn.FlatAppearance.BorderSize = 0;
            SetTooltip(spellCheckBtn, "Spell Check");
            toolbarPanel.Controls.Add(spellCheckBtn);

            // RichTextBox
            RichTextBox rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(15),
                EnableAutoDragDrop = true,
                AcceptsTab = true,
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Load content
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    if (content.StartsWith("{\\rtf"))
                    {
                        rtb.Rtf = content;
                    }
                    else
                    {
                        rtb.Text = content;
                    }
                }
                catch
                {
                    rtb.Text = content;
                }
            }

            // Store current font settings
            Font currentDefaultFont = new Font("Segoe UI", 11);

            // Text changed event
            rtb.TextChanged += (s, e) =>
            {
                markUnsavedCallback?.Invoke();
            };

            // Selection changed - update toolbar
            rtb.SelectionChanged += (s, e) =>
            {
                if (rtb.SelectionFont != null)
                {
                    Font sf = rtb.SelectionFont;

                    if (fontCombo.Items.Contains(sf.FontFamily.Name))
                        fontCombo.SelectedItem = sf.FontFamily.Name;

                    if (sizeCombo.Items.Contains(((int)sf.Size).ToString()))
                        sizeCombo.SelectedItem = ((int)sf.Size).ToString();

                    boldBtn.BackColor = sf.Bold ? Color.FromArgb(220, 70, 0) : Color.FromArgb(100, 100, 100);
                    italicBtn.BackColor = sf.Italic ? Color.FromArgb(220, 70, 0) : Color.FromArgb(100, 100, 100);
                    underlineBtn.BackColor = sf.Underline ? Color.FromArgb(220, 70, 0) : Color.FromArgb(100, 100, 100);
                    strikeBtn.BackColor = sf.Strikeout ? Color.FromArgb(220, 70, 0) : Color.FromArgb(100, 100, 100);
                }
            };

            // Font change
            fontCombo.SelectedIndexChanged += (s, e) =>
            {
                if (fontCombo.SelectedItem == null) return;

                string fontName = fontCombo.SelectedItem.ToString();
                float fontSize = rtb.SelectionFont?.Size ?? 11;
                FontStyle style = rtb.SelectionFont?.Style ?? FontStyle.Regular;

                if (rtb.SelectionLength > 0)
                {
                    rtb.SelectionFont = new Font(fontName, fontSize, style);
                }
                else
                {
                    currentDefaultFont = new Font(fontName, fontSize, style);
                    rtb.Font = currentDefaultFont;
                }
                rtb.Focus();
            };

            // Size change
            sizeCombo.SelectedIndexChanged += (s, e) =>
            {
                if (sizeCombo.SelectedItem == null) return;

                if (float.TryParse(sizeCombo.SelectedItem.ToString(), out float size))
                {
                    string fontName = rtb.SelectionFont?.FontFamily.Name ?? "Segoe UI";
                    FontStyle style = rtb.SelectionFont?.Style ?? FontStyle.Regular;

                    if (rtb.SelectionLength > 0)
                    {
                        rtb.SelectionFont = new Font(fontName, size, style);
                    }
                    else
                    {
                        currentDefaultFont = new Font(fontName, size, style);
                        rtb.Font = currentDefaultFont;
                    }
                    rtb.Focus();
                }
            };

            // Style buttons
            boldBtn.Click += (s, e) => { ToggleFontStyle(rtb, FontStyle.Bold, ref currentDefaultFont); rtb.Focus(); };
            italicBtn.Click += (s, e) => { ToggleFontStyle(rtb, FontStyle.Italic, ref currentDefaultFont); rtb.Focus(); };
            underlineBtn.Click += (s, e) => { ToggleFontStyle(rtb, FontStyle.Underline, ref currentDefaultFont); rtb.Focus(); };
            strikeBtn.Click += (s, e) => { ToggleFontStyle(rtb, FontStyle.Strikeout, ref currentDefaultFont); rtb.Focus(); };

            // Color buttons
            colorBtn.Click += (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.FullOpen = true;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        if (rtb.SelectionLength > 0)
                            rtb.SelectionColor = cd.Color;
                        else
                            rtb.ForeColor = cd.Color;
                        colorBtn.BackColor = cd.Color;
                        rtb.Focus();
                    }
                }
            };

            highlightBtn.Click += (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.FullOpen = true;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        if (rtb.SelectionLength > 0)
                            rtb.SelectionBackColor = cd.Color;
                        highlightBtn.ForeColor = cd.Color;
                        rtb.Focus();
                    }
                }
            };

            // Theme change
            themeCombo.SelectedIndexChanged += (s, e) =>
            {
                ApplyTheme(rtb, containerPanel, toolbarPanel, themeCombo.SelectedIndex);
            };

            // Alignment
            alignLeftBtn.Click += (s, e) => { rtb.SelectionAlignment = HorizontalAlignment.Left; rtb.Focus(); };
            alignCenterBtn.Click += (s, e) => { rtb.SelectionAlignment = HorizontalAlignment.Center; rtb.Focus(); };
            alignRightBtn.Click += (s, e) => { rtb.SelectionAlignment = HorizontalAlignment.Right; rtb.Focus(); };

            // Bullet/Numbered lists
            bulletBtn.Click += (s, e) => { rtb.SelectionBullet = !rtb.SelectionBullet; rtb.Focus(); };
            numberedBtn.Click += (s, e) => { InsertNumberedList(rtb); rtb.Focus(); };

            // Indent
            increaseIndentBtn.Click += (s, e) => { rtb.SelectionIndent += 20; rtb.Focus(); };
            decreaseIndentBtn.Click += (s, e) => { rtb.SelectionIndent = Math.Max(0, rtb.SelectionIndent - 20); rtb.Focus(); };

            // Clear formatting
            clearFormatBtn.Click += (s, e) =>
            {
                if (rtb.SelectionLength > 0)
                {
                    rtb.SelectionFont = new Font("Segoe UI", 11, FontStyle.Regular);
                    rtb.SelectionColor = Color.Black;
                    rtb.SelectionBackColor = Color.White;
                    rtb.SelectionAlignment = HorizontalAlignment.Left;
                    rtb.SelectionBullet = false;
                    rtb.SelectionIndent = 0;
                }
                rtb.Focus();
            };

            // NEW: Find & Replace functionality
            findReplaceBtn.Click += (s, e) =>
            {
                ShowFindReplaceDialog(rtb);
            };

            // NEW: Word Count Statistics
            wordCountBtn.Click += (s, e) =>
            {
                ShowTextStatistics(rtb);
            };

            // NEW: Spell Check (Basic implementation)
            spellCheckBtn.Click += (s, e) =>
            {
                MessageBox.Show("Spell check feature coming soon!\n\nFor now, use the built-in spell checker by right-clicking on text.",
                    "Spell Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // Keyboard shortcuts
            rtb.KeyDown += (s, e) =>
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.B:
                            ToggleFontStyle(rtb, FontStyle.Bold, ref currentDefaultFont);
                            e.Handled = true;
                            break;
                        case Keys.I:
                            ToggleFontStyle(rtb, FontStyle.Italic, ref currentDefaultFont);
                            e.Handled = true;
                            break;
                        case Keys.U:
                            ToggleFontStyle(rtb, FontStyle.Underline, ref currentDefaultFont);
                            e.Handled = true;
                            break;
                        case Keys.F:
                            ShowFindReplaceDialog(rtb);
                            e.Handled = true;
                            break;
                    }
                }
            };

            containerPanel.Controls.Add(rtb);
            containerPanel.Controls.Add(toolbarPanel);
            tab.Controls.Add(containerPanel);

            return tab;
        }

        private void ShowFindReplaceDialog(RichTextBox rtb)
        {
            Form findDialog = new Form
            {
                Text = "Find & Replace",
                Size = new Size(450, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblFind = new Label { Text = "Find:", Location = new Point(20, 20), AutoSize = true };
            TextBox txtFind = new TextBox { Location = new Point(20, 45), Width = 390 };

            Label lblReplace = new Label { Text = "Replace with:", Location = new Point(20, 80), AutoSize = true };
            TextBox txtReplace = new TextBox { Location = new Point(20, 105), Width = 390 };

            CheckBox chkMatchCase = new CheckBox { Text = "Match case", Location = new Point(20, 145), AutoSize = true };

            Button btnFind = new Button
            {
                Text = "Find Next",
                Location = new Point(20, 175),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnReplace = new Button
            {
                Text = "Replace",
                Location = new Point(120, 175),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnReplaceAll = new Button
            {
                Text = "Replace All",
                Location = new Point(220, 175),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(320, 175),
                Size = new Size(90, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            int lastSearchIndex = 0;

            btnFind.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtFind.Text)) return;

                StringComparison comparison = chkMatchCase.Checked ?
                    StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                int index = rtb.Text.IndexOf(txtFind.Text, lastSearchIndex, comparison);

                if (index == -1 && lastSearchIndex > 0)
                {
                    lastSearchIndex = 0;
                    index = rtb.Text.IndexOf(txtFind.Text, 0, comparison);
                }

                if (index != -1)
                {
                    rtb.Select(index, txtFind.Text.Length);
                    rtb.ScrollToCaret();
                    lastSearchIndex = index + 1;
                }
                else
                {
                    MessageBox.Show("No more occurrences found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnReplace.Click += (s, e) =>
            {
                if (rtb.SelectionLength > 0)
                {
                    rtb.SelectedText = txtReplace.Text;
                }
            };

            btnReplaceAll.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtFind.Text)) return;

                StringComparison comparison = chkMatchCase.Checked ?
                    StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                int count = 0;
                int index = 0;
                string text = rtb.Text;

                while ((index = text.IndexOf(txtFind.Text, index, comparison)) != -1)
                {
                    rtb.Select(index, txtFind.Text.Length);
                    rtb.SelectedText = txtReplace.Text;
                    text = rtb.Text;
                    index += txtReplace.Text.Length;
                    count++;
                }

                MessageBox.Show($"Replaced {count} occurrence(s).", "Replace All", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnClose.Click += (s, e) => findDialog.Close();

            findDialog.Controls.AddRange(new Control[] {
                lblFind, txtFind, lblReplace, txtReplace, chkMatchCase,
                btnFind, btnReplace, btnReplaceAll, btnClose
            });

            findDialog.ShowDialog();
        }

        private void ShowTextStatistics(RichTextBox rtb)
        {
            string text = rtb.Text;

            // Calculate statistics
            int charCount = text.Length;
            int charNoSpaces = text.Count(c => !char.IsWhiteSpace(c));
            int wordCount = string.IsNullOrWhiteSpace(text) ? 0 :
                text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int lineCount = rtb.Lines.Length;
            int paragraphCount = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
            int sentenceCount = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;

            string stats = $@"📊 TEXT STATISTICS

Characters (with spaces): {charCount}
Characters (no spaces): {charNoSpaces}
Words: {wordCount}
Lines: {lineCount}
Paragraphs: {paragraphCount}
Sentences: {sentenceCount}

Reading time: ~{Math.Ceiling(wordCount / 200.0)} minutes
(Based on average reading speed of 200 words/minute)";

            MessageBox.Show(stats, "Text Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Button CreateToolbarButton(string text, int x, int y, bool bold)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(32, 28),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, bold ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SetTooltip(Control control, string text)
        {
            ToolTip tip = new ToolTip();
            tip.SetToolTip(control, text);
        }

        private void ToggleFontStyle(RichTextBox rtb, FontStyle style, ref Font defaultFont)
        {
            if (rtb.SelectionLength > 0)
            {
                Font currentFont = rtb.SelectionFont;
                if (currentFont == null) return;

                FontStyle newStyle = currentFont.Style;
                if ((newStyle & style) == style)
                    newStyle &= ~style;
                else
                    newStyle |= style;

                try
                {
                    rtb.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
                }
                catch { }
            }
            else
            {
                FontStyle newStyle = defaultFont.Style;
                if ((newStyle & style) == style)
                    newStyle &= ~style;
                else
                    newStyle |= style;

                defaultFont = new Font(defaultFont.FontFamily, defaultFont.Size, newStyle);
                rtb.Font = defaultFont;
            }
        }

        private void InsertNumberedList(RichTextBox rtb)
        {
            int start = rtb.SelectionStart;
            string[] lines = rtb.Text.Split('\n');
            int currentLine = rtb.GetLineFromCharIndex(start);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]) && !char.IsDigit(lines[i].TrimStart()[0]))
                {
                    lines[i] = $"{i + 1}. {lines[i].TrimStart()}";
                }
            }

            rtb.Text = string.Join("\n", lines);
        }

        private void ApplyTheme(RichTextBox rtb, Panel container, Panel toolbar, int themeIndex)
        {
            switch (themeIndex)
            {
                case 0: // Default
                    rtb.BackColor = Color.White;
                    rtb.ForeColor = Color.Black;
                    container.BackColor = Color.White;
                    toolbar.BackColor = Color.FromArgb(250, 250, 250);
                    break;
                case 1: // Dark Mode
                    rtb.BackColor = Color.FromArgb(30, 30, 30);
                    rtb.ForeColor = Color.White;
                    container.BackColor = Color.FromArgb(30, 30, 30);
                    toolbar.BackColor = Color.FromArgb(40, 40, 40);
                    break;
                case 2: // Ocean Blue
                    rtb.BackColor = Color.FromArgb(230, 240, 255);
                    rtb.ForeColor = Color.FromArgb(0, 40, 80);
                    container.BackColor = Color.FromArgb(230, 240, 255);
                    toolbar.BackColor = Color.FromArgb(200, 220, 255);
                    break;
                case 3: // Soft Pink
                    rtb.BackColor = Color.FromArgb(255, 240, 245);
                    rtb.ForeColor = Color.FromArgb(60, 20, 40);
                    container.BackColor = Color.FromArgb(255, 240, 245);
                    toolbar.BackColor = Color.FromArgb(255, 220, 235);
                    break;
                case 4: // Forest Green
                    rtb.BackColor = Color.FromArgb(240, 255, 240);
                    rtb.ForeColor = Color.FromArgb(20, 60, 20);
                    container.BackColor = Color.FromArgb(240, 255, 240);
                    toolbar.BackColor = Color.FromArgb(220, 255, 220);
                    break;
                case 5: // Sunset Orange
                    rtb.BackColor = Color.FromArgb(255, 245, 230);
                    rtb.ForeColor = Color.FromArgb(80, 40, 0);
                    container.BackColor = Color.FromArgb(255, 245, 230);
                    toolbar.BackColor = Color.FromArgb(255, 230, 200);
                    break;
                case 6: // Royal Purple
                    rtb.BackColor = Color.FromArgb(245, 240, 255);
                    rtb.ForeColor = Color.FromArgb(40, 0, 80);
                    container.BackColor = Color.FromArgb(245, 240, 255);
                    toolbar.BackColor = Color.FromArgb(230, 220, 255);
                    break;
                case 7: // Coffee Brown
                    rtb.BackColor = Color.FromArgb(250, 245, 235);
                    rtb.ForeColor = Color.FromArgb(60, 40, 20);
                    container.BackColor = Color.FromArgb(250, 245, 235);
                    toolbar.BackColor = Color.FromArgb(240, 230, 210);
                    break;
                case 8: // Ice Blue
                    rtb.BackColor = Color.FromArgb(240, 250, 255);
                    rtb.ForeColor = Color.FromArgb(0, 50, 100);
                    container.BackColor = Color.FromArgb(240, 250, 255);
                    toolbar.BackColor = Color.FromArgb(220, 240, 255);
                    break;
            }
        }

        public string GetContent(TabPage tab)
        {
            foreach (Control c in tab.Controls)
            {
                if (c is Panel panel)
                {
                    foreach (Control innerC in panel.Controls)
                    {
                        if (innerC is RichTextBox rtb)
                        {
                            try
                            {
                                // Only return RTF if there's actual content
                                if (string.IsNullOrWhiteSpace(rtb.Text))
                                    return "";
                                return rtb.Rtf;
                            }
                            catch
                            {
                                return rtb.Text;
                            }
                        }
                    }
                }
            }
            return "";
        }
    }
}