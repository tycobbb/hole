using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Game: MonoBehaviour {
    // -- commands --
    /// reset the current scene
    void Reset() {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    /// take a screenshot of the scene
    void Screenshot() {
        var app = Application.productName.ToLower();
        ScreenCapture.CaptureScreenshot($"{app}.png");
    }

    // -- events --
    /// catch the reset input event
    public void OnReset(InputAction.CallbackContext ctx) {
        Reset();
    }

    /// catch the screenshot input event
    public void OnScreenshot(InputAction.CallbackContext ctx) {
        Screenshot();
    }
}
