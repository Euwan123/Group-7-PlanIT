using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PlanIT2.Managers
{
    public class ThemeManager
    {
        public bool IsDarkMode { get; set; }

        public Color BackColor => IsDarkMode ? Color.FromArgb(20, 20, 20) : Color.FromArgb(245, 247, 250);
        public Color ForeColor => IsDarkMode ? Color.White : Color.Black;
        public Color PanelColor => IsDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(230, 230, 255);
        public Color RichTextBoxColor => IsDarkMode ? Color.FromArgb(50, 50, 50) : Color.WhiteSmoke;
        public Color ControlBackColor => IsDarkMode ? Color.FromArgb(60, 60, 60) : SystemColors.Control;

        public ThemeManager(bool darkMode = false)
        {
            IsDarkMode = darkMode;
        }

        public void ApplyTheme(Form form, TabControl tabControl, params Control[] additionalControls)
        {
            form.BackColor = BackColor;

            foreach (Control c in form.Controls)
            {
                ApplyToControl(c);
            }

            if (tabControl != null)
            {
                ApplyToTabControl(tabControl);
            }

            foreach (var ctrl in additionalControls)
            {
                if (ctrl != null)
                    ApplyToControl(ctrl);
            }
        }

        private void ApplyToControl(Control control)
        {
            control.ForeColor = ForeColor;

            if (control is Panel || control is TableLayoutPanel)
            {
                control.BackColor = PanelColor;
            }

            if (control is RichTextBox rtb)
            {
                rtb.BackColor = RichTextBoxColor;
                rtb.ForeColor = ForeColor;
            }

            if (control is TextBox tb)
            {
                tb.BackColor = RichTextBoxColor;
                tb.ForeColor = ForeColor;
            }

            if (control is DataGridView dgv)
            {
                dgv.BackgroundColor = RichTextBoxColor;
                dgv.DefaultCellStyle.BackColor = RichTextBoxColor;
                dgv.DefaultCellStyle.ForeColor = ForeColor;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = RichTextBoxColor;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = ForeColor;
                dgv.EnableHeadersVisualStyles = false;
            }

            if (control is FlowLayoutPanel flp)
            {
                flp.BackColor = RichTextBoxColor;
                foreach (Control checkItem in flp.Controls)
                {
                    checkItem.ForeColor = ForeColor;
                }
            }

            if (control is PictureBox pb)
            {
                pb.BackColor = RichTextBoxColor;
            }

            if (control is ToolStrip ts)
            {
                ts.BackColor = ControlBackColor;
            }

            if (control is ComboBox cb)
            {
                cb.BackColor = ControlBackColor;
                cb.ForeColor = ForeColor;
            }

            if (control is CheckBox chk)
            {
                chk.ForeColor = ForeColor;
            }

            if (control is Label lbl)
            {
                lbl.ForeColor = ForeColor;
            }

            if (control is Button btn)
            {
                if (btn.BackColor == Color.LightGray)
                {
                    btn.BackColor = IsDarkMode ? Color.FromArgb(80, 80, 80) : Color.LightGray;
                }
                btn.ForeColor = btn.BackColor == Color.LightGray || btn.BackColor == Color.FromArgb(80, 80, 80) ?
                    ForeColor : Color.White;
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child);
            }
        }

        private void ApplyToTabControl(TabControl tabControl)
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                tab.BackColor = PanelColor;
                foreach (Control ctrl in tab.Controls)
                {
                    ApplyToControl(ctrl);
                }
            }
            tabControl.Invalidate();
        }

        public Color GetStatusColor(bool isSaved)
        {
            if (isSaved)
                return IsDarkMode ? Color.LightGreen : Color.DarkGreen;
            else
                return IsDarkMode ? Color.LightCoral : Color.Red;
        }

        public Color GetAccentColor()
        {
            return IsDarkMode ? Color.LightBlue : Color.DarkBlue;
        }
    }
}