using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YR_TM.Utils;

namespace YR_TM.View
{
    public partial class PageFile : UserControl
    {
        private TreeView treeDirs;
        private FlowLayoutPanel panelThumbs;
        private PictureBox picPreview;
        private Label lblFileInfo;

        private SplitContainer mainSplit;
        private SplitContainer rightSplit;

        public PageFile()
        {
            InitializeComponent();
            InitializeUI();

            this.Load += PageFile_Load;
        }

        private void PageFile_Load(object sender, EventArgs e)
        {
            int totalWidth = this.ClientSize.Width;

            mainSplit.SplitterDistance = (int)(totalWidth * 0.25);
            rightSplit.SplitterDistance = (int)(totalWidth * 0.35);
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(45, 50, 55);

            // 一级分割：左（目录） 右（文件与预览）
            mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                IsSplitterFixed = false,
                //Location = new Point(0, 60),
                BackColor = Color.FromArgb(45, 50, 55)
            };
            this.Controls.Add(mainSplit);
            mainSplit.BringToFront();

            // 二级分割：中（缩略图）右（预览）
            rightSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                //SplitterDistance = 500,
                SplitterWidth = 6,
                IsSplitterFixed = false,
                BackColor = Color.FromArgb(45, 50, 55)
            };
            mainSplit.Panel2.Controls.Add(rightSplit);

