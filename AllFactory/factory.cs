using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using IPhysics;
using Microsoft.Win32;
using System.Threading;

namespace AllFactory
{
    public partial class factory : Form
    {
        RemoteInterface remote;
        int handle = -1;
        ExternalAppDock iPhysicsDoc;
        bool loading_done;


        public int port;
        public string modelName;
        //////////////-----------------------
        #region
        //初始化
        public void Initialization()
        {
            remote = new RemoteInterface(true, true);

            iPhysicsDoc = new ExternalAppDock();
            this.panel1.Controls.Add(iPhysicsDoc);
            iPhysicsDoc.Dock = DockStyle.Fill;
            if (!remote.IsStarted)
                remote.StartIPhysics(port);
            int timeout = 30000;
            int sleepTime = 500;
            while (!remote.IsConnected && timeout > 0)
            {
                System.Threading.Thread.Sleep(sleepTime);
                timeout -= sleepTime;

                if (!remote.IsConnected)
                    remote.Connect("localhost", port);
            }

            iPhysicsDoc.DockExternalApp(remote.ExeProcess, iPhysicsDoc.Parent);
            if (!(timeout > 0))
                throw new Exception("Error, cannot connect to industrialPhysics");
            //Default File Load
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            String Path = key.GetValue(
                "Software\\machineering\\industrialPhysics\\InstallDir(x64)"
                , "c:\\Program Files\\machineering\\industrialPhysics(x64)").ToString();
            //IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + "/" + modelName);
            //remote.execute(command);
            //remote.lookFromIsometry();
            //remote.switchModePresentation(true);
            //remote.zoomOut(0);
            //loading_done = true;
        }
        //加载模型 线程
        private void loadMode()
        {
            IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory  + modelName);
            remote.execute(command);
            //remote.lookFromIsometry();
            if (System.Configuration.ConfigurationManager.AppSettings["isDebug"].ToString() == "0")
            {
                remote.switchModePresentation(true);
            }
            
            //remote.zoomOut(0);
            loading_done = true;
            remote.sendReset();
        }

        //建立链接
        private int Connect()
        {
            if (handle >= 0)
                return -10;

            ComTCPLib.Init();

            // Create a new comtcp node
            int localhandle = ComTCPLib.CreateNode("Hello_industrialPhysics");
            if (localhandle < 0)
                return -1;

            // open the config file
            string filepath = Environment.CurrentDirectory + System.Configuration.ConfigurationManager.AppSettings["CodeGen"].ToString();
            //string filepath = Environment.CurrentDirectory + @"\CodeGen.xml";
            if (ComTCPLib.Result.Failed == ComTCPLib.LoadConfig(localhandle, filepath))
                return -2;

            // Start the internal thread system
            if (ComTCPLib.Result.Failed == ComTCPLib.Start(localhandle))
                return -3;

            // Connect to running industrialPhysics as specified in the file
            if (ComTCPLib.Result.Failed == ComTCPLib.Connect(localhandle))
                return -4;

            int numOutputs = ComTCPLib.GetNumOutputs(localhandle);
            //float[] outputValues = new float[numOutputs];

            //if (6 != numOutputs)
            //{
            //    return Disconnect();
            //}

            handle = localhandle;

            return 0;
        }

        //关闭连接
        private int Disconnect()
        {
            if (handle < 0)
                return -1;

            // Disconnect
            if (ComTCPLib.Result.Failed == ComTCPLib.Disconnect(handle))
                return -1000;

            // Stop the threading
            if (ComTCPLib.Result.Failed == ComTCPLib.Stop(handle))
                return -1001;

            // delete the node
            if (ComTCPLib.Result.Failed == ComTCPLib.DeleteNode(handle))
                return -1002;

            return 0;
        }

        //webservice
        public WebReference.MDCService serviceLight;
        Dictionary<String, string> Dic_JC;

