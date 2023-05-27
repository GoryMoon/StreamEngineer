using System;

namespace GoryMoon.StreamEngineer.Data
{
    public abstract class DataSource : IDisposable
    {
        protected readonly BaseDataHandler DataHandler;
        private protected IDataPlugin Plugin { get; }

        protected DataSource(BaseDataHandler dataHandler, IDataPlugin plugin)
        {
            Plugin = plugin;
            DataHandler = dataHandler;
        }

        public abstract void Init(string token);
        public abstract void Dispose();
    }
}