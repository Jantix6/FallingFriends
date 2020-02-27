using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngineInternal.Input;

public class CanvasController : MonoBehaviour {

	public static CanvasController Instance { get; private set; }
    public static int _numberOfWins;
    public static float _speedMultiplier;

	public GameObject[] _playersPreview;
	public GameObject[] _camerasPreview;

	public Dropdown _numberOfPlayers;
	public Dropdown _typeOfControlls;
    public Dropdown _numberOfWinsDropdown;
    public Dropdown _gameSpeedDropdown;
	public static bool _playingKeyboard = true;
    public static GameObject _buttonForSwitchCanvasControl;
	
	public GameObject _mainMenuScreenCanvas;
	public GameObject _startScreenCanvas;
	public GameObject _endRoundCanvas;
	public Button _startGameButton;
    public Button _firstStartGameButton;

    private bool _player1Ready = false, _player2Ready = false, _player3Ready = false, _player4Ready = false;
	private int _numOfPlayersReady = 0;
	private GameObject[] _playersPreviewEndRound;

    [SerializeField] private EventSystem _eventSystemController;
    [SerializeField] private GameObject _buttonAfterPlayerReady;
    [SerializeField] private GameObject _buttonOfEndTournament;
    [SerializeField] private Transform _playersPreviewParent;
	[SerializeField] private TextMeshProUGUI[] _readyPlayerTexts;
	private TextMeshProUGUI[] _readyPlayerTextsFinalRound = new TextMeshProUGUI[4];
    [SerializeField] private GameObject[] _buttonsToGetReadyInPlayersStartScreen;
    private GameObject[] _buttonsToGetReadyBetweenRounds = new GameObject[4];

    public GameObject _PreviewsPlayersObject;
    public Animator[] _playersPreviewsAnimators;
    public GameObject[] _endOfRoundParticles;
    public GameObject[] _endOfGameParticles;


    private void Awake()
	{
		if (Instance == null)
			Instance = this;

        _buttonForSwitchCanvasControl = _buttonOfEndTournament;
    }

    private void Start()
    {
        _numberOfPlayers.onValueChanged.AddListener(ChangePlayerPreview);
        _typeOfControlls.onValueChanged.AddListener(ChangePlayerControls);
    }

