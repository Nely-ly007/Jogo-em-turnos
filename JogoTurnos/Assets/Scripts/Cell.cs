using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representa uma célula do tabuleiro.
/// Usa TextMeshPro para exibir X e O — sem sprites necessários.
/// </summary>
[RequireComponent(typeof(Button))]
public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ─────────────────────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────────────────────
    [Header("Referências")]
    [SerializeField] private TextMeshProUGUI _symbolText;
    [SerializeField] private Image _backgroundImage;

    [Header("Cores")]
    [SerializeField] private Color _defaultColor = new Color(0.15f, 0.15f, 0.20f);
    [SerializeField] private Color _hoverColor   = new Color(0.22f, 0.22f, 0.30f);
    [SerializeField] private Color _xColor       = new Color(0.93f, 0.35f, 0.35f);
    [SerializeField] private Color _oColor       = new Color(0.35f, 0.75f, 0.93f);
    [SerializeField] private Color _winColor     = new Color(1.00f, 0.85f, 0.20f);

    [Header("Animação")]
    [SerializeField] private float _popDuration  = 0.25f;
    [SerializeField] private AnimationCurve _popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ─────────────────────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────────────────────
    private int _index;
    private Button _button;
    private GameManager.Player _owner = GameManager.Player.None;
    private bool _isWinCell = false;

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        _button.transition = Selectable.Transition.None;
        SetBackground(_defaultColor);

        // Esconde via componente, não desativa o GameObject
        if (_symbolText != null)
        {
            _symbolText.enabled = false;
            _symbolText.transform.localScale = Vector3.one;
        }
    }


    private void OnDestroy()
    {
        if (_button) _button.onClick.RemoveListener(OnClick);
    }

    // ─────────────────────────────────────────────────────────────
    //  API Pública
    // ─────────────────────────────────────────────────────────────

    public void Initialize(int index)
    {
        _index = index;
    }

    public void SetSymbol(GameManager.Player player)
    {
        if (player == GameManager.Player.None) return;

        _owner = player;
        _button.interactable = false;

        if (_symbolText != null)
        {
            _symbolText.text  = player == GameManager.Player.X ? "X" : "O";
            _symbolText.color = player == GameManager.Player.X ? _xColor : _oColor;
            _symbolText.enabled = true; // ← liga o componente, não o GameObject
        }

        StartCoroutine(PopAnimation());
    }
    
    public void HighlightAsWin()
    {
        _isWinCell = true;
        SetBackground(_winColor);
        StartCoroutine(PulseAnimation());
    }

    public void Reset()
    {
        StopAllCoroutines();
        _owner     = GameManager.Player.None;
        _isWinCell = false;
        _button.interactable = true;

        if (_symbolText != null)
        {
            _symbolText.enabled = false; // ← desliga o componente
            _symbolText.transform.localScale = Vector3.one;
        }

        if (_backgroundImage != null)
            _backgroundImage.transform.localScale = Vector3.one;

        SetBackground(_defaultColor);
    }
    // ─────────────────────────────────────────────────────────────
    //  Eventos de mouse
    // ─────────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_owner == GameManager.Player.None && !_isWinCell)
            SetBackground(_hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_owner == GameManager.Player.None && !_isWinCell)
            SetBackground(_defaultColor);
    }

    // ─────────────────────────────────────────────────────────────
    //  Interação
    // ─────────────────────────────────────────────────────────────

    private void OnClick()
    {
        GameManager.Instance?.TryMakeMove(_index);
    }

    // ─────────────────────────────────────────────────────────────
    //  Animações
    // ─────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator PopAnimation()
    {
        if (_symbolText == null) yield break;

        Transform t = _symbolText.transform;
        float elapsed = 0f;

        while (elapsed < _popDuration)
        {
            elapsed += Time.deltaTime;
            float progress = _popCurve.Evaluate(elapsed / _popDuration);
            float scale = Mathf.Lerp(0f, 1f, progress);
            t.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    private System.Collections.IEnumerator PulseAnimation()
    {
        if (_backgroundImage == null) yield break;

        Transform t = _backgroundImage.transform;
        float speed = 2.5f;

        while (_isWinCell)
        {
            float s = 1f + 0.05f * Mathf.Sin(Time.time * speed);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    // ─────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────

    private void SetBackground(Color color)
    {
        if (_backgroundImage) _backgroundImage.color = color;
    }
}
