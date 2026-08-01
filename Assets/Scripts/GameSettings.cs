using UnityEngine;
using UnityEngine.Audio;

public enum GameDifficulty
{
    Easy,
    Normal,
    Hardcore
}

/// <summary>
/// Zentraler Zugriff auf Spielstand und Einstellungen.
/// Alle Properties lesen und schreiben direkt in PlayerPrefs.
/// </summary>
public sealed class GameSettings
{
    private const string PlayerNameKey = "PlayerName";
    private const string CurrentLevelKey = "CurrentLevel";
    private const string ScoreKey = "Score";
    private const string PlaytimeKey = "Playtime";

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private const string EasyDifficultyKey = "EasyDifficulty";
    private const string NormalDifficultyKey = "NormalDifficulty";
    private const string HardcoreDifficultyKey = "HardCoreDifficulty";

    private const string ShowHudKey = "ShowHUD";
    private const string ShowStatusbarKey = "ShowStatusbar";
    private const string FullscreenKey = "Fullscreen";

    private const string ResolutionWidthKey = "ResolutionWidth";
    private const string ResolutionHeightKey = "ResolutionHeight";
    private const string LastSceneIndexKey = "LastSceneIndex";

    private const string MusicMixerParameter = "MusicVolume";
    private const string SfxMixerParameter = "SfxVolume";

    private static GameSettings instance;

    /// <summary>
    /// Liefert immer dieselbe GameSettings-Instanz.
    /// Beim ersten Aufruf werden fehlende PlayerPrefs mit Standardwerten angelegt.
    /// </summary>
    public static GameSettings Load()
    {
        if (instance == null)
        {
            instance = new GameSettings();
        }

        return instance;
    }

    private GameSettings()
    {
    }

    public string PlayerName
    {
        get => PlayerPrefs.GetString(PlayerNameKey, "Player");
        set => PlayerPrefs.SetString(PlayerNameKey, string.IsNullOrWhiteSpace(value) ? "Player" : value.Trim());
    }

    public int CurrentLevel
    {
        get => PlayerPrefs.GetInt(CurrentLevelKey, 1);
        set => PlayerPrefs.SetInt(CurrentLevelKey, Mathf.Max(1, value));
    }

    public int Score
    {
        get => PlayerPrefs.GetInt(ScoreKey, 0);
        set => PlayerPrefs.SetInt(ScoreKey, Mathf.Max(0, value));
    }

    public float Playtime
    {
        get => PlayerPrefs.GetFloat(PlaytimeKey, 0f);
        set => PlayerPrefs.SetFloat(PlaytimeKey, Mathf.Max(0f, value));
    }

