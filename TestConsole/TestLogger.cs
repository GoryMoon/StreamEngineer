using System;
using GoryMoon.StreamEngineer.Data;

namespace TestConsole;

internal class TestLogger : ILogger
{
    public void WriteLine(string msg)
    {
        Console.WriteLine($"[Info] {msg}");
    }

    public void WriteError(string msg)
    {
        Console.Error.WriteLine($"[Error] {msg}");
    }

    public void WriteError(Exception e, string msg = "")
    {
        if (!string.IsNullOrEmpty(msg))
            WriteError(msg);
        Console.WriteLine(e);
    }

    public void WriteAndChat(string msg)
    {
        WriteLine(msg);
    }
}