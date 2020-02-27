using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [FMODUnity.EventRef]
    public string EasyDifficultMusic = "event:/SFX/Music/Easy Difficulty/Easy Difficulty Music";
    FMOD.Studio.EventInstance easyDifficultMusic;    
    [FMODUnity.EventRef]
    public string MediumDifficultMusic = "event:/SFX/Music/Medium Difficulty/Medium Difficulty Music";
    FMOD.Studio.EventInstance mediumDifficultMusic;    
    [FMODUnity.EventRef]
    public string HardDifficultMusic = "event:/SFX/Music/Hard Diffifulty/Hard Difficulty Music";
    FMOD.Studio.EventInstance hardDifficultMusic;    
    [FMODUnity.EventRef]
    public string MenuMusic = "event:/SFX/Music/Menu/Menu Music";
    FMOD.Studio.EventInstance menuMusic;
    
    [FMODUnity.EventRef]
    public string WinRound = "event:/SFX/Music/Win round/Win Round Music";
    FMOD.Studio.EventInstance winRound;
    [FMODUnity.EventRef]
    public string WinGame = "event:/SFX/Music/Win tournament/Win Tournament Music";
    FMOD.Studio.EventInstance winMusic;
    
    [FMODUnity.EventRef]
    public string CupcakeDeath = "event:/SFX/Characters/Cupcake/Cupcake Death";
    [FMODUnity.EventRef]
    public string CupcakeSelection = "event:/SFX/Characters/Cupcake/Cupcake Selection";
    [FMODUnity.EventRef]
    public string CupcakeWin = "event:/SFX/Characters/Cupcake/Cupcake Win";
    
    [FMODUnity.EventRef]
    public string HamburgerDeath = "event:/SFX/Characters/Hamburger/Hamburger Death";
    [FMODUnity.EventRef]
    public string HamburgerSelection = "event:/SFX/Characters/Hamburger/Hamburger Selection";
    [FMODUnity.EventRef]
    public string HamburgerWin = "event:/SFX/Characters/Hamburger/Hamburger Win";
    
    [FMODUnity.EventRef]
    public string RobotDeath = "event:/SFX/Characters/Robot/Robot Death";
    [FMODUnity.EventRef]
    public string RobotSelection= "event:/SFX/Characters/Robot/Robot Selection";
    [FMODUnity.EventRef]
    public string RobotWin = "event:/SFX/Characters/Robot/Robot Win";
    
    [FMODUnity.EventRef]
    public string UnicornDeath = "event:/SFX/Characters/Unicorn/Unicorn Death";
    [FMODUnity.EventRef]
    public string UnicornSelection = "event:/SFX/Characters/Unicorn/Unicorn Selection";
    [FMODUnity.EventRef]
    public string UnicornWin = "event:/SFX/Characters/Unicorn/Unicorn Win";
    
    [FMODUnity.EventRef]
    public string CubeFalling = "event:/SFX/Cubes/Cube Falling";   
    [FMODUnity.EventRef]
    public string Earthquake = "event:/SFX/Events/Earthquake/Earthquake";    
    [FMODUnity.EventRef]
    public string InvertedControls= "event:/SFX/Events/Inverted controls/Inverted Controls";  
    [FMODUnity.EventRef]
    public string Storm = "event:/SFX/Events/Storm/Storm";
    
    [FMODUnity.EventRef]
    public string BombExplosion = "event:/SFX/Habilities/Bomb/Bomb Explosion";
    [FMODUnity.EventRef]
    public string LandmineExplosion = "event:/SFX/Habilities/Landmine/Landmine Explosion";
    [FMODUnity.EventRef]
    public string MissileExplosion = "event:/SFX/Habilities/Missile/Missile exlplosion";
    [FMODUnity.EventRef]
    public string MissileThrow = "event:/SFX/Habilities/Missile/Missile Throw";
    
    [FMODUnity.EventRef]
    public string Click = "event:/SFX/Menu/Click";
    [FMODUnity.EventRef]
    public string ClickAccept = "event:/SFX/Menu/Click 2 (Accept)";
    
    [FMODUnity.EventRef]
    public string GameOver = "event:/SFX/Others/GameOver";
    [FMODUnity.EventRef]
    public string StartGong = "event:/SFX/Others/StartGong";
    [FMODUnity.EventRef]
    public string WaterSplash = "event:/SFX/Others/Water Splash";
    
    [FMODUnity.EventRef]
    public string DropAbilty = "event:/SFX/Habilities/Drop Ability";
    [FMODUnity.EventRef]
    public string PickUpAbility = "event:/SFX/Habilities/PickUp Ability";
    [FMODUnity.EventRef]
    public string Draw = "event:/SFX/Music/Draw Music/Draw Event";
    FMOD.Studio.EventInstance drawMusic;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        easyDifficultMusic = FMODUnity.RuntimeManager.CreateInstance(EasyDifficultMusic);
        mediumDifficultMusic = FMODUnity.RuntimeManager.CreateInstance(MediumDifficultMusic);
        hardDifficultMusic = FMODUnity.RuntimeManager.CreateInstance(HardDifficultMusic);
        menuMusic = FMODUnity.RuntimeManager.CreateInstance(MenuMusic);
        winMusic = FMODUnity.RuntimeManager.CreateInstance(WinGame);
        winRound = FMODUnity.RuntimeManager.CreateInstance(WinRound);
        drawMusic = FMODUnity.RuntimeManager.CreateInstance(Draw);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayOneShot(string eventName)
    {
        var sound = FMODUnity.RuntimeManager.CreateInstance(eventName);
        sound.start();
    }

    public void PlayEvent(string eventName)
    {
        switch (eventName)
        {
            case "EasyDifficult":
                easyDifficultMusic.start();
                break;
            case "MediumDifficult":
                mediumDifficultMusic.start();
                break;
            case "HardDifficult":
                hardDifficultMusic.start();
                break;
            case "MenuMusic":
                menuMusic.start();
                break;
            case "WinMusic":
                winMusic.start();
                break;
            case "WinRound":
                winRound.start();
                break;
            case "Draw":
                drawMusic.start();
                break;
        }
    }

    public void StopEvent(string eventName)
    {
        switch (eventName)
        {
            case "EasyDifficult":
                easyDifficultMusic.stop(STOP_MODE.ALLOWFADEOUT);
                break;
            case "MediumDifficult":
                mediumDifficultMusic.stop(STOP_MODE.ALLOWFADEOUT);
                break;
            case "HardDifficult":
                hardDifficultMusic.stop(STOP_MODE.ALLOWFADEOUT);
                break;
            case "MenuMusic":
                menuMusic.stop(STOP_MODE.ALLOWFADEOUT);
                break;
            case "WinMusic":
                winMusic.stop(STOP_MODE.IMMEDIATE);
                break;
            case "WinRound":
                winRound.stop(STOP_MODE.IMMEDIATE);
                break;
            case "Draw":
                drawMusic.stop(STOP_MODE.IMMEDIATE);
                break;
        }
    }

    public void StopAllEvents()
    {
        easyDifficultMusic.stop(STOP_MODE.ALLOWFADEOUT);
        mediumDifficultMusic.stop(STOP_MODE.ALLOWFADEOUT);
        hardDifficultMusic.stop(STOP_MODE.ALLOWFADEOUT);
        menuMusic.stop(STOP_MODE.ALLOWFADEOUT);
    }
}
