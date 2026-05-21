using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Stub de NetworkManager para futura integração TCP.
/// 
/// COMO USAR:
///   1. Ative networkMode = true no GameManager.
///   2. Defina o IP/Porta do servidor.
///   3. Chame Connect() para estabelecer a conexão.
///   4. O NetworkManager encaminhará as jogadas recebidas ao GameManager.
///
/// PROTOCOLO ESPERADO (simples):
///   Cliente → Servidor: "MOVE:index\n"      ex: "MOVE:4\n"
///   Servidor → Cliente: "MOVE:index:player\n"  ex: "MOVE:4:2\n"
///   Ambos    → Ambos:   "RESET\n"
///   Ambos    → Ambos:   "STATE:json\n"
/// </summary>
public class NetworkManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Singleton
    // ─────────────────────────────────────────────────────────────
    public static NetworkManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────────────────────
    [Header("Configurações TCP")]
    public string serverIP   = "127.0.0.1";
    public int    serverPort = 7777;

    [Header("Status (somente leitura)")]
    [SerializeField] private bool _isConnected = false;
    public bool IsConnected => _isConnected;

    // ─────────────────────────────────────────────────────────────
    //  Internals
    // ─────────────────────────────────────────────────────────────
    private TcpClient   _client;
    private NetworkStream _stream;
    private Thread      _receiveThread;
    private readonly System.Collections.Generic.Queue<string> _messageQueue
        = new System.Collections.Generic.Queue<string>();
    private readonly object _queueLock = new object();

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Processa mensagens recebidas na thread principal (Unity não é thread-safe)
        lock (_queueLock)
        {
            while (_messageQueue.Count > 0)
                ProcessMessage(_messageQueue.Dequeue());
        }
    }

    private void OnDestroy() => Disconnect();

    // ─────────────────────────────────────────────────────────────
    //  API Pública
    // ─────────────────────────────────────────────────────────────

    public void Connect()
    {
        if (_isConnected) return;
        try
        {
            _client = new TcpClient(serverIP, serverPort);
            _stream = _client.GetStream();
            _isConnected = true;

            _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            _receiveThread.Start();

            Debug.Log($"[NetworkManager] Conectado a {serverIP}:{serverPort}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkManager] Falha ao conectar: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        _isConnected = false;
        _receiveThread?.Abort();
        _stream?.Close();
        _client?.Close();
        Debug.Log("[NetworkManager] Desconectado.");
    }

    /// <summary>Envia a jogada do jogador local ao servidor.</summary>
    public void SendMove(int cellIndex)
    {
        if (!_isConnected) return;
        Send($"MOVE:{cellIndex}");
    }

    /// <summary>Envia sinal de reinício de jogo.</summary>
    public void SendReset()
    {
        if (!_isConnected) return;
        Send("RESET");
    }

    // ─────────────────────────────────────────────────────────────
    //  I/O TCP
    // ─────────────────────────────────────────────────────────────

    private void Send(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            _stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkManager] Erro ao enviar: {ex.Message}");
            _isConnected = false;
        }
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        StringBuilder sb = new StringBuilder();

        while (_isConnected)
        {
            try
            {
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) { _isConnected = false; break; }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                // Processa mensagens delimitadas por '\n'
                string raw = sb.ToString();
                int idx;
                while ((idx = raw.IndexOf('\n')) >= 0)
                {
                    string msg = raw.Substring(0, idx).Trim();
                    raw = raw.Substring(idx + 1);
                    if (msg.Length > 0)
                        lock (_queueLock) { _messageQueue.Enqueue(msg); }
                }
                sb.Clear();
                sb.Append(raw);
            }
            catch (Exception ex)
            {
                if (_isConnected)
                    Debug.LogError($"[NetworkManager] Erro ao receber: {ex.Message}");
                _isConnected = false;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Processamento de mensagens (thread principal)
    // ─────────────────────────────────────────────────────────────

    private void ProcessMessage(string msg)
    {
        Debug.Log($"[NetworkManager] Recebido: {msg}");

        if (msg.StartsWith("MOVE:"))
        {
            // Formato: "MOVE:index:player"
            string[] parts = msg.Split(':');
            if (parts.Length >= 3 &&
                int.TryParse(parts[1], out int cellIndex) &&
                int.TryParse(parts[2], out int playerInt))
            {
                GameManager.Player player = (GameManager.Player)playerInt;
                GameManager.Instance?.ApplyNetworkMove(cellIndex, player);
            }
        }
        else if (msg == "RESET")
        {
            GameManager.Instance?.StartGame();
        }
        else if (msg.StartsWith("STATE:"))
        {
            // Futuro: sincronização de estado completo
            string json = msg.Substring(6);
            Debug.Log($"[NetworkManager] Estado recebido: {json}");
            // TODO: deserializar GameStateData e sincronizar
        }
    }
}
