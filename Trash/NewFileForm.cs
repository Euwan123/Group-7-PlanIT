//using System;
//using System.IO;
//using System.Windows.Forms;

//namespace PlanIT2
//{
//    public class NewFileForm : Form
//    {
//        private string username;
//        private TextBox fileNameBox;
//        private TextBox contentBox;

//        public NewFileForm(string user)
//        {
//            username = user;

//            this.Text = "Create New File";
//            this.Size = new System.Drawing.Size(500, 400);
//            this.StartPosition = FormStartPosition.CenterParent;

//            Label fileLabel = new Label() { Text = "File Name:", Top = 20, Left = 20, Width = 100 };
//            fileNameBox = new TextBox() { Top = 20, Left = 130, Width = 300 };

//            Label contentLabel = new Label() { Text = "Content:", Top = 60, Left = 20, Width = 100 };
//            contentBox = new TextBox() { Top = 60, Left = 130, Width = 300, Height = 200, Multiline = true, ScrollBars = ScrollBars.Vertical };

//            Button saveButton = new Button() { Text = "Save File", Top = 280, Left = 130, Width = 150 };
//            saveButton.Click += SaveButton_Click;

//            this.Controls.Add(fileLabel);
//            this.Controls.Add(fileNameBox);
//            this.Controls.Add(contentLabel);
//            this.Controls.Add(contentBox);
//            this.Controls.Add(saveButton);
//        }

//        private void SaveButton_Click(object sender, EventArgs e)
//        {
//            string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PlanIT", "Users", username);
//            Directory.CreateDirectory(userFolder);

//            string filePath = Path.Combine(userFolder, fileNameBox.Text + ".txt");
//            File.WriteAllText(filePath, contentBox.Text);

//            MessageBox.Show("File saved successfully!");
//            this.Close();
//        }
//    }
//}