    public void StartScreenUpdate()
    {
        if (Input.anyKeyDown)
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
            SwitchCanvasButtonController(_firstStartGameButton.gameObject);
        }
    }

    public void PlayersPreviewScreenUpdate()
    {
        if (_numOfPlayersReady == _numberOfPlayers.value + 2)
        {
            _startGameButton.enabled = true;
            SwitchCanvasButtonController(_buttonAfterPlayerReady);
        }
        else
            _startGameButton.enabled = false;

        if (_playingKeyboard)
		    UpdateKeyboardReadyButtons(_playersPreview, _readyPlayerTexts, _buttonsToGetReadyInPlayersStartScreen);
        else
            UpdateControllerReadyButtons(_playersPreview, _readyPlayerTexts, _buttonsToGetReadyInPlayersStartScreen);
    }

    public void EndRoundScreenUpdate()
    {
        if (_numOfPlayersReady == _numberOfPlayers.value + 2)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
            GameplayController.Instance.RestartGame();
        }

        if (_playingKeyboard)
		    UpdateKeyboardReadyButtons(_playersPreviewEndRound, _readyPlayerTextsFinalRound, _buttonsToGetReadyBetweenRounds);
        else
            UpdateControllerReadyButtons(_playersPreviewEndRound, _readyPlayerTextsFinalRound, _buttonsToGetReadyBetweenRounds);
    }

    private void UpdateKeyboardReadyButtons(GameObject[] playersPreview, TextMeshProUGUI[] playerReadyTexts, GameObject[] readyButtonsHelp)
    {
        foreach (var button in readyButtonsHelp)
        {
            button.transform.GetChild(0).gameObject.SetActive(true);
            button.transform.GetChild(1).gameObject.SetActive(false);
        }

	    if (Input.GetKeyDown(KeyCode.LeftShift) && !_player1Ready && playersPreview[0].activeSelf) {
		    _player1Ready = true;
			playerReadyTexts[0].enabled = true;
            readyButtonsHelp[0].SetActive(false);
            _numOfPlayersReady++;
            _playersPreviewsAnimators[0].Play("WinUnicorn");
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.UnicornSelection);
	    }
	    else if (Input.GetKeyDown(KeyCode.LeftShift) && _player1Ready && playersPreview[0].activeSelf) {
		    _player1Ready = false;
		    playerReadyTexts[0].enabled = false;
            readyButtonsHelp[0].SetActive(true);
            _numOfPlayersReady--;
	    }

	    if (Input.GetKeyDown(KeyCode.RightShift) && !_player2Ready && playersPreview[1].activeSelf) {
		    _player2Ready = true;
			playerReadyTexts[1].enabled = true;
            readyButtonsHelp[1].SetActive(false);
            _numOfPlayersReady++;
            _playersPreviewsAnimators[1].Play("WinBurguer");
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.HamburgerSelection);
	    }
	    else if (Input.GetKeyDown(KeyCode.RightShift) && _player2Ready && playersPreview[1].activeSelf) {
		    _player2Ready = false;
		    playerReadyTexts[1].enabled = false;
            readyButtonsHelp[1].SetActive(true);
            _numOfPlayersReady--;
	    }

	    if (Input.GetKeyDown(KeyCode.Keypad0) && !_player3Ready && playersPreview[2].activeSelf) {
		    _player3Ready = true;
		    playerReadyTexts[2].enabled = true;
            readyButtonsHelp[2].SetActive(false);
            _numOfPlayersReady++;
            _playersPreviewsAnimators[2].Play("WinRobot");
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.RobotSelection);
	    }
	    else if (Input.GetKeyDown(KeyCode.Keypad0) && _player3Ready && playersPreview[2].activeSelf) {
		    _player3Ready = false;
		    playerReadyTexts[2].enabled = false;
            readyButtonsHelp[2].SetActive(true);
            _numOfPlayersReady--;
	    }

	    if (Input.GetKeyDown(KeyCode.B) && !_player4Ready && playersPreview[3].activeSelf) {
		    _player4Ready = true;
		    playerReadyTexts[3].enabled = true;
            readyButtonsHelp[3].SetActive(false);
            _numOfPlayersReady++;
            _playersPreviewsAnimators[3].Play("WinCupcake");
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.CupcakeSelection);
	    }
	    else if (Input.GetKeyDown(KeyCode.B) && _player4Ready && playersPreview[3].activeSelf) {
		    _player4Ready = false;
		    playerReadyTexts[3].enabled = false;
            readyButtonsHelp[3].SetActive(true);
            _numOfPlayersReady--;
	    }
    }

    private void UpdateControllerReadyButtons(GameObject[] playersPreview, TextMeshProUGUI[] playerReadyTexts, GameObject[] readyButtonsHelp)
    {
        foreach (var button in readyButtonsHelp)
        {
            button.transform.GetChild(0).gameObject.SetActive(false);
            button.transform.GetChild(1).gameObject.SetActive(true);
        }

        if (Input.GetButtonDown("AbilityButton0") && !_player1Ready && playersPreview[0].activeSelf)
        {
            _player1Ready = true;
            playerReadyTexts[0].enabled = true;
            readyButtonsHelp[0].SetActive(false);
            _numOfPlayersReady++;
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.UnicornSelection);
        }
        else if (Input.GetButtonDown("AbilityButton0") && _player1Ready && playersPreview[0].activeSelf)
        {
            _player1Ready = false;
            playerReadyTexts[0].enabled = false;
            readyButtonsHelp[0].SetActive(true);
            _numOfPlayersReady--;
        }

        if (Input.GetButtonDown("AbilityButton1") && !_player2Ready && playersPreview[1].activeSelf)
        {
            _player2Ready = true;
            playerReadyTexts[1].enabled = true;
            readyButtonsHelp[1].SetActive(false);
            _numOfPlayersReady++;
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.HamburgerSelection);
        }
        else if (Input.GetButtonDown("AbilityButton1") && _player2Ready && playersPreview[1].activeSelf)
        {
            _player2Ready = false;
            playerReadyTexts[1].enabled = false;
            readyButtonsHelp[1].SetActive(true);
            _numOfPlayersReady--;
        }

        if (Input.GetButtonDown("AbilityButton2") && !_player3Ready && playersPreview[2].activeSelf)
        {
            _player3Ready = true;
            playerReadyTexts[2].enabled = true;
            readyButtonsHelp[2].SetActive(false);
            _numOfPlayersReady++;
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.RobotSelection);
        }
        else if (Input.GetButtonDown("AbilityButton2") && _player3Ready && playersPreview[2].activeSelf)
        {
            _player3Ready = false;
            playerReadyTexts[2].enabled = false;
            readyButtonsHelp[2].SetActive(true);
            _numOfPlayersReady--;
        }

        if (Input.GetButtonDown("AbilityButton3") && !_player4Ready && playersPreview[3].activeSelf)
        {
            _player4Ready = true;
            playerReadyTexts[3].enabled = true;
            readyButtonsHelp[3].SetActive(false);
            _numOfPlayersReady++;
            SoundManager.Instance.PlayOneShot(SoundManager.Instance.CupcakeSelection);
        }
        else if (Input.GetButtonDown("AbilityButton3") && _player4Ready && playersPreview[3].activeSelf)
        {
            _player4Ready = false;
            playerReadyTexts[3].enabled = false;
            readyButtonsHelp[3].SetActive(true);
            _numOfPlayersReady--;
        }
    }

    public void ResetReadyPlayerState()
    {
	    _player1Ready = false;
	    _player2Ready = false;
	    _player3Ready = false;
	    _player4Ready = false;
	    _numOfPlayersReady = 0;
	    foreach (var text in _readyPlayerTexts)
	    {
		    text.enabled = false;
	    }
        foreach (var text in _readyPlayerTextsFinalRound)
        {
            text.enabled = false;
        }

        foreach (var button in _buttonsToGetReadyInPlayersStartScreen)
        {
            button.SetActive(true);
        }
        foreach (var button in _buttonsToGetReadyBetweenRounds)
        {
            button.SetActive(true);
        }
    }
    
	private void ChangePlayerPreview(int numOfPlayers)
	{
		for (int i = 2; i < _playersPreview.Length; i++)
		{
			_camerasPreview[i].SetActive(false);
			_playersPreview[i].SetActive(false);
		}
		for (int i = 2; i < numOfPlayers+2; i++)
		{
			_camerasPreview[i].SetActive(true);
            _playersPreview[i].SetActive(true);
		}
	}
	
	private void ChangePlayerControls(int typeControl)
	{
		if (_typeOfControlls.value == 0)
			_playingKeyboard = true;
		else
			_playingKeyboard = false;
	}

	public void AssignPlayersPreviewInEndRoundCanvas()
	{
        _playersPreviewEndRound = new GameObject[_playersPreview.Length];

		for (int i = 0; i < _playersPreviewParent.childCount; i++)
		{
			Destroy(_playersPreviewParent.GetChild(i).gameObject);
		}

		for (int i = 0; i < _playersPreview.Length; i++)
		{
			_playersPreviewEndRound[i] = Instantiate(_playersPreview[i], _playersPreviewParent);
			_readyPlayerTextsFinalRound[i] = _playersPreviewEndRound[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            _buttonsToGetReadyBetweenRounds[i] = _playersPreviewEndRound[i].transform.GetChild(3).gameObject;
            _buttonsToGetReadyBetweenRounds[i].SetActive(true);
        }
	}

    public void SwitchCanvasButtonController(GameObject button)
    {
        _eventSystemController.SetSelectedGameObject(button, null);
    }

    public void AssignGameplayValues()
    {
        switch (_numberOfWinsDropdown.value)
        {
            case 0:
                _numberOfWins = 3;
                break;
            case 1:
                _numberOfWins = 5;
                break;
            case 2:
                _numberOfWins = 7;
                break;
        }

        switch (_gameSpeedDropdown.value)
        {
            case 0:
                _speedMultiplier = 0.7f;
                break;
            case 1:
                _speedMultiplier = 1.0f;
                break;
            case 2:
                _speedMultiplier = 1.3f;
                break;
        }
    }
    
}
