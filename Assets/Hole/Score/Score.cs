using System;
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
    /// the maximum height reached
    float mMaxHeight = 0.0f;

    /// the time spent w/ 4 contacts
    float mContactTime = 0.0f;

    /// if the contact time is ticking
    bool mIsContactTimeTicking = false;

    // -- lifecycle --
    void Update() {
        if (mIsContactTimeTicking) {
            AddContactTime();
        }
    }

    // -- commands --
    /// record the current number of contacts
    public void RecordContacts(int nContacts) {
        mIsContactTimeTicking = nContacts == 4;
    }

    /// record the height, storing the highest
    public void RecordHeight(float height) {
        if (height > mMaxHeight) {
            mMaxHeight = height;
            ShowScore();
        }
    }

    /// adds this frame to the contact time
    void AddContactTime() {
        // update the contact time
        var prev = mContactTime;
        var next = prev + Time.deltaTime;
        mContactTime = next;

        // if the second changed, show score
        if ((int)prev != (int)next) {
            ShowScore();
        }
    }

    /// show the current score
    void ShowScore() {
        var score = (int)mContactTime + (int)(mMaxHeight * 10.0f);
        mLabel.SetText(score.ToString());
    }
}