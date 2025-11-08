//using System;
//using System.IO;
//using System.Windows.Forms;

//namespace PlanIT2
//{
//    public class LoadFileForm : Form
//    {
//        private string username;
//        private ListBox fileList;
//        private TextBox fileContent;

//        public LoadFileForm(string user)
//        {
//            username = user;

//            this.Text = "Load Files";
//            this.Size = new System.Drawing.Size(600, 400);
//            this.StartPosition = FormStartPosition.CenterParent;

//            fileList = new ListBox() { Left = 20, Top = 20, Width = 200, Height = 300 };
//            fileList.SelectedIndexChanged += FileList_SelectedIndexChanged;

//            fileContent = new TextBox() { Left = 240, Top = 20, Width = 320, Height = 300, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };

//            this.Controls.Add(fileList);
//            this.Controls.Add(fileContent);

//            LoadUserFiles();
//        }

//        private void LoadUserFiles()
//        {
//            string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PlanIT", "Users", username);
//            Directory.CreateDirectory(userFolder);

//            string[] files = Directory.GetFiles(userFolder, "*.txt");

//            fileList.Items.Clear();
//            foreach (string file in files)
//            {
//                fileList.Items.Add(Path.GetFileName(file));
//            }
//        }

//        private void FileList_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            if (fileList.SelectedItem != null)
//            {
//                string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PlanIT", "Users", username);
//                string filePath = Path.Combine(userFolder, fileList.SelectedItem.ToString());

//                fileContent.Text = File.ReadAllText(filePath);
//            }
//        }
//    }
//}
