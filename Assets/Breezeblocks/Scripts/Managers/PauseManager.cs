using Sirenix.OdinInspector;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    #region Variables and Properties
    private PauseManager Instance = null;

    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _pausePanel = null;
    #endregion

    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // ========================================================================

    private void TogglePause()
    {
        if (_pausePanel != null)
        {
            bool isActive = _pausePanel.activeSelf;
            _pausePanel.SetActive(!isActive);
            Time.timeScale = isActive ? 1f : 0f;
        }
    }

    // ========================================================================
}
