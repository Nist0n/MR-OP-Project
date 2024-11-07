using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
        public static AudioManager instance;

        [SerializeField] AudioSource musicSource;
        [SerializeField] AudioSource SFXSource;
        [SerializeField] List<Sound> music, sounds;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void PlayMusic(string soundName)
        {
            Sound s = music.Find(x => x.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Music: " + soundName + " not found!");
                return;
            }
            
            musicSource.clip = s.audio;
            musicSource.Play();
        }

        public void PlaySFX(string soundName)
        {
            Sound s = sounds.Find(x => x.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound " + soundName + " not found!");
                return;
            }
            
            SFXSource.PlayOneShot(s.audio);
        }

        public void PlayRandomSoundByName(string soundName, AudioSource source)
        {
            List<Sound> matchingSounds = sounds.FindAll(sound => sound.name == soundName);

            if (matchingSounds.Count > 0)
            {
                int randomIndex = Random.Range(0, matchingSounds.Count);
                Sound randomSound = matchingSounds[randomIndex];

                source.PlayOneShot(randomSound.audio);
            }
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }
}
