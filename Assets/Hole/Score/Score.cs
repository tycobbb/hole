using TMPro;
using UnityEngine;

/// the score ui label
public class Score: MonoBehaviour {
    // -- module --
    /// get the module
    public static Score Get {
        get => FindObjectOfType<Score>();
    }

    // -- config --
    /// the text label
    [SerializeField] TMP_Text mLabel;

    // -- props --
    int mMax = 0;

    // -- commands --
    /// record the score, storing the highest
    public void Record(int score) {
        if (score > mMax) {
            mMax = score;
            mLabel.SetText(score.ToString());
        }
    }
}