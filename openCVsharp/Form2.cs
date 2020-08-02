using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;

namespace openCVsharp
{
    public partial class Form2 : Form
    {
        public Form1 form1;
        public string Path_data { set; get; }
        public double W_rate { set; get; }
        public double Uh_rate { set; get; }
        public double Ratio { set; get; }
        public int Minsize_w { set; get; }
        public int Maxsize_w { set; get; }
        public float Scale_factor { set; get; }
        public int Min_neighbors { set; get; }
        public string[] Array { set; get; }
        public string CurDir { set; get; }
        public FolderBrowserDialog FbDialog { set; get; }
        public int Array_Length { set; get; }
        public int J { set; get; }
        private string judge;

        public Form2()
        {
            InitializeComponent();
        }

        ////フォームのLoadイベントハンドラ
        private void Form1_Load(object sender, EventArgs e)
        {
            {
                //処理が行われているときは、何もしない
                if (backgroundWorker1.IsBusy)
                    return;

                //Button2を有効にする
                button1.Enabled = true;

                //コントロールを初期化する
                progressBar1.Minimum = 0;
                progressBar1.Maximum = Array_Length;
                progressBar1.Value = 0;
                label1.Text = "0";

                //BackgroundWorkerのProgressChangedイベントが発生するようにする
                backgroundWorker1.WorkerReportsProgress = true;
                //キャンセルできるようにする
                backgroundWorker1.WorkerSupportsCancellation = true;
                //DoWorkで取得できるパラメータ(Array_Length)を指定して、処理を開始する
                backgroundWorker1.RunWorkerAsync(Array_Length);

            }
        }


        //Button1のClickイベントハンドラ
        public void Button1_Click(object sender, EventArgs e)
        {
            //Button1を無効にする
            button1.Enabled = false;

            //キャンセルする
            backgroundWorker1.CancelAsync();
        }

        //BackgroundWorker1のDoWorkイベントハンドラ
        //ここで時間のかかる処理を行う
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = (BackgroundWorker)sender;

            //パラメータを取得する
            int maxLoops = (int)e.Argument;
            int j = 1;

            //時間のかかる処理を開始する
            for (int i = 0; i < maxLoops; i++)
            {
                //キャンセルされたか調べる
                if (bgWorker.CancellationPending)
                {
                    //キャンセルされたとき
                    e.Cancel = true;
                    return;
                }
                string newfile = System.IO.Path.Combine(Path_data, "/", Array[i]);
                string ext = System.IO.Path.GetExtension(newfile);
                W_rate = W_rate;
                Uh_rate = Uh_rate;
                Ratio = Ratio;
                Minsize_w = Minsize_w;
                Maxsize_w = Maxsize_w;
                Scale_factor = Scale_factor;
                Min_neighbors = Min_neighbors;
                if ((string.Compare(ext, ".jpg", true) == 0) ||
                    (string.Compare(ext, ".png", true) == 0))
                {
                    Bitmap bmpOrg = Image.FromFile(newfile) as Bitmap;
                    int w = bmpOrg.Width;
                    int h = bmpOrg.Height;
                    using (Mat mat = new Mat(newfile))
                    {
                        // 分類機の用意
                        using (CascadeClassifier cascade = new CascadeClassifier(CurDir + @"\haarcascade_frontalface_default.xml"))
                        {
                            OpenCvSharp.Size minsize = new OpenCvSharp.Size(Minsize_w, Minsize_w);
                            OpenCvSharp.Size maxsize = new OpenCvSharp.Size(Maxsize_w, Maxsize_w);
                            foreach (Rect rectFace in cascade.DetectMultiScale(mat, Scale_factor, Min_neighbors, 0, minsize, maxsize))
                            {
                                int trimedWidth = (int)(rectFace.Width * W_rate);
                                int trimedHeight = (int)(trimedWidth * Ratio);
                                int partOftrimedHeight = (int)(rectFace.Height * Uh_rate);
                                int dx = (rectFace.Width - trimedWidth) / 2;
                                int dy = (rectFace.Height - partOftrimedHeight) / 2;
                                string err = "";
                                judge = "成功";

                                // 認識した顔の右余白が足りない
                                if ((rectFace.X + dx + trimedWidth) > w)
                                {
                                    dx = w - rectFace.X - trimedWidth;
                                    err += "_右";
                                    judge = "失敗";
                                }
                                // 認識した顔の左余白が足りない
                                if (rectFace.X + dx < 0)
                                {
                                    dx = -rectFace.X;
                                    err += "_左";
                                    judge = "失敗";
                                }
                                // 認識した顔の下余白が足りない
                                if ((rectFace.Y + dy + (int)(trimedWidth * Ratio)) > h)
                                {
                                    dy = h - rectFace.Y - (int)(trimedWidth * Ratio);
                                    err += "_下";
                                    judge = "失敗";
                                }
                                // 認識した顔の上余白が足りない
                                if (rectFace.Y + dy < 0)
                                {
                                    dy = -rectFace.Y;
                                    err += "_上";
                                    judge = "失敗";
                                }
                                // 設定通りの顔幅の比率にできない
                                if (trimedWidth > w)
                                {
                                    //dx = 0;
                                    trimedWidth = w;
                                    err += "_幅";
                                    judge = "失敗";
                                }
                                // 設定通りの縦横比にできない
                                if (trimedHeight > h)
                                {
                                    trimedHeight = h;
                                    err += "_比";
                                    judge = "失敗";
                                }
                                Rect rect = new Rect(rectFace.X + dx, rectFace.Y + dy, trimedWidth, trimedHeight);
                                Mat clipedMat = mat.Clone(rect);
                                _ = Cv2.ImWrite(Path_data + "/" + judge + "/" + j++.ToString("D2") + err + ext, clipedMat);
                                clipedMat.Dispose();
                                J = j;
                            }
                        }
                    }
                }
                bgWorker.ReportProgress(i + 1);
            }
            //ProgressChangedで取得できる結果を設定する
            e.Result = maxLoops;
        }

        //BackgroundWorker1のProgressChangedイベントハンドラ
        //コントロールの操作は必ずここで行い、DoWorkでは絶対にしない
        private void BackgroundWorker1_ProgressChanged(
            object sender, ProgressChangedEventArgs e)
        {
            //ProgressBar1の値を変更する
            progressBar1.Value = e.ProgressPercentage;
            //Label1のテキストを変更する
            label1.Text = e.ProgressPercentage.ToString() + "つ目の画像ファイルを処理";
        }

        //BackgroundWorker1のRunWorkerCompletedイベントハンドラ
        //処理が終わったときに呼び出される
        private void BackgroundWorker1_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //エラーが発生したとき
                _ = MessageBox.Show("エラー:" + e.Error.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Cancelled)
            {
                //キャンセルされたとき
                _ = MessageBox.Show("キャンセルされました。", "キャンセル", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //正常に終了したとき
                int result = (int)e.Result;
                _ = MessageBox.Show(result.ToString() + "つの画像ファイルから" + (J - 1).ToString() + "個の顔を検出しました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            Close();
            form1.Close();
            _ = System.Diagnostics.Process.Start("EXPLORER.EXE", Path_data);
        }
    }
}
