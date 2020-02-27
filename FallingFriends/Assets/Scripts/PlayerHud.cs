using TMPro;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{

    //[SerializeField] private TextMeshProUGUI _playerIdText;
    //[SerializeField] private TextMeshProUGUI _currentAbilityText;
    [SerializeField] private TextMeshProUGUI _winsText;
    //[SerializeField] private Image _panelImage;
    [SerializeField] private RawImage _pickedAbility;
    [SerializeField] private RenderTexture _missileSprite;
    [SerializeField] private RenderTexture _bombSprite;
    [SerializeField] private RenderTexture _mineSprite;
    [SerializeField] private Animator _headAnimator;

    private Player _myPlayer;
    private float _timeLeftPickAnimation = 2.0f;
    private bool _animIsPlaying;
    
    public void Init(int playerId, Player thisPlayer)
    {
        _myPlayer = thisPlayer;
        gameObject.SetActive(true);
        SetCurrentAbility();
        SetWins();
        /*_playerIdText.text = "Player Id : " + playerId;
        switch (playerId)
        {
            case 0:
                _panelImage.color = Color.yellow;
                break;
            case 1:
                _panelImage.color = Color.blue;
                break;
            case 2:
                _panelImage.color = Color.magenta;
                break;
            case 3:
                _panelImage.color = Color.green;
                break;
        }*/
    }

    private void Update()
    {
        if (_animIsPlaying)
            _timeLeftPickAnimation -= Time.deltaTime;
        
        if (_timeLeftPickAnimation < 0.0f)
            EndPickedUpAbility();
    }

    public void SetCurrentAbility(AbilityType abilityType = AbilityType.None)
    {
        //_currentAbilityText.text = "Current Ability : " + abilityType;

        switch (abilityType)
        {
            case AbilityType.Bomb:
                _pickedAbility.enabled = true;
                _pickedAbility.texture = _bombSprite;
                _myPlayer.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                _myPlayer.gameObject.transform.GetChild(1).GetChild(1).GetComponent<RawImage>().texture = _bombSprite;
                _animIsPlaying = true;
                break;
            case AbilityType.Missile:
                _pickedAbility.enabled = true;
                _pickedAbility.texture = _missileSprite;
                _myPlayer.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                _myPlayer.gameObject.transform.GetChild(1).GetChild(1).GetComponent<RawImage>().texture = _missileSprite;
                _animIsPlaying = true;
                break;
            case AbilityType.Mine:
                _pickedAbility.enabled = true;
                _pickedAbility.texture = _mineSprite;
                _myPlayer.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                _myPlayer.gameObject.transform.GetChild(1).GetChild(1).GetComponent<RawImage>().texture = _mineSprite;
                _animIsPlaying = true;
                break;
            default:
                _pickedAbility.enabled = false;
                _pickedAbility.texture = null;
                break;
        }
    }

    public void EndPickedUpAbility()
    {
        _timeLeftPickAnimation = 2.0f;
        _animIsPlaying = false;
        _myPlayer.gameObject.transform.GetChild(1).gameObject.SetActive(false);
        _myPlayer.gameObject.transform.GetChild(1).GetChild(1).GetComponent<RawImage>().texture = null;
    }

    public void SetWins(int wins = 0)
    {
        _winsText.text = " " + wins;//WINS :
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Reactivate()
    {
        gameObject.SetActive(true);
        SetCurrentAbility();
    }

    public void PlayTakeAbilityAnimation()
    {
        _headAnimator.Play("AbilityTake");
    }
    
}