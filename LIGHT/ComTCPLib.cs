using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;



class ComTCPLib
{
    private static int MAX_NAME_LENGTH = 256;
    
    public enum Result
    {
        Success = 0,
        Failed = -1
    };

    public enum VarType
    {
        VT_BOOL = 0,
        VT_UINT,
        VT_INT,
        VT_REAL32,
        VT_NOT_SUPPORTED
    };

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_Connect")]
    public static extern Result Connect(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetConnectionState")]
    public static extern Result GetConnectionState(int handle, out int result);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_CreateNode")]
    public static extern int CreateNode(string name);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_DeleteNode")]
    public static extern Result DeleteNode(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetNumNodes")]
    public static extern int GetNumNodes();

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetNodeHandle")]
    public static extern Result GetNodeHandle(int index, out int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_LoadConfig")]
    public static extern Result LoadConfig(int handle, string filepath);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_Disconnect")]
    public static extern Result Disconnect(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_Start")]
    public static extern Result Start(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_Stop")]
    public static extern Result Stop(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetNumOutputs")]
    public static extern int GetNumOutputs(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "ctcp_GetOutputName")]
    private static extern Result _GetOutputName(int handle, int index, StringBuilder value, int valueSize);

    public static Result GetOutputName(int handle, int index, out string value)
    {
        StringBuilder bla = new StringBuilder(MAX_NAME_LENGTH);
        Result r = _GetOutputName(handle, index, bla, MAX_NAME_LENGTH);
        if (r == Result.Success)
        {
            value = bla.ToString();
        }
        else
        {
            value = "";
        }
        return r;
    }

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetOutputType")]
    public static extern Result GetOutputType(int handle, int index, out VarType result);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_SetOutputAsINT")]
    public static extern Result SetOutputAsINT(int handle, int index, int value);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_SetOutputAsUINT")]
    public static extern Result SetOutputAsUINT(int handle, int index, uint value);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_SetOutputAsREAL32")]
    public static extern Result SetOutputAsREAL32(int handle, int index, float value);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_SetOutputAsBOOL")]
    public static extern Result SetOutputAsBOOL(int handle, int index, bool value);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_UpdateData")]
    public static extern Result UpdateData(int handle, out double time, out double timeStep);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetNumInputs")]
    public static extern int GetNumInputs(int handle);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "ctcp_GetInputName")]
    private static extern Result _GetInputName(int handle, int index, StringBuilder value, int valueSize);

    public static Result GetInputName(int handle, int index, out string value)
    {
        StringBuilder bla = new StringBuilder(MAX_NAME_LENGTH);
        Result r = _GetInputName(handle, index, bla, MAX_NAME_LENGTH);
        if (r == Result.Success)
        {
            value = bla.ToString();
        }
        else
        {
            value = "";
        }
        return r;
    }

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetInputType")]
    public static extern Result GetInputType(int handle, int index, out VarType result);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetInputAsINT")]
    public static extern Result GetInputAsINT(int handle, int index, out int result);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetInputAsUINT")]
    public static extern Result GetInputAsUINT(int handle, int index, out uint result);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetInputAsREAL32")]
    public static extern Result GetInputAsREAL32(int handle, int index, out float result);

    [DllImport("ComTCPLib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ctcp_GetInputAsBOOL")]
    public static extern Result GetInputAsBOOL(int handle, int index, out bool result);

    public static void Init()
    {
        RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        string dllDirectory;
        key = key.OpenSubKey("Software");
        key = key.OpenSubKey("machineering");
        key = key.OpenSubKey("ComTCPLib");

        if (IntPtr.Size == 8)
        {
            dllDirectory = key.GetValue("InstallDir(x64)", "C:\\Program Files\\machineering\\ComTCPLib(x64)").ToString();
        }
        else
        {
            dllDirectory = key.GetValue("InstallDir(x64)", "C:\\Program Files\\machineering\\ComTCPLib(x64)").ToString();
        }

        if (false)
        {
            dllDirectory = @"D:\01_mwb\build-main-Qt_32-Release\src\Base\Net\IOExchangeLib";
        }
        else
        {
            dllDirectory += @"\bin";
        }

        string[] lines = { "[Paths]", "Plugins=" };
        lines[1] += dllDirectory + @"\platforms";
        lines[1] = lines[1].Replace("\\", "/");
        System.IO.File.WriteAllLines(@".\qt.conf", lines);

        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);
    
    }
    
}
