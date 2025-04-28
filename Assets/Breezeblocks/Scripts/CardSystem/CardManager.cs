using Sirenix.OdinInspector;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [FoldoutGroup("Position Icons", expanded: true)]
    [SerializeField]
    private Sprite _frontPosIcon = null;
    public Sprite FrontPositionIcon => _frontPosIcon;
    [FoldoutGroup("Position Icons", expanded: true)]
    [SerializeField]
    private Sprite _midFrontPosIcon = null;
    public Sprite MidFrontPositionIcon => _midFrontPosIcon;
    [FoldoutGroup("Position Icons", expanded: true)]
    [SerializeField]
    private Sprite _midBackPosIcon = null;
    public Sprite MidBackPositionIcon => _midBackPosIcon;
    [FoldoutGroup("Position Icons", expanded: true)]
    [SerializeField]
    private Sprite _backPosIcon = null;
    public Sprite BackPositionIcon => _backPosIcon;

    [FoldoutGroup("Target Icons", expanded: true)]
    [SerializeField]
    private Sprite _frontTargetIcon = null;
    public Sprite FrontTargetIcon => _frontTargetIcon;
    [FoldoutGroup("Target Icons", expanded: true)]
    [SerializeField]
    private Sprite _midFrontTargetIcon = null;
    public Sprite MidFrontTargetIcon => _midFrontTargetIcon;
    [FoldoutGroup("Target Icons", expanded: true)]
    [SerializeField]
    private Sprite _midBackTargetIcon = null;
    public Sprite MidBackTargetIcon => _midBackTargetIcon;
    [FoldoutGroup("Target Icons", expanded: true)]
    [SerializeField]
    private Sprite _backTargetIcon = null;
    public Sprite BackTargetIcon => _backTargetIcon;
}
