using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicInventoryUI : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _relicImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicNameText = null;
    private RelicData _myRelic = null;
    #endregion

    // ========================================================================

    #region Initialization Methods
    public void Initialize()
    {
        _myRelic = null;
        _relicImage.sprite = null;
        _relicNameText.text = string.Empty;

        _relicImage.color = Color.clear;
    }

    public void Initialize(RelicData relic)
    {
        _myRelic = relic;     
        _relicImage.sprite = relic.RelicImage;  
        _relicNameText.text = relic.RelicName;  

        _relicImage.color = Color.white;
    }
    #endregion

    // ========================================================================

    public void SelectRelic()
    {
        if (_myRelic == null) return;

        InventoryManager.SelectRelic(_myRelic);
    }

    // ========================================================================
}
