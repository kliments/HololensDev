using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class hapticHelper : MonoBehaviour {
    public AudioClip hapticSound;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void HapticFeedback(float volumeScale)
    {
        Debug.Log("Subpack should start vibrating");
        AudioSource source;
        source = GetComponent<AudioSource>();
        source.PlayOneShot(hapticSound, volumeScale);
    }

}
