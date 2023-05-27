using System;
using System.Collections.Generic;
using GoryMoon.StreamEngineer.Data;

namespace TestConsole;

public class Program : IDataPlugin
{
    private readonly StreamlabsDataSource _streamlabsDataSource;
    private readonly TwitchExtensionDataSource _twitchExtDataSource;
    private readonly StreamElementsDataSource _streamElementsDataSource;
    private readonly IntegrationAppDataSource _integrationAppDataSource;
    private readonly string _streamlabsToken;
    private readonly string _twitchToken;
    private readonly string _streamElementsToken;

    public ILogger Logger { get; set; } = new TestLogger();

    public static void Main(string[] args)
    {
        new Program(args).Start();
    }

    private Program(IReadOnlyList<string> args)
    {
        var dataHandler = new TestBaseDataHandler(this);
        _streamlabsDataSource = new StreamlabsDataSource(dataHandler, this);
        _twitchExtDataSource = new TwitchExtensionDataSource(dataHandler, this);
        _streamElementsDataSource = new StreamElementsDataSource(dataHandler, this);
        _integrationAppDataSource = new IntegrationAppDataSource(dataHandler, this);
        _streamlabsToken = args.Count > 0 ? args[0] : "";
        _twitchToken = args.Count > 1 ? args[1] : "";
        _streamElementsToken = args.Count > 2 ? args[2] : "";
    }

    private void Start()
    {
        Console.WriteLine("Stop with: q");
            
        if (_streamlabsToken.Length > 0)
            _streamlabsDataSource.Init(_streamlabsToken);

        if (_twitchToken.Length > 0)
            _twitchExtDataSource.Init(_twitchToken);

        if (_streamElementsToken.Length > 0)
            _streamElementsDataSource.Init(_streamElementsToken);
        
        _integrationAppDataSource.Init("");

        while (true)
        {
            var text = Console.ReadLine();
            if ("q".Equals(text))
            {
                _streamlabsDataSource.Dispose();
                _twitchExtDataSource.Dispose();
                _streamElementsDataSource.Dispose();
                _integrationAppDataSource.Dispose();
                return;
            }
        }
    }

    public void ConnectionError(string name, string msg)
    {
        Logger.WriteError($"[{name}] Unable to connect with message {msg}");
    }
}