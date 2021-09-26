using TMPro;
using UnityEngine;

/// the text ui prompt
public class Prompt: MonoBehaviour {
    // -- module --
    /// get the module
    public static Prompt Get {
        get => FindObjectOfType<Prompt>();
    }

    // -- config --
    /// the text label
    [SerializeField] TMP_Text mLabel;

    // -- lifecycle --
    void Awake() {
        // start hidden
        Hide();
    }

    // -- props(hot) --
    /// set the text and show prompt if necessary
    public void Show(string msg) {
        mLabel.text = msg;
    }

    /// hide the prompt
    public void Hide() {
        mLabel.text = "";
    }
}