using System;

namespace GoryMoon.StreamEngineer.Data
{
    public interface ILogger
    {
        void WriteLine(string msg);

        void WriteError(string msg);
        void WriteError(Exception e, string msg = "");

        void WriteAndChat(string msg);
    }
}