    public float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
    }

    public float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        set => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
    }

    public GameDifficulty GameDifficulty
    {
        get
        {
            if (PlayerPrefs.GetInt(HardcoreDifficultyKey, 0) == 1)
                return GameDifficulty.Hardcore;

            if (PlayerPrefs.GetInt(NormalDifficultyKey, 1) == 1)
                return GameDifficulty.Normal;

            return GameDifficulty.Easy;
        }
        set
        {
            PlayerPrefs.SetInt(
                EasyDifficultyKey,
                value == GameDifficulty.Easy ? 1 : 0);

            PlayerPrefs.SetInt(
                NormalDifficultyKey,
                value == GameDifficulty.Normal ? 1 : 0);

            PlayerPrefs.SetInt(
                HardcoreDifficultyKey,
                value == GameDifficulty.Hardcore ? 1 : 0);
        }
    }

    public bool ShowHud
    {
        get => PlayerPrefs.GetInt(ShowHudKey, 1) == 1;
        set => PlayerPrefs.SetInt(ShowHudKey, value ? 1 : 0);
    }

    public bool ShowStatusbar
    {
        get => PlayerPrefs.GetInt(ShowStatusbarKey, 1) == 1;
        set => PlayerPrefs.SetInt(ShowStatusbarKey, value ? 1 : 0);
    }

    public bool IsFullscreen
    {
        get => PlayerPrefs.GetInt(
            FullscreenKey,
            Screen.fullScreen ? 1 : 0) == 1;

        set => PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
    }

    public int ResolutionWidth
    {
        get => PlayerPrefs.GetInt(
            ResolutionWidthKey,
            Screen.currentResolution.width);

        set => PlayerPrefs.SetInt(
            ResolutionWidthKey,
            Mathf.Max(1, value));
    }

    public int ResolutionHeight
    {
        get => PlayerPrefs.GetInt(
            ResolutionHeightKey,
            Screen.currentResolution.height);

        set => PlayerPrefs.SetInt(
            ResolutionHeightKey,
            Mathf.Max(1, value));
    }

    public int LastSceneIndex
    {
        get => PlayerPrefs.GetInt(LastSceneIndexKey, 0);
        set => PlayerPrefs.SetInt(LastSceneIndexKey, Mathf.Max(0, value));
    }

    /// <summary>
    /// Legt ausschließlich fehlende PlayerPrefs an.
    /// Bereits gespeicherte Werte werden nicht überschrieben.
    /// </summary>
    public void Initialize()
    {
        SetDefault(PlayerNameKey, "Player");
        SetDefault(CurrentLevelKey, 1);
        SetDefault(ScoreKey, 0);
        SetDefault(PlaytimeKey, 0f);

        SetDefault(MusicVolumeKey, 1f);
        SetDefault(SfxVolumeKey, 1f);

        SetDefault(EasyDifficultyKey, 0);
        SetDefault(NormalDifficultyKey, 1);
        SetDefault(HardcoreDifficultyKey, 0);

        SetDefault(ShowHudKey, 1);
        SetDefault(ShowStatusbarKey, 1);
        SetDefault(FullscreenKey, Screen.fullScreen ? 1 : 0);

        SetDefault(
            ResolutionWidthKey,
            Screen.currentResolution.width);

        SetDefault(
            ResolutionHeightKey,
            Screen.currentResolution.height);

        SetDefault(LastSceneIndexKey, 0);

        NormalizeDifficulty();
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Überträgt die gespeicherten Bildschirm- und Audioeinstellungen auf Unity.
    /// </summary>
    public void UseSettings(AudioMixer audioMixer)
    {
        int width = Mathf.Max(1, ResolutionWidth);
        int height = Mathf.Max(1, ResolutionHeight);

        Screen.SetResolution(
            width,
            height,
            IsFullscreen);

        if (audioMixer == null)
            return;

        audioMixer.SetFloat(
            MusicMixerParameter,
            LinearToDb(MusicVolume));

        audioMixer.SetFloat(
            SfxMixerParameter,
            LinearToDb(SfxVolume));
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Setzt nur die von GameSettings verwalteten Werte zurück.
    /// Andere PlayerPrefs im Projekt bleiben erhalten.
    /// </summary>
    public void Reset()
    {
        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.DeleteKey(CurrentLevelKey);
        PlayerPrefs.DeleteKey(ScoreKey);
        PlayerPrefs.DeleteKey(PlaytimeKey);

        PlayerPrefs.DeleteKey(MusicVolumeKey);
        PlayerPrefs.DeleteKey(SfxVolumeKey);

        PlayerPrefs.DeleteKey(EasyDifficultyKey);
        PlayerPrefs.DeleteKey(NormalDifficultyKey);
        PlayerPrefs.DeleteKey(HardcoreDifficultyKey);

        PlayerPrefs.DeleteKey(ShowHudKey);
        PlayerPrefs.DeleteKey(ShowStatusbarKey);
        PlayerPrefs.DeleteKey(FullscreenKey);

        PlayerPrefs.DeleteKey(ResolutionWidthKey);
        PlayerPrefs.DeleteKey(ResolutionHeightKey);
        PlayerPrefs.DeleteKey(LastSceneIndexKey);

        Initialize();
    }

    private static float LinearToDb(float value)
    {
        return value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;
    }

    private void NormalizeDifficulty()
    {
        bool easy = PlayerPrefs.GetInt(EasyDifficultyKey, 0) == 1;
        bool normal = PlayerPrefs.GetInt(NormalDifficultyKey, 1) == 1;
        bool hardcore = PlayerPrefs.GetInt(HardcoreDifficultyKey, 0) == 1;

        int selectedCount =
            (easy ? 1 : 0) +
            (normal ? 1 : 0) +
            (hardcore ? 1 : 0);

        if (selectedCount == 1)
            return;

        GameDifficulty = global::GameDifficulty.Normal;
    }

    private static void SetDefault(string key, int value)
    {
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, value);
    }

    private static void SetDefault(string key, float value)
    {
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetFloat(key, value);
    }

    private static void SetDefault(string key, string value)
    {
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetString(key, value);
    }
}