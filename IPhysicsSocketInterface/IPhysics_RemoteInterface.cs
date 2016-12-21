using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using System.Globalization;

namespace IPhysics
{
    
    public class RemoteInterface
    {
        NumberFormatInfo nfi = new NumberFormatInfo();

        private Socket socket;
        private Process exeProcess;
        private bool autoClose;
        private bool autoAttach;
        private bool useSpecialVersion;
        private bool disableRescue;

         // disable-rescue

        public bool DisableRescue { get { return disableRescue; } set { disableRescue = value; } }
        public Process ExeProcess { get { return exeProcess; } }
        public bool UseSpecialVersion { get { return useSpecialVersion; } set { useSpecialVersion = value; } }
        public string SpecialIPhysicsPath { get; set; }
        
        public bool IsConnected { get { return socket.Connected; } }
        public bool IsStarted { get { return exeProcess != null && exeProcess.Responding; } }
        
        public bool AutoClose { get { return autoClose; } set { autoClose = value; } }
        public bool AutoAttach { get { return autoAttach; } set { autoAttach = value; } }

        public RemoteInterface( 
            bool autoAttachToRunningProcess = false
            , bool autoCloseProcessOnExit = false
            , bool disableRescue = true
            )
        {
            nfi.NumberDecimalSeparator = ".";
            Console.WriteLine("Logging started");
            newSocket();

            AutoClose = autoCloseProcessOnExit;
            AutoAttach = autoAttachToRunningProcess;
            DisableRescue = disableRescue;
            if (autoAttach)
            {
                Process[] processes = Process.GetProcessesByName("industrialPhysics");
                // for now just take the potential first process.
                if (processes.Count() > 0)
                {
                    exeProcess = processes[0];
                }
            }
        }   

        ~RemoteInterface()
        {
            if (exeProcess != null) {
                if (AutoClose)
                {
                    //sendData("application.quit(true)");
                    //exeProcess.CloseMainWindow();
                    //exeProcess.Close();
                    exeProcess.Kill();
                }
            }
        }

        private void newSocket()
        {
            socket = new Socket(
                AddressFamily.InterNetwork
                , SocketType.Stream
                , ProtocolType.Tcp
                );

            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        }
       
        
        public void sendData(String program)
        {
            if (!socket.Connected)
            {
                Console.WriteLine("Socket is not connected. Cannot send!");
                return;
            }

            byte[] buffer = Encoding.ASCII.GetBytes(program);

            try
            {
                socket.Send(buffer);
                socket.Receive(buffer);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not send "+ buffer.Length + " bytes of data: " + ex.Message);
            }
        }

        public void execute(IPhysics_Command command)
        {
            sendData(command.toJavaScript());
        }

        public void execute(IPhysics_Command[] commands)
        {
            //ToDo: Block everything else until done
            for(int i = 0; i < commands.Length; i++)
                sendData(commands[i].toJavaScript());
        }

        public void ShutDown()
        {
            socket.Disconnect(true);
            newSocket();
        }

