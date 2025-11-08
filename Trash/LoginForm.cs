//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;

//namespace PlanIT2
//{
//    public class LoginForm : Form
//    {
//        private CustomTextBox txtUsername, txtPassword;
//        private CheckBox chkRememberMe;
//        private Button btnLogin;
//        private Label lblBrand, lblWelcome, lblSub, lblStatus;
//        private Panel leftPanel, rightPanel, topBar, bottomBar;
//        private string rememberedFilePath;
//        private Label lblPasswordToggle;
//        private Timer imageSliderTimer;
//        private int currentImageIndex = 0;
//        private List<Image> slideImages = new List<Image>();
//        private PictureBox picSlider;

//        public LoginForm()
//        {
//            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlanIT");
//            Directory.CreateDirectory(appDataFolder);
//            rememberedFilePath = Path.Combine(appDataFolder, "rememberedUser.txt");

//            InitializeComponent();
//            LoadRememberedUser();
//            LoadSliderImages();
//            StartImageSlider(); 

//        }


//        private void LoadSliderImages()
//        {
//            string[] possiblePaths = {
//                @"C:\Users\Euwan\source\repos\PlanIT2\Resources",
//                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"),
//                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources")
//            };

//            string[] imageFiles = { "Mapua 1.jpeg", "Mapua 2.jpg", "Mapua 3.jpg" };

//            foreach (string basePath in possiblePaths)
//            {
//                if (Directory.Exists(basePath))
//                {
//                    foreach (string imageFile in imageFiles)
//                    {
//                        string imagePath = Path.Combine(basePath, imageFile);
//                        if (File.Exists(imagePath))
//                        {
//                            try
//                            {
//                                slideImages.Add(Image.FromFile(imagePath));
//                            }
//                            catch (Exception) { }
//                        }
//                    }

//                    if (slideImages.Count > 0)
//                        break;
//                }
//            }

//            if (slideImages.Count > 0 && picSlider != null)
//            {
//                picSlider.Image = slideImages[0];
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
//            closeButton.Click += (s, e) => Application.Exit();
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
//                Text = "Welcome Back",
//                Font = new Font("Segoe UI", 34, FontStyle.Bold),
//                ForeColor = Color.White,
//                AutoSize = true,
//                BackColor = Color.Transparent
//            };
//            lblSub = new Label
//            {
//                Text = "Let's plan smarter together.",
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
//            int currentY = 120;

//            var header = new Label
//            {
//                Text = "Sign In",
//                Font = new Font("Segoe UI", 32, FontStyle.Bold),
//                ForeColor = Color.Black,
//                Location = new Point(contentX, currentY),
//                AutoSize = true
//            };
//            rightPanel.Controls.Add(header);
//            currentY += 90;

//            txtUsername = new CustomTextBox
//            {
//                PlaceholderText = "Email or Username",
//                ForeColor = Color.Gray,
//                Font = new Font("Segoe UI", 14),
//                Size = new Size(contentWidth, 60),
//                Location = new Point(contentX, currentY),
//                Radius = 12,
//                BackColor = Color.White
//            };
//            rightPanel.Controls.Add(txtUsername);
//            currentY += 80;

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
//                Cursor = Cursors.Hand,
//                Text = "👁️",
//                Font = new Font("Segoe UI Emoji", 20, FontStyle.Regular),
//                TextAlign = ContentAlignment.MiddleCenter, 
//                BackColor = Color.Transparent
//            };
//            lblPasswordToggle.Click += LblPasswordToggle_Click;
//            rightPanel.Controls.Add(lblPasswordToggle);
//            lblPasswordToggle.BringToFront();

//            currentY += 70;

//            chkRememberMe = new CheckBox
//            {
//                Text = "Remember Me",
//                Font = new Font("Segoe UI", 11),
//                ForeColor = Color.FromArgb(60, 60, 60),
//                Location = new Point(contentX, currentY),
//                AutoSize = true
//            };
//            rightPanel.Controls.Add(chkRememberMe);
//            currentY += 50;

