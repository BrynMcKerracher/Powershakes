using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// From the Valheim Modding Community Page: https://github.com/Valheim-Modding/Wiki/wiki/Custom-Sounds
///</summary>
namespace ValheimModdingWiki
{
    public class AudioBinder : MonoBehaviour 
    {
        private void Awake() 
        {
            GetComponent<AudioSource>().outputAudioMixerGroup = AudioMan.instance.m_ambientMixer;
        }
    }
}