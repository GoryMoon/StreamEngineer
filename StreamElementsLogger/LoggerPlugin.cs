using GoryMoon.StreamEngineer.Data;

namespace StreamElementsLogger;

public class Dumper: IDataPlugin
{
    private readonly StreamElementsDataSource _streamElementsDataSource;
    public string Key { get; }

    public Dumper(string key)
    {
        Key = key;
        var dataHandler = new DummyDataHandler(this);
        _streamElementsDataSource = new StreamElementsDataSource(dataHandler, this);
    }

    public ILogger Logger { get; set; } = new Logger();
    public void ConnectionError(string name, string msg)
    {
        Logger.WriteError("Unable to connect to " + name + " with message " + msg);
    }

    public void Start()
    {
        Logger.WriteLine("Starting StreamElements logger\n\n");
        Logger.WriteLine("Stop with: q");
        _streamElementsDataSource.Init(Key);
        
        while (true)
        {
            var text = Console.ReadLine();
            if ("q".Equals(text))
            {
                _streamElementsDataSource.Dispose();
                return;
            }
        }
    }
}