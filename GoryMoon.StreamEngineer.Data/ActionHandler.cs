using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public class ActionHandler : IDisposable
    {
        private static ActionHandler _handler;
        private readonly List<BaseAction> _actions = new List<BaseAction>();
        private readonly Dictionary<string, Type> _actionTypes = new Dictionary<string, Type>();
        private readonly string _fileName;
        private readonly JsonSerializer _jsonSerializer;
        private readonly ILogger _logger;

        private readonly string _path;
        private byte[] _lastEventsHash = new byte[0];

        private FileSystemWatcher _watcher;

        public ActionHandler(string path, string fileName, ILogger logger)
        {
            _handler = this;
            _path = path;
            _fileName = fileName;
            _logger = logger;
            _jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {Context = new StreamingContext(StreamingContextStates.File, this)});
        }

        public static ILogger Logger => _handler._logger;

        public void Dispose()
        {
            _watcher.Dispose();
        }

        public void StartWatching()
        {
            _watcher = new FileSystemWatcher(_path, _fileName);
            _watcher.Changed += (sender, args) =>
            {
                byte[] hash;
                using (var stream = File.Open(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var md5 = MD5.Create())
                    {
                        hash = md5.ComputeHash(stream);
                    }
                }

                if (_lastEventsHash.SequenceEqual(hash) == false)
                {
                    _lastEventsHash = hash;
                    ParseEvents();
                }
            };
            _watcher.EnableRaisingEvents = true;
            ParseEvents();
        }

        public void AddAction(string type, Type action)
        {
            _actionTypes.Add(type, action);
        }

        private void ParseEvents()
        {
            Console.WriteLine("Parsing events");
            try
            {
                var text = new StreamReader(File.Open($"{_path}\\{_fileName}", FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite)).ReadToEnd();
                var data = JArray.Parse(text);
                var actions = new List<BaseAction>();
                foreach (var token in data.Children())
                {
                    var type = (string) token["type"];
                    if (type != null && GetAction(type, token["action"], out var action)) actions.Add(action);
                }

                _actions.Clear();
                _actions.AddRange(actions);
            }
            catch (Exception e)
            {
                _logger.WriteLine(e);
            }
        }

        public bool GetAction(string type, JToken data, out BaseAction action)
        {
            if (type != null && _actionTypes.TryGetValue(type, out var actionType))
                if (data != null)
                {
                    action = (BaseAction) data.ToObject(actionType, _jsonSerializer);
                    return true;
                }

            action = null;
            return false;
        }

        public List<BaseAction> GetActions(Data eventData)
        {
            return _actions.Where(action => action.Test(eventData)).ToList();
        }
    }

    public struct Data
    {
        public EventType Type;
        public int Amount;

        // Twitch sub tier
        public int Tier;
        
        // Twitch channel points
        public string Id;
    }
}