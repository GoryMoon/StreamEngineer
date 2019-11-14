using System;
using GoryMoon.StreamEngineer.Data;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;

namespace GoryMoon.StreamEngineer
{
    public class Logger: ILogger
    {
        public void WriteLine(string msg)
        {
            MySandboxGame.Log.WriteLineAndConsole($"[StreamEngineer]: {msg}");
        }

        public void WriteAndChat(string msg)
        {
            WriteLine(msg);
            Utils.SendChat(msg);
        }

        public void WriteLine(Exception e)
        {
            WriteLine("Exception: ");
            MySandboxGame.Log.WriteLine(e);
        }
    }
}