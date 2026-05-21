using UnityEngine;

/// <summary>
/// Helper de configuração da cena para uso no Editor.
/// Valida referências e exibe dicas no Inspector.
/// Não é necessário em build final.
/// </summary>
public class SceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Checklist de Configuração")]
    [SerializeField] private GameManager    _gameManager;
    [SerializeField] private BoardController _boardController;
    [SerializeField] private UIManager      _uiManager;
    [SerializeField] private AudioManager   _audioManager;
    [SerializeField] private NetworkManager _networkManager;

    private void OnValidate()
    {
        Validate("GameManager",     _gameManager);
        Validate("BoardController", _boardController);
        Validate("UIManager",       _uiManager);
        Validate("AudioManager",    _audioManager);
    }

    private void Validate(string name, Object obj)
    {
        if (obj == null)
            Debug.LogWarning($"[SceneSetup] {name} não está referenciado!", this);
    }
#endif
}