        public Dictionary<String, string> getDic_JC()
        {
            Dictionary<String, string> aa = new Dictionary<string, string>();
            string str_jc = System.Configuration.ConfigurationManager.AppSettings["jc_array"].ToString();
            string[] arr = str_jc.Split(';');
            if (arr.Length > 0)
            {
                foreach(string str in arr)
                {
                    if(str.Split(',').Length > 0)
                    {
                         aa.Add(str.Split(',')[0].Trim(), str.Split(',')[1].Trim());
                    }
                   
                }
            }
            return aa;

            //Dic_JC.Add("2040VMC", "light101_input_LIGHT");
            //Dic_JC.Add("BFN63-004-0004", "light201_input_LIGHT");
            //Dic_JC.Add("BFN63-004-0005", "light301_input_LIGHT");
            //Dic_JC.Add("CK61100-011-039", "light401_input_LIGHT");
            //Dic_JC.Add("CK61100-011-040", "light501_input_LIGHT");
        }

        public factory()
        {
            InitializeComponent();
            port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["port"].ToString());
            modelName =  System.Configuration.ConfigurationManager.AppSettings["modelName"].ToString();

            Initialization();
            Thread t = new Thread(loadMode);
            t.Start(); 
            //loadMode();
            serviceLight = new WebReference.MDCService();
            Dic_JC = getDic_JC();
            //remote.sendReset();
            //remote.setCustomView("V_ALL");
        }
        //play
        private void bt_play_Click(object sender, EventArgs e)
        {
            remote.sendPlay();
            Connect();
            timer1.Enabled = true;
            timer2.Enabled = true;
        }
        //pause

        private void bt_stop_Click(object sender, EventArgs e)
        {
            remote.sendPause();
        }
        //reset
        private void bt_reset_Click(object sender, EventArgs e)
        {
            remote.sendReset();
            //Disconnect();
            timer1.Enabled = false;
            timer2.Enabled = false;
        }
        //机床点击事件
        private void timer2_Tick(object sender, EventArgs e)
        {
            getBtnState();
        }
        //祝灯状态监控
        private void timer1_Tick(object sender, EventArgs e)
        {
            //contrLightThtead();
            contrLightThtead2();
        }
