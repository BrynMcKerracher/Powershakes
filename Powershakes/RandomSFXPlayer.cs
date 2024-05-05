using System;
using System.Collections;
using UnityEngine;
using ValheimModdingWiki;

namespace Powershakes
{
    public class RandomSFXPlayer : MonoBehaviour
    {
        public float MaxDelay = 10;
        public float MinDelay = 5;

        public void Trigger(Transform attachPoint)
        {
            StartCoroutine(SpawnSFX(attachPoint));
        }

        protected IEnumerator SpawnSFX(Transform attachPoint)
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(MinDelay, MaxDelay));

                GameObject gutBlasterEffectsSFX = Instantiate(PowershakesPlugin.GuckshakeSFXPrefab,  attachPoint);
                gutBlasterEffectsSFX.transform.localPosition = Vector3.zero;
                gutBlasterEffectsSFX.AddComponent<AudioBinder>();
            }
        }
    }
}
