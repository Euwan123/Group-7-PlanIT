//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text.RegularExpressions;
//using System.Windows.Forms;
//using MySql.Data.MySqlClient;

//namespace PlanIT2
//{
//    public class SignUpForm : Form
//    {
//        private CustomTextBox txtUsername, txtEmail, txtPassword, txtConfirmPassword;
//        private Button btnSignup;
//        private Label lblBrand, lblWelcome, lblSub, lblStatus;
//        private Panel leftPanel, rightPanel, topBar, bottomBar;
//        private Label lblPasswordToggle, lblConfirmPasswordToggle;
//        private Timer imageSliderTimer;
//        private int currentImageIndex = 0;
//        private List<Image> slideImages = new List<Image>();
//        private PictureBox picSlider;

//        public SignUpForm()
//        {
//            InitializeComponent();
//            LoadSliderImages();
//            StartImageSlider();
//        }

//        // Constructor to accept position from login form
//        public SignUpForm(Point location)
//        {
//            InitializeComponent();
//            LoadSliderImages();
//            StartImageSlider();
//            this.StartPosition = FormStartPosition.Manual;
//            this.Location = location;
//        }

//        private void LoadSliderImages()
//        {
//            string[] possiblePaths = {
//                @"C:\Users\Euwan\source\repos\PlanIT2\Resources",
//                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"),
//                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources")
//            };

//            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

//            foreach (string basePath in possiblePaths)
//            {
//                if (Directory.Exists(basePath))
//                {
//                    foreach (string file in Directory.GetFiles(basePath))
//                    {
//                        string ext = Path.GetExtension(file).ToLower();
//                        string name = Path.GetFileNameWithoutExtension(file).ToLower();
//                        if (validExtensions.Contains(ext) && name.StartsWith("mapua"))
//                        {
//                            try
//                            {
//                                slideImages.Add(Image.FromFile(file));
//                            }
//                            catch { }
//                        }
//                    }
//                }

//                if (slideImages.Count > 0)
//                    break;
//            }

//            if (slideImages.Count > 0 && picSlider != null)
//            {
//                picSlider.Image = slideImages[0];
//                picSlider.SizeMode = PictureBoxSizeMode.Zoom;
//                picSlider.Dock = DockStyle.Fill;
//                picSlider.Refresh();
//            }
//            else
//            {
//                picSlider.BackColor = Color.FromArgb(40, 40, 40);
//            }
//        }

//        private void StartImageSlider()
//        {
//            if (slideImages.Count > 1)
//            {
//                imageSliderTimer = new Timer();
//                imageSliderTimer.Interval = 3000;
//                imageSliderTimer.Tick += ImageSliderTimer_Tick;
//                imageSliderTimer.Start();
//            }
//        }

//        private void ImageSliderTimer_Tick(object sender, EventArgs e)
//        {
//            if (slideImages.Count > 0)
//            {
//                currentImageIndex = (currentImageIndex + 1) % slideImages.Count;
//                if (picSlider != null)
//                {
//                    picSlider.Image = slideImages[currentImageIndex];
//                }
//            }
//        }

//        private void InitializeComponent()
//        {
//            this.Size = new Size(1200, 700);
//            this.StartPosition = FormStartPosition.CenterScreen;
//            this.FormBorderStyle = FormBorderStyle.None;
//            this.BackColor = Color.White;
//            this.DoubleBuffered = true;
//            this.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, this.Width, this.Height, 35, 35));

//            // Top black bar
//            topBar = new Panel
//            {
//                Size = new Size(this.Width, 60),
//                Location = new Point(0, 0),
//                BackColor = Color.Black
//            };
//            this.Controls.Add(topBar);
//            topBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) NativeMethods.DragMove(this.Handle); };