            // ===== 左侧目录树 =====
            treeDirs = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(55, 60, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            treeDirs.AfterSelect += TreeDirs_AfterSelect;
            mainSplit.Panel1.Controls.Add(treeDirs);

            // ===== 中间缩略图区 =====
            panelThumbs = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 65, 70),
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(10)
            };
            rightSplit.Panel1.Controls.Add(panelThumbs);

            // ===== 右侧预览区 =====
            picPreview = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 400,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(50, 55, 60)
            };
            lblFileInfo = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10),
                Text = "请选择图片..."
            };
            rightSplit.Panel2.Controls.Add(lblFileInfo);
            rightSplit.Panel2.Controls.Add(picPreview);

            // 顶部选择根目录按钮
            var btnSelectRoot = new Button
            {
                Text = LanguageManager.GetString("SelectDirectory_Text"),
                Width = 120,
                Height = 35,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSelectRoot.FlatAppearance.BorderSize = 0;
            btnSelectRoot.Click += BtnSelectRoot_Click;
            this.Controls.Add(btnSelectRoot);
        }

        private void BtnSelectRoot_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadDirectoryTree(dialog.SelectedPath);
            }
        }

        private void LoadDirectoryTree(string rootPath)
        {
            treeDirs.Nodes.Clear();
            var rootNode = new TreeNode(Path.GetFileName(rootPath)) { Tag = rootPath };
            treeDirs.Nodes.Add(rootNode);
            LoadSubDirs(rootNode);
            rootNode.Expand();
        }

        private void LoadSubDirs(TreeNode node)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(node.Tag.ToString());
                foreach (var dir in dirs)
                {
                    var subNode = new TreeNode(Path.GetFileName(dir)) { Tag = dir };
                    node.Nodes.Add(subNode);
                    // 只显示一层结构，用于日期展开
                    if (Directory.GetDirectories(dir).Any())
                        subNode.Nodes.Add("...");
                }
            }
            catch { /* ignore access denied */ }
        }

        private void TreeDirs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedPath = e.Node.Tag.ToString();
            LoadThumbnails(selectedPath);
        }

        //private void LoadThumbnails(string dir)
        //{
        //    panelThumbs.Controls.Clear();
        //    if (!Directory.Exists(dir)) return;

        //    var imgs = Directory.GetFiles(dir, "*.*")
        //                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
        //                                    || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
        //                                    || f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
        //                        .ToArray();

        //    foreach (var img in imgs)
        //    {
        //        var thumbPanel = new Panel
        //        {
        //            Width = 140,
        //            Height = 140,
        //            Margin = new Padding(8),
        //            BackColor = Color.FromArgb(70, 75, 80)
        //        };

        //        var pic = new PictureBox
        //        {
        //            Image = Image.FromFile(img),
        //            SizeMode = PictureBoxSizeMode.Zoom,
        //            Dock = DockStyle.Top,
        //            Height = 100,
        //            Cursor = Cursors.Hand,
        //            Tag = img
        //        };
        //        pic.Click += Pic_Click;

        //        var lbl = new Label
        //        {
        //            Text = Path.GetFileName(img),
        //            Dock = DockStyle.Bottom,
        //            ForeColor = Color.White,
        //            Font = new Font("Segoe UI", 8),
        //            TextAlign = ContentAlignment.MiddleCenter,
        //            Height = 30
        //        };

        //        thumbPanel.Controls.Add(lbl);
        //        thumbPanel.Controls.Add(pic);
        //        panelThumbs.Controls.Add(thumbPanel);
        //    }
        //}

        private CancellationTokenSource _thumbLoadCts;

        private void LoadThumbnails(string dir)
        {
            // 如果上一次加载还在进行，先取消
            _thumbLoadCts?.Cancel();
            _thumbLoadCts = new CancellationTokenSource();
            var token = _thumbLoadCts.Token;

            // 清除旧的控件并释放图片资源
            foreach (Control ctrl in panelThumbs.Controls)
            {
                if (ctrl is Panel p)
                {
                    foreach (Control c in p.Controls)
                    {
                        if (c is PictureBox pb && pb.Image != null)
                        {
                            pb.Image.Dispose();
                            pb.Image = null;
                        }
                    }
                }
            }
            panelThumbs.Controls.Clear();

            if (!Directory.Exists(dir)) return;

            var imgs = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (imgs.Count == 0) return;

            // 异步加载防止卡UI
            Task.Run(() =>
            {
                int index = 0;
                foreach (var img in imgs)
                {
                    if (token.IsCancellationRequested)
                        break;

                    // 生成缩略图
                    Image thumb = null;
                    try
                    {
                        using (var original = Image.FromFile(img))
                        {
                            int w = 100;
                            int h = (int)((double)original.Height / original.Width * w);
                            thumb = original.GetThumbnailImage(w, h, null, IntPtr.Zero);
                        }
                    }
                    catch
                    {
                        continue; // 图片损坏或加载失败
                    }

                    if (token.IsCancellationRequested)
                    {
                        thumb?.Dispose();
                        break;
                    }

                    // 回到主线程更新UI
                    panelThumbs.Invoke((Action)(() =>
                    {
                        var thumbPanel = new Panel
                        {
                            Width = 140,
                            Height = 140,
                            Margin = new Padding(8),
                            BackColor = Color.FromArgb(70, 75, 80)
                        };

                        var pic = new PictureBox
                        {
                            Image = thumb,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Dock = DockStyle.Top,
                            Height = 100,
                            Cursor = Cursors.Hand,
                            Tag = img
                        };
                        pic.Click += Pic_Click;

                        var lbl = new Label
                        {
                            Text = Path.GetFileName(img),
                            Dock = DockStyle.Bottom,
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 8),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Height = 30
                        };

                        thumbPanel.Controls.Add(lbl);
                        thumbPanel.Controls.Add(pic);
                        panelThumbs.Controls.Add(thumbPanel);

                        // 每加载50张强制刷新一次，防止UI延迟
                        if (index++ % 50 == 0)
                            Application.DoEvents();
                    }));
                }
            }, token);
        }


        private void Pic_Click(object sender, EventArgs e)
        {
            var pic = sender as PictureBox;
            string path = pic.Tag.ToString();
            picPreview.Image = Image.FromFile(path);

            var info = new FileInfo(path);
            lblFileInfo.Text = $"文件名：{info.Name}\n" +
                                $"路径：{info.FullName}\n" +
                                $"大小：{info.Length / 1024.0:F2} KB\n" +
                                $"创建时间：{info.CreationTime}";
        }
    }
}
