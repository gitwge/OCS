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
using System.Xml;
using WZYB.Model;
using WZYB.BLL;
using System.Collections;


namespace TestSetPosition
{
    public partial class Form1 : Form
    {
        #region 数据定义

        /// <summary>
        /// 车辆数量
        /// </summary>
        private int carCount = 5;
        /// <summary>
        /// 速度
        /// </summary>
        private float speed = 0.05f;
        /// <summary>
        /// 起始位置
        /// </summary>
        private float startPos = 0.01f;
        /// <summary>
        /// 车身宽度
        /// </summary>
        private float carWidth = 1.1f;
        /// <summary>
        /// 小车线程
        /// </summary>
        private Thread[] newThread;
        /// <summary>
        /// 小车位置
        /// </summary>
        private float[] carPos;
        /// <summary>
        /// 小车缓存数据
        /// </summary>
        private List<OCSStatus> ocsModelList = new List<OCSStatus>();
        /// <summary>
        /// 是否启动
        /// </summary>
        private bool isStart = false;
        /// <summary>
        /// 线体长度
        /// </summary>
        private float[] lineLength = { 0,   12.838f,    12.445f,    12.445f,    12.445f,    12.46f,     1.16f,      1.15f,      1.14f,      1.171f,     12.443f,
                                            1.142f,     12.443f,    1.15f,      12.443f,    1.154f,     12.443f,    1.151f,     12.443f,    1.144f,     12.443f,
                                            1.153f,     12.443f,    1.151f,     12.443f,    1.151f,     12.443f,    1.152f,     12.443f,    1.152f,     12.443f,
                                            1.143f,     12.443f,    1.152f,     12.443f,    1.578f,     11.688f,    1.145f,     1.151f,     1.145f,     1.151f,
                                            1.151f,     1.152f,     1.151f,     1.153f,     1.144f,     1.151f,     1.153f,     1.151f,     1.142f,     1.153f,
                                            1.145f,     1.148f,     1.154f,     0.785f};

        /// <summary>
        /// 路线
        /// </summary>
        private string[] path = { "","1,6,7,8,9,10,50,51,52,53,54", 
                                  "1,6,7,4,52,53,54",
                                  "6,3,53,54,1",
                                  "7,4,52,53,54,1",
                                  "8,5,51,52,53,54,1"};

        private int[] tmpLine;

        #endregion

        #region 初始化

        RemoteInterface remote;
        int handle = -1;
        ExternalAppDock iPhysicsDoc;
        bool loading_done;

        public Form1()
        {
            InitializeComponent();
            Initialization();
            loadDemo();
        }

        /// <summary>
        /// 初始化三维模型
        /// </summary>
        public void Initialization()
        {
            remote = new RemoteInterface(true, true);

            iPhysicsDoc = new ExternalAppDock();
            this.panel1.Controls.Add(iPhysicsDoc);
            iPhysicsDoc.Dock = DockStyle.Fill;
            if (!remote.IsStarted)
                remote.StartIPhysics(6000);
            int timeout = 30000;
            int sleepTime = 500;
            while (!remote.IsConnected && timeout > 0)
            {
                System.Threading.Thread.Sleep(sleepTime);
                timeout -= sleepTime;

                if (!remote.IsConnected)
                    remote.Connect("localhost", 6000);
            }

            iPhysicsDoc.DockExternalApp(remote.ExeProcess, iPhysicsDoc.Parent);
            if (!(timeout > 0))
                throw new Exception("Error, cannot connect to industrialPhysics");
            //Default File Load
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            String Path = key.GetValue(
                "Software\\machineering\\industrialPhysics\\InstallDir(x64)"
                , "c:\\Program Files\\machineering\\industrialPhysics(x64)").ToString();
            
        }

        /// <summary>
        /// 加载三维模型
        /// </summary>
        public void loadDemo()
        {
            IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + System.Configuration.ConfigurationManager.AppSettings["modefile"].ToString());
            remote.execute(command);
            //remote.switchModePresentation(true);
            loading_done = true;
        }

        /// <summary>
        /// 与三维模型建立链接
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 关闭三维模型连接
        /// </summary>
        /// <returns></returns>
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

        #endregion
                
        #region 窗体逻辑