//            var minimizeButton = new Button
//            {
//                Text = "─",
//                Size = new Size(45, 45),
//                Location = new Point(this.Width - 110, 7),
//                FlatStyle = FlatStyle.Flat,
//                BackColor = Color.Black,
//                ForeColor = Color.White,
//                Cursor = Cursors.Hand
//            };
//            minimizeButton.FlatAppearance.BorderSize = 0;
//            minimizeButton.Font = new Font("Segoe UI", 16, FontStyle.Bold);
//            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
//            topBar.Controls.Add(minimizeButton);

//            var closeButton = new Button
//            {
//                Text = "✕",
//                Size = new Size(45, 45),
//                Location = new Point(this.Width - 55, 7),
//                FlatStyle = FlatStyle.Flat,
//                BackColor = Color.Black,
//                ForeColor = Color.White,
//                Cursor = Cursors.Hand
//            };
//            closeButton.FlatAppearance.BorderSize = 0;
//            closeButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
//            closeButton.Click += (s, e) => this.Close();
//            topBar.Controls.Add(closeButton);

//            // Bottom black bar
//            bottomBar = new Panel
//            {
//                Size = new Size(this.Width, 60),
//                Location = new Point(0, this.Height - 60),
//                BackColor = Color.Black
//            };
//            this.Controls.Add(bottomBar);

//            int leftPanelWidth = (int)(this.Width * 0.58);
//            leftPanel = new Panel
//            {
//                Size = new Size(leftPanelWidth, this.Height),
//                Location = new Point(0, 0),
//                BackColor = Color.FromArgb(20, 20, 20)
//            };
//            this.Controls.Add(leftPanel);
//            leftPanel.SendToBack();

//            picSlider = new PictureBox
//            {
//                Size = new Size(leftPanel.Width, leftPanel.Height),
//                Location = new Point(0, 0),
//                SizeMode = PictureBoxSizeMode.Zoom,
//                BackColor = Color.Black
//            };
//            leftPanel.Controls.Add(picSlider);

//            var overlay = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BackColor = Color.FromArgb(110, 0, 0, 0)
//            };
//            leftPanel.Controls.Add(overlay);
//            overlay.BringToFront();

//            lblBrand = new Label
//            {
//                Text = "PlanIT",
//                Font = new Font("Segoe UI", 56, FontStyle.Bold),
//                ForeColor = Color.White,
//                AutoSize = true,
//                BackColor = Color.Transparent
//            };
//            lblWelcome = new Label
//            {
//                Text = "Join Us Today",
//                Font = new Font("Segoe UI", 34, FontStyle.Bold),
//                ForeColor = Color.White,
//                AutoSize = true,
//                BackColor = Color.Transparent
//            };
//            lblSub = new Label
//            {
//                Text = "Create your account and start planning.",
//                Font = new Font("Segoe UI", 16, FontStyle.Italic),
//                ForeColor = Color.White,
//                AutoSize = true,
//                BackColor = Color.Transparent
//            };

//            overlay.Controls.Add(lblBrand);
//            overlay.Controls.Add(lblWelcome);
//            overlay.Controls.Add(lblSub);

//            leftPanel.Resize += (s, e) => PositionLeftText();
//            PositionLeftText();

//            int rightPanelWidth = this.Width - leftPanelWidth;
//            rightPanel = new Panel
//            {
//                Size = new Size(rightPanelWidth, this.Height),
//                Location = new Point(leftPanelWidth, 0),
//                BackColor = Color.White
//            };
//            this.Controls.Add(rightPanel);

//            int contentX = 70;
//            int contentWidth = rightPanelWidth - 2 * contentX;
//            int currentY = 80;

//            var header = new Label
//            {
//                Text = "Sign Up",
//                Font = new Font("Segoe UI", 32, FontStyle.Bold),
//                ForeColor = Color.Black,
//                Location = new Point(contentX, currentY),
//                AutoSize = true
//            };
//            rightPanel.Controls.Add(header);
//            currentY += 75;

