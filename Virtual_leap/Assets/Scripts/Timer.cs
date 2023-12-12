using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private int minutes;
    [SerializeField] private int seconds;
    [SerializeField] private int miliseconds;
    [SerializeField] private TextMeshProUGUI UI;
    [SerializeField] private CapsuleCollider hitbox;
    [SerializeField] private bool active;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(active == true){
            time += Time.deltaTime;
            minutes = Mathf.FloorToInt(time / 60f);
            seconds = Mathf.FloorToInt(time - minutes * 60f);
            miliseconds = Mathf.FloorToInt((time * 1000) % 1000);
            UI.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, miliseconds);
        }
    }
    public void ChangeStatus(bool status){
        active = status;
    }
}
