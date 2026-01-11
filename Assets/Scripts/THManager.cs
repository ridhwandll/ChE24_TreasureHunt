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
}

public class THManager : MonoBehaviour
{
    [SerializeField] private UIDocument Document;
    private VisualElement _bigRoot;
    private Image _riddleImage;
    private TextField _defuseCodeField;
    private Button _verifyButton;
    private Button _abandonButton;
    
    private VisualElement _defuseMenu;
    private VisualElement _bombLoadMenu;
    private VisualElement _progressMenu;
    
    // Bomb Status
    private Label _RIDDLE_NUMBER; //TODO: REMOVE
    private Label _attemptsLeftLabel;
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
    
    //Progress Menu 
    private Button _progressButton;
    private Label _progressInfoLabel;
    private List<ProgressMenuBombDisplay> _progressMenuBombs = new List<ProgressMenuBombDisplay>(6);
    
    // Save-Load Data
    string SavePath => Path.Combine(Application.persistentDataPath, "ChE24THDO_NOT_TRY_TO_OPEN_BETA01.json");
    
    void Start()
    {
        _RIDDLE_NUMBER = Document.rootVisualElement.Q<Label>("RiddleNumber");
            
        _bigRoot = Document.rootVisualElement.Query<VisualElement>("BigRoot");
        
        _bombDefusalCodes = new BombDefusalCodes();
        _bombDefusalCodes.LoadCodes();
        _normalBgTintColor = new Color(0.1f, 0.1f, 0.1f, 1f);
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
        
        _defuseMenu = Document.rootVisualElement.Q<VisualElement>("DefuseMenu");
        _bombLoadMenu = Document.rootVisualElement.Q<VisualElement>("BombLoadMenu");
        _progressMenu = Document.rootVisualElement.Q<VisualElement>("ProgressMenu");
        
        _selectedBombName = Document.rootVisualElement.Q<Label>("SelectedBombName");
        _attemptsLeftLabel = Document.rootVisualElement.Q<Label>("AttemptsLeft");
        _progressLabel = Document.rootVisualElement.Q<Label>("Progress");
        _riddleImage = Document.rootVisualElement.Q<Image>("RiddleImage");
        _defuseCodeField = Document.rootVisualElement.Q<TextField>("DefuseCodeField"); 
        _verifyButton = Document.rootVisualElement.Q<Button>("VerifyButton");
        _abandonButton = Document.rootVisualElement.Q<Button>("AbandonButton"); 

        _progressInfoLabel = Document.rootVisualElement.Q<Label>("ProgressInfoLabel");
        _selectedBombForInitLabel = Document.rootVisualElement.Q<Label>("SelectedBombForInitiation");
        _bombInitStatusLabel = Document.rootVisualElement.Q<Label>("BombInitStatus");
        _bombInitiationTextField = Document.rootVisualElement.Q<TextField>("BombInitCodeField");
        _progressButton = Document.rootVisualElement.Q<Button>("ProgressButton");
        _verifyBombInitCodeButton = Document.rootVisualElement.Q<Button>("VerifyInitCodeButton");
        
        _progressButton.clicked += OnProgressButtonClicked;
        _verifyBombInitCodeButton.clicked += OnVerifyBombInitButtonClicked;
        
        _verifyButton.clicked += OnVerifyButtonClicked;
        _abandonButton.clicked += () =>
        {
            _bombs[_selectedBomb].AbandonBomb();
            InvalidateBombDisplay(_bombs[_selectedBomb]);
        };
        
        // LoadSavedData (Override current data if new data exists)
        Load();
        
        for (int i = 0; i < 6; i++)
        {
            int bombNumber = i;
            _bombButtons[i] = Document.rootVisualElement.Q<Button>("Bomb" + (bombNumber+1));
            _bombButtons[i].clicked += () => OnBombButtonClicked(bombNumber);
            InvalidateBombDisplay(_bombs[bombNumber]);
        }

        for(int i = 0; i < 6; i++)
        {
            _progressMenuBombs.Add(new ProgressMenuBombDisplay());
            ProgressMenuBombDisplay progressMenuBomb = _progressMenuBombs[i];
            progressMenuBomb.Container = Document.rootVisualElement.Q<VisualElement>("B" + (i + 1)); 
            progressMenuBomb.BombName = Document.rootVisualElement.Q<Label>("Name_" + (i + 1));
            progressMenuBomb.TimeTaken = Document.rootVisualElement.Q<Label>("TimeElapsed_" + (i + 1));
            progressMenuBomb.Status = Document.rootVisualElement.Q<Label>("Status_" + (i + 1));
        }
        
        _bombLoadMenu.style.display = DisplayStyle.None;
        _defuseMenu.style.display = DisplayStyle.None;
        _progressMenu.style.display = DisplayStyle.Flex;
        OnProgressButtonClicked(); // Refresh the progress menu
        
        // Only allow to verify when there is the 6 digit code
        _verifyButton.SetEnabled(false);
        _defuseCodeField.RegisterValueChangedCallback(evt =>
        {
            _verifyButton.SetEnabled(evt.newValue.Length == 6);
        });
    }

