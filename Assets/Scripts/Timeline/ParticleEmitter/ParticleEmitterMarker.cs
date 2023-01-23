using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class ParticleEmitterMarker : Marker, INotification
{
    public PropertyName id => new PropertyName();

    [SerializeField] string message = "";

    public string Message => message;
}
