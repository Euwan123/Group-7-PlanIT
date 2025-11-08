using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class SearchResult
    {
        public TabPage Tab { get; set; }
        public string TabName { get; set; }
        public string MatchedText { get; set; }
        public int Position { get; set; }
        public int LineNumber { get; set; }
        public string Context { get; set; }
    }

    public class AdvancedSearchManager
    {
        private TabControl tabControl;
        private List<SearchResult> currentResults = new List<SearchResult>();
        private int currentResultIndex = -1;

        public AdvancedSearchManager(TabControl mainTabControl)
        {
            tabControl = mainTabControl;
        }

        public void ShowSearchDialog(Form owner)
        {
            using (AdvancedSearchDialog dialog = new AdvancedSearchDialog(this, tabControl))
            {
                dialog.ShowDialog(owner);
            }
        }

        public List<SearchResult> Search(string searchText, bool caseSensitive, bool wholeWord, bool useRegex, bool searchAllTabs)
        {
            currentResults.Clear();
            currentResultIndex = -1;

            if (string.IsNullOrWhiteSpace(searchText))
                return currentResults;

            var tabsToSearch = searchAllTabs
                ? tabControl.TabPages.Cast<TabPage>().ToList()
                : new List<TabPage> { tabControl.SelectedTab };

            foreach (var tab in tabsToSearch)
            {
                if (tab == null) continue;
                SearchInTab(tab, searchText, caseSensitive, wholeWord, useRegex);
            }

            return currentResults;
        }

        private void SearchInTab(TabPage tab, string searchText, bool caseSensitive, bool wholeWord, bool useRegex)
        {
            string content = GetTabContent(tab);
            if (string.IsNullOrEmpty(content))
                return;

            string[] lines = content.Split('\n');
            int currentPos = 0;

            for (int lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                string line = lines[lineNum];
                List<int> matches = FindMatches(line, searchText, caseSensitive, wholeWord, useRegex);

                foreach (int matchPos in matches)
                {
                    int absolutePos = currentPos + matchPos;
                    string context = GetContext(lines, lineNum, matchPos, searchText.Length);

                    currentResults.Add(new SearchResult
                    {
                        Tab = tab,
                        TabName = tab.Text,
                        MatchedText = line.Substring(matchPos, Math.Min(searchText.Length, line.Length - matchPos)),
                        Position = absolutePos,
                        LineNumber = lineNum + 1,
                        Context = context
                    });
                }

                currentPos += line.Length + 1; // +1 for newline
            }
        }

        private List<int> FindMatches(string line, string searchText, bool caseSensitive, bool wholeWord, bool useRegex)
        {
            List<int> positions = new List<int>();

            if (useRegex)
            {
                try
                {
                    RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    MatchCollection matches = Regex.Matches(line, searchText, options);
                    foreach (Match match in matches)
                    {
                        positions.Add(match.Index);
                    }
                }
                catch
                {
                    // Invalid regex, fall back to normal search
                    return FindMatches(line, searchText, caseSensitive, wholeWord, false);
                }
            }
            else
            {
                StringComparison comparison = caseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                int index = 0;
                while ((index = line.IndexOf(searchText, index, comparison)) != -1)
                {
                    if (wholeWord)
                    {
                        bool isWholeWord = true;
                        if (index > 0 && char.IsLetterOrDigit(line[index - 1]))
                            isWholeWord = false;
                        if (index + searchText.Length < line.Length &&
                            char.IsLetterOrDigit(line[index + searchText.Length]))
                            isWholeWord = false;

                        if (isWholeWord)
                            positions.Add(index);
                    }
                    else
                    {
                        positions.Add(index);
                    }

                    index += searchText.Length;
                }
            }

            return positions;
        }

        private string GetContext(string[] lines, int lineNum, int position, int matchLength)
        {
            string line = lines[lineNum];
            int contextStart = Math.Max(0, position - 30);
            int contextEnd = Math.Min(line.Length, position + matchLength + 30);

            string prefix = contextStart > 0 ? "..." : "";
            string suffix = contextEnd < line.Length ? "..." : "";

            return prefix + line.Substring(contextStart, contextEnd - contextStart) + suffix;
        }

        private string GetTabContent(TabPage tab)
        {
            // Find RichTextBox in tab
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

                        if (pc is TableLayoutPanel tlp)
                        {
                            foreach (Control tc in tlp.Controls)
                            {
                                if (tc is RichTextBox trtb)
                                    return trtb.Text;
                            }
                        }
                    }
                }
            }
            return "";
        }

        public void HighlightResult(SearchResult result, Color highlightColor)
        {
            if (result.Tab != tabControl.SelectedTab)
            {
                tabControl.SelectedTab = result.Tab;
            }

            RichTextBox rtb = FindRichTextBox(result.Tab);
            if (rtb != null)
            {
                rtb.Select(result.Position, result.MatchedText.Length);
                rtb.SelectionBackColor = highlightColor;
                rtb.ScrollToCaret();
            }
        }

        public void ClearHighlights()
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                RichTextBox rtb = FindRichTextBox(tab);
                if (rtb != null)
                {
                    rtb.SelectAll();
                    rtb.SelectionBackColor = rtb.BackColor;
                    rtb.Select(0, 0);
                }
            }
        }

        public void ReplaceAll(string searchText, string replaceText, bool caseSensitive, bool wholeWord, bool useRegex)
        {
            var results = Search(searchText, caseSensitive, wholeWord, useRegex, true);
            int replacedCount = 0;

            // Group results by tab
            var resultsByTab = results.GroupBy(r => r.Tab);

            foreach (var tabGroup in resultsByTab)
            {
                RichTextBox rtb = FindRichTextBox(tabGroup.Key);
                if (rtb == null) continue;

                string content = rtb.Text;
                string newContent = content;

                if (useRegex)
                {
                    try
                    {
                        RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        newContent = Regex.Replace(content, searchText, replaceText, options);
                        replacedCount += Regex.Matches(content, searchText, options).Count;
                    }
                    catch
                    {
                        MessageBox.Show("Invalid regular expression.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    StringComparison comparison = caseSensitive
                        ? StringComparison.Ordinal
                        : StringComparison.OrdinalIgnoreCase;

                    int index = 0;
                    while ((index = newContent.IndexOf(searchText, index, comparison)) != -1)
                    {
                        if (wholeWord)
                        {
                            bool isWholeWord = true;
                            if (index > 0 && char.IsLetterOrDigit(newContent[index - 1]))
                                isWholeWord = false;
                            if (index + searchText.Length < newContent.Length &&
                                char.IsLetterOrDigit(newContent[index + searchText.Length]))
                                isWholeWord = false;

                            if (!isWholeWord)
                            {
                                index += searchText.Length;
                                continue;
                            }
                        }

                        newContent = newContent.Substring(0, index) + replaceText +
                                   newContent.Substring(index + searchText.Length);
                        replacedCount++;
                        index += replaceText.Length;
                    }
                }

                rtb.Text = newContent;
            }

            MessageBox.Show($"Replaced {replacedCount} occurrence(s).", "Replace All",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private RichTextBox FindRichTextBox(TabPage tab)
        {
            foreach (Control c in tab.Controls)
            {
                if (c is RichTextBox rtb)
                    return rtb;

                if (c is Panel panel)
                {
                    foreach (Control pc in panel.Controls)
                    {
                        if (pc is RichTextBox prtb)
                            return prtb;

                        if (pc is TableLayoutPanel tlp)
                        {
                            foreach (Control tc in tlp.Controls)
                            {
                                if (tc is RichTextBox trtb)
                                    return trtb;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void GoToNextResult()
        {
            if (currentResults.Count == 0) return;

            currentResultIndex = (currentResultIndex + 1) % currentResults.Count;
            HighlightResult(currentResults[currentResultIndex], Color.Yellow);
        }

        public void GoToPreviousResult()
        {
            if (currentResults.Count == 0) return;

            currentResultIndex--;
            if (currentResultIndex < 0)
                currentResultIndex = currentResults.Count - 1;

            HighlightResult(currentResults[currentResultIndex], Color.Yellow);
        }
    }

    public class AdvancedSearchDialog : Form
    {
        private AdvancedSearchManager manager;
        private TabControl tabControl;

        private TextBox txtSearch;
        private TextBox txtReplace;
        private CheckBox chkCaseSensitive;
        private CheckBox chkWholeWord;
        private CheckBox chkRegex;
        private CheckBox chkAllTabs;
        private ListView lvResults;
        private Label lblResultCount;

        private List<SearchResult> currentResults = new List<SearchResult>();

        public AdvancedSearchDialog(AdvancedSearchManager searchManager, TabControl mainTabControl)
        {
            manager = searchManager;
            tabControl = mainTabControl;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Find and Replace";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.White;
            this.KeyPreview = true;

            Label titleLabel = new Label
            {
                Text = "🔍 Advanced Search",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 70, 0),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            // Search input
            Label lblSearch = new Label
            {
                Text = "Find:",
                Location = new Point(20, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(100, 67),
                Width = 450,
                Font = new Font("Segoe UI", 11)
            };
            txtSearch.TextChanged += (s, e) => PerformSearch();
            this.Controls.Add(txtSearch);

            // Replace input
            Label lblReplace = new Label
            {
                Text = "Replace:",
                Location = new Point(20, 110),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(lblReplace);

            txtReplace = new TextBox
            {
                Location = new Point(100, 107),
                Width = 450,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(txtReplace);

            // Options
            Panel optionsPanel = new Panel
            {
                Location = new Point(20, 150),
                Size = new Size(640, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            chkCaseSensitive = new CheckBox
            {
                Text = "Match case",
                Location = new Point(10, 10),
                AutoSize = true
            };
            chkCaseSensitive.CheckedChanged += (s, e) => PerformSearch();

            chkWholeWord = new CheckBox
            {
                Text = "Whole word",
                Location = new Point(150, 10),
                AutoSize = true
            };
            chkWholeWord.CheckedChanged += (s, e) => PerformSearch();

            chkRegex = new CheckBox
            {
                Text = "Regex",
                Location = new Point(290, 10),
                AutoSize = true
            };
            chkRegex.CheckedChanged += (s, e) => PerformSearch();

            chkAllTabs = new CheckBox
            {
                Text = "Search all tabs",
                Location = new Point(390, 10),
                AutoSize = true,
                Checked = true
            };
            chkAllTabs.CheckedChanged += (s, e) => PerformSearch();

            optionsPanel.Controls.AddRange(new Control[] { chkCaseSensitive, chkWholeWord, chkRegex, chkAllTabs });
            this.Controls.Add(optionsPanel);

            // Results label
            lblResultCount = new Label
            {
                Text = "0 results",
                Location = new Point(20, 210),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblResultCount);

            // Results list
            lvResults = new ListView
            {
                Location = new Point(20, 240),
                Size = new Size(640, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9)
            };

            lvResults.Columns.Add("Tab", 120);
            lvResults.Columns.Add("Line", 60);
            lvResults.Columns.Add("Match", 100);
            lvResults.Columns.Add("Context", 340);

            lvResults.DoubleClick += LvResults_DoubleClick;
            this.Controls.Add(lvResults);

            // Action buttons
            Button btnFindNext = new Button
            {
                Text = "▼ Find Next",
                Location = new Point(20, 510),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnFindNext.Click += (s, e) => manager.GoToNextResult();
            this.Controls.Add(btnFindNext);

            Button btnFindPrev = new Button
            {
                Text = "▲ Find Previous",
                Location = new Point(150, 510),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnFindPrev.Click += (s, e) => manager.GoToPreviousResult();
            this.Controls.Add(btnFindPrev);

            Button btnReplace = new Button
            {
                Text = "Replace",
                Location = new Point(290, 510),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnReplace.Click += BtnReplace_Click;
            this.Controls.Add(btnReplace);

            Button btnReplaceAll = new Button
            {
                Text = "Replace All",
                Location = new Point(400, 510),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnReplaceAll.Click += BtnReplaceAll_Click;
            this.Controls.Add(btnReplaceAll);

            Button btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(520, 510),
                Size = new Size(70, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            btnClear.Click += (s, e) => { manager.ClearHighlights(); txtSearch.Clear(); txtReplace.Clear(); };
            this.Controls.Add(btnClear);

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(600, 510),
                Size = new Size(60, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnClose);

            // Keyboard shortcuts
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F3)
                {
                    manager.GoToNextResult();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F3 && e.Shift)
                {
                    manager.GoToPreviousResult();
                    e.Handled = true;
                }
            };

            txtSearch.Focus();
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                lvResults.Items.Clear();
                lblResultCount.Text = "0 results";
                lblResultCount.ForeColor = Color.Gray;
                currentResults.Clear();
                return;
            }

            currentResults = manager.Search(
                txtSearch.Text,
                chkCaseSensitive.Checked,
                chkWholeWord.Checked,
                chkRegex.Checked,
                chkAllTabs.Checked
            );

            DisplayResults();
        }

        private void DisplayResults()
        {
            lvResults.Items.Clear();

            foreach (var result in currentResults)
            {
                var item = new ListViewItem(result.TabName);
                item.SubItems.Add(result.LineNumber.ToString());
                item.SubItems.Add(result.MatchedText);
                item.SubItems.Add(result.Context);
                item.Tag = result;
                lvResults.Items.Add(item);
            }

            lblResultCount.Text = $"{currentResults.Count} result{(currentResults.Count != 1 ? "s" : "")}";
            lblResultCount.ForeColor = currentResults.Count > 0 ? Color.Green : Color.Red;
        }

        private void LvResults_DoubleClick(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count > 0)
            {
                var result = (SearchResult)lvResults.SelectedItems[0].Tag;
                manager.HighlightResult(result, Color.Yellow);
            }
        }

        private void BtnReplace_Click(object sender, EventArgs e)
        {
            // Replace current selection
            if (tabControl.SelectedTab != null)
            {
                RichTextBox rtb = FindRichTextBox(tabControl.SelectedTab);
                if (rtb != null && rtb.SelectionLength > 0)
                {
                    rtb.SelectedText = txtReplace.Text;
                    manager.GoToNextResult();
                }
            }
        }

        private void BtnReplaceAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $"Replace all {currentResults.Count} occurrence(s)?",
                "Confirm Replace All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                manager.ReplaceAll(
                    txtSearch.Text,
                    txtReplace.Text,
                    chkCaseSensitive.Checked,
                    chkWholeWord.Checked,
                    chkRegex.Checked
                );
                PerformSearch(); // Refresh results
            }
        }

        private RichTextBox FindRichTextBox(TabPage tab)
        {
            foreach (Control c in tab.Controls)
            {
                if (c is RichTextBox rtb)
                    return rtb;

                if (c is Panel panel)
                {
                    foreach (Control pc in panel.Controls)
                    {
                        if (pc is RichTextBox prtb)
                            return prtb;

                        if (pc is TableLayoutPanel tlp)
                        {
                            foreach (Control tc in tlp.Controls)
                            {
                                if (tc is RichTextBox trtb)
                                    return trtb;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}