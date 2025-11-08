using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using PlanIT2.Database;


namespace PlanIT2
{
    public class AuthForm : Form
    {
        private CustomTextBox txtUsername, txtEmail, txtPassword, txtConfirmPassword, txtCaptcha;
        private CheckBox chkRememberMe;
        private Button btnSubmit;
        private LinkLabel btnToggleMode;
        private Label lblBrand, lblWelcome, lblSub, lblStatus, lblCaptcha;
        private Panel leftPanel, rightPanel, topBar, bottomBar, formContainer, scrollablePanel;
        private Label lblPasswordToggle, lblConfirmPasswordToggle;
        private bool isLoginMode = true;
        private string rememberedFilePath;

        private Panel pnlPasswordStrength;
        private Label lblStrengthText;
        private ProgressBar pbPasswordStrength;
        private FlowLayoutPanel pnlRequirements;
        private Label lblReqLength, lblReqLetter, lblReqNumber, lblReqMatch;
        private int loginAttempts = 0;
        private DateTime lastAttemptTime = DateTime.MinValue;
        private const int MAX_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 5;

        private string currentCaptcha = "";
        private Random random = new Random();
        private Panel captchaPanel;

        // Consistent MainMenu size
        private const int WINDOW_WIDTH = 1400;
        private const int WINDOW_HEIGHT = 800;
        private const int TOP_BAR_HEIGHT = 60;
        private const int BOTTOM_BAR_HEIGHT = 60;

        public AuthForm()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlanIT");
            Directory.CreateDirectory(appDataFolder);
            rememberedFilePath = Path.Combine(appDataFolder, "rememberedUser.txt");

            InitializeComponent();
            LoadRememberedUser();
        }

        private void InitializeComponent()
        {
            // 1. Set consistent size
            this.Size = new Size(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, this.Width, this.Height, 35, 35));

