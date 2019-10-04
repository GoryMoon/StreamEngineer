using System;
using GoryMoon.StreamEngineer.Data;
using Sandbox;

namespace GoryMoon.StreamEngineer
{
    public class Logger: ILogger
    {
        public void WriteLine(string msg)
        {
            MySandboxGame.Log.WriteLineAndConsole($"[StreamEngineer]: {msg}");
        }

        public void WriteLine(Exception e)
        {
            WriteLine("Exception: ");
            MySandboxGame.Log.WriteLine(e);
        }
    }
}