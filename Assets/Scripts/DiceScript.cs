using System;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class DiceScript : MonoBehaviour
{
    public Action<int,DiceScript> OnDiceStopped;
    public Rigidbody body;
    [SerializeField] Transform[] _diceSides;
    [SerializeField] float _force = 5f;
    [SerializeField] float _torque = 5f;

    [SerializeField] public GameObject sparks;

    public int diceFaceNum;

    public bool isSelectable = false, isScored = false, isSelected = false, isRolling = false;


    //Change dice parameters when selected
    public Renderer rend;
    public Color originalColor;

    public Material originalMaterial;
    public Material selectedMaterial;

    public Vector3 originalScale;

    public Vector3 originalPosition;
    public Vector3 placedPosition;

    public ScoreScript scoreScript;

    [SerializeField] public AudioSource colisionSound;
    
    [SerializeField] public AudioSource selectDiceSound;

    [SerializeField] public AudioSource mouseOverDiceSound;

    // Hovering and mouseover
    public float scaleMultiplier = 1.1f;
    public float scaleSpeed = 5f;
    private bool isHovered = false;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        scoreScript = FindAnyObjectByType<ScoreScript>();
        originalMaterial = rend.material;
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalColor = rend.material.color;
        body.isKinematic = true;
        transform.rotation = new Quaternion(Random.Range(0,360), Random.Range(0,360),Random.Range(0,360),0);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Ground") && isRolling)
            colisionSound.Play();
    }

    void FixedUpdate()
    {
        if (isSelectable)
        {
            Vector3 targetScale = isHovered ? originalScale * scaleMultiplier : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
        if (isScored)
        {
            rend.material = originalMaterial;
            transform.localScale = originalScale;
            transform.Rotate(Vector3.up * 45f * Time.deltaTime, Space.World);
        }
        else if (body.IsSleeping() && isRolling)
        {
            int result = GetFaceNum();
            OnDiceStopped.Invoke(result, this);
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

        placedPosition = transform.localPosition;

        foreach (Transform side in _diceSides)
        {
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
            selectDiceSound.Play();
            isSelected = !isSelected;

            if (isSelected)
            {
                sparks.SetActive(false);
                rend.material = selectedMaterial;
                transform.localScale *= 1.2f; 
                scoreScript._isNewDiceInBankScore = true;
                scoreScript._selectedDices.Add(this);
            }
            else
            {
                sparks.SetActive(true);
                rend.material = originalMaterial;
                transform.localScale /= 1.2f;
                scoreScript._isNewDiceInBankScore = true;
                scoreScript._selectedDices.Remove(this);
            }
        }
    }
    
    void OnMouseEnter()
    {
        if (isSelectable)
        {
            mouseOverDiceSound.Play();
        }
        isHovered = true;
    }

    void OnMouseExit()
    {
        isHovered = false;
    }

    public void resetValues()
    {
        isSelectable = false;
        isScored = false;
        isSelected = false;
        rend.material = originalMaterial;
        transform.localScale = originalScale;
    }

    public void resetPosition(){
        resetValues();
        transform.localPosition = originalPosition;
    }

}
