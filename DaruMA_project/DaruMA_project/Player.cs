/*だるまさんがころんだ　プレイヤークラス
                        担当：1123060 常盤*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DaruMA_project
{
    /// <summary>
    /// プレイヤークラス
    /// </summary>
    class Player
    {
        private int point;          // ポイント
        private int id;             //TrackingIDからプレイヤーの割り振り
        private Boolean dir_flag;   // 進行方向フラグ
        private Boolean play_flag;  // プレイ状態用フラグ
        Skeleton skl;               // 骨格情報
        SkeletonTrackingState state;// 追跡状態
        JudgeClass jc;              // 判定処理クラス
        CheckClass cc;              // シャトルラン処理用

        public Player()
        {
            this.point = 0;
            this.dir_flag = false;
            this.play_flag = false;
            this.skl = new Skeleton();
            this.id = 0;
            this.state = SkeletonTrackingState.NotTracked;
            this.jc = new JudgeClass();
            this.cc = new CheckClass();
        }

        #region アクセサ

        public int Point
        {
            get { return this.point; }
            set { this.point = value; }
        }

        public Boolean Dir_flag
        {
            get { return this.dir_flag; }
            set { this.dir_flag = value; }
        }

        public Boolean Play_flag
        {
            get { return this.play_flag; }
            set { this.play_flag = value; }
        }

        public Skeleton Skl
        {
            get { return this.skl; }
            set { this.skl = value; }
        }

        public SkeletonTrackingState State
        {
            get { return skl.TrackingState; }
            set { this.state = value; }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        #endregion

        #region getter
        // 各種ゲッター
        //public int getPoint()
        //{
        //    return this.point;
        //}

        //public Boolean getDir_flag()
        //{
        //    return this.dir_flag;
        //}

        //public Boolean getPlay_flag()
        //{
        //    return this.play_flag;
        //}

        //public Skeleton getSkl()
        //{
        //    return this.skl;
        //}

        //public SkeletonTrackingState getState()
        //{
        //    return this.state;
        //}

        //public int getPlayerID()
        //{
        //    return this.skl.TrackingId;
        //}
        //#endregion


        //#region setter
        //// 各種セッター
        //public void setPoint(int p)
        //{
        //    this.point = p;
        //}

        //public void setDir_flag(Boolean df)
        //{
        //    this.dir_flag = df;
        //}

        //public void setPlay_flag(Boolean pf)
        //{
        //    this.play_flag = pf;
        //}

        //public void setSkl(Skeleton s)
        //{
        //    this.skl = s;
        //}

        //public void setState(SkeletonTrackingState st)
        //{
        //    this.state = st;
        //}
        #endregion

        #region 初期化
        public void playerReset()
        {
            this.Point = 0;
            this.Dir_flag = false;
            this.Play_flag = false;
            this.Skl = new Skeleton();
            this.ID = 0;
            this.state = SkeletonTrackingState.NotTracked;
        }
        #endregion

        #region JudgeClass関連

        #region 判定用のスケルトン保存

        public void setprvSkl()
        {
            jc.Set_Skel(skl);
        }

        #endregion

        #region スケルトンの判定処理

        public bool judge()
        {
            return jc.Judge_Skel(skl);
        }

        #endregion

        #region 比較用スケルトンデータの削除
        public void delSkl()
        {
            jc.Reset_Skel();
        }
        #endregion

        #endregion

        #region CheckClass関連

        #region ポイント/進行方向の処理
        
        public void checkPoint()
        {
            this.Point = cc.addPoint(this.skl, this.point, this.dir_flag);
            this.Dir_flag = cc.changeDir(this.skl, this.dir_flag);
        }

        #endregion

        #region ゲームオーバーの処理

        public void gameOver()
        {
            this.play_flag = false;
            this.State = SkeletonTrackingState.NotTracked;
        }

        #endregion

        #endregion
    }
}