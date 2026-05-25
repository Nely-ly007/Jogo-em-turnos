using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gerencia o estado principal do Jogo da Velha.
/// Preparado para integração futura com comunicação TCP.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Singleton
    // ─────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────
    //  Enums
    // ─────────────────────────────────────────────────────────────
    public enum Player { None = 0, X = 1, O = 2 }

    public enum GameState
    {
        WaitingToStart,
        PlayerXTurn,
        PlayerOTurn,
        PlayerXWon,
        PlayerOWon,
        Draw
    }

    // ─────────────────────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────────────────────
    [Header("Configurações")]
    [Tooltip("Habilita modo de rede (desabilita input local do adversário)")]
    public bool networkMode = false;

    [Tooltip("Jogador local quando em modo de rede")]
    public Player localPlayer = Player.X;

    // ─────────────────────────────────────────────────────────────
    //  Eventos (permite desacoplamento com UI e rede)
    // ─────────────────────────────────────────────────────────────
    [Header("Eventos")]
    public UnityEvent<GameState> OnGameStateChanged;
    public UnityEvent<int, Player> OnCellChanged;   // (índice 0-8, jogador)
    public UnityEvent<Player[]> OnBoardReset;

    // ─────────────────────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────────────────────
    private Player[] _board = new Player[9];
    private GameState _state = GameState.WaitingToStart;
    private int _moveCount = 0;

    // Combinações vencedoras (índices do tabuleiro)
    private static readonly int[][] WinConditions =
    {
        new[] { 0, 1, 2 }, // linha 0
        new[] { 3, 4, 5 }, // linha 1
        new[] { 6, 7, 8 }, // linha 2
        new[] { 0, 3, 6 }, // coluna 0
        new[] { 1, 4, 7 }, // coluna 1
        new[] { 2, 5, 8 }, // coluna 2
        new[] { 0, 4, 8 }, // diagonal principal
        new[] { 2, 4, 6 }  // diagonal secundária
    };

    // ─────────────────────────────────────────────────────────────
    //  Propriedades públicas (leitura)
    // ─────────────────────────────────────────────────────────────
    public GameState State => _state;
    public Player CurrentPlayer => _state == GameState.PlayerXTurn ? Player.X
                                 : _state == GameState.PlayerOTurn ? Player.O
                                 : Player.None;
    public Player[] Board => (Player[])_board.Clone();

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("[GameManager] Iniciado. Começando nova partida...");
        StartGame();
    }

    // ─────────────────────────────────────────────────────────────
    //  API Pública
    // ─────────────────────────────────────────────────────────────

    /// <summary>Inicia / reinicia o jogo.</summary>
    public void StartGame()
    {
        _board = new Player[9];
        _moveCount = 0;
        SetState(GameState.PlayerXTurn);
        OnBoardReset?.Invoke(_board);
        Debug.Log("[GameManager] Tabuleiro resetado. Vez do Player X.");
    }

    /// <summary>
    /// Tenta realizar uma jogada na célula indicada.
    /// Retorna true se a jogada foi aceita.
    /// </summary>
    public bool TryMakeMove(int cellIndex)
    {
        if (!IsGameActive())
        {
            Debug.LogWarning($"[GameManager] Jogada ignorada — jogo não está ativo. State: {_state}");
            return false;
        }
        if (cellIndex < 0 || cellIndex > 8) return false;
        if (_board[cellIndex] != Player.None)
        {
            Debug.LogWarning($"[GameManager] Célula {cellIndex} já ocupada!");
            return false;
        }

        // Em modo rede, só aceita jogada do jogador local
        if (networkMode && CurrentPlayer != localPlayer) return false;

        ApplyMove(cellIndex, CurrentPlayer);
        return true;
    }

    /// <summary>
    /// Aplica uma jogada recebida da rede (ignora restrição de jogador local).
    /// Chamado pelo NetworkManager futuro.
    /// </summary>
    public bool ApplyNetworkMove(int cellIndex, Player player)
    {
        if (!IsGameActive()) return false;
        if (cellIndex < 0 || cellIndex > 8) return false;
        if (_board[cellIndex] != Player.None) return false;
        if (CurrentPlayer != player) return false;

        ApplyMove(cellIndex, player);
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    //  Lógica interna
    // ─────────────────────────────────────────────────────────────

    private void ApplyMove(int index, Player player)
    {
        _board[index] = player;
        _moveCount++;

        // Log da jogada no formato solicitado
        int linha  = index / 3 + 1;
        int coluna = index % 3 + 1;
        Debug.Log($"Jogada: ({linha},{coluna}) — Player {player}");

        OnCellChanged?.Invoke(index, player);

        if (CheckWin(player))
        {
            Debug.Log($"★ Player {player} venceu a rodada!");
            SetState(player == Player.X ? GameState.PlayerXWon : GameState.PlayerOWon);
            return;
        }

        if (_moveCount == 9)
        {
            Debug.Log("★ Empate! Nenhum jogador venceu.");
            SetState(GameState.Draw);
            return;
        }

        Player proximo = player == Player.X ? Player.O : Player.X;
        Debug.Log($"[GameManager] Vez do Player {proximo}.");
        SetState(player == Player.X ? GameState.PlayerOTurn : GameState.PlayerXTurn);
    }

    private bool CheckWin(Player player)
    {
        foreach (var combo in WinConditions)
            if (_board[combo[0]] == player &&
                _board[combo[1]] == player &&
                _board[combo[2]] == player)
                return true;
        return false;
    }

    private void SetState(GameState newState)
    {
        _state = newState;
        OnGameStateChanged?.Invoke(_state);
    }

    private bool IsGameActive() =>
        _state == GameState.PlayerXTurn || _state == GameState.PlayerOTurn;

    // ─────────────────────────────────────────────────────────────
    //  Utilitários (úteis para IA ou rede futura)
    // ─────────────────────────────────────────────────────────────

    /// <summary>Serializa o estado atual para envio via TCP.</summary>
    public GameStateData GetStateData() => new GameStateData
    {
        board = (int[])System.Array.ConvertAll(_board, p => (int)p),
        currentPlayer = (int)CurrentPlayer,
        state = (int)_state,
        moveCount = _moveCount
    };
}

// ─────────────────────────────────────────────────────────────────
//  DTO para serialização (JSON / TCP)
// ─────────────────────────────────────────────────────────────────
[System.Serializable]
public class GameStateData
{
    public int[] board;       // 0=None, 1=X, 2=O
    public int currentPlayer;
    public int state;
    public int moveCount;
}