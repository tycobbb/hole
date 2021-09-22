using Hertzole.GoldPlayer;
using UnityEngine;

public class AdjustInput: MonoBehaviour {
    void Awake() {
        var gpc = GetComponent<GoldPlayerController>();

        // reduce sensitivity in webgl
        #if !UNITY_EDITOR && UNITY_WEBGL
        gpc.Camera.MouseSensitivity *= 0.25f;
        #endif
    }
}
