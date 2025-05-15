using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine.Analytics;

public class ScoreScript : MonoBehaviour
{
    [SerializeField] DiceScript[] _dices;

    [SerializeField] DiceScript[] _allDices;

    [SerializeField] public List<DiceScript> _selectedDices;

    [SerializeField] public List<DiceScript> _playableDices;
    [SerializeField] public TextMeshProUGUI _bankScoreTMP;

    [SerializeField] public TextMeshProUGUI _totalScoreTMP;

    [SerializeField] public TextMeshProUGUI _centerMessageTMP;

    [SerializeField] private GameObject _stayButton;

    [SerializeField] public GameObject _fireEffect;

    Dictionary<int, int> faceCounts = new Dictionary<int, int>();

    Dictionary<int, int> scoreCounts = new Dictionary<int, int>();
    public int _totalScore = 0, _bankScore = 0, _accumulatedBank = 0, _straightScore = 0, _goalScore = 5000;
    private int _diceStillRolling, round = 0;

    public bool _isRolling = false, _isNewDiceInBankScore = false, _isStraight = false, _isPifia = true;

    [SerializeField] public AudioSource pifiaSound;

    private Color _originalCenterTextColor;


    void Start()
    {
        _originalCenterTextColor = _centerMessageTMP.color;
        _totalScoreTMP.text = "Total Score: " + _totalScore.ToString() + "/" + _goalScore.ToString();
        _allDices = _dices;
        _playableDices = _dices.ToList();
        foreach (DiceScript dice in _dices)
        {
            dice.OnDiceStopped += OnDiceStopped;
        }
    }


    void Update()
    {
        if (Input.GetButtonDown("Jump") && _selectedDices.Count == 0 && _dices.Any() && !_isPifia && round != 0)
        {
            _centerMessageTMP.fontSize = 50;
            _centerMessageTMP.color = Color.darkRed;
            _centerMessageTMP.text = "You can't roll dices without scoring!";
            _centerMessageTMP.fontSize = 200;
        }
        else if (Input.GetButtonDown("Jump") && !_isRolling)
        {
            if (_selectedDices.Count != 0)
            {
                int nDices = _selectedDices.Count();
                _accumulatedBank += _bankScore;
                foreach (DiceScript dice in _selectedDices)
                {
                    dice.isScored = true;
                    dice.isSelected = false;
                    dice.isSelectable = false;
                    dice.body.isKinematic = true;
                    dice.transform.localPosition = new Vector3(-21.31f + (2 * nDices - 1), 3.39f, 8.19f - (2 * round - 1));
                    _playableDices.Remove(dice);
                    nDices--;
                }
                _dices = _playableDices.ToArray();
            }
            if (!_dices.Any())
            {
                _dices = _allDices;
                _playableDices = _dices.ToList();
                foreach (DiceScript dice in _dices)
                {
                    dice.resetPosition();
                }
                round = 0;
            }
            _centerMessageTMP.text = "";
            _stayButton.SetActive(false);
            RollDices();
            round++;
        }
        if (_isNewDiceInBankScore)
            CheckScore();
    }

    void RollDices()
    {
        _isRolling = true;
        _diceStillRolling = _dices.Length;
        RoundResetValues();
        foreach (DiceScript dice in _dices)
        {
            dice.sparks.SetActive(false);
            if (!dice.isScored)
                dice.RollDice();
        }
    }

    void OnDiceStopped(int result, DiceScript dice)
    {

        _diceStillRolling--;
        dice.diceFaceNum = result;

        CheckSelectable(result, dice);
        if (_diceStillRolling != 0) return;
        _isRolling = false;
        if (_dices.Count() >= 5)
        {
            _isStraight = HasStraight();
        }
        CheckPifia();

    }

    void CheckSelectable(int result, DiceScript dice)
    {
        if (result == 1 || result == 5)
        {
            dice.isSelectable = true;
            dice.sparks.SetActive(true);
        }
        else
        {
            if (faceCounts.ContainsKey(result))
            {
                faceCounts[result]++;
            }
            else
            {
                faceCounts[result] = 1;
            }
        }

        foreach (DiceScript checkDice in _dices)
        {
            if (faceCounts.Keys.Contains(checkDice.diceFaceNum) && faceCounts[checkDice.diceFaceNum] >= 3)
            {
                checkDice.isSelectable = true;
                checkDice.sparks.SetActive(true);
            }
        }
    }

