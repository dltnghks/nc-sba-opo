using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StringEventChannel", menuName = "nc-sba-opo/Events/String Event Channel")]
public sealed class StringEventChannelSO : ScriptableObject
{
    public event Action<string> OnEventRaised;

    public void RaiseEvent(string value)
    {
        OnEventRaised?.Invoke(value);
    }
}
