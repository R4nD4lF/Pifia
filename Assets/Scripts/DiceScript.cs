using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class DiceScript : MonoBehaviour
{
    public Action<int,DiceScript> OnDiceStopped;
    public Rigidbody body;
    [SerializeField] Transform[] _diceSides;
    [SerializeField] float _force = 5f;
    [SerializeField] float _torque = 5f;

    public int diceFaceNum;

    public bool isSelectable = false, isScored = false, isSelected = false, isRolling = false;


    //Change dice parameters when selected
    public Renderer rend;
    public Color originalColor;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.green;

    public Vector3 originalScale;

    public Vector3 originalPosition;

    public ScoreScript scoreScript;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        scoreScript = FindAnyObjectByType<ScoreScript>();
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalColor = rend.material.color;
        body.isKinematic = true;
        transform.rotation = new Quaternion(Random.Range(0,360), Random.Range(0,360),Random.Range(0,360),0);
    }
    

    void FixedUpdate()
    {
        if(isScored){
            rend.material.color = originalColor;
            transform.localScale = originalScale;
        }else if(body.IsSleeping() && isRolling){
            int result = GetFaceNum();
            OnDiceStopped.Invoke(result,this);
        }
    }
    public void RollDice(){

        body.isKinematic = false;

        resetValues();

        Vector3 force = new Vector3(0f,_force,0f);
        Vector3 torque = new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)) * _torque;

        body.AddForce(force, ForceMode.Impulse);
        body.AddTorque(torque, ForceMode.Impulse);
        
        isRolling = true;
    }

    int GetFaceNum(){
        Transform upSide = null;
        float maxFace = -1;

        foreach(Transform side in _diceSides){
            float face = Vector3.Dot(side.up, Vector3.up);

            if (!(face > maxFace)) continue;
                maxFace = face;
                upSide = side;
        }
        
        isRolling = false;

        if(upSide != null) return int.Parse(upSide.name);
        return 0;
    }

    
     void OnMouseDown()
    {
        if (isSelectable)
        {
            isSelected = !isSelected;

            if (isSelected)
            {
                rend.material.color = selectedColor;
                transform.localScale *= 1.2f; 
                scoreScript._isNewDiceInBankScore = true;
                scoreScript._selectedDices.Add(this);
            }
            else
            {
                rend.material.color = originalColor;
                transform.localScale /= 1.2f;
                scoreScript._isNewDiceInBankScore = true;
                scoreScript._selectedDices.Remove(this);
            }
        }
    }

    public void resetValues(){
        isSelectable = false;
        isScored = false;
        isSelected = false;
        rend.material.color = originalColor;
        transform.localScale = originalScale;
    }

    public void resetPosition(){
        resetValues();
        transform.localPosition = originalPosition;
    }

}
