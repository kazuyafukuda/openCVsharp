using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;

namespace OpenCVSharpSample
{
    public partial class Form1 : Form
    {
        private double w_rate;      //トリミング幅 / 顔幅
        private double uh_rate;     //上部のトリミング高さ / 顔高さ
        private double ratio;       //トリミングした画像の縦 / 横 比
        private int minsize_w;      //minSizeの一辺
        private int maxsize_w;      //maxSizeの一辺
        private float scale_factor; //scale factor
        private int min_neighbors;  //min neighbors

        readonly string curDir = Directory.GetCurrentDirectory();

        public Form1()
        {
            InitializeComponent();
        }

        [Obsolete]
        public void File_select_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbDialog = new FolderBrowserDialog
            {
                Description = "トリミングしようとする画像のあるフォルダを選択してください",
                SelectedPath = textBox5.Text,
                ShowNewFolderButton = false
            };

            if (fbDialog.ShowDialog() == DialogResult.OK)
            {
                string path = fbDialog.SelectedPath;
                textBox5.Text = path;
                int j = 1;
                string[] array = Directory.GetFiles(path);
                Directory.CreateDirectory(path + "/成功");

                try
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        string newfile = Path.Combine(path, "/", array[i]);
                        string ext = Path.GetExtension(newfile);
                        w_rate = double.Parse(textBox1.Text);
                        uh_rate = double.Parse(textBox2.Text);
                        ratio = double.Parse(textBox3.Text);
                        minsize_w = int.Parse(textBox4.Text);
                        maxsize_w = int.Parse(textBox6.Text);
                        scale_factor = float.Parse(textBox7.Text);
                        min_neighbors = int.Parse(textBox8.Text);
                        if ((string.Compare(ext, ".jpg", true) == 0) ||
                            (string.Compare(ext, ".png", true) == 0))
                        {
                            Bitmap bmpOrg = Image.FromFile(newfile) as Bitmap;
                            int w = bmpOrg.Width;
                            int h = bmpOrg.Height;
                            using (Mat mat = new Mat(newfile))
                            {
                                // 分類機の用意
                                using (CascadeClassifier cascade = new CascadeClassifier(curDir + @"\haarcascade_frontalface_default.xml"))
                                {
                                    OpenCvSharp.Size minsize = new OpenCvSharp.Size(minsize_w, minsize_w);
                                    OpenCvSharp.Size maxsize = new OpenCvSharp.Size(maxsize_w, maxsize_w);
                                    foreach (Rect rectFace in cascade.DetectMultiScale(mat, scale_factor, min_neighbors, 0, minsize, maxsize))
                                    {
                                        int px = (int)(rectFace.Width * (1 - w_rate) / 2);
                                        int py = (int)(rectFace.Height * (1 - uh_rate) / 2);
                                        if (rectFace.X + px < 0) { px = -rectFace.X; }
                                        if (rectFace.Y + py < 0) { py = -rectFace.Y; }
                                        if ((rectFace.X + px + (int)(rectFace.Width * w_rate)) > w)
                                        { px = w - rectFace.X - (int)(rectFace.Width * w_rate); }
                                        if ((rectFace.Y + py + (int)(rectFace.Width * w_rate * ratio)) > h)
                                        { py = h - rectFace.Y - (int)(rectFace.Width * w_rate * ratio); }
                                        Rect rect = new Rect(rectFace.X + px, rectFace.Y + py, (int)(rectFace.Width * w_rate), (int)(rectFace.Height * w_rate * ratio));
                                        Mat clipedMat = mat.Clone(rect);
                                        _ = Cv2.ImWrite(path + "/成功/" + j++.ToString() + ext, clipedMat);
                                    }
                                }
                            }
                        }
                    }
                    _ = MessageBox.Show((j - 1).ToString() + "個の顔を検出しました。\r\n「成功」フォルダの中にあります。\r\n但し，検出できなかったり誤検出した画像があります。\r\n後は，手作業でお願いします。", "成功", MessageBoxButtons.OK, MessageBoxIcon.None);
                    _ = System.Diagnostics.Process.Start("EXPLORER.EXE", path);
                        Close();
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("テキストボックスに適正な値を入力し直してください。", "注意", MessageBoxButtons.OK, MessageBoxIcon.None);
                    fbDialog.Dispose();
                }
                catch (OpenCVException)
                {
                    _ = MessageBox.Show("OpenCVExceptionが発生しました。\r\nこの後，表示する画像フォルダ，テキストボックスの入力値を確かめてください。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    _ = System.Diagnostics.Process.Start("EXPLORER.EXE", path);
                    Close();
                }
                catch (Exception)
                {
                    _ = MessageBox.Show("Exceptionが発生しました。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    _ = System.Diagnostics.Process.Start("EXPLORER.EXE", path);
                    Close();
                }
            }
            else
            {
                _ = MessageBox.Show("トリミングしようとする画像のあるフォルダを選択してください", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            fbDialog.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = openCVsharp.Properties.Settings.Default.w_rate;
            textBox2.Text = openCVsharp.Properties.Settings.Default.uh_rate;
            textBox3.Text = openCVsharp.Properties.Settings.Default.ratio;
            textBox4.Text = openCVsharp.Properties.Settings.Default.minsize_w;
            textBox5.Text = openCVsharp.Properties.Settings.Default.Address;
            textBox6.Text = openCVsharp.Properties.Settings.Default.maxsize_w;
            textBox7.Text = openCVsharp.Properties.Settings.Default.scale;
            textBox8.Text = openCVsharp.Properties.Settings.Default.minNeighbors;
            PictureSet();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            openCVsharp.Properties.Settings.Default.w_rate = textBox1.Text;
            openCVsharp.Properties.Settings.Default.uh_rate = textBox2.Text;
            openCVsharp.Properties.Settings.Default.ratio = textBox3.Text;
            openCVsharp.Properties.Settings.Default.minsize_w = textBox4.Text;
            openCVsharp.Properties.Settings.Default.Address = textBox5.Text;
            openCVsharp.Properties.Settings.Default.maxsize_w = textBox6.Text;
            openCVsharp.Properties.Settings.Default.scale = textBox7.Text;
            openCVsharp.Properties.Settings.Default.minNeighbors = textBox8.Text;

            openCVsharp.Properties.Settings.Default.Save();
        }

        private void TextBox1_Leave(object sender, EventArgs e)
        {
            PictureSet();
        }
        private void TextBox2_Leave(object sender, EventArgs e)
        {
            PictureSet();
        }
        private void TextBox3_Leave(object sender, EventArgs e)
        {
            PictureSet();
        }

        private void PictureSet()
        {
            if (double.TryParse(textBox1.Text, out double t1) & double.TryParse(textBox2.Text, out double t2) & double.TryParse(textBox3.Text, out double t3))
            {
                int face_w = 90;    //顔幅のピクセル数
                int form_w = 489;   //form1の幅
                int form_h = 265;   //form1の高さ
                int panel_w = 123;  //panel1の高さ
                int panel_h = 210;  //panel1の最大高さ
                int panel_x = 343;  //panel1のx座標
                int panel_y = 6;    //panel1のy座標
                int pic_w = 138;    //picture1の幅
                int pic_h = 221;    //picture1の高さ
                int gap = 75;       //picture1上端と顔上端の差
                panel1.Size = new System.Drawing.Size((int)(face_w * t1), (int)(face_w * t1 * t3));
                panel1.Location = new System.Drawing.Point(panel_x, panel_y);
                pictureBox1.Size = new System.Drawing.Size((int)(face_w * t1), pic_h);
                pictureBox1.Location = new System.Drawing.Point((int)(panel_w - pic_w)/2, (int)((face_w/2 * t2) - gap));
                Width = panel1.Size.Width > panel_w ? form_w - panel_w + (int)(face_w * t1) : form_w;
                Height = panel1.Size.Height > panel_h ? form_h - panel_h + (int)(face_w * t1 * t3) : form_h;
            }
        }

        [System.Security.Permissions.UIPermission(
            System.Security.Permissions.SecurityAction.Demand,
            Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            //Returnキーが押されているか調べる
            //AltかCtrlキーが押されている時は、本来の動作をさせる
            if (((keyData & Keys.KeyCode) == Keys.Return) &&
                ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
            {
                //Tabキーを押した時と同じ動作をさせる
                //Shiftキーが押されている時は、逆順にする
                _ = ProcessTabKey((keyData & Keys.Shift) != Keys.Shift);
                //本来の処理はさせない
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}