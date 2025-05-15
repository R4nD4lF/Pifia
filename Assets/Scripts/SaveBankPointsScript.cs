using UnityEngine;
using UnityEngine.EventSystems;

public class SaveBankPointsScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public ScoreScript scoreScript;
    [SerializeField] private GameObject _stayButton;

    private Vector3 originalScaleButton;
    public float scaleMultiplier = 1.5f;
    public float scaleSpeed = 10f;
    private bool isHovered = false;

    
    void Start()
    {
        scoreScript = FindAnyObjectByType<ScoreScript>();
        originalScaleButton = _stayButton.transform.localScale;

    }

    void Update()
    {
        Vector3 targetScale = isHovered ? originalScaleButton * scaleMultiplier : originalScaleButton;
        _stayButton.transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void SaveScore()
    {
        scoreScript._totalScore += scoreScript._accumulatedBank + scoreScript._bankScore;
        scoreScript.checkWin();
        scoreScript._totalScoreTMP.text = "Total Score: " + scoreScript._totalScore.ToString() + "/" + scoreScript._goalScore.ToString();
        scoreScript.ResetFullValues();
        _stayButton.SetActive(false);
        scoreScript._fireEffect.SetActive(false);
    }
    
      public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
