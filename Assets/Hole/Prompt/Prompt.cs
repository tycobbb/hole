using TMPro;
using UnityEngine;

public class Prompt: MonoBehaviour {
    // -- module --
    /// get the module
    public static Prompt Get {
        get => FindObjectOfType<Prompt>();
    }

    // -- nodes --
    TMP_Text mText;

    // -- lifecycle --
    void Awake() {
        // get node deps
        mText = GetComponent<TMP_Text>();
    }

    // -- commands --
    /// set the text and show prompt if necessary
    public void Set(string msg) {
        mText.text = msg;

        if (!mText.IsActive()) {
            mText.SetActive(true);
        }
    }

    /// hide the prompt
    public void Hide() {
        mText.text = "";
        mText.SetActive(false);
    }
}
