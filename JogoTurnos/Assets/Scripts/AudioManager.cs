using UnityEngine;

/// <summary>
/// Gerencia os efeitos sonoros do jogo.
/// Sons gerados proceduralmente — não requer assets externos.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configurações")]
    [Range(0f, 1f)] public float volume = 0.6f;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _source = GetComponent<AudioSource>();
        _source.volume = volume;
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCellChanged.AddListener(OnCellChanged);
        GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCellChanged.RemoveListener(OnCellChanged);
        GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
    }

    private void OnCellChanged(int index, GameManager.Player player)
        => PlayClick(player == GameManager.Player.X ? 880f : 660f);

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.PlayerXWon ||
            state == GameManager.GameState.PlayerOWon)
            PlayWin();
        else if (state == GameManager.GameState.Draw)
            PlayDraw();
    }

    // ─────────────────────────────────────────────────────────────
    //  Sons procedurais (sem assets externos necessários)
    // ─────────────────────────────────────────────────────────────

    private void PlayClick(float frequency)
    {
        AudioClip clip = GenerateTone(frequency, 0.08f, decay: 8f);
        _source.PlayOneShot(clip, volume);
    }

    private void PlayWin()
    {
        // Acorde ascendente
        float[] notes = { 523f, 659f, 784f, 1047f };
        float delay = 0f;
        foreach (float note in notes)
        {
            AudioClip clip = GenerateTone(note, 0.15f, decay: 3f);
            // Unity não suporta PlayScheduled facilmente sem AudioSettings; usamos corrotina
            StartCoroutine(PlayDelayed(clip, delay));
            delay += 0.12f;
        }
    }

    private void PlayDraw()
    {
        float[] notes = { 523f, 494f, 440f };
        float delay = 0f;
        foreach (float note in notes)
        {
            AudioClip clip = GenerateTone(note, 0.18f, decay: 4f);
            StartCoroutine(PlayDelayed(clip, delay));
            delay += 0.14f;
        }
    }

    private System.Collections.IEnumerator PlayDelayed(AudioClip clip, float delay)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        _source.PlayOneShot(clip, volume);
    }

    private AudioClip GenerateTone(float frequency, float duration, float decay = 5f)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int samples    = Mathf.CeilToInt(sampleRate * duration);
        float[] data   = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t        = (float)i / sampleRate;
            float envelope = Mathf.Exp(-decay * t);
            data[i]        = envelope * Mathf.Sin(2f * Mathf.PI * frequency * t);
        }

        AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
