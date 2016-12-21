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

namespace CGK
{
    public partial class CGK : Form
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
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); //  = new Microsoft.Win32.RegistryKey();
            String Path = key.GetValue(
                "Software\\machineering\\industrialPhysics\\InstallDir(x64)"
                , "c:\\Program Files\\machineering\\industrialPhysics(x64)").ToString();
            //IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + "../../../../AGV_Path.iphz");
            //textBox1.Text = Environment.CurrentDirectory + "/AGV_Path.iphz";
            IPhysics_Command command = new LoadDocument(Environment.CurrentDirectory + "/cgk_csharp.iphz");
            remote.execute(command);

            remote.lookFromIsometry();
            //remote.switchModePresentation(true);
            remote.zoomOut(0);
            //remote.sendPlay();
            loading_done = true;
            //remote.sendReset();
            //remote.sendPlay();
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
        public CGK()
        {
            InitializeComponent();
            Initialization();

           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            remote.sendReset();
            remote.sendPlay();
            Connect();
            timer1.Enabled = true;
        }

        private void CGK_Load(object sender, EventArgs e)
        {

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            uint aa;
            int index = GetIdex.getDicInputIndex("QZJ01_DDJ_state");
            ComTCPLib.GetInputAsUINT(handle, index, out aa);
            if(aa == 0)
            {
                index = GetIdex.getDicOutputIndex("SSJ01_TP_NUM");
                ComTCPLib.SetOutputAsUINT(handle, index, 3);

                index = GetIdex.getDicOutputIndex("Car01_input_GOTO_state");
                ComTCPLib.SetOutputAsUINT(handle, index, 2);
                
                ComTCPLib.UpdateData(handle, out time, out timeStep);
                aa = 1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ComTCPLib.SetOutputAsUINT(handle, 0, 3);
            ComTCPLib.SetOutputAsUINT(handle, 1, 1);
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
        }
    }
}
