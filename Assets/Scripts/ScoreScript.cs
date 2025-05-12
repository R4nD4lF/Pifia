using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

public class ScoreScript : MonoBehaviour
{
    [SerializeField] DiceScript[] _dices;

    [SerializeField] DiceScript[] _allDices;

    [SerializeField] public List<DiceScript> _selectedDices;

    [SerializeField] public List<DiceScript> _playableDices;
    [SerializeField] public TextMeshProUGUI _bankScoreTMP;

    [SerializeField] public TextMeshProUGUI _totalScoreTMP;

    [SerializeField] private TextMeshProUGUI _centerMessageTMP;

    [SerializeField] private GameObject _stayButton;

    Dictionary<int, int> faceCounts = new Dictionary<int, int>();

    Dictionary<int, int> scoreCounts = new Dictionary<int, int>();
    public int _totalScore = 0, _bankScore = 0, _accumulatedBank;
    private int _diceStillRolling, round = 0;

    public bool _isRolling = false , _isNewDiceInBankScore = false;

    
    void Start()
    {
        _allDices = _dices;
        _playableDices = _dices.ToList();
        foreach(DiceScript dice in _dices){
            dice.OnDiceStopped += OnDiceStopped;
        }
    }


    void Update()
    {
        if(Input.GetButtonDown("Jump") && !_isRolling){
            if(_selectedDices.Count != 0){
                int nDices = _selectedDices.Count()-1;
                _accumulatedBank = _bankScore;
                foreach(DiceScript dice in _selectedDices){
                    dice.isScored = true;
                    dice.isSelected = false;
                    dice.isSelectable = false;
                    dice.body.isKinematic = true;
                    dice.transform.localPosition = new Vector3(-21.31f + (2 * nDices),3.39f,8.19f - (2 * round-1));
                    _playableDices.Remove(dice);
                    nDices --;
                }
                _dices = _playableDices.ToArray();
            }
            _centerMessageTMP.text = "";
            _stayButton.SetActive(false);
            RollDices();
            round++;
        }
        if(_isNewDiceInBankScore)
            CheckScore();
    }

    void RollDices(){
        _isRolling = true;
        _diceStillRolling = _dices.Length;
        RoundResetValues();
        foreach(DiceScript dice in _dices){
            if(!dice.isScored)
                dice.RollDice();
        }
    }

    void OnDiceStopped(int result, DiceScript dice){

        _diceStillRolling --;
        dice.diceFaceNum = result;

        CheckSelectable(result,dice); 
        if (_diceStillRolling != 0) return;
        _isRolling = false;

        CheckPifia();

    }

    void CheckSelectable(int result, DiceScript dice){
        if( result == 1 || result == 5){
            dice.isSelectable = true;
        }else{
            if (faceCounts.ContainsKey(result)) {
                faceCounts[result]++;
            } else {
                faceCounts[result] = 1;
            }
        }

        foreach(DiceScript checkDice in _dices){
            if(faceCounts.Keys.Contains(checkDice.diceFaceNum) && faceCounts[checkDice.diceFaceNum] >= 3){
                checkDice.isSelectable = true;
            }
        }
    }

    void CheckScore(){
        _bankScore =  0;
        scoreCounts.Clear();
        foreach(DiceScript dice in _selectedDices){
            if (scoreCounts.ContainsKey(dice.diceFaceNum)) {
                scoreCounts[dice.diceFaceNum]++;
            } else {
                scoreCounts[dice.diceFaceNum] = 1;
            }
        }
        foreach(int key in scoreCounts.Keys){

            if(scoreCounts[key] >= 3){
                int multiplicador = Mathf.Max(1,(int)Mathf.Pow(2, scoreCounts[key] - 3));
                if(key != 1){
                    _bankScore += key * 100 * multiplicador;
                }else{
                    _bankScore += 1000 * multiplicador;
                }
            }else if(key == 1){
                _bankScore += scoreCounts[key] * 100;
            }else if(key == 5){
                _bankScore += scoreCounts[key] * 50;
            }
        }
        _isNewDiceInBankScore = false;
        _bankScoreTMP.text = "BankScore: "+(_accumulatedBank + _bankScore).ToString();
    }

    void CheckPifia(){
        bool pifia = true;
        foreach(DiceScript dice in _dices){
            if(dice.isSelectable){
                pifia = false;
                break;
            }
        }
        if(pifia){
            _centerMessageTMP.color = Color.darkRed;
            _centerMessageTMP.text = "¡PIFIA!";
            ResetFullValues();

        }else{
            _stayButton.SetActive(true);
        }

    }
    void RoundResetValues(){
        faceCounts.Clear();
        _selectedDices.Clear();
        scoreCounts.Clear();
        _centerMessageTMP.text = "";
    }
    void ResetFullValues(){
        faceCounts.Clear();
        _selectedDices.Clear();
        scoreCounts.Clear();
        _bankScore = 0;
        _accumulatedBank = 0;
        round = 0;
        _bankScoreTMP.text = "BankScore: "+_accumulatedBank.ToString();
        _dices = _allDices;
        _playableDices = _dices.ToList();
        foreach(DiceScript dice in _dices){
            dice.transform.localPosition = dice.originalPosition;
            dice.transform.localScale = dice.originalScale;
            dice.isRolling = false;
            dice.isScored = false;
            dice.isSelectable = false;
            dice.isSelected = false;
            dice.body.isKinematic = true;
        }
    }


    void StayBankScore(){
        _totalScore += _bankScore;
        _totalScoreTMP.text = "Total Score" + _totalScore.ToString();
    }
}
