#region Runtime Unity Integration (Overlay)
using AbstractConsole;
using UnityEngine;
using UnityEngine.InputSystem;

// Attach this MonoBehaviour to a GameObject in a scene (or create via script) to enable the overlay console.
public class ConsoleOverlay : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.BackQuote; // ` ~ key
    public bool startVisible = false;
    public int maxLines = 200;
    public bool captureUnityLogs = true;

    bool cursorVisibleBefore = true;

    ConsoleCore _core;
    bool _visible;
    string _input = "";
    Vector2 _scroll = Vector2.zero;

    void Awake()
    {
        _core = new ConsoleCore(maxLines);
        _visible = startVisible;
        // Register commands by reflection
        CommandRegistrar.RegisterAll(_core);

        if (captureUnityLogs)
            Application.logMessageReceived += OnUnityLog;
        _core.OnNewLog += e =>
        {
            // You could hook other systems here (file logging, analytics, etc.)
        };
    }

    void OnDestroy()
    {
        if (captureUnityLogs)
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
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            _visible = !_visible;

            if (_visible)
            {
                cursorVisibleBefore = Cursor.visible;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Debug.Log("visible");
            }
            else
            {
                Cursor.visible = cursorVisibleBefore;
                Cursor.lockState = cursorVisibleBefore
                    ? CursorLockMode.None
                    : CursorLockMode.Locked;
                Debug.Log("invisible");
            }
        }
        if (!_visible)
            return;
    }

    void OnGUI()
    {
        if (!_visible)
            return;

        var area = new Rect(10, 10, Screen.width - 20, Screen.height / 2);
        GUI.Box(area, "");
        GUILayout.BeginArea(area);
        _scroll = GUILayout.BeginScrollView(_scroll);

        int shown = 0;
        foreach (var e in _core.Query())
        { // newest first
            if (shown++ > 1000)
                break;
            GUI.contentColor =
                (e.Type == ConsoleLogType.Error)
                    ? Color.red
                    : (e.Type == ConsoleLogType.Warning ? Color.yellow : Color.white);
            GUILayout.Label(e.ToString());
        }
        GUI.contentColor = Color.white;

        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();

        _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));

        if (
            GUILayout.Button("Send", GUILayout.Width(80))
            || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return
        )
            ExecuteSend();

        if (GUILayout.Button("Clear", GUILayout.Width(80)))
            _core.ClearLogs();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void ExecuteSend()
    {
        if (!string.IsNullOrWhiteSpace(_input))
        {
            _core.Log($"> {_input}", ConsoleLogType.Log, "input");
            _core.Execute(_input);
            _input = "";
        }
    }
}


#endregion