        private void Form1_Load(object sender, EventArgs e)
        {
            carPos = new float[carCount + 1];
            newThread = new Thread[carCount];
            tmpLine = new int[carCount + 1];

            int[] tmpSequence = new int[1000];

            for (int i = 1; i <= carCount; i++)
            {
                tmpLine[i] = 0;
                OCSStatus model = OCSStatusBLL.GetModel(i);
                model.line = int.Parse(path[i].Split(',')[0]);
                model.sequence = tmpSequence[model.line] + 1;
                tmpSequence[model.line]++;
                OCSStatusBLL.Update(model);
            }
            timer2.Enabled = true;
        }

        /// <summary>
        /// 模拟生成数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            for (int i = 1; i < carPos.Length; i++)
            {
                OCSStatus model = OCSStatusBLL.GetModel(i);
                
                if (carPos[i] > 0)
                {
                    if (carPos[i] >= lineLength[int.Parse(path[i].Split(',')[tmpLine[i]])])
                    {
                        if (tmpLine[i] + 1 >= path[i].Split(',').Length)
                            tmpLine[i] = 0;
                        else
                            tmpLine[i]++;
                        model.line = int.Parse(path[i].Split(',')[tmpLine[i]]);
                        int count = OCSStatusBLL.getCountByLine(model.line);
                        model.sequence = count + 1;
                        OCSStatusBLL.Update(model);
                    }
                }
            }
        }

        #endregion

        #region 界面按钮处理
        private void button1_Click(object sender, EventArgs e)
        {
            remote.sendReset();
            remote.sendPlay();
            Connect();

            Thread.Sleep(1000);
            isStart = true;

            for (int i = 0; i < carCount; i++)
            {
                newThread[i] = new Thread(new ParameterizedThreadStart(ThreadFunc));
                newThread[i].Start(i + 1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            remote.sendReset();
            remote.sendReset();
            Disconnect();

            isStart = false;
            for (int i = 0; i < 2; i++)
            {
                newThread[i].Abort();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            remote.sendPause();
            isStart = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            remote.sendPlay();
            isStart = true;
        }

        #endregion

        #region 小车处理逻辑

        /// <summary>
        /// 小车线程处理
        /// </summary>
        /// <param name="o"></param>
        private void ThreadFunc(object o)
        {
            int index = Convert.ToInt32(o);

            while (true)
            {
                if (isStart)
                {
                    //数据库最新数据
                    OCSStatus model = OCSStatusBLL.GetModel(index);

                    if (model.position == -1)
                    {
                        //内存数据
                        OCSStatus oldModel = ocsModelList.Find(s => s.carId == index);

                        //初始
                        if (oldModel == null)
                        {
                            int count = OCSStatusBLL.getCountByLine(model.line);

                            carPos[index] = (count - model.sequence) * carWidth + startPos;
                            ocsModelList.Add(model);
                        }
                        else
                        {
                            //驱动段改变
                            if (oldModel.line != model.line)
                            {
                                carPos[index] = startPos;
                            }
                            else
                            {
                                if (model.direction == 1)
                                    carPos[index] += speed;
                                else if (model.direction == 2)
                                    carPos[index] -= speed;
                            }

                            int i = ocsModelList.FindIndex(s => s.carId == index);
                            ocsModelList[i] = model;
                        }
                    }
                    else
                    {
                        carPos[index] = float.Parse(model.position.ToString());
                    }
                                        
                    updateValue("car" + index + "01_input_pos", carPos[index].ToString(), 2);
                    updateValue("car" + index + "01_input_Path", model.line.ToString(), 4);

                    Thread.Sleep(300);
                }
            }
        }

        #endregion

        #region 其它

        /// <summary>
        /// 1 uint,2 float,3 bool,4 int
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type">1 uint,2 float,3 bool,4 int</param>
        /// <param name="handle"></param>
        public void updateValue(string name, string value, int type)
        {
            double time, timeStep;

            int index = GetIdex.getDicOutputIndex(name);
            if (type == 1)
            {
                ComTCPLib.SetOutputAsUINT(handle, index, uint.Parse(value));
            }
            else if (type == 2)
            {
                ComTCPLib.SetOutputAsREAL32(handle, index, float.Parse(value));
            }
            else if (type == 3)
            {
                ComTCPLib.SetOutputAsBOOL(handle, index, bool.Parse(value));
            }
            else if (type == 4)
            {
                ComTCPLib.SetOutputAsINT(handle, index, int.Parse(value));
            }
            ComTCPLib.UpdateData(handle, out time, out timeStep);
        }

        #endregion

    }
}
