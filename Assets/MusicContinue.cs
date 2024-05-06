using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicContinue : MonoBehaviour
{
    [SerializeField]
    private AudioSource backgroundmusic;

        private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void PlayMusic()
    {
      if (backgroundmusic.isPlaying)
      {
        return;
      }
      backgroundmusic.Play();
    }
    public void StopMusic()
    {
        backgroundmusic.Stop();
    }
    
    
}