#endregion

        //祝灯逻辑----------------------
        #region 
        private void getDetalisByName(object name1)
        {
            string name2 = name1.ToString();
            //DataTable dt = serviceLight.GetCurrentDetailInfo(name2);
            //DataTable dt = serviceLight.GetMachineDetail(DateTime.Now,DateTime.Now.AddSeconds(-5),name2);
            DataTable dt = serviceLight.GetMachineDetail(DateTime.Now.AddMonths(-18), DateTime.Now.AddMonths(-18).AddSeconds(-5), name2);
            string state = "0";
            string name = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                name = name2;
                //关机
                if (dt.Rows[0]["STATUS"].ToString() == "关机")
                {
                    state = "0";
                }
                //运行
                else if (dt.Rows[0]["STATUS"].ToString() == "运行")
                {
                    state = "2";
                }
                //报警
                else if (dt.Rows[0]["STATUS"].ToString() == "报警")
                {
                    state = "1";
                }
                //空闲
                else if (dt.Rows[0]["STATUS"].ToString() == "空闲")
                {
                    state = "3";
                }
                updateLight(name, state);
            }
        }
        public void updateLight(string name, string state)
        {
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            int index = GetIdex.getDicOutputIndex(Dic_JC[name]);
            ComTCPLib.SetOutputAsUINT(handle, index, uint.Parse(state));
        }
        public void contrLightThtead()
        {
            foreach (KeyValuePair<string, string> kvp in Dic_JC)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(getDetalisByName), kvp.Key);
            }

        }
        public void contrLightThtead2()
        {
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            for (int i = 0; i < Dic_JC.Count;i++ )
            {
                if (i < 1)
                {
                    ComTCPLib.SetOutputAsUINT(handle, i, 0);
                }
                else
                {
                    ComTCPLib.SetOutputAsUINT(handle, i, 2);
                }
            }
        }
        #endregion


        //机床逻辑---------------------
        #region
        public bool windowState = false;
        private void getBtnState()
        {
            if (windowState)
            {
                return;
            }
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            foreach (KeyValuePair<string, string> kvp in Dic_JC)
            {
                int index = GetIdex.getDicOutputIndex(kvp.Value.ToString());
                bool result;

                ComTCPLib.GetInputAsBOOL(handle, index, out result);
                if (result)
                {
                    Thread.Sleep(50);
                    windowState = true;
                    showWindowBynmae(kvp.Key.ToString());
                    return;
                }
            }
        }

        private void showWindowBynmae(string name)
        {
            browser f2 = new browser();
            string mesLink = System.Configuration.ConfigurationManager.AppSettings["mesLink"].ToString();
            f2.url = mesLink.Replace("[name]", name);
            f2.ShowDialog();
            if (f2.DialogResult == DialogResult.OK)
            {
                this.windowState = false;
                f2.Close();
            }

        }


        #endregion

       

        //video视频---------------------
        #region 
        int videoIndex = 0;
        private void bt_video_Click(object sender, EventArgs e)
        {
            //videByIndex();
            videoByView();
           
        }
        public void videByIndex()
        {
            VedioForm vf = new VedioForm();
            string[] videoArr = System.Configuration.ConfigurationManager.AppSettings["video_array"].ToString().Split(',');
            if (videoIndex >= videoArr.Length)
            {
                videoIndex = 0;
            }
            vf.Url = Environment.CurrentDirectory + "/file/video/" + videoArr[videoIndex];
            vf.ShowDialog();
            videoIndex++;
        }

        public void videoByView()
        {
            string[] videoArr = System.Configuration.ConfigurationManager.AppSettings["video_view"].ToString().Split(';');
            string videoname = "";
            foreach (string str in videoArr)
            {
                if (str.Split(',')[0] == videoIndex.ToString())
                {
                    videoname = str.Split(',')[1];
                    break;
                }
            }
            if (videoname.Length < 1)
            {
                return;
            }
            VedioForm vf = new VedioForm();
            vf.Url = Environment.CurrentDirectory +"/file/video/" +videoname;
            vf.ShowDialog();
            videoIndex++;
        }
        #endregion



        ///-------------
        ///旋转
        #region 
        private void btnViewTop_Click(object sender, EventArgs e)
        {
            remote.lookFromTop();
        }

        private void btnViewLeft_Click(object sender, EventArgs e)
        {
            remote.lookFromLeft();
        }

        private void btnViewFront_Click(object sender, EventArgs e)
        {
            remote.lookFromFront();
        }

        private void btnViewBottom_Click(object sender, EventArgs e)
        {
            remote.lookFromBottom();
        }

        private void btnViewRight_Click(object sender, EventArgs e)
        {
            remote.lookFromRight();
        }

        private void btnViewBack_Click(object sender, EventArgs e)
        {
            remote.lookFromBack();
        }

        private void btnViewIso_Click(object sender, EventArgs e)
        {
            remote.lookFromIsometry();
        }
        #endregion


        ///----------
        ///坐标
        ///
        #region
        private void btnPanUp_Click(object sender, EventArgs e)
        {
            remote.panUp(0.2f);
        }

        private void btnPanLeft_Click(object sender, EventArgs e)
        {
            remote.panLeft(0.2f);
        }

        private void btnPanDown_Click(object sender, EventArgs e)
        {
            remote.panDown(0.2f);
        }

        private void btnPanRight_Click(object sender, EventArgs e)
        {
            remote.panRight(0.2f);
        }
        #endregion


        ///----------
        ///场景
        ///
        #region
        private void btnResetZoom_Click(object sender, EventArgs e)
        {
            remote.resetZoom();
        }
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            remote.zoomIn(0.2f);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            remote.zoomOut(0.2f);
        }
        #endregion


        /// <summary>
        /// 视角调整--------------------------------------------
        /// </summary>
        #region

        //采购库
        private void button15_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_CGK");
            videoIndex = 0;
        }

        //配餐车
        private void button16_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_PCC");
            videoIndex = 2;
        }
        //柔性制造
        private void button17_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_RXZZ");
            videoIndex = 0;
        }
        //MF制造
        private void button18_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_MFZZ");
            videoIndex = 0;
        }
        //轴杆加工
        private void button19_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_ZGJG");
            videoIndex = 1;
        }
        //悬挂链
        private void button20_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_XGL");
            videoIndex = 0;
        }
        //全場景
        private void button21_Click(object sender, EventArgs e)
        {
            remote.setCustomView("V_ALL");
        }


        #endregion

        private void button22_Click(object sender, EventArgs e)
        {

        }

       

    }
}
