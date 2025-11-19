using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace AbstractConsole
{
    #region Core Data Structures

    public enum ConsoleLogType
    {
        Log,
        Warning,
        Error,
    }

    [Serializable]
    public struct LogEntry
    {
        public DateTime Timestamp;
        public ConsoleLogType Type;
        public string Channel;
        public string Message;
        public string StackTrace;

        public LogEntry(
            ConsoleLogType type,
            string channel,
            string message,
            string stackTrace = null
        )
        {
            Timestamp = DateTime.Now;
            Type = type;
            Channel = channel ?? "default";
            Message = message;
            StackTrace = stackTrace;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] [{Channel}] [{Type}] {Message}";
        }
    }

    public class RingBuffer<T>
    {
        readonly T[] _buffer;
        int _index = 0;
        int _count = 0;

        public int Capacity => _buffer.Length;
        public int Count => _count;

        public RingBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be > 0");
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            _buffer[_index] = item;
            _index = (_index + 1) % _buffer.Length;
            if (_count < _buffer.Length)
                _count++;
        }

        public IEnumerable<T> GetNewestFirst()
        {
            for (int i = 0; i < _count; i++)
            {
                int idx = (_index - 1 - i);
                if (idx < 0)
                    idx += _buffer.Length;
                yield return _buffer[idx];
            }
        }

        public IEnumerable<T> GetOldestFirst()
        {
            int start = (_index - _count);
            while (start < 0)
                start += _buffer.Length;
            for (int i = 0; i < _count; i++)
            {
                int idx = (start + i) % _buffer.Length;
                yield return _buffer[idx];
            }
        }

        public void Clear()
        {
            _index = 0;
            _count = 0;
        }
    }

    #endregion

    #region Console Core Interfaces & Implementation

    public interface IConsole
    {
        event Action<LogEntry> OnNewLog;
        void Log(
            string message,
            ConsoleLogType type = ConsoleLogType.Log,
            string channel = null,
            string stackTrace = null
        );
        void RegisterCommand(IConsoleCommand command);
        bool Execute(string input);
        IEnumerable<LogEntry> Query(Func<LogEntry, bool> predicate = null);
        void ClearLogs();
    }

    public interface IConsoleCommand
    {
        string Name { get; }
        string Description { get; }
        string[] Aliases { get; }
        string Usage { get; }
        void Execute(IConsole runtime, string[] args);
    }

    public class ConsoleCore : IConsole
    {
        public event Action<LogEntry> OnNewLog;

        readonly RingBuffer<LogEntry> _logs;
        readonly Dictionary<string, IConsoleCommand> _commands = new Dictionary<
            string,
            IConsoleCommand
        >(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, string> _aliases = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase
        );

        public ConsoleCore(int logCapacity = 2048)
        {
            _logs = new RingBuffer<LogEntry>(logCapacity);
        }

        public void Log(
            string message,
            ConsoleLogType type = ConsoleLogType.Log,
            string channel = null,
            string stackTrace = null
        )
        {
            var entry = new LogEntry(type, channel, message, stackTrace);
            _logs.Add(entry);
            OnNewLog?.Invoke(entry);
        }

        public void RegisterCommand(IConsoleCommand command)
        {
            if (command == null)
                return;
            if (string.IsNullOrEmpty(command.Name))
                return;
            if (!_commands.ContainsKey(command.Name))
                _commands[command.Name] = command;
            if (command.Aliases != null)
            {
                foreach (var a in command.Aliases)
                {
                    if (string.IsNullOrEmpty(a))
                        continue;
                    _aliases[a] = command.Name;
                }
            }
        }

        public bool Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var tokens = Tokenize(input);
            if (tokens.Length == 0)
                return false;
            var cmdName = tokens[0];

            if (_aliases.TryGetValue(cmdName, out var real))
                cmdName = real;

            if (!_commands.TryGetValue(cmdName, out var command))
            {
                Log($"Unknown command: {cmdName}", ConsoleLogType.Warning, "console");
                return false;
            }

            var args = tokens.Skip(1).ToArray();
            try
            {
                command.Execute(this, args);
            }
            catch (Exception ex)
            {
                Log(
                    $"Command '{command.Name}' threw: {ex.Message}",
                    ConsoleLogType.Error,
                    "console",
                    ex.StackTrace
                );
            }
            return true;
        }

        public IEnumerable<LogEntry> Query(Func<LogEntry, bool> predicate = null)
        {
            var seq = _logs.GetOldestFirst();
            if (predicate == null)
                return seq;
            return seq.Where(predicate);
        }

        public void ClearLogs() => _logs.Clear();

        static string[] Tokenize(string input)
        {
            var list = new List<string>();
            bool inQuote = false;
            var cur = new System.Text.StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }
                if (char.IsWhiteSpace(c) && !inQuote)
                {
                    if (cur.Length > 0)
                    {
                        list.Add(cur.ToString());
                        cur.Clear();
                    }
                    continue;
                }
                cur.Append(c);
            }
            if (cur.Length > 0)
                list.Add(cur.ToString());
            return list.ToArray();
        }
    }

    #endregion

    #region Command Attribute + Auto Registration

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ConsoleCommandAttribute : Attribute
    {
        public string Name { get; }
        public string[] Aliases { get; }

        public ConsoleCommandAttribute(string name, params string[] aliases)
        {
            Name = name;
            Aliases = aliases;
        }
    }

    public static class CommandRegistrar
    {
        public static void RegisterAll(ConsoleCore core)
        {
            var types = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t =>
                    typeof(IConsoleCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface
                );

            foreach (var t in types)
            {
                var attr = t.GetCustomAttribute<ConsoleCommandAttribute>();
                try
                {
                    var instance = (IConsoleCommand)Activator.CreateInstance(t);

                    core.RegisterCommand(instance);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"Failed to instantiate console command {t.FullName}: {ex.Message}"
                    );
                }
            }
        }

        static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }
    }

    #endregion



    #region Editor Integration
