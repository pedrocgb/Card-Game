using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UConsole : MonoBehaviour
{
    #region Variables and Properties
    public static UConsole Instance = null;

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]
    private int _maxLogLines = 100;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private GameObject _consolePanel = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private ScrollRect _consoleScroll = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _consoleText = null;

    private Queue<string> _logLines = new Queue<string>();
    #endregion

    // ========================================================================

    #region Static Methods
    public static void Log(string message)
    {
        if (Instance != null)
            Instance.log(message);
        else
            Debug.LogWarning("UConsole instance is null. Cannot log message.");
    }
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Application.logMessageReceived += HandleLog;
    }
    #endregion

    // ========================================================================

    #region Loop Methods
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _consolePanel.SetActive(!_consolePanel.activeSelf);
        }
    }
    #endregion

    // ========================================================================

    #region Console Methods
    public void log(string message)
    {
        AddLine(message);
    }

    private void HandleLog(string LogString, string StackTrace, LogType Type)
    {
        string color = Type switch
        {
            LogType.Warning => "yellow",
            LogType.Error => "red",
            LogType.Exception => "red",
            _ => "white"
        };

        string formatted = $"<color={color}>[{Type}] {LogString}</color>";
        AddLine(formatted);
    }

    private void AddLine(string Line)
    {
        bool shouldAutoScroll = _consoleScroll.verticalNormalizedPosition <= 0.01f;

        _logLines.Enqueue(Line);
        if (_logLines.Count > _maxLogLines)
            _logLines.Dequeue();

        _consoleText.text = string.Join("\n", _logLines);

        Canvas.ForceUpdateCanvases();

        if (shouldAutoScroll)
            _consoleScroll.verticalNormalizedPosition = 0f;
    }
    #endregion

    // ========================================================================
}
