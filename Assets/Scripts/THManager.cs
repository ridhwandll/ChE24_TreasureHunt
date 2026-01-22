using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BombDefusalCodes
{
    // BombNo - Codes
    private Dictionary<int, String> _Codes;

    public BombDefusalCodes()
    {
        _Codes = new Dictionary<int, string>();
    }

    public void LoadCodes()
    {
        _Codes[0] = "riddlemethis";
        _Codes[1] = "thetruthistheriddle";
        _Codes[2] = "iamjustaninstrument";
        _Codes[3] = "cruelpoeticorblind";
        _Codes[4] = "yourviolenceyoumayfind";
        _Codes[5] = "youareapartofthistoo";
    }
    
    public bool CheckInitiationCode(int bombNo, String givenCode)
    {
        if (_Codes[bombNo] == givenCode)
            return true;
        
        return false;
    }
}

public class ProgressMenuBombDisplay
{
    public VisualElement Container;
    public Label BombName;
    public Label TimeTaken;
    public Label Status;
}

[System.Serializable]
public class BombsSaveData
{
    public BombSaveData[] bombs;
    public string teamName;
}

public class THManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private UIDocument Document;
    private VisualElement _bigRoot;
    private Image _riddleImage;
    private Image _bombLocationImage;
    private TextField _defuseCodeField;
    private Button _verifyButton;
    private Button _abandonButton;
    
    private MenuManager _menuManager;
    
    // Bomb Status
    private Label _attemptsLeftLabel;
    private Label _pointsLabel;
    private Label _progressLabel;
    private Label _selectedBombName;
    
    // Bomb Initiation Stuff
    private BombDefusalCodes _bombDefusalCodes;
    private Label _selectedBombForInitLabel;
    private Label _bombInitStatusLabel;
    private TextField _bombInitiationTextField;
    private Button _verifyBombInitCodeButton;
    
    private Button[] _bombButtons =  new Button[6];
    
    private List<Bomb> _bombs = new List<Bomb>(6);
    private int _selectedBomb = 0;
    private int _riddleLoadIndex;
    private Color _normalBgTintColor;
    
    // CLUE Menu
    private Label _clueLabel;
    private Button _riddlerClueButton;
    private List<Button> _riddlerClueButtons = new List<Button>(6);
    
    //Progress Menu 
    private Button _progressButton;
    private Label _progressInfoLabel;
    private List<ProgressMenuBombDisplay> _progressMenuBombs = new List<ProgressMenuBombDisplay>(6);
    
    // TeamName Menu
    private TextField _teamNameField;
    private Label _teamNameInfoField;
    private Button _teamNameVerifyButton;
    private String _teamName;
    
    // Save-Load Data
    private string SavePath => Path.Combine(Application.persistentDataPath, "RiddlersMayhem.json");
    
    void Start()
    {
        _bigRoot = Document.rootVisualElement.Query<VisualElement>("BigRoot");
        
        _bombDefusalCodes = new BombDefusalCodes();
        _bombDefusalCodes.LoadCodes();
        _normalBgTintColor = new Color(0.03529412f, 0.03529412f, 0.03529412f, 1f);
        _bigRoot.style.unityBackgroundImageTintColor = _normalBgTintColor;
        
        _bombs.Add(new Bomb(0));
        _bombs.Add(new Bomb(1));
        _bombs.Add(new Bomb(2));
        _bombs.Add(new Bomb(3));
        _bombs.Add(new Bomb(4));
        _bombs.Add(new Bomb(5));
        
        // Assign Riddles to all bombs
        List<int> shuffledIds = new List<int>(AllRiddles.Riddles.Keys);

        // Fisher–Yates
        for (int i = shuffledIds.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffledIds[i], shuffledIds[j]) = (shuffledIds[j], shuffledIds[i]);
        }
        
        // Assign Riddles
        int index = 0;
        foreach (Bomb bomb in _bombs)
        {
            var key1 = shuffledIds[index++];
            var key2 = shuffledIds[index++];
            bomb.AssignRiddles(key1, key2);
        }

        _menuManager = new MenuManager();
        _menuManager.Initialize(Document);
        AudioManager.Init(_audioSource);
        
        _teamNameInfoField = Document.rootVisualElement.Q<Label>("TeamNameInfoField");
        _selectedBombName = Document.rootVisualElement.Q<Label>("SelectedBombName");
        _attemptsLeftLabel = Document.rootVisualElement.Q<Label>("AttemptsLeft");
        _pointsLabel = Document.rootVisualElement.Q<Label>("Points");
        _progressLabel = Document.rootVisualElement.Q<Label>("Progress");
        _riddleImage = Document.rootVisualElement.Q<Image>("RiddleImage");
        _bombLocationImage = Document.rootVisualElement.Q<Image>("BombLocationImage");
        _defuseCodeField = Document.rootVisualElement.Q<TextField>("DefuseCodeField"); 
        _verifyButton = Document.rootVisualElement.Q<Button>("VerifyButton");
        _abandonButton = Document.rootVisualElement.Q<Button>("AbandonButton"); 
        
        _teamNameVerifyButton = Document.rootVisualElement.Q<Button>("VerifyTeamNameButton");
        _teamNameField = Document.rootVisualElement.Q<TextField>("TeamNameField");

        _progressInfoLabel = Document.rootVisualElement.Q<Label>("ProgressInfoLabel");
        _selectedBombForInitLabel = Document.rootVisualElement.Q<Label>("SelectedBombForInitiation");
        _bombInitStatusLabel = Document.rootVisualElement.Q<Label>("BombInitStatus");
        _bombInitiationTextField = Document.rootVisualElement.Q<TextField>("BombInitCodeField");
        _progressButton = Document.rootVisualElement.Q<Button>("ProgressButton");
        _verifyBombInitCodeButton = Document.rootVisualElement.Q<Button>("VerifyInitCodeButton");
        _riddlerClueButton = Document.rootVisualElement.Q<Button>("RiddlerClueButton");
        _clueLabel = Document.rootVisualElement.Q<Label>("ClueLabel");    
        
        for(int i = 0; i < 6; i++)
        {
            _progressMenuBombs.Add(new ProgressMenuBombDisplay());
            ProgressMenuBombDisplay progressMenuBomb = _progressMenuBombs[i];
            progressMenuBomb.Container = Document.rootVisualElement.Q<VisualElement>("B" + (i + 1)); 
            progressMenuBomb.BombName = Document.rootVisualElement.Q<Label>("Name_" + (i + 1));
            progressMenuBomb.TimeTaken = Document.rootVisualElement.Q<Label>("TimeElapsed_" + (i + 1));
            progressMenuBomb.Status = Document.rootVisualElement.Q<Label>("Status_" + (i + 1));
        }
        
        for(int i = 0; i < 6; i++)
        {
            _riddlerClueButtons.Add(new Button());
            _riddlerClueButtons[i] = Document.rootVisualElement.Q<Button>("CLUE_" + (i + 1));

            switch (i)
            {
                case 0: _riddlerClueButtons[i].text = "#1 Arrogant Invitation"; break;
                case 1: _riddlerClueButtons[i].text = "#2 Tease"; break;
                case 2: _riddlerClueButtons[i].text = "#3 False Name"; break;
                case 3: _riddlerClueButtons[i].text = "#4 Mask and Glasses"; break;
                case 4: _riddlerClueButtons[i].text = "#5 Voice"; break;
                case 5: _riddlerClueButtons[i].text = "#6 Challenge"; break;
            }
            
            var ii = i;
            _riddlerClueButtons[i].clicked += () =>
            { 
                Debug.Log("Clue Button " + (ii + 1) + " clicked");
                AudioManager.PlayClue("Clue"+ (ii + 1));
            };
        }

        _riddlerClueButton.clicked += OnClueButtonClicked;
        _progressButton.clicked += OnProgressButtonClicked;
        _verifyBombInitCodeButton.clicked += OnVerifyBombInitButtonClicked;
        
        _verifyButton.clicked += OnVerifyButtonClicked;
        _abandonButton.clicked += () =>
        {
            _bombs[_selectedBomb].AbandonBomb();
            InvalidateBombDisplay(_bombs[_selectedBomb]);
        };

        _teamNameInfoField.text = SavedDataExists() ? "ENTER YOUR TEAM NAME" : "REGISTER YOUR TEAM";
        
        _teamNameVerifyButton.clicked += () =>
        {
            bool isTeamNameValid = TeamManager.IsTeamNameValid(_teamNameField.text);
            if (isTeamNameValid)
            {
                bool loadResult = Load();
            
                if (!loadResult)
                {
                    _menuManager.Show(MenuType.ProgressMenu);
                    _teamName = _teamNameField.text;
                    _progressButton.text = _teamNameField.text;
                    FlashBGOnce(false);
                    OnProgressButtonClicked(); // Refresh the progress menu
                }
                else
                {
                    Debug.Log("Stored Team Name: " + _teamName);
                    if (_teamNameField.text != _teamName)
                    {
                        _menuManager.Show(MenuType.TeamNameMenu);
                        FlashBGOnce(true);
                        _teamNameInfoField.text = "Please put YOUR team name, not others!";
                    }
                    else
                    {
                        _menuManager.Show(MenuType.ProgressMenu);
                        _teamName = _teamNameField.text;
                        _progressButton.text = _teamNameField.text;
                        FlashBGOnce(false);
                        OnProgressButtonClicked(); // Refresh the progress menu
                    }
                }
            }
            else
            {
                _menuManager.Show(MenuType.TeamNameMenu);
                _teamNameInfoField.text = "No team with name \"" + _teamNameField.text + "\" exists";
                FlashBGOnce(true);
            }
        };
        
        for (int i = 0; i < 6; i++)
        {
            int bombNumber = i;
            _bombButtons[i] = Document.rootVisualElement.Q<Button>("Bomb" + (bombNumber+1));
            _bombButtons[i].clicked += () => OnBombButtonClicked(bombNumber);
            InvalidateBombDisplay(_bombs[bombNumber]);
        }
        
        // Only allow to verify when there is the 6 digit code
        _verifyButton.SetEnabled(false);
        _defuseCodeField.RegisterValueChangedCallback(evt =>
        {
            _verifyButton.SetEnabled(evt.newValue.Length == 6);
        });

        _menuManager.Show(MenuType.TeamNameMenu);
    }

    private void OnClueButtonClicked()
    {
        _menuManager.Show(MenuType.RiddlerClueMenu);
        int clueAmount = CalculateAmountOfClueToShow();

        // Hide all clues
        foreach (var btn in _riddlerClueButtons)
            btn.style.display = DisplayStyle.None;
         
        if (clueAmount == 0)
            _clueLabel.text = "Defuse bombs to get audio\nclues about Riddler!";
        else
            _clueLabel.text = "Put your device volume to Maximum\n while listening.You have to find\nwho is the Riddler based on these.";
        
        // Only activate on amount of Bomb Defused
        for (int i = 0; i < clueAmount; i++)
        {
            _riddlerClueButtons[i].style.display = DisplayStyle.Flex;
        }
    }
    
    void OnProgressButtonClicked()
    {
        // Show Progress Menu, Hide others
        _menuManager.Show(MenuType.ProgressMenu);
        
        int inactiveBombCount = 0;
        for(int i = 0; i < 6; i++)
        {
            var bmb = _bombs[i];
            BombState state = bmb.GetBombState();
            
            ProgressMenuBombDisplay progressMenuBomb = _progressMenuBombs[i];
            
            if (state != BombState.NOT_DEFUSING)
            {
                progressMenuBomb.Container.style.display = DisplayStyle.Flex;

                progressMenuBomb.BombName.text = "BOMB 0" + (i + 1);
                progressMenuBomb.BombName.style.color = Bomb.BombStateToColor(state);
                progressMenuBomb.TimeTaken.text = bmb.GetTotalDefusingTimeString();
                progressMenuBomb.Status.text = Bomb.BonbStateToString(state);
            }
            else
            {
                inactiveBombCount++;
                progressMenuBomb.Container.style.display = DisplayStyle.None;
            }
        }

        if (inactiveBombCount == 6)
        {
            _progressInfoLabel.text = "Nothing to see here yet!\nStart defusing bombs & comeback here\nto check your progress";
        }
        else
        {
            _progressInfoLabel.text = "Total Points: " + GetTotalPoints();
        }
        
        HideMobileKeyboard();
    }
    
    void OnVerifyBombInitButtonClicked()
    {
        if (_bombDefusalCodes.CheckInitiationCode(_selectedBomb, _bombInitiationTextField.value))
        {
            _bombs[_selectedBomb].MarkAsDefusing();
            _bombInitStatusLabel.text = "";
            InvalidateBombDisplay(_bombs[_selectedBomb]);
            FlashBGOnce(false);
            Save();
        }
        else
        {
            FlashBGOnce(true);
            _bombInitStatusLabel.text = "Invalid Code!";
        }
        
        HideMobileKeyboard();
        _bombInitiationTextField.value = "";
    }

    void OnVerifyButtonClicked()
    {
        Bomb defusingBomb = _bombs[_selectedBomb];

        String allUpperCaseDefuseCode = _defuseCodeField.value.ToUpperInvariant(); 
        _riddleLoadIndex = defusingBomb.VerifyRiddle(allUpperCaseDefuseCode);
        
        //-69 means wrong code; -67 means bomb defused successfully; -68 means bomb detonated
        if (_riddleLoadIndex == -67)
        {
            Debug.Log("Bomb DEFUSED successfully");
            _riddleImage.style.visibility = Visibility.Hidden;
        }
        else if (_riddleLoadIndex == -69)
        {
            FlashBGOnce(true);
            Debug.Log("You Entered wrong code Lil Bro");
        }
        else if (_riddleLoadIndex == -68)
        {
            FlashBGOnce(true);
            Debug.Log("You are out of attempts Lil Bro, bomb exploded");
        }
        else
        {
            FlashBGOnce(false);
            //Show the next riddle
            LoadRiddleImage(_riddleLoadIndex);
        }
        
        Save();
        InvalidateBombDisplay(defusingBomb);
        _defuseCodeField.value = "";
        HideMobileKeyboard();
    }
    
    void OnBombButtonClicked(int bombNo)
    {
        Bomb selectedBomb = _bombs[bombNo];
        _selectedBomb = bombNo;

        _selectedBombName.text = "BOMB " + (bombNo + 1);

        HideMobileKeyboard();
        
        // Refresh bomb display code here
        InvalidateBombDisplay(selectedBomb);
        Save();
    }

    private void InvalidateBombDisplay(Bomb bomb)
    {
        if (bomb.GetBombState() == BombState.NOT_DEFUSING)
        {
            _bombInitStatusLabel.text = "";
            _selectedBombForInitLabel.text = "BOMB 0" + (bomb.GetBombNumber() + 1);
            _bombLocationImage.image = Resources.Load<Texture2D>("Maps/BombLocationImage_" + (bomb.GetBombNumber() + 1));
            _menuManager.Show(MenuType.BombLoadMenu);
            
            if (CanDefuseMoreBombs())
            {
                _bombInitiationTextField.style.display = DisplayStyle.Flex;
                _verifyBombInitCodeButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _bombInitStatusLabel.text = "You can not defuse more than 2 bombs at once.\nAbandon/Defuse one bomb to continue";
                _bombInitiationTextField.style.display = DisplayStyle.None;
                _verifyBombInitCodeButton.style.display = DisplayStyle.None;
            }
        }
        else
        {
            _menuManager.Show(MenuType.DefuseMenu);
            var bombSelectionButton = _bombButtons[bomb.GetBombNumber()];

            Color color = Bomb.BombStateToColor(bomb.GetBombState());
            if (bomb.GetBombState() == BombState.DEFUSING)
            {
                bombSelectionButton.style.backgroundColor= color;
                _selectedBombName.style.color = color;
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _pointsLabel.text = "Points: " + bomb.GetPoints();
                _progressLabel.text = "Progress: " + bomb.GetDefusingProgress() + "/3";

                if (bomb.GetDefusingProgress() == 2)
                    _verifyButton.text = "DEFUSE";
                else
                    _verifyButton.text = "VERIFY";
                
                LoadRiddleImage(bomb.GetCurrentRiddleIndex());
            }
            else if (bomb.GetBombState() == BombState.DEFUSED)
            {
                
                bombSelectionButton.style.backgroundColor= color;
                _selectedBombName.style.color = color;
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _pointsLabel.text = "Points: " + bomb.GetPoints();
                _progressLabel.text = "Status: DEFUSED";
                HideRiddleInput();
            }
            else if (bomb.GetBombState() == BombState.DETONATED)
            {
                bombSelectionButton.style.backgroundColor= color;
                _selectedBombName.style.color = color;
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _pointsLabel.text = "Points: " + bomb.GetPoints();
                _progressLabel.text = "Status: DETONATED";
                HideRiddleInput();
            }
            else if (bomb.GetBombState() == BombState.ABANDONED)
            {
                bombSelectionButton.style.backgroundColor= color;
                _selectedBombName.style.color = color;
                
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _pointsLabel.text = "Points: " + bomb.GetPoints();
                _progressLabel.text = "Status: ABANDONED";
                HideRiddleInput();
            }
        }
    }
    
    private void HideRiddleInput()
    {
        _defuseCodeField.style.visibility = Visibility.Hidden;
        _verifyButton.style.visibility = Visibility.Hidden;
        _riddleImage.style.visibility = Visibility.Hidden;
        _abandonButton.style.visibility = Visibility.Hidden;
        _riddleImage.image = null;
    }
    
    private void LoadRiddleImage(int riddleIndex)
    {
        _defuseCodeField.style.visibility = Visibility.Visible;
        _verifyButton.style.visibility = Visibility.Visible;
        _riddleImage.style.visibility = Visibility.Visible;
        _abandonButton.style.visibility = Visibility.Visible;
        _riddleImage.image = Resources.Load<Texture2D>(riddleIndex.ToString());
    }

    private bool CanDefuseMoreBombs()
    {
        int result = 0;
        foreach (Bomb bomb in _bombs)
        {
            if (bomb.GetBombState() == BombState.DEFUSING)
                result++;
        }
        
        // Can Defuse 2 bombs at MAX
        if (result >= 2)
            return false;
        
        return true;
    }
    
    void FlashBGOnce(bool isError)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(isError));
    }

    IEnumerator FlashRoutine(bool isError)
    {
        Color flash = isError ? Color.softRed : Color.softGreen;
        
        float duration = 0.8f;
        float t = 0f;

        // Flash IN
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _bigRoot.style.unityBackgroundImageTintColor = Color.Lerp(_normalBgTintColor, flash, t);
            yield return null;
        }

        t = 0f;

        // Flash OUT
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _bigRoot.style.unityBackgroundImageTintColor = Color.Lerp(flash, _normalBgTintColor, t);
            yield return null;
        }

        _bigRoot.style.unityBackgroundImageTintColor = _normalBgTintColor;
    }
    
    private void Save()
    {
        BombsSaveData save = new BombsSaveData();
        
        save.bombs = new BombSaveData[_bombs.Count];
        save.teamName = _teamNameField.text;
        
        for (int i = 0; i < _bombs.Count; i++)
        {
            save.bombs[i] = _bombs[i].Serialize();
        }

        string json = JsonUtility.ToJson(save, true);
        string encrypted = SaveCrypto.Encrypt(json);
        File.WriteAllText(SavePath, encrypted);

        Debug.Log("Saved to: " + SavePath);
    }

    private bool SavedDataExists()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file");
            return false;
        }
        return true;
    }
    
    private bool Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file");
            return false;
        }
        
        string encrypted = File.ReadAllText(SavePath);
        string json = SaveCrypto.Decrypt(encrypted);
        BombsSaveData save =  JsonUtility.FromJson<BombsSaveData>(json);
        
        _teamName =  save.teamName;
        
        for (int i = 0; i < _bombs.Count; i++)
        {
            if (i != save.bombs[i].bombNumber) 
                Debug.LogError("Fahh you i!=bombNumber");
            
            Bomb newBomb = new Bomb(i);
            newBomb.Deserialize(save.bombs[i]);
            _bombs[i] = newBomb;
            InvalidateBombDisplay(_bombs[i]);
        }
        
        Debug.Log("Loaded bombs");
        return true;
    }

    private int GetTotalPoints()
    {
        int result = 0;
        foreach (var bomb in _bombs)
        {
            result += bomb.GetPoints();
        }

        return result;
    }
    
    private void HideMobileKeyboard()
    {
        _bombInitiationTextField.Blur();
        _defuseCodeField.Blur();
    }

    private int CalculateAmountOfClueToShow()
    {
        int result = 0;

        foreach (Bomb bomb in _bombs)
        {
            if (bomb.GetBombState() == BombState.DEFUSED)
                result++;
        }
        
        return result;
    }
}
