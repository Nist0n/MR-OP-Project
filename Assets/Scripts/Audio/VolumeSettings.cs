using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Audio
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider SFXSlider;

        private void Start()
        {
            if (PlayerPrefs.HasKey("MusicVolume") && PlayerPrefs.HasKey("SFXVolume"))
            {
                LoadVolume();
            }
            else
            {
                SetMusicVolume();
                SetSFXVolume();
            }
        }

        public void SetMusicVolume()
        {
            float volume = musicSlider.value;
            audioMixer.SetFloat("Music", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        public void SetSFXVolume()
        {
            float volume = SFXSlider.value;
            audioMixer.SetFloat("SFX", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        private void LoadVolume()
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            musicSlider.value = musicVolume;

            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            SFXSlider.value = sfxVolume;
        }
    }
}
