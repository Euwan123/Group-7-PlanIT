//using System;
//using System.Drawing;
//using System.Windows.Forms;
//using PlanIT2.Managers; // <-- ADD THIS

//namespace PlanIT2.TabTypes
//{
//    public class TextTabManager
//    {
//        // private Action MarkUnsaved; // <-- REMOVE: No longer needed for file saving
//        private readonly DatabaseManager _dbManager; // <-- ADD
//        private readonly Func<string> _getWorkspaceName; // <-- ADD to get current workspace key
//        private bool isDarkMode;
//        private Action<RichTextBox> UpdateWordCount;

//        // 1. UPDATE ALL CONSTRUCTORS
//        // Remove old MarkUnsaved constructors and replace them with database logic
//        public TextTabManager(DatabaseManager dbManager, Func<string> getWorkspaceName, Action<RichTextBox> updateWordCount, bool darkMode)
//        {
//            _dbManager = dbManager;
//            _getWorkspaceName = getWorkspaceName;
//            UpdateWordCount = updateWordCount;
//            isDarkMode = darkMode;
//        }

//        // Remove or update the two-parameter constructor if you still use it:
//        /*
//        public TextTabManager(Action markUnsaved, bool darkMode) 
//        {
//             // ...
//        }
//        */

//        public TabPage CreateTextTab(string name, string content = null)
//        {
//            TabPage tab = new TabPage(name);
//            // ... (setup code)

//            // 2. Add Tab Type to Tag for use in Save/Load
//            tab.Tag = "Text";

//            var richTextBox = new RichTextBox
//            {
//                // ... (setup code)
//                Text = content ?? string.Empty
//            };

//            richTextBox.TextChanged += (s, e) =>
//            {
//                // MarkUnsaved(); // <-- REMOVE the old file-based save call

//                // 3. ADD NEW DATABASE SAVE CALL
//                string tabName = tab.Text;
//                string tabType = (string)tab.Tag;
//                string workspaceName = _getWorkspaceName();

//                _dbManager.SaveTab(workspaceName, tabName, tabType, richTextBox.Text);

//                UpdateWordCount?.Invoke(richTextBox);
//            };

//            // ... (Rest of tab creation code)
//            return tab;
//        }

//        // ... (Keep the GetContent method, though it's now mostly unused)
//    }
//}