//            btnLogin = new Button
//            {
//                Text = "Sign in now",
//                Font = new Font("Segoe UI", 14, FontStyle.Bold),
//                Size = new Size(contentWidth, 58),
//                Location = new Point(contentX, currentY),
//                BackColor = Color.FromArgb(220, 70, 0),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat,
//                Cursor = Cursors.Hand
//            };
//            SetRoundedCorners(btnLogin, 12);
//            btnLogin.FlatAppearance.BorderSize = 0;
//            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(255, 80, 10);
//            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(220, 70, 0);
//            btnLogin.MouseDown += (s, e) => btnLogin.BackColor = Color.FromArgb(190, 60, 0);
//            btnLogin.MouseUp += (s, e) => btnLogin.BackColor = btnLogin.ClientRectangle.Contains(e.Location) ? Color.FromArgb(255, 80, 10) : Color.FromArgb(220, 70, 0);
//            btnLogin.Click += BtnLogin_Click;
//            rightPanel.Controls.Add(btnLogin);
//            currentY += 70;

//            lblStatus = new Label
//            {
//                Text = "",
//                Font = new Font("Segoe UI", 11, FontStyle.Bold),
//                ForeColor = Color.Green,
//                Location = new Point(contentX, currentY),
//                Size = new Size(contentWidth, 30),
//                TextAlign = ContentAlignment.MiddleCenter,
//                Visible = false
//            };
//            rightPanel.Controls.Add(lblStatus);
//            currentY += 40;

//            var forgot = new LinkLabel
//            {
//                Text = "Lost your password?",
//                Location = new Point(contentX, currentY),
//                Font = new Font("Segoe UI", 11),
//                AutoSize = true,
//                LinkColor = Color.FromArgb(30, 30, 90),
//                Cursor = Cursors.Hand
//            };
//            forgot.LinkClicked += (s, e) => MessageBox.Show("Please contact your administrator to reset your password.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            rightPanel.Controls.Add(forgot);
//            currentY += 45;

//            var noAccount = new Label
//            {
//                Text = "Don't have an account?",
//                Location = new Point(contentX, currentY),
//                Font = new Font("Segoe UI", 11),
//                ForeColor = Color.FromArgb(60, 60, 60),
//                AutoSize = true
//            };
//            rightPanel.Controls.Add(noAccount);

//            var btnSignup = new LinkLabel
//            {
//                Text = "Sign Up",
//                Font = new Font("Segoe UI", 11, FontStyle.Bold),
//                Size = new Size(70, 25),
//                Location = new Point(noAccount.Right + 8, currentY),
//                LinkColor = Color.FromArgb(220, 70, 0),
//                ActiveLinkColor = Color.FromArgb(190, 60, 0),
//                Cursor = Cursors.Hand,
//                AutoSize = true
//            };
//            btnSignup.LinkClicked += (s, e) =>
//            {
//                var signupForm = new SignUpForm(this.Location);
//                this.Hide();
//                signupForm.Show();
//                signupForm.FormClosed += (sender, args) =>
//                {
//                    this.Location = signupForm.Location;
//                    this.Show();
//                };
//            };
//            rightPanel.Controls.Add(btnSignup);

//            if (!chkRememberMe.Checked)
//            {
//                txtUsername.Focus();
//            }
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

//        private void LblPasswordToggle_Click(object sender, EventArgs e)
//        {
//            bool isPasswordVisible = txtPassword.PasswordChar == '\0';

//            if (!txtPassword.IsPlaceholder)
//            {
//                txtPassword.PasswordChar = isPasswordVisible ? '●' : '\0';
//                lblPasswordToggle.Text = isPasswordVisible ? "👁️" : "🙈";
//            }

//            txtPassword.SelectionStart = txtPassword.Text.Length;
//            txtPassword.Focus();
//        }

//        private void BtnLogin_Click(object sender, EventArgs e)
//        {
//            string username = txtUsername.GetRawText().Trim();
//            string password = txtPassword.GetRawText();

//            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
//            {
//                MessageBox.Show("Please enter your username and password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                return;
//            }

//            btnLogin.Enabled = false;
//            btnLogin.Text = "Signing In...";
//            Application.DoEvents();

//            if (Users.VerifyCredentials(username, password))
//            {
//                SaveRememberedUser(username);

//                // Show success message
//                lblStatus.Text = "✓ Login Successful! Redirecting...";
//                lblStatus.ForeColor = Color.FromArgb(34, 139, 34);
//                lblStatus.Visible = true;
//                Application.DoEvents();