            // Top Bar - Always on top and stable
            topBar = new Panel
            {
                Size = new Size(this.Width, TOP_BAR_HEIGHT),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(15, 15, 15)
            };
            topBar.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    topBar.ClientRectangle,
                    Color.FromArgb(15, 15, 15),
                    Color.FromArgb(25, 25, 25),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, topBar.ClientRectangle);
                }
            };
            this.Controls.Add(topBar);
            topBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) NativeMethods.DragMove(this.Handle); };
            topBar.BringToFront(); // Crucial for flicker fix

            var minimizeButton = new Button
            {
                Text = "─",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 110, 7),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            minimizeButton.MouseEnter += (s, e) => minimizeButton.BackColor = Color.FromArgb(50, 50, 50);
            minimizeButton.MouseLeave += (s, e) => minimizeButton.BackColor = Color.Transparent;
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            topBar.Controls.Add(minimizeButton);

            var closeButton = new Button
            {
                Text = "✕",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 55, 7),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            closeButton.MouseEnter += (s, e) => closeButton.BackColor = Color.FromArgb(180, 40, 40);
            closeButton.MouseLeave += (s, e) => closeButton.BackColor = Color.Transparent;
            closeButton.Click += (s, e) => Application.Exit();
            topBar.Controls.Add(closeButton);

            // Bottom Bar - Always on top and stable
            bottomBar = new Panel
            {
                Size = new Size(this.Width, BOTTOM_BAR_HEIGHT),
                Location = new Point(0, this.Height - BOTTOM_BAR_HEIGHT),
                BackColor = Color.FromArgb(15, 15, 15)
            };
            bottomBar.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    bottomBar.ClientRectangle,
                    Color.FromArgb(25, 25, 25),
                    Color.FromArgb(15, 15, 15),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, bottomBar.ClientRectangle);
                }
            };
            this.Controls.Add(bottomBar);
            bottomBar.BringToFront(); // Crucial for flicker fix

            // Left Panel (Dark Side - for Login/Welcome)
            int leftPanelWidth = (int)(WINDOW_WIDTH * 0.58);
            leftPanel = new Panel
            {
                Size = new Size(leftPanelWidth, this.Height),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(20, 20, 20)
            };
            this.Controls.Add(leftPanel);

            var overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            overlay.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    overlay.ClientRectangle,
                    Color.FromArgb(20, 20, 20),
                    Color.FromArgb(40, 40, 60),
                    LinearGradientMode.ForwardDiagonal))
                {
                    e.Graphics.FillRectangle(brush, overlay.ClientRectangle);
                }
            };
            leftPanel.Controls.Add(overlay);

            lblBrand = new Label
            {
                Text = "PlanIT",
                Font = new Font("Segoe UI", 56, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblWelcome = new Label
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 34, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblSub = new Label
            {
                Text = "Let's plan smarter together.",
                Font = new Font("Segoe UI", 16, FontStyle.Italic),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            overlay.Controls.Add(lblBrand);
            overlay.Controls.Add(lblWelcome);
            overlay.Controls.Add(lblSub);

            leftPanel.Resize += (s, e) => PositionLeftText();
            PositionLeftText();

            // Right Panel (White Side - for Forms)
            int rightPanelWidth = this.Width - leftPanelWidth;
            rightPanel = new Panel
            {
                Size = new Size(rightPanelWidth, this.Height),
                Location = new Point(leftPanelWidth, 0),
                BackColor = Color.White
            };
            this.Controls.Add(rightPanel);
            rightPanel.SendToBack(); // Set it behind the top and bottom bars

            scrollablePanel = new Panel
            {
                Size = new Size(rightPanel.Width, rightPanel.Height - TOP_BAR_HEIGHT - BOTTOM_BAR_HEIGHT),
                Location = new Point(0, TOP_BAR_HEIGHT),
                BackColor = Color.White,
                AutoScroll = true
            };
            rightPanel.Controls.Add(scrollablePanel);

            formContainer = new Panel
            {
                Size = new Size(scrollablePanel.Width, 800),
                Location = new Point(0, 0),
                BackColor = Color.White
            };
            scrollablePanel.Controls.Add(formContainer);

            BuildLoginForm();
            // Re-assert layering to ensure stability
            topBar.BringToFront();
            bottomBar.BringToFront();
        }

        private void BuildLoginForm()
        {
            isLoginMode = true;
            formContainer.Controls.Clear();
            formContainer.Height = 600;
            scrollablePanel.AutoScrollPosition = new Point(0, 0);

            // Re-adjust size for login mode (split screen)
            int leftPanelWidth = (int)(WINDOW_WIDTH * 0.58);
            leftPanel.Width = leftPanelWidth;
            leftPanel.Visible = true;
            rightPanel.Width = WINDOW_WIDTH - leftPanelWidth;
            rightPanel.Location = new Point(leftPanelWidth, 0);
            scrollablePanel.Width = rightPanel.Width;
            formContainer.Width = scrollablePanel.Width - 15; // Give a little margin for scrollbar width

            int contentX = 70;
            int contentWidth = formContainer.Width - 2 * contentX;
            int currentY = 100;

            var header = new Label
            {
                Text = "Login",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(contentX, currentY),
                AutoSize = true
            };
            formContainer.Controls.Add(header);
            currentY += 80;

            txtUsername = new CustomTextBox
            {
                PlaceholderText = "Email or Username",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Size = new Size(contentWidth, 60),
                Location = new Point(contentX, currentY),
                Radius = 12,
                BackColor = Color.White
            };
            txtUsername.KeyDown += TextBox_KeyDown;
            formContainer.Controls.Add(txtUsername);
            currentY += 75;

            txtPassword = new CustomTextBox
            {
                PlaceholderText = "Password",
                IsPassword = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Size = new Size(contentWidth, 60),
                Location = new Point(contentX, currentY),
                Radius = 12,
                BackColor = Color.White
            };
            txtPassword.KeyDown += TextBox_KeyDown;
            formContainer.Controls.Add(txtPassword);

            lblPasswordToggle = new Label
            {
                Size = new Size(40, 40),
                Location = new Point(txtPassword.Right - 50, txtPassword.Top + 10),
                Cursor = Cursors.Hand,
                Text = "👁",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(120, 120, 120)
            };
            lblPasswordToggle.Click += (s, e) => TogglePasswordVisibility(txtPassword, lblPasswordToggle);
            lblPasswordToggle.MouseEnter += (s, e) => lblPasswordToggle.ForeColor = Color.FromArgb(220, 70, 0);
            lblPasswordToggle.MouseLeave += (s, e) => lblPasswordToggle.ForeColor = Color.FromArgb(120, 120, 120);
            formContainer.Controls.Add(lblPasswordToggle);
            lblPasswordToggle.BringToFront();
            currentY += 75;

            chkRememberMe = new CheckBox
            {
                Text = "Remember Me",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(contentX, currentY),
                AutoSize = true
            };
            formContainer.Controls.Add(chkRememberMe);
            currentY += 50;

            btnSubmit = new Button
            {
                Text = "Login now",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(contentWidth, 58),
                Location = new Point(contentX, currentY),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            SetRoundedCorners(btnSubmit, 12);
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.MouseEnter += (s, e) => {
                if (btnSubmit.Enabled)
                    AnimateButtonColor(btnSubmit, Color.FromArgb(255, 90, 20));
            };
            btnSubmit.MouseLeave += (s, e) => AnimateButtonColor(btnSubmit, Color.FromArgb(220, 70, 0));
            btnSubmit.MouseDown += (s, e) => btnSubmit.BackColor = Color.FromArgb(190, 60, 0);
            btnSubmit.Click += BtnSubmit_Click;
            formContainer.Controls.Add(btnSubmit);
            currentY += 70;

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Green,
                Location = new Point(contentX, currentY),
                Size = new Size(contentWidth, 40),
                TextAlign = ContentAlignment.TopCenter,
                Visible = false
            };
            formContainer.Controls.Add(lblStatus);
            currentY += 45;

            var noAccount = new Label
            {
                Text = "Don't have an account?",
                Location = new Point(contentX, currentY),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoSize = true
            };
            formContainer.Controls.Add(noAccount);

            btnToggleMode = new LinkLabel
            {
                Text = "Sign Up",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(70, 25),
                Location = new Point(noAccount.Right + 8, currentY),
                LinkColor = Color.FromArgb(220, 70, 0),
                ActiveLinkColor = Color.FromArgb(190, 60, 0),
                VisitedLinkColor = Color.FromArgb(220, 70, 0),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            ((LinkLabel)btnToggleMode).LinkClicked += (s, e) => ToggleMode();
            formContainer.Controls.Add(btnToggleMode);

            // Final height
            formContainer.Height = currentY + 70;

            // Center the form content vertically if it's smaller than the scrollable area
            if (formContainer.Height < scrollablePanel.Height)
            {
                formContainer.Location = new Point(formContainer.Location.X, (scrollablePanel.Height - formContainer.Height) / 2);
            }
            else
            {
                formContainer.Location = new Point(formContainer.Location.X, 0);
            }

            txtUsername.Focus();
        }

        private void BuildSignUpForm()
        {
            isLoginMode = false;
            formContainer.Controls.Clear();
            scrollablePanel.AutoScrollPosition = new Point(0, 0);

            // Adjust size for sign-up mode (full screen)
            leftPanel.Width = 0;
            leftPanel.Visible = false;
            rightPanel.Width = this.Width;
            rightPanel.Location = new Point(0, 0);
            scrollablePanel.Width = rightPanel.Width;
            formContainer.Width = scrollablePanel.Width - 15;

            // Form layout dimensions
            int formX = 70;
            int formWidth = formContainer.Width - 2 * formX;
            int currentY = 50;

            // Header
            var header = new Label
            {
                Text = "Sign Up",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(formX, currentY),
                AutoSize = true
            };
            formContainer.Controls.Add(header);
            currentY += 70;

            // --- Two-Column Layout Setup ---
            int leftColumnWidth = 450;
            int rightColumnWidth = 400;
            int gap = 50;

            // Adjust formX to center the two columns
            int totalFormWidth = leftColumnWidth + gap + rightColumnWidth;
            int centerOffsetX = (formContainer.Width - totalFormWidth) / 2;

            int column1X = centerOffsetX;
            int column2X = centerOffsetX + leftColumnWidth + gap;
            int fieldHeight = 60;
            int fieldSpacing = 70;

            int initialY = currentY;

            // --- Left Column Fields (Username, Email, Password, Confirm Password) ---

            // Username
            txtUsername = new CustomTextBox
            {
                PlaceholderText = "Username",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Size = new Size(leftColumnWidth, fieldHeight),
                Location = new Point(column1X, initialY),
                Radius = 12,
                BackColor = Color.White
            };
            txtUsername.KeyDown += TextBox_KeyDown;
            formContainer.Controls.Add(txtUsername);

            // Email
            txtEmail = new CustomTextBox
            {
                PlaceholderText = "Email Address",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Size = new Size(leftColumnWidth, fieldHeight),
                Location = new Point(column1X, initialY + fieldSpacing),
                Radius = 12,
                BackColor = Color.White
            };
            txtEmail.KeyDown += TextBox_KeyDown;
            formContainer.Controls.Add(txtEmail);

            // Password
            txtPassword = new CustomTextBox
            {
                PlaceholderText = "Password",
                IsPassword = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Size = new Size(leftColumnWidth, fieldHeight),
                Location = new Point(column1X, initialY + fieldSpacing * 2),
                Radius = 12,
                BackColor = Color.White
            };
            txtPassword.TextChanged += TxtPassword_TextChanged;
            txtPassword.KeyDown += TextBox_KeyDown;
            formContainer.Controls.Add(txtPassword);

            // Password Toggle (for Password field)
            lblPasswordToggle = new Label
            {
                Size = new Size(40, 40),
                Location = new Point(txtPassword.Right - 50, txtPassword.Top + 10),
                Cursor = Cursors.Hand,
                Text = "👁",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(120, 120, 120)
            };
            lblPasswordToggle.Click += (s, e) => TogglePasswordVisibility(txtPassword, lblPasswordToggle);
            lblPasswordToggle.MouseEnter += (s, e) => lblPasswordToggle.ForeColor = Color.FromArgb(220, 70, 0);
            lblPasswordToggle.MouseLeave += (s, e) => lblPasswordToggle.ForeColor = Color.FromArgb(120, 120, 120);
            formContainer.Controls.Add(lblPasswordToggle);
            lblPasswordToggle.BringToFront();

            // Password Strength/Requirements Panel (Below Password Field)
            int strengthY = txtPassword.Bottom + 5;
            pnlPasswordStrength = new Panel
            {
                Size = new Size(leftColumnWidth, 30),
                Location = new Point(column1X, strengthY),
                Visible = true
            };

            pbPasswordStrength = new ProgressBar
            {
                Size = new Size(leftColumnWidth - 120, 8),
                Location = new Point(0, 15),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            pnlPasswordStrength.Controls.Add(pbPasswordStrength);

            lblStrengthText = new Label
            {
                Text = "Weak",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(leftColumnWidth - 110, 10),
                AutoSize = true
            };
            pnlPasswordStrength.Controls.Add(lblStrengthText);

            formContainer.Controls.Add(pnlPasswordStrength);

            // Requirements (Below strength bar)
            int requirementsY = pnlPasswordStrength.Bottom + 5;
            pnlRequirements = new FlowLayoutPanel
            {
                Size = new Size(leftColumnWidth, 80),
                Location = new Point(column1X, requirementsY),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Visible = true
            };

            lblReqLength = CreateRequirementLabel("✗ At least 6 characters");
            lblReqLetter = CreateRequirementLabel("✗ Contains letters");
            lblReqNumber = CreateRequirementLabel("✗ Contains numbers");

            pnlRequirements.Controls.Add(lblReqLength);
            pnlRequirements.Controls.Add(lblReqLetter);
            pnlRequirements.Controls.Add(lblReqNumber);

            formContainer.Controls.Add(pnlRequirements);

            // Confirm Password (Placed visually below the requirements)
            int confirmPasswordY = pnlRequirements.Bottom + 10;
            txtConfirmPassword = new CustomTextBox
            {
                PlaceholderText = "Confirm Password",
                IsPassword = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Size = new Size(leftColumnWidth, fieldHeight),
                Location = new Point(column1X, confirmPasswordY),
                Radius = 12,
                BackColor = Color.White
            };
            txtConfirmPassword.TextChanged += TxtConfirmPassword_TextChanged;
            txtConfirmPassword.KeyDown += TextBox_KeyDown;
            formContainer.Controls.Add(txtConfirmPassword);

            // Confirm Password Toggle
            lblConfirmPasswordToggle = new Label
            {
                Size = new Size(40, 40),
                Location = new Point(txtConfirmPassword.Right - 50, txtConfirmPassword.Top + 10),
                Cursor = Cursors.Hand,
                Text = "👁",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(120, 120, 120)
            };
            lblConfirmPasswordToggle.Click += (s, e) => TogglePasswordVisibility(txtConfirmPassword, lblConfirmPasswordToggle);
            lblConfirmPasswordToggle.MouseEnter += (s, e) => lblConfirmPasswordToggle.ForeColor = Color.FromArgb(220, 70, 0);
            lblConfirmPasswordToggle.MouseLeave += (s, e) => lblConfirmPasswordToggle.ForeColor = Color.FromArgb(120, 120, 120);
            formContainer.Controls.Add(lblConfirmPasswordToggle);
            lblConfirmPasswordToggle.BringToFront();

            // Password Match Label (Below Confirm Password)
            lblReqMatch = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                Location = new Point(column1X, txtConfirmPassword.Bottom + 5),
                AutoSize = true,
                Visible = false
            };
            formContainer.Controls.Add(lblReqMatch);

            // --- Right Column Fields (CAPTCHA) ---

            GenerateCaptcha();

            captchaPanel = new Panel
            {
                Size = new Size(rightColumnWidth, 220),
                Location = new Point(column2X, initialY),
                BackColor = Color.FromArgb(250, 250, 250)
            };
            SetRoundedCorners(captchaPanel, 12);

            var captchaTitle = new Label
            {
                Text = "Verify You're Human",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(20, 15),
                AutoSize = true
            };
            captchaPanel.Controls.Add(captchaTitle);

            lblCaptcha = new Label
            {
                Text = currentCaptcha,
                Font = new Font("Courier New", 28, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 70, 0),
                BackColor = Color.White,
                Size = new Size(rightColumnWidth - 40, 70),
                Location = new Point(20, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            captchaPanel.Controls.Add(lblCaptcha);

            txtCaptcha = new CustomTextBox
            {
                PlaceholderText = "Enter CAPTCHA",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 12),
                Size = new Size(rightColumnWidth - 40, 50),
                Location = new Point(20, 135),
                Radius = 8,
                BackColor = Color.White
            };
            txtCaptcha.KeyDown += TextBox_KeyDown;
            captchaPanel.Controls.Add(txtCaptcha);

            formContainer.Controls.Add(captchaPanel);

            // Determine the maximum required height for the form container
            int maxBottomY = Math.Max(lblReqMatch.Bottom, captchaPanel.Bottom);
            currentY = maxBottomY + 30; // Move below the lowest element in the two columns

            // --- Submit Button (Centered Below Columns) ---

            btnSubmit = new Button
            {
                Text = "Create Account",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(totalFormWidth, 58),
                Location = new Point(centerOffsetX, currentY),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            SetRoundedCorners(btnSubmit, 12);
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.MouseEnter += (s, e) => {
                if (btnSubmit.Enabled)
                    AnimateButtonColor(btnSubmit, Color.FromArgb(255, 90, 20));
            };
            btnSubmit.MouseLeave += (s, e) => AnimateButtonColor(btnSubmit, Color.FromArgb(220, 70, 0));
            btnSubmit.MouseDown += (s, e) => btnSubmit.BackColor = Color.FromArgb(190, 60, 0);
            btnSubmit.Click += BtnSubmit_Click;
            formContainer.Controls.Add(btnSubmit);
            currentY += 70;

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Green,
                Location = new Point(centerOffsetX, currentY),
                Size = new Size(totalFormWidth, 40),
                TextAlign = ContentAlignment.TopCenter,
                Visible = false
            };
            formContainer.Controls.Add(lblStatus);
            currentY += 45;

            // --- Already Have Account? (Below CAPTCHA/Status) ---

            var hasAccount = new Label
            {
                Text = "Already have an account?",
                // Calculate position to center the text block + link combination
                Location = new Point(centerOffsetX + (totalFormWidth / 2) - 100, currentY),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoSize = true
            };
            formContainer.Controls.Add(hasAccount);

            // Adjust LinkLabel position based on the label above
            btnToggleMode = new LinkLabel
            {
                Text = "Login",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(70, 25),
                Location = new Point(hasAccount.Right + 8, currentY),
                LinkColor = Color.FromArgb(220, 70, 0),
                ActiveLinkColor = Color.FromArgb(190, 60, 0),
                VisitedLinkColor = Color.FromArgb(220, 70, 0),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            ((LinkLabel)btnToggleMode).LinkClicked += (s, e) => ToggleMode();
            formContainer.Controls.Add(btnToggleMode);
            currentY += 45; // Move down after the link

            // Final formContainer height adjustment to avoid scrollbar when content fits
            formContainer.Height = currentY + 30;
            if (formContainer.Height < scrollablePanel.Height)
            {
                // Center the form content vertically if it's smaller than the scrollable area
                formContainer.Location = new Point(formContainer.Location.X, (scrollablePanel.Height - formContainer.Height) / 2);
            }
            else
            {
                formContainer.Location = new Point(formContainer.Location.X, 0);
            }


            txtUsername.Focus();
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            currentCaptcha = "";
            for (int i = 0; i < 6; i++)
            {
                currentCaptcha += chars[random.Next(chars.Length)];
            }
        }

        private Label CreateRequirementLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(180, 180, 180),
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 3)
            };
        }

        private void TxtPassword_TextChanged(object sender, EventArgs e)
        {
            string password = txtPassword.GetRawText();

            if (string.IsNullOrEmpty(password))
            {
                pbPasswordStrength.Value = 0;
                lblStrengthText.Text = "Weak";
                lblStrengthText.ForeColor = Color.Red;

                UpdateRequirement(lblReqLength, false, "✓ At least 6 characters", "✗ At least 6 characters");
                UpdateRequirement(lblReqLetter, false, "✓ Contains letters", "✗ Contains letters");
                UpdateRequirement(lblReqNumber, false, "✓ Contains numbers", "✗ Contains numbers");
                return;
            }

            bool hasLength = password.Length >= 6;
            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
            bool hasNumber = Regex.IsMatch(password, @"\d");

            UpdateRequirement(lblReqLength, hasLength, "✓ At least 6 characters", "✗ At least 6 characters");
            UpdateRequirement(lblReqLetter, hasLetter, "✓ Contains letters", "✗ Contains letters");
            UpdateRequirement(lblReqNumber, hasNumber, "✓ Contains numbers", "✗ Contains numbers");

            int strength = 0;
            if (hasLength) strength += 33;
            if (hasLetter) strength += 33;
            if (hasNumber) strength += 34;

            pbPasswordStrength.Value = strength;

            if (strength <= 33)
            {
                lblStrengthText.Text = "Weak";
                lblStrengthText.ForeColor = Color.FromArgb(220, 50, 50);
            }
            else if (strength <= 66)
            {
                lblStrengthText.Text = "Medium";
                lblStrengthText.ForeColor = Color.FromArgb(255, 165, 0);
            }
            else
            {
                lblStrengthText.Text = "Strong";
                lblStrengthText.ForeColor = Color.FromArgb(50, 180, 50);
            }

            if (txtConfirmPassword != null && !txtConfirmPassword.IsPlaceholder)
            {
                TxtConfirmPassword_TextChanged(null, null);
            }
        }

        private void TxtConfirmPassword_TextChanged(object sender, EventArgs e)
        {
            if (txtConfirmPassword == null || txtConfirmPassword.IsPlaceholder || txtPassword.IsPlaceholder)
            {
                if (lblReqMatch != null) lblReqMatch.Visible = false;
                return;
            }

            string password = txtPassword.GetRawText();
            string confirmPassword = txtConfirmPassword.GetRawText();

            if (string.IsNullOrEmpty(confirmPassword))
            {
                lblReqMatch.Visible = false;
                return;
            }

            lblReqMatch.Visible = true;

            if (password == confirmPassword)
            {
                lblReqMatch.Text = "✓ Passwords match";
                lblReqMatch.ForeColor = Color.FromArgb(50, 180, 50);
            }
            else
            {
                lblReqMatch.Text = "✗ Passwords do not match";
                lblReqMatch.ForeColor = Color.FromArgb(220, 50, 50);
            }
        }

        private void UpdateRequirement(Label label, bool met, string textMet, string textNotMet)
        {
            label.Text = met ? textMet : textNotMet;
            label.ForeColor = met ? Color.FromArgb(50, 180, 50) : Color.FromArgb(180, 180, 180);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSubmit_Click(sender, e);
            }
        }

        private void ToggleMode()
        {
            // Use SuspendLayout to prevent flickering during control repositioning
            this.SuspendLayout();
            rightPanel.SuspendLayout();
            leftPanel.SuspendLayout();

            bool wasLoginMode = isLoginMode;
            isLoginMode = !isLoginMode;

            Timer slideTimer = new Timer();
            slideTimer.Interval = 1;
            int steps = 0;
            const int maxSteps = 40; // Increased steps for smoother look

            int loginLeftWidth = (int)(WINDOW_WIDTH * 0.58);
            int startLeftWidth = wasLoginMode ? loginLeftWidth : 0;
            int targetLeftWidth = isLoginMode ? loginLeftWidth : 0;

            // Prepare the destination form (but keep it hidden by the slide)
            if (isLoginMode)
            {
                leftPanel.Visible = true;
                BuildLoginForm();
            }
            else
            {
                BuildSignUpForm();
            }

            // Ensure the Z-order is correct: Controls > Panels
            rightPanel.BringToFront();
            topBar.BringToFront();
            bottomBar.BringToFront();

            slideTimer.Tick += (s, e) =>
            {
                steps++;
                float progress = (float)steps / maxSteps;
                // Easing function (smoother start/stop)
                progress = (float)(0.5 - 0.5 * Math.Cos(progress * Math.PI));

                if (steps <= maxSteps)
                {
                    int currentLeftWidth = (int)(startLeftWidth + (targetLeftWidth - startLeftWidth) * progress);

                    leftPanel.Width = currentLeftWidth;
                    rightPanel.Width = WINDOW_WIDTH - currentLeftWidth;
                    rightPanel.Location = new Point(currentLeftWidth, 0);
                    scrollablePanel.Width = rightPanel.Width;

                    // Re-position the form container based on the current width for centering
                    // This prevents form jumpiness during the slide
                    formContainer.Width = scrollablePanel.Width - 15;
                    if (!isLoginMode)
                    {
                        // Re-calculate center for two-column layout in Sign Up mode
                        int totalFormWidth = 450 + 50 + 400; // Left + gap + Right
                        int centerOffsetX = (formContainer.Width - totalFormWidth) / 2;

                        // Update control locations if they are based on centerOffsetX (not necessary if based on constant coordinates)
                        // For simplicity and flickering avoidance, we ensure the form content is built correctly and let the panel move.
                    }
                }

                if (steps >= maxSteps)
                {
                    // Final layout lock
                    leftPanel.Width = targetLeftWidth;
                    rightPanel.Width = WINDOW_WIDTH - targetLeftWidth;
                    rightPanel.Location = new Point(targetLeftWidth, 0);
                    scrollablePanel.Width = rightPanel.Width;
                    formContainer.Width = scrollablePanel.Width - 15;

                    if (!isLoginMode)
                    {
                        leftPanel.Visible = false;
                    }

                    // Re-center form content after transition is complete
                    if (formContainer.Height < scrollablePanel.Height)
                    {
                        formContainer.Location = new Point(formContainer.Location.X, (scrollablePanel.Height - formContainer.Height) / 2);
                    }
                    else
                    {
                        formContainer.Location = new Point(formContainer.Location.X, 0);
                    }

                    slideTimer.Stop();
                    slideTimer.Dispose();

                    // Release layout suspension
                    leftPanel.ResumeLayout();
                    rightPanel.ResumeLayout();
                    this.ResumeLayout();
                    this.Refresh();
                }
            };

            // Start the animation
            slideTimer.Start();

            // Release initial layout suspension (must be before timer start, but we use it to wrap the whole preparation)
            leftPanel.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void AnimateButtonColor(Button btn, Color targetColor)
        {
            Timer colorTimer = new Timer();
            colorTimer.Interval = 16;

            Color startColor = btn.BackColor;
            float progress = 0f;

            colorTimer.Tick += (s, e) =>
            {
                progress += 0.15f;
                if (progress >= 1f)
                {
                    btn.BackColor = targetColor;
                    colorTimer.Stop();
                    colorTimer.Dispose();
                }
                else
                {
                    int r = (int)(startColor.R + (targetColor.R - startColor.R) * progress);
                    int g = (int)(startColor.G + (targetColor.G - startColor.G) * progress);
                    int b = (int)(startColor.B + (targetColor.B - startColor.B) * progress);
                    btn.BackColor = Color.FromArgb(r, g, b);
                }
            };
            colorTimer.Start();
        }

        private void PositionLeftText()
        {
            int totalHeight = lblBrand.Height + 15 + lblWelcome.Height + 20 + lblSub.Height;
            int startY = (leftPanel.Height / 2) - (totalHeight / 2);
            int pad = 70;

            lblBrand.Location = new Point(pad, startY);
            lblWelcome.Location = new Point(pad, lblBrand.Bottom + 15);
            lblSub.Location = new Point(pad, lblWelcome.Bottom + 20);
        }

        private void TogglePasswordVisibility(CustomTextBox textBox, Label toggleLabel)
        {
            bool isPasswordVisible = textBox.PasswordChar == '\0';

            if (!textBox.IsPlaceholder)
            {
                // Toggle between '●' (default) and '\0' (visible)
                textBox.PasswordChar = isPasswordVisible ? '●' : '\0';
                toggleLabel.Text = isPasswordVisible ? "👁" : "👁‍🗨";
            }

            // Keep cursor at the end of the text
            textBox.SelectionStart = textBox.Text.Length;
            textBox.Focus();
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (isLoginMode)
                HandleLogin();
            else
                HandleSignUp();
        }

        private bool CheckRateLimit()
        {
            TimeSpan timeSinceLastAttempt = DateTime.Now - lastAttemptTime;

            if (loginAttempts >= MAX_ATTEMPTS && timeSinceLastAttempt.TotalMinutes < LOCKOUT_MINUTES)
            {
                int remainingMinutes = LOCKOUT_MINUTES - (int)timeSinceLastAttempt.TotalMinutes;
                ShowError($"Too many failed attempts. Please try again in {remainingMinutes} minute(s).");
                return false;
            }

            if (timeSinceLastAttempt.TotalMinutes >= LOCKOUT_MINUTES)
            {
                loginAttempts = 0;
            }

            return true;
        }

        private void HandleLogin()
        {
            if (!CheckRateLimit())
            {
                return;
            }

            string username = txtUsername.GetRawText().Trim();
            string password = txtPassword.GetRawText();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your username and password.");
                return;
            }

            btnSubmit.Enabled = false;
            btnSubmit.Text = "Logging In...";
            btnSubmit.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                // Assuming Users.VerifyCredentials connects to the planit MySQL database
                if (Users.VerifyCredentials(username, password))
                {
                    loginAttempts = 0;
                    SaveRememberedUser(username);

                    lblStatus.Text = "✓ Login Successful! Redirecting...";
                    lblStatus.ForeColor = Color.FromArgb(34, 139, 34);
                    lblStatus.Visible = true;
                    Application.DoEvents();

                    // Smooth Fade-out before MainMenu opens
                    Timer fadeTimer = new Timer();
                    fadeTimer.Interval = 10;
                    float opacity = 1.0f;

                    fadeTimer.Tick += (s, e) =>
                    {
                        opacity -= 0.05f;
                        if (opacity <= 0)
                        {
                            fadeTimer.Stop();
                            this.Hide();

                            MainMenuForm mainMenu = new MainMenuForm(username);
                            mainMenu.Show();
                            this.Opacity = 1.0f; // Reset opacity for future use

                            // Close the AuthForm only when the MainMenuForm is closed
                            mainMenu.FormClosed += (s_main, args_main) => this.Close();

                        }
                        else
                        {
                            this.Opacity = opacity;
                        }
                    };
                    fadeTimer.Start();
                }
                else
                {
                    loginAttempts++;
                    lastAttemptTime = DateTime.Now;

                    int remainingAttempts = MAX_ATTEMPTS - loginAttempts;
                    if (remainingAttempts > 0)
                    {
                        ShowError($"Invalid username or password. {remainingAttempts} attempt(s) remaining.");
                    }
                    else
                    {
                        ShowError($"Too many failed attempts. Account locked for {LOCKOUT_MINUTES} minutes.");
                    }

                    SaveRememberedUser(null);
                    btnSubmit.Enabled = true;
                    btnSubmit.Text = "Login now";
                    btnSubmit.Cursor = Cursors.Hand;
                }
            }
            catch (MySqlException ex)
            {
                ShowError("Database connection error. Please check your network connection.");
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Login now";
                btnSubmit.Cursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred. Please try again.");
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Login now";
                btnSubmit.Cursor = Cursors.Hand;
            }
        }

        private void HandleSignUp()
        {
            string username = txtUsername.GetRawText().Trim();
            string email = txtEmail.GetRawText().Trim();
            string password = txtPassword.GetRawText();
            string confirmPassword = txtConfirmPassword.GetRawText();
            string enteredCaptcha = txtCaptcha.GetRawText().ToUpper();

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Username is required.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Email is required.");
                txtEmail.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Password is required.");
                txtPassword.Focus();
                return;
            }

            if (!IsPasswordValid(password))
            {
                ShowError("Password must be at least 6 characters with letters and numbers.");
                txtPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Please confirm your password.");
                txtConfirmPassword.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match.");
                txtConfirmPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(enteredCaptcha))
            {
                ShowError("Please enter the CAPTCHA.");
                txtCaptcha.Focus();
                return;
            }

            if (enteredCaptcha != currentCaptcha)
            {
                ShowError("Incorrect CAPTCHA. Please try again.");
                GenerateCaptcha();
                lblCaptcha.Text = currentCaptcha;
                txtCaptcha.Clear();
                txtCaptcha.Focus();
                return;
            }

            btnSubmit.Enabled = false;
            btnSubmit.Text = "Creating Account...";
            btnSubmit.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            try
            {
                // Restored database connection and logic
                using (var conn = PlanIT2.Database.Database.GetConnection())
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";

                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        checkCmd.Parameters.AddWithValue("@email", email);

                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            ShowError("Username or email already exists!");
                            btnSubmit.Enabled = true;
                            btnSubmit.Text = "Create Account";
                            btnSubmit.Cursor = Cursors.Hand;
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO users (username, email, password) VALUES (@username, @email, @password)";

                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.ExecuteNonQuery();
                    }

                    string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "PlanIT", username);
                    Directory.CreateDirectory(userFolder);

                    lblStatus.Text = "✓ Account Created Successfully! Redirecting to login...";
                    lblStatus.ForeColor = Color.FromArgb(34, 139, 34);
                    lblStatus.Visible = true;
                    Application.DoEvents();

                    System.Threading.Thread.Sleep(1500);
                    isLoginMode = false; // Set to false so ToggleMode transitions to Login (true)
                    ToggleMode();
                }
            }
            catch (MySqlException ex)
            {
                ShowError($"Database error: {ex.Message}");
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Create Account";
                btnSubmit.Cursor = Cursors.Hand;
            }
            catch (IOException ex)
            {
                ShowError("Error creating user folder. Please check permissions.");
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Create Account";
                btnSubmit.Cursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Create Account";
                btnSubmit.Cursor = Cursors.Hand;
            }
        }

        private void ShowError(string message)
        {
            lblStatus.Text = "✗ " + message;
            lblStatus.ForeColor = Color.FromArgb(220, 20, 20);
            lblStatus.Visible = true;

            Timer hideTimer = new Timer();
            hideTimer.Interval = 5000;
            hideTimer.Tick += (s, args) =>
            {
                lblStatus.Visible = false;
                hideTimer.Stop();
                hideTimer.Dispose();
            };
            hideTimer.Start();
        }

        private void LoadRememberedUser()
        {
            try
            {
                if (File.Exists(rememberedFilePath))
                {
                    string savedUser = File.ReadAllText(rememberedFilePath).Trim();
                    if (!string.IsNullOrEmpty(savedUser) && isLoginMode)
                    {
                        txtUsername.Text = savedUser;
                        txtUsername.ForeColor = Color.Black;
                        chkRememberMe.Checked = true;
                        txtUsername.IsPlaceholder = false;
                        txtPassword.Focus();
                    }
                }
            }
            catch (Exception) { }
        }

        private void SaveRememberedUser(string username)
        {
            try
            {
                if (chkRememberMe != null && chkRememberMe.Checked && !string.IsNullOrEmpty(username))
                {
                    File.WriteAllText(rememberedFilePath, username);
                }
                else
                {
                    if (File.Exists(rememberedFilePath))
                    {
                        File.Delete(rememberedFilePath);
                    }
                }
            }
            catch (Exception) { }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsPasswordValid(string password)
        {
            if (password.Length < 6) return false;
            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
            bool hasNumber = Regex.IsMatch(password, @"\d");
            return hasLetter && hasNumber;
        }

        private void SetRoundedCorners(Control c, int radius)
        {
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius * 2, radius * 2), 180, 90);
            path.AddLine(radius, 0, c.Width - radius, 0);
            path.AddArc(new Rectangle(c.Width - radius * 2, 0, radius * 2, radius * 2), 270, 90);
            path.AddLine(c.Width, radius, c.Width, c.Height - radius);
            path.AddArc(new Rectangle(c.Width - radius * 2, c.Height - radius * 2, radius * 2, radius * 2), 0, 90);
            path.AddLine(c.Width - radius, c.Height, radius, c.Height);
            path.AddArc(new Rectangle(0, c.Height - radius * 2, radius * 2, radius * 2), 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        private static class NativeMethods
        {
            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HTCAPTION = 0x2;

            [DllImport("user32.dll")]
            public static extern bool ReleaseCapture();

            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
            public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
                int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

            public static void DragMove(IntPtr handle)
            {
                ReleaseCapture();
                SendMessage(handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }

    public class CustomTextBox : TextBox
    {
        private string _placeholderText;
        private bool _isPlaceholder = true;
        private int _radius = 0;
        private bool _isPassword = false;
        private Color _borderColor = Color.FromArgb(200, 200, 200);
        private Color _focusBorderColor = Color.FromArgb(0, 120, 215);

        public int Radius { get => _radius; set { _radius = value; UpdateBorder(); } }
        public string PlaceholderText
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value;
                if (_isPlaceholder) Text = value;
            }
        }
        public bool IsPlaceholder { get => _isPlaceholder; set => _isPlaceholder = value; }
        public bool IsPassword { get => _isPassword; set => _isPassword = value; }

        public CustomTextBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
            Multiline = false;
            Padding = new Padding(18, 15, 55, 15);
        }

        public string GetRawText()
        {
            return _isPlaceholder ? string.Empty : Text;
        }

        private void UpdateBorder()
        {
            Region = null;
            Invalidate();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (_isPlaceholder)
            {
                Text = _placeholderText;
                ForeColor = Color.Gray;
            }
            else
            {
                ForeColor = Color.Black;
            }
            UpdateBorder();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateBorder();
        }

        protected override void OnEnter(EventArgs e)
        {
            if (_isPlaceholder)
            {
                Text = "";
                ForeColor = Color.Black;
                _isPlaceholder = false;
                if (_isPassword) PasswordChar = '●';
            }
            _borderColor = _focusBorderColor;
            Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                Text = _placeholderText;
                ForeColor = Color.Gray;
                _isPlaceholder = true;
                if (_isPassword) PasswordChar = '\0';
            }
            _borderColor = Color.FromArgb(200, 200, 200);
            Invalidate();
            base.OnLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(_borderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}