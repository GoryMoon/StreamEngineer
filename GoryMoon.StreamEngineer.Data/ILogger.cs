using System;

namespace GoryMoon.StreamEngineer.Data
{
    public interface ILogger
    {
        void WriteLine(string msg);

        void WriteLine(Exception e);

        void WriteAndChat(string msg);
    }
}