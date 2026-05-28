using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/OnCriticalHit")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "OnCriticalHit", message: "Agent got CriticalHit", category: "Events", id: "1ae66d36341030f8cbc693a904c3f9c6")]
public sealed partial class OnCriticalHit : EventChannel { }
