using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia todos os elementos de interface:
/// painel de status, placar, botão de restart e tela de resultado.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Inspector — Textos de status
    // ─────────────────────────────────────────────────────────────
    [Header("Status do Turno")]
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private Image _turnIndicatorX;
    [SerializeField] private Image _turnIndicatorO;

    [Header("Placar")]
    [SerializeField] private TextMeshProUGUI _scoreXText;
    [SerializeField] private TextMeshProUGUI _scoreOText;
    [SerializeField] private TextMeshProUGUI _scoreDrawText;

    [Header("Painel de Resultado")]
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Image _resultBackground;

    [Header("Botões")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _restartFromResultButton;

    [Header("Cores")]
    [SerializeField] private Color _xColor  = new Color(0.93f, 0.35f, 0.35f);
    [SerializeField] private Color _oColor  = new Color(0.35f, 0.75f, 0.93f);
    [SerializeField] private Color _drawColor = new Color(0.85f, 0.85f, 0.85f);

    // ─────────────────────────────────────────────────────────────
    //  Placar interno
    // ─────────────────────────────────────────────────────────────
    private int _scoreX   = 0;
    private int _scoreO   = 0;
    private int _scoreDraw = 0;

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _restartButton?.onClick.AddListener(OnRestartClicked);
        _restartFromResultButton?.onClick.AddListener(OnRestartClicked);
        _resultPanel?.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        GameManager.Instance.OnBoardReset.AddListener(OnBoardReset);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
        GameManager.Instance.OnBoardReset.RemoveListener(OnBoardReset);
    }

    // ─────────────────────────────────────────────────────────────
    //  Handlers
    // ─────────────────────────────────────────────────────────────

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.PlayerXTurn:
                SetTurnUI("Vez de X", _xColor, activeX: true);
                break;

            case GameManager.GameState.PlayerOTurn:
                SetTurnUI("Vez de O", _oColor, activeX: false);
                break;

            case GameManager.GameState.PlayerXWon:
                _scoreX++;
                UpdateScoreUI();
                ShowResult("X Venceu!", _xColor);
                break;

            case GameManager.GameState.PlayerOWon:
                _scoreO++;
                UpdateScoreUI();
                ShowResult("O Venceu!", _oColor);
                break;

            case GameManager.GameState.Draw:
                _scoreDraw++;
                UpdateScoreUI();
                ShowResult("Empate!", _drawColor);
                break;
        }
    }

    private void OnBoardReset(GameManager.Player[] board)
    {
        _resultPanel?.SetActive(false);
        SetTurnUI("Vez de X", _xColor, activeX: true);
    }

    // ─────────────────────────────────────────────────────────────
    //  Botão Restart
    // ─────────────────────────────────────────────────────────────

    private void OnRestartClicked()
    {
        GameManager.Instance?.StartGame();
    }

    // ─────────────────────────────────────────────────────────────
    //  Helpers de UI
    // ─────────────────────────────────────────────────────────────

    private void SetTurnUI(string message, Color color, bool activeX)
    {
        if (_turnText)
        {
            _turnText.text  = message;
            _turnText.color = color;
        }

        float alphaActive   = 1f;
        float alphaInactive = 0.25f;

        if (_turnIndicatorX)
            _turnIndicatorX.color = new Color(
                _xColor.r, _xColor.g, _xColor.b,
                activeX ? alphaActive : alphaInactive);

        if (_turnIndicatorO)
            _turnIndicatorO.color = new Color(
                _oColor.r, _oColor.g, _oColor.b,
                activeX ? alphaInactive : alphaActive);
    }

    private void ShowResult(string message, Color bgColor)
    {
        if (_resultPanel)  _resultPanel.SetActive(true);
        if (_resultText)   _resultText.text = message;
        if (_resultBackground) _resultBackground.color = bgColor;
    }

    private void UpdateScoreUI()
    {
        if (_scoreXText)    _scoreXText.text    = _scoreX.ToString();
        if (_scoreOText)    _scoreOText.text    = _scoreO.ToString();
        if (_scoreDrawText) _scoreDrawText.text = _scoreDraw.ToString();
    }
}
