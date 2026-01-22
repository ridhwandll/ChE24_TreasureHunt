using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public enum BombState
{
    NOT_DEFUSING,
    DEFUSING,
    DEFUSED,
    DETONATED,
    ABANDONED
}

[System.Serializable]
public class BombSaveData
{
    public BombState bombState;
    public int bombNumber;
    public int defusingProgress;
    public int attemptsLeft;
    public long startTimeTicks;
    public long endTimeTicks;
    public int riddleKey1;
    public int riddleKey2;
    public int points;
}

public class Bomb
{
    private int _points;
    private BombState _bombState; 
    private int _bombNumber;
    private Dictionary<int, String> _riddles;
    private int _defusingProgress;
    private int _attemptsLeft;
    private DateTime _startTime;
    private DateTime _endTime;
    
    public Bomb(int bombNumber)
    {
        _points = 0;
        _attemptsLeft = 8;
        _defusingProgress = 0;
        _bombNumber = bombNumber;
        _riddles =  new Dictionary<int, String>(3);
        _bombState = BombState.NOT_DEFUSING;
    }

    public void AssignRiddles(int riddleKey1, int riddleKey2)
    {
        var riddle1 = new  KeyValuePair<int, String>(riddleKey1, AllRiddles.Riddles[riddleKey1]);
        var riddle2 = new KeyValuePair<int, String>(riddleKey2, AllRiddles.Riddles[riddleKey2]);
        var hardRiddle = AllRiddles.GetHardRiddle(_bombNumber);
        
        _riddles[riddle1.Key] = riddle1.Value;
        _riddles[riddle2.Key] = riddle2.Value;
        _riddles[hardRiddle.Key] = hardRiddle.Value;
        //Debug.LogError("BOMB NUMBER #" + _bombNumber +"\n Riddle01: " + riddle1 +"\n Riddle02: " + riddle2 +"\n Riddle03: "+ hardRiddle);
    }

    public string GetRiddleAnswer(int riddleIndex)
    {
        return _riddles[riddleIndex];
    }
    
    public void MarkAsDefusing()
    {
        _startTime = DateTime.Now;
        _bombState = BombState.DEFUSING;
    }
    
    public BombState GetBombState()
    {
        return _bombState;
    }

    public void AbandonBomb()
    {
        if (_attemptsLeft <= 3)
            _points -= 5;
            
        _bombState = BombState.ABANDONED;
        _endTime = DateTime.Now;
    }
    
    public int GetBombNumber()
    {
        return _bombNumber;
    }

    private int CheckAttemptsLeft()
    {
        if (_attemptsLeft <= 0)
        {
            _points -= 15;
            _bombState = BombState.DETONATED;
            _endTime = DateTime.Now;
            Debug.Log("Bomb Detonated");
            return -68;
        }

        return 0;
    }
    
    // Returns the ID of the next image to load, -69 if wrong code
    public int VerifyRiddle(String defuseCode)
    {
        if (_bombState == BombState.DEFUSING)
        {
            _attemptsLeft--;
            
            KeyValuePair<int, String> currentRiddle = _riddles.ElementAt(_defusingProgress);
            if (defuseCode == currentRiddle.Value)
            {
                _defusingProgress += 1;
                _points += 5; // For Solving a Riddle
                Debug.Log("Bomb defusing progress: " + _defusingProgress + "/3");
                if (_defusingProgress < 3)
                    return _riddles.ElementAt(_defusingProgress).Key;
                if (_defusingProgress == 3)
                {
                    _endTime = DateTime.Now;
                    _bombState = BombState.DEFUSED;
                    _points += 10;
                    _points += _attemptsLeft;
                    return -67;
                }
            }
            else
            {
                int result = CheckAttemptsLeft();
                if (result == -68)
                    return result; // Out of attempts
            }
        }
        else
            Debug.LogError("Attempt To Verify Riddle of Bomb that is not in defusing state!");
        
        return -69;
    }

    public int GetPoints()
    {
        return _points;
    }
    
    public String GetTotalDefusingTimeString()
    {
        // If Bomb is detonated/abandoned/defused only then return total time elapsed
        if (_bombState != BombState.NOT_DEFUSING && _bombState != BombState.DEFUSING)
        {
            TimeSpan elapsed = _endTime - _startTime;
            string result = elapsed.TotalMinutes.ToString("F2") + " minutes";
            return result;
        }
        return "... minutes";
    }
    
    public int GetCurrentRiddleIndex()
    {
        if (_bombState == BombState.DEFUSING)
            return _riddles.ElementAt(_defusingProgress).Key;
        
        Debug.LogError("BOMB not Defusing an and you are trying to get Riddle Index!");
        return -1;
    }
    
    public int GetRemainingAttemptCount()
    {
        return _attemptsLeft;
    }

    public int GetDefusingProgress()
    {
        return _defusingProgress;
    }
    
    public BombSaveData Serialize()
    {
        return new BombSaveData
        {
            bombState = _bombState,
            bombNumber = _bombNumber,
            defusingProgress = _defusingProgress,
            attemptsLeft = _attemptsLeft,
            startTimeTicks = _startTime.Ticks,
            endTimeTicks = _endTime.Ticks,
            points = _points,
            
            //Save only 1st 2 riddles, 3rd one is fixed
            riddleKey1 = _riddles.ElementAt(0).Key,
            riddleKey2 = _riddles.ElementAt(1).Key,
        };
    }
    
    public void Deserialize(BombSaveData data)
    {
        _points = data.points;
        _bombState = data.bombState;
        _bombNumber = data.bombNumber;
        _defusingProgress = data.defusingProgress;
        _attemptsLeft = data.attemptsLeft;

        _startTime = new DateTime(data.startTimeTicks);
        _endTime = new DateTime(data.endTimeTicks);
        
        // Load riddles
        AssignRiddles(data.riddleKey1, data.riddleKey2);
    }

    public static String BonbStateToString(BombState bombState)
    {
        switch (bombState)
        {
            case BombState.NOT_DEFUSING:
                return "INACTIVE";
            case BombState.ABANDONED:
                return "ABANDONED";
            case BombState.DEFUSED:
                return "  DEFUSED  ";
            case BombState.DEFUSING:
                return " DEFUSING ";
            case BombState.DETONATED:
                return "DETONATED";
        }
        
        return "Invalid State";
    }
    
    public static Color BombStateToColor(BombState bombState)
    {
        switch (bombState)
        {
            case BombState.NOT_DEFUSING:
                return Color.antiqueWhite;
            case BombState.ABANDONED:
                return Color.gray5;
            case BombState.DEFUSED:
                return Color.softGreen;
            case BombState.DEFUSING:
                return Color.darkOrange;
            case BombState.DETONATED:
                return Color.softRed;
        }
        return Color.antiqueWhite;
    }
}
