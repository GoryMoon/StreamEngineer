namespace GoryMoon.StreamEngineer.Data
{
    public interface IDataPlugin
    {
        
        ILogger Logger { get; set; }

        void ConnectionError(string name, string msg);
    }
}