        public void Connect(string address, int port)
        {
            newSocket();
            try
            {
                socket.Connect(address, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect: " + ex.Message);
            }
        }

        public void sendPlay()
        {
            sendData("application.simulation.play();");
        }

        public void sendPause()
        {
            sendData("application.simulation.pause();");   
        }

        public void sendReset()
        {
            sendData("application.simulation.reset(true);");
        }

        public void sendRewind()
        {
            sendData("application.simulation.rewind(true);");
        }

        public void sendStep()
        {
            sendData("application.simulation.step();");
        }

        public void switchModePresentation(bool enabled)
        {
            sendData("application.presentationMode(" + enabled.ToString().ToLower() + ")");
        }

        public void switchModeFullscreen(bool enabled)
        {
            sendData("application.fullScreenMode(" + enabled.ToString().ToLower() + ")");
        }

        public void StartIPhysics(decimal port)
        {
            if (exeProcess != null)
                return;

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;

            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); //  = new Microsoft.Win32.RegistryKey();
            startInfo.FileName = key.GetValue(
                "Software\\machineering\\industrialPhysics\\InstallDir(x64)"
                , "C:\\Program Files\\machineering\\industrialPhysics(x64)").ToString();
            startInfo.FileName += "\\bin\\industrialPhysics.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            
            if (DisableRescue)
                startInfo.Arguments += "disable-rescue ";
            
            startInfo.Arguments += "js-server " + port.ToString();

            if (useSpecialVersion)
                startInfo.FileName = SpecialIPhysicsPath;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                exeProcess = Process.Start(startInfo);
                int timeoutMs = 30000;
                int sleepTime = 500;
                while (!exeProcess.Responding && timeoutMs > 0)
                {
                    Thread.Sleep(sleepTime);
                    timeoutMs -= sleepTime;
                }

                if (timeoutMs <= 0)
                {
                    Console.WriteLine("Error starting industrialPhysics, process timed out (" + 30000 + ".");
                }
            }
            catch
            {
                Console.WriteLine("Error starting industrialPhysics!");
            }
        }

        public void ShutDownIPhysics()
        {
            if (exeProcess == null)
                return;

            exeProcess.CloseMainWindow();
        }

        public bool SpecialVersionExists() 
        {
            return System.IO.File.Exists(SpecialIPhysicsPath);
        }

        public void push3DImageRemote(int channel)
        {
            sendData("push3DImageRemote(" + channel.ToString() + ")");
        }

        public void abo3DImageRemote(int rate)
        {
            sendData("abo3DImageRemote(" + rate.ToString() + ")");
        }

        public void resetZoom()
        {
            sendData("view3D.zoomAll()");
        }

        public void zoomIn(float value)
        {
            sendData("view3D.zoomIn(" + value.ToString(nfi) + ")");
        }

        public void zoomOut(float value)
        {
            sendData("view3D.zoomOut(" + value.ToString(nfi) + ")");
        }

        public void panLeft(float value)
        {
            sendData("view3D.panLeft(" + value.ToString(nfi) + ")");
        }

        public void panRight(float value)
        {
            sendData("view3D.panRight(" + value.ToString(nfi) + ")");
        }

        public void panUp(float value)
        {
            sendData("view3D.panUp(" + value.ToString(nfi) + ")");
        }

        public void panDown(float value)
        {
            sendData("view3D.panDown(" + value.ToString(nfi) + ")");
        }

        public void lookLeft(float value)
        {
            sendData("view3D.lookLeft(" + value.ToString(nfi) + ")");
        }

        public void lookRight(float value)
        {
            sendData("view3D.lookRight(" + value.ToString(nfi) + ")");
        }

        public void lookUp(float value)
        {
            sendData("view3D.lookUp(" + value.ToString(nfi) + ")");
        }

        public void lookDown(float value)
        {
            sendData("view3D.lookDown(" + value.ToString(nfi) + ")");
        }

        public void lookFromTop()
        {
            sendData("view3D.lookFromTop()");
        }

        public void lookFromBottom()
        {
            sendData("view3D.lookFromBottom()");
        }

        public void lookFromLeft()
        {
            sendData("view3D.lookFromLeft()");
        }

        public void lookFromRight()
        {
            sendData("view3D.lookFromRight()");
        }

        public void lookFromFront()
        {
            sendData("view3D.lookFromFront()");
        }

        public void lookFromBack()
        {
            sendData("view3D.lookFromBackside()");
        }

        public void lookFromIsometry()
        {
            sendData("view3D.lookFromIsometry()");
        }

        public void changeBackgroundColor(String color)
        {
            sendData("view3D.setBackground('" + color + "')");
        }

        public void setCustomView(String name)
        {
            sendData("setCustomView('" + name + "')");
        }
    }
}
