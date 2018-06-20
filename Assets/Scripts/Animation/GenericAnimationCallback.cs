using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class GenericAnimationCallback : MonoBehaviour
{
    public AnimCallback UponEvent = new AnimCallback();

	public void Event(AnimationEvent e)
    {
        if(UponEvent != null)
        {
            UponEvent.Invoke(e);
        }
    }
}

[Serializable]
public class AnimCallback : UnityEvent<AnimationEvent>
{

}