#if UNITY_EDITOR
    public class ConsoleEditorWindow : EditorWindow
    {
        ConsoleCore _core;
        Vector2 _scroll;
        string _input = "";

        [MenuItem("Window/Abstract Console")]
        public static void Open()
        {
            var w = GetWindow<ConsoleEditorWindow>("Console");
            w.Show();
        }

        void OnEnable()
        {
            _core = new ConsoleCore(4096);
            CommandRegistrar.RegisterAll(_core);
            Application.logMessageReceived += OnUnityLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= OnUnityLog;
        }

        void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            var t = ConsoleLogType.Log;
            if (type == LogType.Warning)
                t = ConsoleLogType.Warning;
            if (type == LogType.Error || type == LogType.Exception)
                t = ConsoleLogType.Error;
            _core.Log(condition, t, "unity", stackTrace);
            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            _scroll = EditorGUILayout.BeginScrollView(
                _scroll,
                GUILayout.Height(position.height - 50)
            );
            foreach (var e in _core.Query())
            {
                GUIStyle s = new GUIStyle(EditorStyles.label);
                if (e.Type == ConsoleLogType.Error)
                    s.normal.textColor = Color.red;
                else if (e.Type == ConsoleLogType.Warning)
                    s.normal.textColor = Color.yellow;
                EditorGUILayout.LabelField(e.ToString(), s);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            _input = EditorGUILayout.TextField(_input);
            if (GUILayout.Button("Send", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(_input))
                {
                    _core.Log($"> {_input}", ConsoleLogType.Log, "input");
                    _core.Execute(_input);
                    _input = "";
                }
            }
            if (GUILayout.Button("Clear", GUILayout.Width(80)))
                _core.ClearLogs();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
#endif
    #endregion
}
