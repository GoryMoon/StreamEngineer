using System.Security.Cryptography;
using Nett;
using Nett.Coma;

namespace ConfigTest.Config
{
    internal sealed class CustomFileStore : IConfigStore
        {
            private readonly string _filePath;
            private readonly TomlSettings _settings;
            private byte[]? _latestFileHash;
    
            private CustomFileStore(TomlSettings settings, string filePath)
            {
                _settings = settings;
                _filePath = filePath;
            }
    
            public bool EnsureExists(TomlTable content)
            {
                Save(File.Exists(_filePath)
                    ? CombineNested(content, Load(), s => s.IncludingAllComments().ForAllTargetRows())
                    : content);
    
                return true;
            }
    
            public bool WasChangedExternally() => !HashEquals(_latestFileHash, ComputeHash(_filePath));
    
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
    
            public static CustomFileStore Create(string path) => new CustomFileStore(TomlSettings.Create(), path);
    
            public static CustomFileStore Create(string path, TomlSettings settings) => new CustomFileStore(settings, path);
    
            private static byte[] ComputeHash(string filePath)
            {
                using (var fileStream = File.OpenRead(filePath))
                    return SHA1.Create().ComputeHash(fileStream);
            }
    
            private static bool HashEquals(IReadOnlyList<byte>? x, IReadOnlyList<byte>? y)
            {
                if (Equals(x, y))
                    return true;
                if (x == null || y == null || x.Count != y.Count)
                    return false;
                return !x.Where((t, index) => t != y[index]).Any();
            }
    
            private static TomlTable CombineNested(TomlTable target, TomlTable tomlTable, Func<ICommentOperationOrRowSelector, ITableCombiner> operation)
            {
                var table = TomlTable.Combine(s => operation(s.Overwrite(target).With(tomlTable)));
                foreach (var (key, value) in target.Rows)
                {
                    if (key != null && value?.TomlType == TomlObjectType.Table && tomlTable.ContainsKey(key))
                        table[key] = CombineNested(value as TomlTable, tomlTable[key] as TomlTable, operation);
                }
    
                return table;
            }
        }
}