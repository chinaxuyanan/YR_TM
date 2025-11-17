using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_TM.Utils;

namespace YR_TM.View
{
    public partial class PageLog : UserControl
    {
        private SplitContainer splitMain;
        private SplitContainer splitRight;
        private TreeView treeDirs;
        private ListView listFiles;
        private RichTextBox richPreview;
        private Panel panelTools;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;

        public PageLog()
        {
            InitializeComponent();
            InitializeUI();

            this.Load += PageLog_Load;

            LoadRootDirectory();
        }

        private void PageLog_Load(object sender, EventArgs e)
        {
            int totalWidth = this.ClientSize.Width;

            splitMain.SplitterDistance = (int)(totalWidth * 0.25);
            splitRight.SplitterDistance = (int)(totalWidth * 0.35);
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 50, 55);

            // 主分割容器（左侧目录树 + 右侧内容区）
            splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 250, // 左侧目录宽度
                BackColor = Color.FromArgb(60, 65, 70)
            };
            this.Controls.Add(splitMain);

            // 右侧再分割（文件列表 + 预览）
            splitRight = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 400, // 中间列表宽度
                BackColor = Color.FromArgb(60, 65, 70)
            };
            splitMain.Panel2.Controls.Add(splitRight);

            // 左侧 TreeView
            treeDirs = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 55, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            treeDirs.AfterSelect += TreeDirs_AfterSelect;
            splitMain.Panel1.Controls.Add(treeDirs);

            // 中间文件列表
            listFiles = new ListView
            {
                Dock = DockStyle.Fill,
                View = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = Color.FromArgb(55, 60, 65),
                ForeColor = Color.White
            };
            listFiles.Columns.Add(LanguageManager.GetString("FileName_Text"), 250);
            listFiles.Columns.Add(LanguageManager.GetString("ModificationTime_Text"), 150);
            listFiles.SelectedIndexChanged += ListFiles_SelectedIndexChanged;
            splitRight.Panel1.Controls.Add(listFiles);

            // 日志预览区
            richPreview = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 35),
                ForeColor = Color.White,
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            splitRight.Panel2.Controls.Add(richPreview);

            // 右侧顶部工具栏
            panelTools = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(40, 45, 50)
            };
            splitRight.Panel2.Controls.Add(panelTools);

            txtSearch = new TextBox
            {
                Width = 200,
                Location = new Point(10, 8),
                ForeColor = Color.Black
            };
            panelTools.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = LanguageManager.GetString("Btn_Query"),
                Location = new Point(220, 6),
                Width = 80,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 130, 180)
            };
            btnSearch.Click += BtnSearch_Click;
            panelTools.Controls.Add(btnSearch);

            btnRefresh = new Button
            {
                Text = LanguageManager.GetString("Btn_Refresh"),
                Location = new Point(310, 6),
                Width = 80,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 130, 180)
            };
            btnRefresh.Click += BtnRefresh_Click;
            panelTools.Controls.Add(btnRefresh);
        }

        private void LoadRootDirectory()
        {
            // 可修改路径为你的日志目录
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            var rootNode = new TreeNode("日志文件夹") { Tag = rootPath };
            treeDirs.Nodes.Add(rootNode);
            LoadDirectories(rootNode);
        }

        private void LoadDirectories(TreeNode node)
        {
            string path = node.Tag.ToString();
            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    var subNode = new TreeNode(Path.GetFileName(dir)) { Tag = dir };
                    node.Nodes.Add(subNode);
                    LoadDirectories(subNode);
                }
            }
            catch { }
        }

        private void TreeDirs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedPath = e.Node.Tag.ToString();
            LoadLogFiles(selectedPath);
        }

        private void LoadLogFiles(string folder)
        {
            listFiles.Items.Clear();
            foreach (var file in Directory.GetFiles(folder, "*.log"))
            {
                var info = new FileInfo(file);
                var item = new ListViewItem(info.Name);
                item.SubItems.Add(info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                item.Tag = file;
                listFiles.Items.Add(item);
            }
        }

        private void ListFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listFiles.SelectedItems.Count == 0) return;
            string path = listFiles.SelectedItems[0].Tag.ToString();

            try
            {
                //共享读取方式打开文件（允许其他进程写入）
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    richPreview.Text = reader.ReadToEnd();
                }
            }
            catch(IOException ex)
            {
                MessageBox.Show($"无法读取文件：{Path.GetFileName(path)} \n原因：{ex.Message}", "文件被占用", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"读取日志文件时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            int startIndex = 0;
            richPreview.SelectAll();
            richPreview.SelectionBackColor = Color.Transparent;

            while (startIndex < richPreview.TextLength)
            {
                int index = richPreview.Find(keyword, startIndex, RichTextBoxFinds.None);
                if (index == -1) break;

                richPreview.Select(index, keyword.Length);
                richPreview.SelectionBackColor = Color.Yellow;
                startIndex = index + keyword.Length;
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (treeDirs.SelectedNode != null)
            {
                string path = treeDirs.SelectedNode.Tag.ToString();
                LoadLogFiles(path);
            }
        }
    }
}
