using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODPlayer : MonoBehaviour
{
    public void PlayOneshot(StudioEventEmitter sfx)
    {
        EventInstance inst = RuntimeManager.CreateInstance(sfx.EventReference);
        inst.start();
        inst.release();
    }
}