using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LIGHT
{
    public partial class VedioForm : Form
    {
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        public VedioForm()
        {
            InitializeComponent();
            InitVedio();  
        }

        private void VedioForm_Load(object sender, EventArgs e)
        {
            InitVedioUrl();
            InitEvent();
            
        }  

        //初始化播放控件  
        private void InitVedio()
        {
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(0, 0);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(800, 500);
            this.Size = new Size(805, 540);
            
            this.axWindowsMediaPlayer1.TabIndex = 2;
            this.Controls.Add(this.axWindowsMediaPlayer1);
        }
        //初始化播放控件的视频文件地址  
        protected void InitVedioUrl()
        {
            this.axWindowsMediaPlayer1.URL = @"D:/Program Files (x86)/Bandicam/video/bandicam 2016-09-04 21-19-11-324.avi";
        }


        protected void InitEvent()
        {
            axWindowsMediaPlayer1.StatusChange += new EventHandler(axWindowsMediaPlayer1_StatusChange);
        }


        protected void axWindowsMediaPlayer1_StatusChange(object sender, EventArgs e)
        {
            /*  0 Undefined Windows Media Player is in an undefined state.(未定义) 
                1 Stopped Playback of the current media item is stopped.(停止) 
                2 Paused Playback of the current media item is paused. When a media item is paused, resuming playback begins from the same location.(停留) 
                3 Playing The current media item is playing.(播放) 
                4 ScanForward The current media item is fast forwarding. 
                5 ScanReverse The current media item is fast rewinding. 
                6 Buffering The current media item is getting additional data from the server.(转换) 
                7 Waiting Connection is established, but the server is not sending data. Waiting for session to begin.(暂停) 
                8 MediaEnded Media item has completed playback. (播放结束) 
                9 Transitioning Preparing new media item. 
                10 Ready Ready to begin playing.(准备就绪) 
                11 Reconnecting Reconnecting to stream.(重新连接) 
            */
            //判断视频是否已停止播放  
            if ((int)axWindowsMediaPlayer1.playState == 1)
            {
                //停顿2秒钟再重新播放  
                System.Threading.Thread.Sleep(2000);
                //重新播放  
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

       
    }
}
