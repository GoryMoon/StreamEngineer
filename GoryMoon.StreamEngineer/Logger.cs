using System;
using GoryMoon.StreamEngineer.Data;
using Sandbox;
using VRage.Utils;

namespace GoryMoon.StreamEngineer
{
    public class Logger: ILogger
    {
        public void WriteLine(string msg)
        {
            MySandboxGame.Log.WriteLineAndConsole($"[StreamEngineer]: {msg}");
        }

        public void WriteError(string msg)
        {
            MySandboxGame.Log.Error(msg);
        }

        public void WriteAndChat(string msg)
        {
            WriteLine(msg);
            Utils.SendChat(msg);
        }

        public void WriteError(Exception e, string msg = "")
        {
            WriteError(msg);
            MySandboxGame.Log.WriteLine(e);
        }
    }
}