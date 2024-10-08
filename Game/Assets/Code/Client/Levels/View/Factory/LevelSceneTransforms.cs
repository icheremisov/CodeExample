using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Client.Levels.View.Factory
{
    [Serializable]
    public class LevelSceneTransforms
    {
        [Required] public Transform RootTransform;
        [Required] public Transform ItemHolder;
        [Required] public Transform CameraDefaultTarget;
    }
}