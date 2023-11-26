using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioSources;
    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = audioSources[0];
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeTrack(int num)
    {
        if(audioSource.clip.ToString() != audioSources[num].ToString())
        {
            audioSource.clip = audioSources[num];
            audioSource.Play();
        }
        
    }
}
