using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PlanIT2
{
    public class ProfilePanel : Panel
    {
        private string connectionString;
        private int userId;
        private string loggedInUser;
        private Label headerLabel;

        public ProfilePanel(string username, int userId, string connectionString)
        {
            this.loggedInUser = username;
            this.userId = userId;
            this.connectionString = connectionString;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.AutoScroll = true;

            CreateHeader();
            LoadProfileData();
        }

        private void CreateHeader()
        {
            var header = new Panel
            {
                Size = new Size(1100, 120),
                Location = new Point(20, 20),
                BackColor = Color.FromArgb(220, 70, 0)
            };
            this.Controls.Add(header);
            SetRoundedCorners(header, 20);

            headerLabel = new Label
            {
                Text = "👤 My Profile",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 40),
                AutoSize = true
            };
            header.Controls.Add(headerLabel);
        }

        private void LoadProfileData()
        {
            try
            {
                UserProfile profile = null;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM profiles WHERE user_id = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                profile = new UserProfile
                                {
                                    FullName = reader["full_name"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Bio = reader["bio"].ToString(),
                                    AvatarColor = reader["avatar_color"].ToString(),
                                    UpdatedAt = Convert.ToDateTime(reader["updated_at"])
                                };
                            }
                        }
                    }
                }

                if (profile != null)
                    DisplayProfileForm(profile);
                else
                    MessageBox.Show("Profile data not found for this user.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading profile: " + ex.Message);
            }
        }

        private void DisplayProfileForm(UserProfile profile)
        {
            var controlsToRemove = this.Controls.Cast<Control>()
                .Where(c => c.Location.Y > 150).ToList();
            foreach (var control in controlsToRemove)
                this.Controls.Remove(control);

            int mainWidth = 1100;
            int y = 160;

            var card = new Panel
            {
                Size = new Size(mainWidth, 500),
                Location = new Point(20, y),
                BackColor = Color.White
            };
            this.Controls.Add(card);
            SetRoundedCorners(card, 15);

            var nameLabel = new Label
            {
                Text = "Full Name",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 30),
                AutoSize = true
            };
            card.Controls.Add(nameLabel);

            var nameBox = new TextBox
            {
                Text = profile.FullName,
                Location = new Point(30, 60),
                Size = new Size(mainWidth - 60, 35),
                Font = new Font("Segoe UI", 12)
            };
            card.Controls.Add(nameBox);

            var emailLabel = new Label
            {
                Text = "Email",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 120),
                AutoSize = true
            };
            card.Controls.Add(emailLabel);

            var emailBox = new TextBox
            {
                Text = profile.Email,
                Location = new Point(30, 150),
                Size = new Size(mainWidth - 60, 35),
                Font = new Font("Segoe UI", 12)
            };
            card.Controls.Add(emailBox);

            var bioLabel = new Label
            {
                Text = "Bio",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 210),
                AutoSize = true
            };
            card.Controls.Add(bioLabel);

            var bioBox = new TextBox
            {
                Text = profile.Bio,
                Multiline = true,
                Location = new Point(30, 240),
                Size = new Size(mainWidth - 60, 100),
                Font = new Font("Segoe UI", 11)
            };
            card.Controls.Add(bioBox);

            var saveBtn = new Button
            {
                Text = "💾 Save Changes",
                Location = new Point(30, 380),
                Size = new Size(200, 50),
                BackColor = Color.FromArgb(220, 70, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Cursor = Cursors.Hand;
            saveBtn.Click += (s, e) => SaveProfile(nameBox.Text, emailBox.Text, bioBox.Text);
            card.Controls.Add(saveBtn);
        }

        private void SaveProfile(string fullName, string email, string bio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Full Name and Email cannot be empty.");
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE profiles 
                                     SET full_name = @full, email = @em, bio = @bio, updated_at = @up
                                     WHERE user_id = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@full", fullName);
                        cmd.Parameters.AddWithValue("@em", email);
                        cmd.Parameters.AddWithValue("@bio", bio);
                        cmd.Parameters.AddWithValue("@up", DateTime.Now);
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Profile updated successfully!");
                LoadProfileData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating profile: " + ex.Message);
            }
        }

        private void SetRoundedCorners(Control c, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.StartFigure();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddLine(radius, 0, c.Width - radius, 0);
            path.AddArc(c.Width - radius, 0, radius, radius, 270, 90);
            path.AddLine(c.Width, radius, c.Width, c.Height - radius);
            path.AddArc(c.Width - radius, c.Height - radius, radius, radius, 0, 90);
            path.AddLine(c.Width - radius, c.Height, radius, c.Height);
            path.AddArc(0, c.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        }
    }
}
