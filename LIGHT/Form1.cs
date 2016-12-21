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

namespace LIGHT
{
    public partial class Form1 : Form
    {
        RemoteInterface remote;
        int handle = -1;
        ExternalAppDock iPhysicsDoc;
        bool loading_done;

        //初始化
        public void Initialization()
        {
            remote = new RemoteInterface(true, true);

            iPhysicsDoc = new ExternalAppDock();
            this.panel1.Controls.Add(iPhysicsDoc);
            iPhysicsDoc.Dock = DockStyle.Fill;
            if (!remote.IsStarted)
                remote.StartIPhysics(6001);
            int timeout = 30000;
            int sleepTime = 500;
            while (!remote.IsConnected && timeout > 0)
            {
                System.Threading.Thread.Sleep(sleepTime);
                timeout -= sleepTime;

                if (!remote.IsConnected)
                    remote.Connect("localhost", 6001);
            }

            iPhysicsDoc.DockExternalApp(remote.ExeProcess, iPhysicsDoc.Parent);
            if (!(timeout > 0))
                throw new Exception("Error, cannot connect to industrialPhysics");
            //Default File Load
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); 
            String Path = key.GetValue(
                "Software\\machineering\\industrialPhysics\\InstallDir(x64)"
                , "c:\\Program Files\\machineering\\industrialPhysics(x64)").ToString();
            //IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + "/light.iphz");
            //remote.execute(command);
            //remote.lookFromIsometry();
            //remote.switchModePresentation(true);
            //remote.zoomOut(0);
            //remote.sendPlay();
            //loading_done = true;
            //remote.sendReset();
            //remote.sendPlay();
        }

        public void loadDemo()
        {
            IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + "/PCC0904.iphz");
            //IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + "/light.iphz");
            remote.execute(command);
            //remote.switchModePresentation(true);
            loading_done = true;
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
            string filepath = Environment.CurrentDirectory + "/CodeGen.xml";
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

        public Form1()
        {
            InitializeComponent();

            Initialization();
            Thread thread = new Thread(new ThreadStart(loadDemo));
            thread.Start();



            //软件初始化
            //Initialization();
            //初始化 回调 webservice
            serviceLight = new WebReference.MDCService();
            serviceLight.GetCurrentDetailInfoCompleted += DncService_GetCurrentDetailInfoCompleted;
            getDic_JC();
        }
        //开始
        private void button1_Click(object sender, EventArgs e)
        {
            remote.sendReset();
            remote.sendPlay();
            Connect();
            timer1.Enabled = true;
            timer2.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            contrLight();
            contrLightThtead();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //double time, timeStep;
            //ComTCPLib.UpdateData(handle, out time, out timeStep);

            //string status="";
            //if (int.Parse(status) > 0)
            //{
            //    int index = GetIdex.getDicOutputIndex("LIGHT01_input_LIGHT");
            //    ComTCPLib.SetOutputAsUINT(handle, index, uint.Parse(status));
            //    ComTCPLib.UpdateData(handle, out time, out timeStep);
            //}
            //contrLight();
            showWindowBynmae("aaa");
        }



        public void getDic_JC()
        {
            Dic_JC = new Dictionary<string, string>();
            Dic_JC.Add("2040VMC", "light101_input_LIGHT");
            Dic_JC.Add("BFN63-004-0004", "light201_input_LIGHT");
            Dic_JC.Add("BFN63-004-0005", "light301_input_LIGHT");
            Dic_JC.Add("CK61100-011-039", "light401_input_LIGHT");
            Dic_JC.Add("CK61100-011-040", "light501_input_LIGHT");
        }
        #region
        //////////////// webservice 逻辑
        //初始化字典
        public void contrLight()
        {
            foreach (KeyValuePair<string, string> kvp in Dic_JC)
            {
                getDetailsInfoByNmae(kvp.Key);
            }  
        }
        
        //Status：状态名（运行、空闲、报警、关机）
        private void  getDetailsInfoByNmae(string name )
        {
            serviceLight.GetCurrentDetailInfoAsync(name);
        }
        //BASESTATE ALARMSTATE
        private void DncService_GetCurrentDetailInfoCompleted(object sender, WebReference.GetCurrentDetailInfoCompletedEventArgs e)
        {
            string state = "0";
            string name = "";
            DataTable dt = e.Result;
            if(dt!=null && dt.Rows.Count > 0)
            {
                name = dt.Rows[0]["MTNAME"].ToString();
                //关机
                if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "0";
                }
                //运行
                else if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "2";
                }
                //报警
                else if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "1";
                }
                //空闲
                else if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "3";
                }
                updateLight(name, state);
            }
            
        }
        public void updateLight(string name,string state)
        {
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            int index = GetIdex.getDicOutputIndex(Dic_JC[name]);
            ComTCPLib.SetOutputAsUINT(handle, index, uint.Parse(state));
        }

        #endregion



        #region
        //同步方法--------------------------------------------------------------------
        public  WebReference.MDCService serviceLight2 = new WebReference.MDCService();
        private   void getDetalisByName(object name1)
        {
            string name2 = name1.ToString();
            DataTable dt = serviceLight2.GetCurrentDetailInfo(name2);
            string state = "0";
            string name = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                name = dt.Rows[0]["MTNAME"].ToString();
                //关机
                if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "1";
                }
                //运行
                else if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "2";
                }
                //报警
                else if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "1";
                }
                //空闲
                else if (dt.Rows[0]["BASESTATE"].ToString() == "OFFLINE")
                {
                    state = "3";
                }
                updateLight2(name, state);
            }
        }
        public  void updateLight2(string name, string state)
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
#endregion

        




        ////////点击机床
        public bool windowState = false;
        #region
        private void timer2_Tick(object sender, EventArgs e)
        {

            getBtnState();
        }

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
                    showWindowBynmae(kvp.Value.ToString());
                    return;
                }
            } 
        }

        private void showWindowBynmae(string name)
        {
            //browser bb = new browser();
            //bb.MdiParent = this;
            //bb.url = "http://www.baidu.com";
            //bb.Show();

            browser f2 = new browser();
            f2.url = "http://www.baidu.com";
            f2.ShowDialog();
            if (f2.DialogResult == DialogResult.OK)
            {
                this.windowState = false;
                f2.Close();
            }

        }




        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            VedioForm vf = new VedioForm();
            vf.ShowDialog();
        }
    }
}
