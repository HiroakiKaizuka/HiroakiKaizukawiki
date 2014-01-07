using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Threading;


namespace DaruMA_project
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Kinectを扱うためのオブジェクト
        /// </summary>
        KinectSensor sensor;

        /// <summary>
        /// Kinectセンサーからの画像情報を受け取る
        /// </summary>
        //追加事項
        private byte[] colorPixels;

        /// <summary>
        /// 画面に表示するビットマップ
        /// </summary>
        //追加事項
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// 「だるまさんが転んだ」を再生するタイマー
        /// </summary>
        private DispatcherTimer playTime;

        /// <summary>
        /// 0.5秒ごとに判定するタイマー
        /// </summary>
        private DispatcherTimer checkTime;

        /// <summary>
        /// 音声の再生経過を追う変数
        /// </summary>
        private int soundTimer;

        /// <summary>
        /// playDaruMAオブジェクト
        /// </summary>
        private playDaruMA playDaruMA;

        /// <summary>
        /// 判定の経過を追う変数
        /// </summary>
        private int checkTimer;

        /// <summary>
        /// 秒数の定義
        /// </summary>
        private const long sec = 10000000;   // 1秒 = 10000000 * 100 ナノ秒
        private const long halfsec = 5000000;

        /// <summary>
        /// 再生する音声をランダムに決める
        /// </summary>
        private Random rand;

        /// <summary>
        /// ランダムに決めた数値の保持変数
        /// </summary>
        private int fileno;
        /// <summary>
        /// {音声の秒数}
        /// </summary>
        private int[] p_daruma = new int[] {3,2,3,4,3,3};

        /// <summary>
        /// プレイヤー
        /// </summary>
        private Player[] player;

        /// <summary>
        /// 骨格情報割り当て用フラグ/ スケルトンデータを格納したかの判定フラグ
        /// </summary> 
        private bool[] set_flag;

        /// <summary>
        /// プレイヤーの腰の位置を格納する変数
        /// </summary>
        private DepthImagePoint[] hip;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 画面がロードされたときに呼び出される。
        /// 初期化の処理はここに記入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            #region 接続の確認
            //Kinectセンサーの接続を確認
            //接続が確認されたセンサーがあれば
            //それを扱うことにして処理を抜ける。
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            #endregion

            #region 接続の有無による分岐
            //Kinectが確認できなかったら
            if (null == this.sensor)
            {

            }

            //Kinecが接続されたときの処理
            if (null != this.sensor)
            {
                //RGBカメラの使用。カラーストリームの準備
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                //距離カメラの使用。デプスストリームの準備
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                //骨格情報の使用。スケルトンストリームの準備
                this.sensor.SkeletonStream.Enable();

                //全てのイベントを一括で処理
                this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);

                #region カラー情報の初期化
                //バッファの初期化
                colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth,
                                                  sensor.ColorStream.FrameHeight,
                                                  96.0, 96.0, PixelFormats.Bgr32, null);
                this.MainImage.Source = colorBitmap;

                #endregion

                //センサー作動
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
            #endregion

            #region プレイの処理をする準備
            playTime = new DispatcherTimer();   
            playTime.Tick += new EventHandler(playTime_Tick);
            playTime.Interval = new TimeSpan(sec);  //1秒ごとに呼び出す
            soundTimer = 0; //1秒ごとにカウントアップされて、音声の秒数を超えたかをチェック
            rand = new Random();
            fileno = rand.Next(0,5); //音声ファイルを6種類用意
            playDaruMA = new playDaruMA();
            playTime.Start();
            #endregion

            #region 判定の処理をする準備
            checkTime = new DispatcherTimer();
            checkTime.Tick += new EventHandler(checkTime_Tick);
            checkTime.Interval = new TimeSpan(halfsec); //0.5秒ごとに呼び出される。
            checkTimer = 0; //0.5秒ごとにカウントアップされる。カウント6で判定終了。
            #endregion

            #region プレイヤーの初期化
            player = new Player[2];
            player[0] = new Player();    // プレイヤー1
            player[1] = new Player();    // プレイヤー2
            set_flag = new bool[2];
            for (int i = 0; i < 2; i++) { set_flag[i] = false; }
            #endregion

            //頭の位置を確保する配列宣言
            hip = new DepthImagePoint[2];     
        }

        /// <summary>
        /// 判定処理を行うための関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkTime_Tick(object sender, EventArgs e)
        {
            /*判定モードに入ったら*/
            if (checkTimer == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    /*プレイヤーが識別出来てるのなら*/
                    if (player[i].State == SkeletonTrackingState.Tracked)
                    {
                        /*判定処理に入った直後のプレイヤーのボーンデータを保持*/
                        player[i].setprvSkl();
                    }
                }
            }

            /*動いたかどうか判定するよ！*/
            for (int i = 0; i < 2; i++)
            {
                /* 保持してるボーンデータと現在のボーンデータを比較する
                 * スケルトンの座標データはfloat型で扱っているので、比較はfloatデータで処理する
                 * 現在は頭、左手、右手のどれかが0.05以上動くか、合計で0.1以上動いた時何か動いたと判定する*/
                if (player[i].judge() == false)
                {
                    /*動いた時の処理を記述してね！
                     * 現在はコンソールに出力してるよ！*/
                    player[i].gameOver();
                    player[i].playerReset();
                    switch (i)
                    {
                        case 0: this.Player1Point.Visibility = System.Windows.Visibility.Hidden; 
                                this.GameOverImage1.Visibility = System.Windows.Visibility.Visible;
                                break;
                        case 1: this.Player2Point.Visibility = System.Windows.Visibility.Hidden; 
                                this.GameOverImage2.Visibility = System.Windows.Visibility.Visible;
                                break;
                        default: break;
                    }
                }
            }

            checkTimer++;   //この関数を通った回数をカウント
            if (checkTimer >= 6)
            {//カウントが6以上(=3秒以上)になったら
                for (int i = 0; i < 2; i++)
                {
                    /* 現在保持されている比較用のスケルトンデータを削除する
                     * この処理が無いと、次の判定処理入った時にプレイヤーを識別できていなくても
                     * 保存したままのスケルトンデータで比較してしまい、誤作動してしまうと考えられる*/
                    player[i].delSkl();
                }
                checkTime.Stop();   //この関数の処理を停止
                checkTimer = 0;     //次の判定に向けて変数を初期化
                playTime.Start();   //プレイ処理を開始する
            }
        }


        /// <summary>
        /// 動けるフェーズの時の処理を記述
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void playTime_Tick(object sender, EventArgs e)
        {
            if (soundTimer == 0)
            {//まだ音声が再生されていない場合
                playDaruMA.play(fileno); //ランダムなファイルを再生
                this.GameOverImage1.Visibility = System.Windows.Visibility.Hidden;
                this.GameOverImage2.Visibility = System.Windows.Visibility.Hidden;
            }
            
            //ポイントの処理
            test();

            soundTimer++;   //この関数を通ったことをカウントすることで秒数をカウント

            if (soundTimer >= p_daruma[fileno])
            {//音声が再生し終わった場合
                playTime.Stop();    //この関数の処理を停止
                soundTimer = 0;     //次の音に向けて変数を初期化
                fileno = rand.Next(0, 5);   //次のファイルをランダムに決定
                checkTime.Start();  //判定を開始する
            }

        }

        /// <summary>
        /// Kinectセンサーのすべての処理をここに記述
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            #region Color系の処理
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    //画像情報の幅・高さ取得
                    int frmWidth = imageFrame.Width;
                    int frmHeight = imageFrame.Height;

                    //ソースの指定
                    Int32Rect src = new Int32Rect(0, 0, frmWidth, frmHeight);
                    //画像情報をバッファにコピー
                    imageFrame.CopyPixelDataTo(colorPixels);
                    //ビットマップに描画
                    colorBitmap.WritePixels(src, colorPixels, frmWidth * 4, 0);
                }
            }
            #endregion

            #region Skeleton系の処理
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            if (skeletons.Length != 0)
            {
                // フラグ切り替え（プレイヤー2がいる状況でプレイヤー1が抜けた場合：F その他：T）
                //if ((player[0].getState() == SkeletonTrackingState.NotTracked) && (player[1].getState() == SkeletonTrackingState.Tracked))
                //{
                //    this.flagSkl = false;
                //}
                //else
                //{
                //    this.flagSkl = true;
                //}

                // 追跡状態の初期化
                //for (int i = 0; i < 2; i++)
                //{
                //    player[i].setState(SkeletonTrackingState.NotTracked);
                //}

                foreach (Skeleton skl in skeletons)
                {
                    /*プレイヤーを検出している*/
                    if (skl.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            //先にPlay_flagをfalseにしてtrueならあとで書き換わるはず。
                            player[i].Play_flag = false;
                            if (player[i].ID == skl.TrackingId) //プレイヤーIDとスケルトンのトラッキングIDが等しいなら
                            {
                                //同じプレイヤーと判断しスケルトン情報を格納
                                player[i].Skl = skl;
                                set_flag[i] = true;
                                player[i].Play_flag = true;
                                player[i].State = SkeletonTrackingState.Tracked;
                                break;
                            }
                            else if (player[i].ID == 0)         //初検出の場合
                            {
                                /* 現在のスケルトンがプレイヤー2の物の場合があるので、
                                 * プレイヤー2が検出されているかを判定*/
                                if (i == 0 && player[i + 1].ID != skl.TrackingId)
                                    player[i].Skl = skl;
                                player[i].ID = skl.TrackingId;
                                player[i].Play_flag = true;
                                player[i].State = SkeletonTrackingState.Tracked;
                                set_flag[i] = true;
                                break;
                            }

                        }
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    //if (player[i].Play_flag == true) { player[i].Play_flag = false; }
                    //else 
                    if (player[i].Play_flag == false) { player[i].playerReset(); }
                }
            }
            #endregion

            #region Depth系の処理
            /*using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {//ラベルを腰に表示する処理です。
                for (int i = 0; i < 2; i++)
                {
                    hip[i] = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(player[i].Skl.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                    switch (i)
                    {
                        case 0: this.Player1Point.Margin = new Thickness((double)hip[i].X, (double)hip[i].Y, 0.0, 0.0); break;
                        case 1: this.Player2Point.Margin = new Thickness((double)hip[i].X, (double)hip[i].Y, 0.0, 0.0); break;
                        default: break;
                    }
                    if (player[i].Play_flag == false)
                    {
                        //プレイフラグが偽ならゲームオーバーとして腰に×を表示
                        switch (i)
                        {
                            case 0: this.GameOverImage1.Margin = new Thickness((double)hip[i].X, (double)hip[i].Y, 0.0, 0.0); break;
                            case 1: this.GameOverImage1.Margin = new Thickness((double)hip[i].X, (double)hip[i].Y, 0.0, 0.0); break;
                        }
                    }
                }
            }*/
            #endregion            
        }

        void test()
        {
            // 判定処理
            for (int i = 0; i < 2; i++)
            {
                // ゲームプレイ判定
                if (player[i].Play_flag == true)
                {
                    // シャトルランの判定(ポイント、進行方向の変更)
                    player[i].checkPoint();
                }
            }

            // 表示処理(仮) テスト用の仮表示です
            if (player[0].State == SkeletonTrackingState.Tracked)
            {
                Player1Point.Content = player[0].Point;
                Player1Point.Visibility = System.Windows.Visibility.Visible;
            }
            else if(player[0].State == SkeletonTrackingState.NotTracked)
            {
                player[0].gameOver();
                Player1Point.Visibility = System.Windows.Visibility.Hidden;
            }
            if (player[1].State == SkeletonTrackingState.Tracked)
            {
                Player2Point.Content = player[1].Point;
                Player2Point.Visibility = System.Windows.Visibility.Visible;
            }
            else if(player[1].State == SkeletonTrackingState.NotTracked)
            {
                player[1].gameOver();
                Player2Point.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