//            txtUsername = new CustomTextBox
//            {
//                PlaceholderText = "Username",
//                ForeColor = Color.Gray,
//                Font = new Font("Segoe UI", 14),
//                Size = new Size(contentWidth, 60),
//                Location = new Point(contentX, currentY),
//                Radius = 12,
//                BackColor = Color.White
//            };
//            rightPanel.Controls.Add(txtUsername);
//            currentY += 70;

//            txtEmail = new CustomTextBox
//            {
//                PlaceholderText = "Email Address",
//                ForeColor = Color.Gray,
//                Font = new Font("Segoe UI", 14),
//                Size = new Size(contentWidth, 60),
//                Location = new Point(contentX, currentY),
//                Radius = 12,
//                BackColor = Color.White
//            };
//            rightPanel.Controls.Add(txtEmail);
//            currentY += 70;

//            txtPassword = new CustomTextBox
//            {
//                PlaceholderText = "Password",
//                IsPassword = true,
//                ForeColor = Color.Gray,
//                Font = new Font("Segoe UI", 14),
//                Size = new Size(contentWidth, 60),
//                Location = new Point(contentX, currentY),
//                Radius = 12,
//                BackColor = Color.White
//            };
//            rightPanel.Controls.Add(txtPassword);

//            lblPasswordToggle = new Label
//            {
//                Size = new Size(45, 45),
//                Location = new Point(txtPassword.Right - 55, txtPassword.Top + 8),
//                Cursor = Cursors.Hand,
//                Text = "👁️",
//                Font = new Font("Segoe UI Emoji", 20, FontStyle.Regular),
//                TextAlign = ContentAlignment.MiddleCenter,
//                BackColor = Color.Transparent
//            };
//            lblPasswordToggle.Click += (s, e) => TogglePasswordVisibility(txtPassword, lblPasswordToggle);
//            rightPanel.Controls.Add(lblPasswordToggle);
//            lblPasswordToggle.BringToFront();
//            currentY += 70;

//            txtConfirmPassword = new CustomTextBox
//            {
//                PlaceholderText = "Confirm Password",
//                IsPassword = true,
//                ForeColor = Color.Gray,
//                Font = new Font("Segoe UI", 14),
//                Size = new Size(contentWidth, 60),
//                Location = new Point(contentX, currentY),
//                Radius = 12,
//                BackColor = Color.White
//            };
//            rightPanel.Controls.Add(txtConfirmPassword);

//            lblConfirmPasswordToggle = new Label
//            {
//                Size = new Size(45, 45),
//                Location = new Point(txtConfirmPassword.Right - 55, txtConfirmPassword.Top + 8),
//                Cursor = Cursors.Hand,
//                Text = "👁️",
//                Font = new Font("Segoe UI Emoji", 20, FontStyle.Regular),
//                TextAlign = ContentAlignment.MiddleCenter,
//                BackColor = Color.Transparent
//            };
//            lblConfirmPasswordToggle.Click += (s, e) => TogglePasswordVisibility(txtConfirmPassword, lblConfirmPasswordToggle);
//            rightPanel.Controls.Add(lblConfirmPasswordToggle);
//            lblConfirmPasswordToggle.BringToFront();
//            currentY += 70;

//            btnSignup = new Button
//            {
//                Text = "Create Account",
//                Font = new Font("Segoe UI", 14, FontStyle.Bold),
//                Size = new Size(contentWidth, 58),
//                Location = new Point(contentX, currentY),
//                BackColor = Color.FromArgb(220, 70, 0),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat,
//                Cursor = Cursors.Hand
//            };
//            SetRoundedCorners(btnSignup, 12);
//            btnSignup.FlatAppearance.BorderSize = 0;
//            btnSignup.MouseEnter += (s, e) => btnSignup.BackColor = Color.FromArgb(255, 80, 10);
//            btnSignup.MouseLeave += (s, e) => btnSignup.BackColor = Color.FromArgb(220, 70, 0);
//            btnSignup.MouseDown += (s, e) => btnSignup.BackColor = Color.FromArgb(190, 60, 0);
//            btnSignup.MouseUp += (s, e) => btnSignup.BackColor = btnSignup.ClientRectangle.Contains(e.Location) ? Color.FromArgb(255, 80, 10) : Color.FromArgb(220, 70, 0);
//            btnSignup.Click += BtnSignup_Click;
//            rightPanel.Controls.Add(btnSignup);
//            currentY += 70;