    void CheckScore()
    {
        _bankScore = 0;
        scoreCounts.Clear();
        foreach (DiceScript dice in _selectedDices)
        {
            if (scoreCounts.ContainsKey(dice.diceFaceNum))
            {
                scoreCounts[dice.diceFaceNum]++;
            }
            else
            {
                scoreCounts[dice.diceFaceNum] = 1;
            }
        }
        if (_isStraight)
        {
            _straightScore = GetStraightScore();
            _bankScore = _straightScore;
            if (_straightScore != 0)
            {
                var faceNumsToRemove = _selectedDices.Select(d => d.diceFaceNum).Distinct();

                foreach (int faceNum in faceNumsToRemove)
                {
                    scoreCounts[faceNum]--;
                }

                DiceScript[] remainingDices = _dices.Except(_selectedDices).ToArray();

                if (remainingDices.Any())
                    if (remainingDices[0].diceFaceNum != 5 && remainingDices[0].diceFaceNum != 1 && remainingDices[0].diceFaceNum != 6)
                        remainingDices[0].isSelectable = false;
            }
        }

        foreach (int key in scoreCounts.Keys)
        {

            if (scoreCounts[key] >= 3)
            {
                int multiplicador = Mathf.Max(1, (int)Mathf.Pow(2, scoreCounts[key] - 3));
                if (key != 1)
                {
                    _bankScore += key * 100 * multiplicador;
                }
                else
                {
                    _bankScore += 1000 * multiplicador;
                }
            }
            else if (key == 1)
            {
                _bankScore += scoreCounts[key] * 100;
            }
            else if (key == 5)
            {
                _bankScore += scoreCounts[key] * 50;
            }
        }

        _isNewDiceInBankScore = false;
        if (_accumulatedBank + _bankScore >= 1000)// _goalScore / 4) Implement graduable fire increasing percent
        {
            _fireEffect.SetActive(true);
        }
        else
        {
            _fireEffect.SetActive(false);
        }
        _bankScoreTMP.text = "Bank Score: " + (_accumulatedBank + _bankScore).ToString();
    }

    void CheckPifia()
    {
        _isPifia = true;
        foreach (DiceScript dice in _dices)
        {
            if (dice.isSelectable)
            {
                _isPifia = false;
                break;
            }
        }
        if (_isPifia)
        {
            _centerMessageTMP.color = Color.darkRed;
            _centerMessageTMP.text = "PIFIA";
            pifiaSound.Play();
            ResetFullValues();

        }
        else
        {
            _stayButton.SetActive(true);
        }
    }
    void RoundResetValues()
    {
        faceCounts.Clear();
        _selectedDices.Clear();
        scoreCounts.Clear();
        _isStraight = false;
        _straightScore = 0;
        _centerMessageTMP.text = "";
    }
    public void ResetFullValues()
    {
        faceCounts.Clear();
        _selectedDices.Clear();
        scoreCounts.Clear();
        _bankScore = 0;
        _accumulatedBank = 0;
        _straightScore = 0;
        _isStraight = false;
        _fireEffect.SetActive(false);
        round = 0;
        _bankScoreTMP.text = "Bank Score: " + _accumulatedBank.ToString();
        _dices = _allDices;
        _playableDices = _dices.ToList();
        foreach (DiceScript dice in _dices)
        {
            dice.transform.localPosition = dice.originalPosition;
            dice.transform.localScale = dice.originalScale;
            dice.rend.material = dice.originalMaterial;
            dice.isRolling = false;
            dice.isScored = false;
            dice.isSelectable = false;
            dice.sparks.SetActive(false);
            dice.isSelected = false;
            dice.body.isKinematic = true;
        }
    }


    bool HasStraight()
    {
        var faceValues = _dices.Select(d => d.diceFaceNum).Distinct().OrderBy(x => x).ToList();

        if (faceValues.Count == 5 && faceValues.SequenceEqual(new List<int> { 1, 2, 3, 4, 5 })
        || faceValues.Count == 5 && faceValues.SequenceEqual(new List<int> { 2, 3, 4, 5, 6 })
        || faceValues.Count == 6 && faceValues.SequenceEqual(new List<int> { 1, 2, 3, 4, 5, 6 }))
        {
            foreach (DiceScript dice in _dices)
            {
                dice.isSelectable = true;
                dice.sparks.SetActive(true);
            }
            return true;
        }
        return false;
    }

    int GetStraightScore()
    {
        var faceValues = _selectedDices.Select(d => d.diceFaceNum).Distinct().OrderBy(x => x).ToList();

        if (faceValues.Count == 5 && faceValues.SequenceEqual(new List<int> { 1, 2, 3, 4, 5 }))
            return 500;


        if (faceValues.Count == 5 && faceValues.SequenceEqual(new List<int> { 2, 3, 4, 5, 6 }))
            return 750;

        if (faceValues.Count == 6 && faceValues.SequenceEqual(new List<int> { 1, 2, 3, 4, 5, 6 }))
            return 1500;

        return 0;
    }

    public void checkWin()
    {
        if (_totalScore >= _goalScore)
        {
            _centerMessageTMP.color = _originalCenterTextColor;
            _centerMessageTMP.text = "YOU WIN!!";
            _totalScoreTMP.gameObject.SetActive(false);
            foreach (DiceScript dice in _allDices)
            {
                dice.gameObject.SetActive(false);
            }
            _bankScoreTMP.gameObject.SetActive(false);
            
        }
    }
}