    void OnProgressButtonClicked()
    {
        // Show Progress Menu, Hide others
        _progressMenu.style.display = DisplayStyle.Flex;
        _bombLoadMenu.style.display = DisplayStyle.None;
        _defuseMenu.style.display = DisplayStyle.None;

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
            _progressInfoLabel.style.display = DisplayStyle.Flex;
            _progressInfoLabel.text = "Nothing to see here yet!\nStart defusing bombs and comeback here\nto check your progress";
        }
        else
            _progressInfoLabel.style.display = DisplayStyle.None;
        
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
        
        HideMobileKeyboard();
        Save();
        InvalidateBombDisplay(defusingBomb);
        _defuseCodeField.value = "";
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
            _bombLoadMenu.style.display = DisplayStyle.Flex;
            _defuseMenu.style.display = DisplayStyle.None;
            _progressMenu.style.display = DisplayStyle.None;
            
            if (CanDefuseMoreBombs())
            {
                _bombInitiationTextField.style.visibility = Visibility.Visible;
                _verifyBombInitCodeButton.style.visibility = Visibility.Visible;
            }
            else
            {
                _bombInitStatusLabel.text = "You can not defuse more than 2 bombs at once.\nAbandon/Defuse one bomb to continue";
                _bombInitiationTextField.style.visibility = Visibility.Hidden;
                _verifyBombInitCodeButton.style.visibility = Visibility.Hidden;
            }
        }
        else
        {
            _progressMenu.style.display = DisplayStyle.None;
            _bombLoadMenu.style.display = DisplayStyle.None;
            _defuseMenu.style.display = DisplayStyle.Flex;
            var bombSelectionButton = _bombButtons[bomb.GetBombNumber()];
            
            if (bomb.GetBombState() == BombState.DEFUSING)
            {
                bombSelectionButton.style.backgroundColor= Color.darkOrange;
                _selectedBombName.style.color = Color.darkOrange;
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _progressLabel.text = "Progress: " + bomb.GetDefusingProgress() + "/3";

                if (bomb.GetDefusingProgress() == 2)
                    _verifyButton.text = "DEFUSE";
                else
                    _verifyButton.text = "VERIFY";

                _RIDDLE_NUMBER.text = "Riddle Number: " + bomb.GetCurrentRiddleIndex() + " (DEV MODE ONLY)";
                LoadRiddleImage(bomb.GetCurrentRiddleIndex());
            }
            else if (bomb.GetBombState() == BombState.DEFUSED)
            {
                bombSelectionButton.style.backgroundColor= Color.softGreen;
                _selectedBombName.style.color = Color.softGreen;
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _progressLabel.text = "Status: DEFUSED";
                HideRiddleInput();
            }
            else if (bomb.GetBombState() == BombState.DETONATED)
            {
                bombSelectionButton.style.backgroundColor= Color.red;
                _selectedBombName.style.color = Color.red;
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
                _progressLabel.text = "Status: DETONATED";
                HideRiddleInput();
            }
            else if (bomb.GetBombState() == BombState.ABANDONED)
            {
                bombSelectionButton.style.backgroundColor= Color.softRed;
                _selectedBombName.style.color = Color.softRed;
                
                _attemptsLeftLabel.text = "Attempts Left: " + bomb.GetRemainingAttemptCount();
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

        for (int i = 0; i < _bombs.Count; i++)
        {
            save.bombs[i] = _bombs[i].Serialize();
        }

        string json = JsonUtility.ToJson(save, true);
        string encrypted = SaveCrypto.Encrypt(json);
        File.WriteAllText(SavePath, encrypted);

        Debug.Log("Saved to: " + SavePath);
    }
    
    private void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file");
            return;
        }
        
        string encrypted = File.ReadAllText(SavePath);
        string json = SaveCrypto.Decrypt(encrypted);
        BombsSaveData save =  JsonUtility.FromJson<BombsSaveData>(json);
        
        for (int i = 0; i < _bombs.Count; i++)
        {
            if (i != save.bombs[i].bombNumber) 
                Debug.LogError("Fahh you i!=bombNumber");
            
            Bomb newBomb = new Bomb(i);
            newBomb.Deserialize(save.bombs[i]);
            _bombs[i] = newBomb;
        }

        Debug.Log("Loaded bombs");
    }

    private void HideMobileKeyboard()
    {
        _bombInitiationTextField.Blur();
        _defuseCodeField.Blur();
    }
}
