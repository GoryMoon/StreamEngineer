using System.IO;
using System.Security.Cryptography;
using Nett;
using Nett.Coma;

namespace GoryMoon.StreamEngineer.Config
{
    internal sealed class CustomFileStore : IConfigStore
    {
        private readonly string _filePath;
        private readonly TomlSettings _settings;
        private byte[] _latestFileHash;

        private CustomFileStore(TomlSettings settings, string filePath)
        {
            _settings = settings;
            _filePath = filePath;
        }

        public bool EnsureExists(TomlTable content)
        {
            if (File.Exists(_filePath))
            {
                var table = Load();
                var newDataTable = TomlTable.Combine(s => s.Overwrite(table).With(content).IncludingAllComments().ForRowsOnlyInSource());
                var removeOldDataTable = TomlTable.Combine(s => s.Overwrite(content).With(newDataTable).ExcludingComments().ForAllTargetRows());
                Save(removeOldDataTable);
            }
            else
            {
                Save(content);
            }

            return true;
        }

        public bool WasChangedExternally()
        {
            return !HashEquals(_latestFileHash, ComputeHash(_filePath));
        }

        public TomlTable Load()
        {
            _latestFileHash = ComputeHash(_filePath);
            return Toml.ReadFile(_filePath, _settings);
        }

        public void Save(TomlTable config)
        {
            Toml.WriteFile(config, _filePath, _settings);
            _latestFileHash = ComputeHash(_filePath);
        }

        public static CustomFileStore Create(string path)
        {
            return new CustomFileStore(TomlSettings.Create(), path);
        }

        public static CustomFileStore Create(string path, TomlSettings settings)
        {
            return new CustomFileStore(settings, path);
        }

        private static byte[] ComputeHash(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return SHA1.Create().ComputeHash(fileStream);
            }
        }

        private static bool HashEquals(byte[] x, byte[] y)
        {
            if (x == y)
                return true;
            if (x == null || y == null || x.Length != y.Length)
                return false;
            for (var index = 0; index < x.Length; ++index)
                if (x[index] != y[index])
                    return false;
            return true;
        }
    }
}