//            lblStatus = new Label
//            {
//                Text = "",
//                Font = new Font("Segoe UI", 10, FontStyle.Bold),
//                ForeColor = Color.Green,
//                Location = new Point(contentX, currentY),
//                Size = new Size(contentWidth, 50),
//                TextAlign = ContentAlignment.TopCenter,
//                Visible = false
//            };
//            rightPanel.Controls.Add(lblStatus);
//            currentY += 55;

//            var hasAccount = new Label
//            {
//                Text = "Already have an account?",
//                Location = new Point(contentX, currentY),
//                Font = new Font("Segoe UI", 11),
//                ForeColor = Color.FromArgb(60, 60, 60),
//                AutoSize = true
//            };
//            rightPanel.Controls.Add(hasAccount);

//            var btnLogin = new LinkLabel
//            {
//                Text = "Sign In",
//                Font = new Font("Segoe UI", 11, FontStyle.Bold),
//                Size = new Size(70, 25),
//                Location = new Point(hasAccount.Right + 8, currentY),
//                LinkColor = Color.FromArgb(220, 70, 0),
//                ActiveLinkColor = Color.FromArgb(190, 60, 0),
//                Cursor = Cursors.Hand,
//                AutoSize = true
//            };
//            btnLogin.LinkClicked += (s, e) => this.Close();
//            rightPanel.Controls.Add(btnLogin);

//            txtUsername.Focus();
//        }

//        private void PositionLeftText()
//        {
//            int totalHeight = lblBrand.Height + 15 + lblWelcome.Height + 20 + lblSub.Height;
//            int startY = (leftPanel.Height / 2) - (totalHeight / 2);
//            int pad = 70;

//            lblBrand.Location = new Point(pad, startY);
//            lblWelcome.Location = new Point(pad, lblBrand.Bottom + 15);
//            lblSub.Location = new Point(pad, lblWelcome.Bottom + 20);
//        }

//        private void TogglePasswordVisibility(CustomTextBox textBox, Label toggleLabel)
//        {
//            bool isPasswordVisible = textBox.PasswordChar == '\0';

//            if (!textBox.IsPlaceholder)
//            {
//                textBox.PasswordChar = isPasswordVisible ? '●' : '\0';
//                toggleLabel.Text = isPasswordVisible ? "👁️" : "🙈";
//            }

//            textBox.SelectionStart = textBox.Text.Length;
//            textBox.Focus();
//        }

//        private void BtnSignup_Click(object sender, EventArgs e)
//        {
//            string username = txtUsername.GetRawText().Trim();
//            string email = txtEmail.GetRawText().Trim();
//            string password = txtPassword.GetRawText();
//            string confirmPassword = txtConfirmPassword.GetRawText();

//            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
//                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
//            {
//                ShowError("Please fill out all fields.");
//                return;
//            }

//            if (!IsValidEmail(email))
//            {
//                ShowError("Please enter a valid email address.");
//                return;
//            }

//            if (!IsPasswordValid(password))
//            {
//                ShowError("Password must be at least 6 characters with letters and numbers.");
//                return;
//            }

//            if (password != confirmPassword)
//            {
//                ShowError("Passwords do not match.");
//                return;
//            }

//            btnSignup.Enabled = false;
//            btnSignup.Text = "Creating Account...";
//            Application.DoEvents();

