using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private static readonly int ThresholdShaderProp = Shader.PropertyToID("_Threshold");

    [Header("Gameplay UI")]

    [SerializeField]
    private TMP_Text _timerText;

    [SerializeField]
    private TMP_Text _scoreText;

    [SerializeField]
    private TMP_Text _currentComboText;

    [SerializeField]
    private GameObject _gameplayScoreGroup;

    [SerializeField]
    private MeshRenderer _comboTimerSlider;

    [Header("Score Screen UI")]

    [SerializeField]
    private GameObject _timeText;

    [SerializeField]
    private TMP_Text _maxComboText;

    [SerializeField]
    private TMP_Text _scoreScreenScoreText;

    [SerializeField]
    private CinemachineCamera _scoreScreenCamera;

    private MaterialPropertyBlock _sliderBlock;

    private void Awake()
    {
        _sliderBlock = new MaterialPropertyBlock();
    }

    public void UpdateUI(GameState state)
    {
        _timerText.text = $"{state.TimeLeft:f0}s";
        _scoreText.text = state.Score.ToString();
        _gameplayScoreGroup.SetActive(state.Phase.IsGameplayPhase());
        _currentComboText.text = state.CurrentCombo.ToString();

        _scoreScreenCamera.enabled = state.Phase is GamePhase.ScoreScreen;
        _timeText.SetActive(state.Phase is GamePhase.ScoreScreen);
        _scoreScreenScoreText.text = state.Score.ToString();
        _maxComboText.text = state.ComboHigh.ToString();

        _sliderBlock.SetFloat(ThresholdShaderProp, state.ComboTimerRemaining / state.Setup.InitialComboTimer);
        _comboTimerSlider.SetPropertyBlock(_sliderBlock);
    }
}