/*だるまさんがころんだ　ポイント／進行方向処理
                        担当：1123060 常盤*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DaruMA_project
{
    class CheckClass
    {
        const double depMin = 1.5;// 手前ライン
        const double depMax = 3.0;// 奥行きライン
       
        public CheckClass()
        {
        }
        /// <summary>
        /// シャトルランの判定をしポイントを返す関数
        /// </summary>
        public int addPoint(Skeleton sklSR, int countSR, Boolean flagSR)
        {
            // 手前へ向かう
            if (flagSR == true)
            {
                // プレイヤーの腰の位置が1.5mより手前の場合ポイント加算
                if (sklSR.Joints[JointType.HipCenter].Position.Z < depMin)
                {
                    countSR++;
                }
            }
            // 奥へ向かう
            else
            {
                // プレイヤーの腰の位置が3.0mより奥の場合ポイント加算
                if (sklSR.Joints[JointType.HipCenter].Position.Z > depMax)
                {
                    countSR++;
                }
            }
            return countSR;
        }

        /// <summary>
        /// シャトルランの判定をし進行方向のフラグを返す関数
        /// </summary>
        public Boolean changeDir(Skeleton sklSR, Boolean flagSR)
        {
            // 手前へ向かう
            if (flagSR == true)
            {
                // プレイヤーの腰の位置が1.5mより手前の場合、進行方向を奥へ
                if (sklSR.Joints[JointType.HipCenter].Position.Z < depMin)
                {
                    flagSR = false;
                }
            }
            // 奥へ向かう
            else
            {
                // プレイヤーの腰の位置が3.0mより奥の場合、進行方向を手前へ
                if (sklSR.Joints[JointType.HipCenter].Position.Z > depMax)
                {
                    flagSR = true;
                }
            }
            return flagSR;
        }
    }
}
