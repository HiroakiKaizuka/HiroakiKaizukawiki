/*だるまさんがころんだ　音声再生処理
                        担当：1123032 鈴木*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;

namespace DaruMA_project
{
    class playDaruMA
    {  
        /// <summary>
        /// サウンドの再生
        /// </summary>
        SoundPlayer player;

        public void play(int no)
        {
            PlaySound("..\\..\\voicedata\\num" + no + ".wav");
        }

        //WAVEファイルを再生する
        private void PlaySound(string waveFile)
        {
            //再生されているときは止める
            if (player != null)
                StopSound();

            //読み込む
            player = new System.Media.SoundPlayer(waveFile);
            
            //非同期再生する
            player.Play();

            //次のようにすると、ループ再生される
            //player.PlayLooping();

            //次のようにすると、最後まで再生し終えるまで待機する
            //player.PlaySync();
        }

        //再生されている音を止める
        private void StopSound()
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                //player.controls.stop();
                //player = null;
            }
        }
    }
}
