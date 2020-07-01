using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

namespace Utils
{
    [CreateAssetMenu(fileName = "DevLogConfig", menuName = "Systems/Utils/DevLogConfig", order = 1)]
    public class DevLogConfig : ScriptableObject
    {
        public Text devBuildLogger => GameObject.Find("DevLogText").GetComponent<Text>();
    }
}
