using System;
using Sandbox;

namespace GoryMoon.StreamEngineer
{
    public class Logger
    {
        public static void WriteLine(string msg)
        {
            MySandboxGame.Log.WriteLineAndConsole($"[StreamEngineer]: {msg}");
        }
        
        public static void WriteLine(Exception e)
        {
            WriteLine("Exception: ");
            MySandboxGame.Log.WriteLine(e);
        }
    }
}