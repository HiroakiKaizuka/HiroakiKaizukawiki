/*だるまさんがころんだ　判定処理
                        担当：0923100 棚橋*/

#region using処理

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

#endregion

namespace DaruMA_project
{
    class JudgeClass
    {
        private Skeleton prev_skel; //比較用のスケルトン情報
        const float SINGLE_JUDGE = 0.3f;   //個別判定処理用変数
        const float TOTAL_JUDGE  = 0.5f;    //総合判定処理用変数

        /*コンストラクタ*/
        public JudgeClass()
        {
            this.prev_skel = new Skeleton();
        }

        /*スケルトン情報の記録*/
        public void Set_Skel(Skeleton s)
        {
            this.prev_skel = s;
        }

        #region 判定処理

        public bool Judge_Skel(Skeleton skel)
        {
            float HeadPos, LhandPos, RhandPos;
            HeadPos = LhandPos = RhandPos = 0;

            HeadPos = Math.Abs(prev_skel.Joints[JointType.Head].Position.X - skel.Joints[JointType.Head].Position.X) +
                        Math.Abs(prev_skel.Joints[JointType.Head].Position.Y - skel.Joints[JointType.Head].Position.Y) +
                        Math.Abs(prev_skel.Joints[JointType.Head].Position.Z - skel.Joints[JointType.Head].Position.Z);

            LhandPos = Math.Abs(prev_skel.Joints[JointType.HandLeft].Position.X - skel.Joints[JointType.HandLeft].Position.X) +
                        Math.Abs(prev_skel.Joints[JointType.HandLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y) +
                        Math.Abs(prev_skel.Joints[JointType.HandLeft].Position.Z - skel.Joints[JointType.HandLeft].Position.Z);

            RhandPos = Math.Abs(prev_skel.Joints[JointType.HandRight].Position.X - skel.Joints[JointType.HandRight].Position.X) +
                        Math.Abs(prev_skel.Joints[JointType.HandRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y) +
                        Math.Abs(prev_skel.Joints[JointType.HandRight].Position.Z - skel.Joints[JointType.HandRight].Position.Z);

            if (HeadPos < SINGLE_JUDGE && LhandPos < SINGLE_JUDGE && RhandPos < SINGLE_JUDGE || HeadPos + LhandPos + RhandPos < TOTAL_JUDGE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region フラグ情報のリセット

        public void Reset_Skel()
        {
            this.prev_skel = new Skeleton(); ;
        }
        #endregion

    }
}