//            try
//            {
//                using (var conn = Database.GetConnection())
//                {
//                    conn.Open();
//                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @Email";
//                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
//                    {
//                        checkCmd.Parameters.AddWithValue("@username", username);
//                        checkCmd.Parameters.AddWithValue("@Email", email);
//                        long count = (long)checkCmd.ExecuteScalar();
//                        if (count > 0)
//                        {
//                            ShowError("Username or email already exists!");
//                            btnSignup.Enabled = true;
//                            btnSignup.Text = "Create Account";
//                            return;
//                        }
//                    }

//                    string insertQuery = "INSERT INTO users (username, email, password) VALUES (@username, @Email, @Password)";
//                    using (var cmd = new MySqlCommand(insertQuery, conn))
//                    {
//                        cmd.Parameters.AddWithValue("@username", username);
//                        cmd.Parameters.AddWithValue("@Email", email);
//                        cmd.Parameters.AddWithValue("@Password", password);
//                        cmd.ExecuteNonQuery();
//                    }

//                    string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PlanIT", username);
//                    Directory.CreateDirectory(userFolder);

//                    lblStatus.Text = "✓ Account Created! Redirecting to login...";
//                    lblStatus.ForeColor = Color.FromArgb(34, 139, 34);
//                    lblStatus.Visible = true;
//                    Application.DoEvents();

//                    System.Threading.Thread.Sleep(1500);
//                    this.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                ShowError("Database error: " + ex.Message);
//                btnSignup.Enabled = true;
//                btnSignup.Text = "Create Account";
//            }
//        }

//        private void ShowError(string message)
//        {
//            lblStatus.Text = "✗ " + message;
//            lblStatus.ForeColor = Color.FromArgb(220, 20, 20);
//            lblStatus.Visible = true;

//            Timer hideTimer = new Timer();
//            hideTimer.Interval = 4000;
//            hideTimer.Tick += (s, args) =>
//            {
//                lblStatus.Visible = false;
//                hideTimer.Stop();
//                hideTimer.Dispose();
//            };
//            hideTimer.Start();
//        }

//        private bool IsValidEmail(string email)
//        {
//            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
//        }

//        private bool IsPasswordValid(string password)
//        {
//            if (password.Length < 6) return false;
//            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
//            bool hasNumber = Regex.IsMatch(password, @"\d");
//            return hasLetter && hasNumber;
//        }

//        private void SetRoundedCorners(Control c, int radius)
//        {
//            var path = new GraphicsPath();
//            path.StartFigure();
//            path.AddArc(new Rectangle(0, 0, radius * 2, radius * 2), 180, 90);
//            path.AddLine(radius, 0, c.Width - radius, 0);
//            path.AddArc(new Rectangle(c.Width - radius * 2, 0, radius * 2, radius * 2), 270, 90);
//            path.AddLine(c.Width, radius, c.Width, c.Height - radius);
//            path.AddArc(new Rectangle(c.Width - radius * 2, c.Height - radius * 2, radius * 2, radius * 2), 0, 90);
//            path.AddLine(c.Width - radius, c.Height, radius, c.Height);
//            path.AddArc(new Rectangle(0, c.Height - radius * 2, radius * 2, radius * 2), 90, 90);
//            path.CloseFigure();
//            c.Region = new Region(path);
//        }

//        protected override void OnFormClosing(FormClosingEventArgs e)
//        {
//            if (imageSliderTimer != null)
//            {
//                imageSliderTimer.Stop();
//                imageSliderTimer.Dispose();
//            }
//            foreach (var img in slideImages)
//            {
//                img?.Dispose();
//            }
//            base.OnFormClosing(e);
//        }

//        private static class NativeMethods
//        {
//            public const int WM_NCLBUTTONDOWN = 0xA1;
//            public const int HTCAPTION = 0x2;

//            [DllImport("user32.dll")]
//            public static extern bool ReleaseCapture();

//            [DllImport("user32.dll")]
//            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

//            [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
//            public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
//                int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

//            public static void DragMove(IntPtr handle)
//            {
//                ReleaseCapture();
//                SendMessage(handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
//            }
//        }
//    }
//}
