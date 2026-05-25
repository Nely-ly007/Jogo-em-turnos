using UnityEngine;

/// <summary>
/// Instancia e coordena as 9 células do tabuleiro.
/// Escuta os eventos do GameManager para atualizar a view.
/// </summary>
public class BoardController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────────────────────
    [Header("Prefab da Célula")]
    [SerializeField] private Cell _cellPrefab;

    [Header("Container do Grid (GridLayoutGroup)")]
    [SerializeField] private Transform _gridContainer;

    // ─────────────────────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────────────────────
    private Cell[] _cells = new Cell[9];

    // Combinações vencedoras — espelhadas aqui para highlight
    private static readonly int[][] WinConditions =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }
    };

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        BuildBoard();
    }

    // REMOVA OnEnable e OnDisable, substitua por:

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[BoardController] GameManager.Instance é null no Start!");
            return;
        }

        GameManager.Instance.OnCellChanged.AddListener(OnCellChanged);
        GameManager.Instance.OnBoardReset.AddListener(OnBoardReset);
        GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

        Debug.Log("[BoardController] Listeners registrados com sucesso.");
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCellChanged.RemoveListener(OnCellChanged);
        GameManager.Instance.OnBoardReset.RemoveListener(OnBoardReset);
        GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCellChanged.RemoveListener(OnCellChanged);
        GameManager.Instance.OnBoardReset.RemoveListener(OnBoardReset);
        GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
    }

    // ─────────────────────────────────────────────────────────────
    //  Construção do tabuleiro
    // ─────────────────────────────────────────────────────────────

    private void BuildBoard()
    {
        // Limpa filhos existentes (útil para reinicializações em editor)
        foreach (Transform child in _gridContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < 9; i++)
        {
            Cell cell = Instantiate(_cellPrefab, _gridContainer);
            cell.name = $"Cell_{i}";
            cell.Initialize(i);
            _cells[i] = cell;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Handlers de eventos
    // ─────────────────────────────────────────────────────────────

    private void OnCellChanged(int index, GameManager.Player player)
    {
        Debug.Log($"[BoardController] OnCellChanged recebido! index={index} player={player}");
        if (index >= 0 && index < 9)
            _cells[index].SetSymbol(player);
    }

    private void OnBoardReset(GameManager.Player[] board)
    {
        foreach (var cell in _cells)
            cell.Reset();
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.PlayerXWon)
            HighlightWinner(GameManager.Player.X);
        else if (state == GameManager.GameState.PlayerOWon)
            HighlightWinner(GameManager.Player.O);
    }

    // ─────────────────────────────────────────────────────────────
    //  Highlight da combinação vencedora
    // ─────────────────────────────────────────────────────────────

    private void HighlightWinner(GameManager.Player winner)
    {
        GameManager.Player[] board = GameManager.Instance.Board;

        foreach (var combo in WinConditions)
        {
            if (board[combo[0]] == winner &&
                board[combo[1]] == winner &&
                board[combo[2]] == winner)
            {
                foreach (int idx in combo)
                    _cells[idx].HighlightAsWin();
                return;
            }
        }
    }
}
