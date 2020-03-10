using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace openCVsharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, System.EventArgs e)
        {
            //イベントハンドラをイベントに関連付ける
            //フォームデザイナを使って関連付けを行った場合は、不要
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(BackgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);
        }

        //Button1のClickイベントハンドラ
        private void Button1_Click(object sender, System.EventArgs e)
        {
            //処理が行われているときは、何もしない
            if (backgroundWorker1.IsBusy)
                return;

            //Button1を無効にする
            button1.Enabled = false;
            //Button2を有効にする
            button2.Enabled = true;

            //コントロールを初期化する
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 10;
            progressBar1.Value = 0;
            label1.Text = "0";

            //BackgroundWorkerのProgressChangedイベントが発生するようにする
            backgroundWorker1.WorkerReportsProgress = true;
            //キャンセルできるようにする
            backgroundWorker1.WorkerSupportsCancellation = true;
            //DoWorkで取得できるパラメータ(10)を指定して、処理を開始する
            //パラメータが必要なければ省略できる
            backgroundWorker1.RunWorkerAsync(10);
        }

        //button2のClickイベントハンドラ
        private void Button2_Click(object sender, System.EventArgs e)
        {
            //Button2を無効にする
            button2.Enabled = false;

            //キャンセルする
            backgroundWorker1.CancelAsync();
        }

        //BackgroundWorker1のDoWorkイベントハンドラ
        //ここで時間のかかる処理を行う
        private void BackgroundWorker1_DoWork(
            object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = (BackgroundWorker)sender;

            //パラメータを取得する
            int maxLoops = (int)e.Argument;

            //時間のかかる処理を開始する
            for (int i = 1; i <= maxLoops; i++)
            {
                //キャンセルされたか調べる
                if (bgWorker.CancellationPending)
                {
                    //キャンセルされたとき
                    e.Cancel = true;
                    return;
                }

                //1秒間待機する（時間のかかる処理があるものとする）
                System.Threading.Thread.Sleep(1000);

                //ProgressChangedイベントハンドラを呼び出し、
                //コントロールの表示を変更する
                bgWorker.ReportProgress(i);
            }

            //ProgressChangedで取得できる結果を設定する
            //結果が必要なければ省略できる
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
            label1.Text = e.ProgressPercentage.ToString();
        }

        //BackgroundWorker1のRunWorkerCompletedイベントハンドラ
        //処理が終わったときに呼び出される
        private void BackgroundWorker1_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //エラーが発生したとき
                label1.Text = "エラー:" + e.Error.Message;
            }
            else if (e.Cancelled)
            {
                //キャンセルされたとき
                label1.Text = "キャンセルされました。";
            }
            else
            {
                //正常に終了したとき
                //結果を取得する
                int result = (int)e.Result;
                label1.Text = result.ToString() + "回で完了しました。";
            }

            //Button1を有効に戻す
            button1.Enabled = true;
            //Button2を無効に戻す
            button2.Enabled = false;
        }
    }
}