//                // Delay for user to see the message
//                System.Threading.Thread.Sleep(1000);

//                // Open MainMenuForm
//                MainMenuForm mainMenu = new MainMenuForm(username);
//                mainMenu.Show();

//                this.Hide();

//                // When main menu closes, close the login form too
//                mainMenu.FormClosed += (s, args) => this.Close();
//            }
//            else
//            {
//                lblStatus.Text = "✗ Invalid username or password";
//                lblStatus.ForeColor = Color.FromArgb(220, 20, 20);
//                lblStatus.Visible = true;

//                SaveRememberedUser(null);
//                btnLogin.Enabled = true;
//                btnLogin.Text = "Sign in now";

//                // Hide error message after 3 seconds
//                Timer hideTimer = new Timer();
//                hideTimer.Interval = 3000;
//                hideTimer.Tick += (s, args) =>
//                {
//                    lblStatus.Visible = false;
//                    hideTimer.Stop();
//                    hideTimer.Dispose();
//                };
//                hideTimer.Start();
//            }
//        }

//        private void LoadRememberedUser()
//        {
//            try
//            {
//                if (File.Exists(rememberedFilePath))
//                {
//                    string savedUser = File.ReadAllText(rememberedFilePath).Trim();
//                    if (!string.IsNullOrEmpty(savedUser))
//                    {
//                        txtUsername.Text = savedUser;
//                        txtUsername.ForeColor = Color.Black;
//                        chkRememberMe.Checked = true;
//                        txtUsername.IsPlaceholder = false;
//                        txtPassword.Focus();
//                    }
//                }
//            }
//            catch (Exception) { }
//        }

//        private void SaveRememberedUser(string username)
//        {
//            try
//            {
//                if (chkRememberMe.Checked && !string.IsNullOrEmpty(username))
//                {
//                    File.WriteAllText(rememberedFilePath, username);
//                }
//                else
//                {
//                    if (File.Exists(rememberedFilePath))
//                    {
//                        File.Delete(rememberedFilePath);
//                    }
//                }
//            }
//            catch (Exception) { }
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

//    public class CustomTextBox : TextBox
//    {
//        private string _placeholderText;
//        private bool _isPlaceholder = true;
//        private int _radius = 0;
//        private bool _isPassword = false;

//        public int Radius { get => _radius; set { _radius = value; UpdateBorder(); } }
//        public string PlaceholderText
//        {
//            get => _placeholderText;
//            set
//            {
//                _placeholderText = value;
//                if (_isPlaceholder) Text = value;
//            }
//        }
//        public bool IsPlaceholder { get => _isPlaceholder; set => _isPlaceholder = value; }
//        public bool IsPassword { get => _isPassword; set => _isPassword = value; }

//        public CustomTextBox()
//        {
//            BorderStyle = BorderStyle.FixedSingle;
//            Multiline = false;
//            Padding = new Padding(18, 15, 55, 15);
//        }

//        public string GetRawText()
//        {
//            return _isPlaceholder ? string.Empty : Text;
//        }

//        private void UpdateBorder()
//        {
//            Region = null; 
//            Invalidate(); 
//        }

//        protected override void OnCreateControl()
//        {
//            base.OnCreateControl();
//            if (_isPlaceholder)
//            {
//                Text = _placeholderText;
//                ForeColor = Color.Gray;
//            }
//            else
//            {
//                ForeColor = Color.Black;
//            }
//            UpdateBorder();
//        }

//        protected override void OnSizeChanged(EventArgs e)
//        {
//            base.OnSizeChanged(e);
//            UpdateBorder();
//        }

//        protected override void OnEnter(EventArgs e)
//        {
//            if (_isPlaceholder)
//            {
//                Text = "";
//                ForeColor = Color.Black;
//                _isPlaceholder = false;
//                if (_isPassword) PasswordChar = '●';
//            }
//            base.OnEnter(e);
//        }

//        protected override void OnLeave(EventArgs e)
//        {
//            if (string.IsNullOrWhiteSpace(Text))
//            {
//                Text = _placeholderText;
//                ForeColor = Color.Gray;
//                _isPlaceholder = true;
//                if (_isPassword) PasswordChar = '\0';
//            }
//            base.OnLeave(e);
//        }
//    }
//}