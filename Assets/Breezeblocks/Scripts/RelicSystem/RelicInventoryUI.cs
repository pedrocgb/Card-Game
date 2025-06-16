using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicInventoryUI : MonoBehaviour
{
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _relicImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicNameText = null;
    private RelicData _myRelic = null;

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]

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

    public void SelectRelic()
    {
        if (_myRelic == null) return;

        InventoryManager.SelectRelic(_myRelic);
    }
}
