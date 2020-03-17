using System;
using System.IO;
using System.Windows.Forms;

namespace openCVsharp
{
    public partial class Form1 : Form
    {
        private double w_rate;      // トリミング幅 / 顔幅
        private double uh_rate;     // 上部のトリミング高さ / 顔高さ
        private double ratio;       // トリミングした画像の縦 / 横 比
        private int minsize_w;      // minSizeの一辺
        private int maxsize_w;      // maxSizeの一辺
        private float scale_factor; // scale factor
        private int min_neighbors;  // min neighbors

        private readonly string curDir = Directory.GetCurrentDirectory();
        private readonly Form2 form2;

        public Form1()
        {
            InitializeComponent();
            form2 = new Form2
            {
                form1 = this
            };
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
                string[] array = Directory.GetFiles(path);
                int array_length = array.Length;
                Directory.CreateDirectory(path + "/成功");

                w_rate = double.Parse(textBox1.Text);      // トリミング幅 / 顔幅
                uh_rate = double.Parse(textBox2.Text);     // 上部のトリミング高さ / 顔高さ
                ratio = double.Parse(textBox3.Text);       // トリミングした画像の縦 / 横 比
                minsize_w = int.Parse(textBox4.Text);      // minSizeの一辺
                maxsize_w = int.Parse(textBox6.Text);      // maxSizeの一辺
                scale_factor = float.Parse(textBox7.Text); // scale factor
                min_neighbors = int.Parse(textBox8.Text);  // min neighbors

                form2.Path_data = path;
                form2.W_rate = w_rate;
                form2.Uh_rate = uh_rate;
                form2.Ratio = ratio;
                form2.Minsize_w = minsize_w;
                form2.Maxsize_w = maxsize_w;
                form2.Scale_factor = scale_factor;
                form2.Min_neighbors = min_neighbors;
                form2.CurDir = curDir;
                form2.FbDialog = fbDialog;
                form2.Array = array;
                form2.Array_Length = array_length;

                form2.Show();

            }
            else
            {
                _ = MessageBox.Show("トリミングしようとする画像のあるフォルダを選択してください", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            fbDialog.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.w_rate;
            textBox2.Text = Properties.Settings.Default.uh_rate;
            textBox3.Text = Properties.Settings.Default.ratio;
            textBox4.Text = Properties.Settings.Default.minsize_w;
            textBox5.Text = Properties.Settings.Default.Address;
            textBox6.Text = Properties.Settings.Default.maxsize_w;
            textBox7.Text = Properties.Settings.Default.scale;
            textBox8.Text = Properties.Settings.Default.minNeighbors;
            PictureSet();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.w_rate = textBox1.Text;
            Properties.Settings.Default.uh_rate = textBox2.Text;
            Properties.Settings.Default.ratio = textBox3.Text;
            Properties.Settings.Default.minsize_w = textBox4.Text;
            Properties.Settings.Default.Address = textBox5.Text;
            Properties.Settings.Default.maxsize_w = textBox6.Text;
            Properties.Settings.Default.scale = textBox7.Text;
            Properties.Settings.Default.minNeighbors = textBox8.Text;

            Properties.Settings.Default.Save();
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
                int face_w = 90;    // 顔幅のピクセル数
                int form_w = 489;   // form1の幅
                int form_h = 265;   // form1の高さ
                int panel_w = 123;  // panel1の高さ
                int panel_h = 210;  // panel1の最大高さ
                int panel_x = 343;  // panel1のx座標
                int panel_y = 6;    // panel1のy座標
                int pic_w = 138;    // picture1の幅
                int pic_h = 221;    // picture1の高さ
                int gap = 75;       // picture1上端と顔上端の差
                panel1.Size = new System.Drawing.Size((int)(face_w * t1), (int)(face_w * t1 * t3));
                panel1.Location = new System.Drawing.Point(panel_x, panel_y);
                pictureBox1.Size = new System.Drawing.Size((int)(face_w * t1), pic_h);
                pictureBox1.Location = new System.Drawing.Point((int)(panel_w - pic_w) / 2, (int)((face_w / 2 * t2) - gap));
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

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox7.ReadOnly = false;
                textBox8.ReadOnly = false;
            }
            else
            {
                textBox7.ReadOnly = true;
                textBox8.ReadOnly = true;
            }
        }
    }
}