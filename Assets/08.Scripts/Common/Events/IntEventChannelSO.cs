using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IntEventChannel", menuName = "nc-sba-opo/Events/Int Event Channel")]
public sealed class IntEventChannelSO : ScriptableObject
{
    public event Action<int> OnEventRaised;

    public void RaiseEvent(int value)
    {
        OnEventRaised?.Invoke(value);
    }
}
