using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public enum MenuState
    {
        None,
        Death,
        Win
    }

    private MenuState mState = MenuState.None;
    private const float kMenuFadeDelay = 1.0f;

    private float mMenuDeathAlpha = 0.0f;

    public float MenuDeathAlpha { get { return mMenuDeathAlpha; } }

    private string mDeathHint;

    public string DeathHint { get { return mDeathHint; } }

    private float mMenuWinAlpha = 0.0f;

    public float MenuWinAlpha { get { return mMenuWinAlpha; } }

    private float mMenuHighScoresAlpha = 0.0f;

    public float MenuHighScoresAlpha { get { return mMenuHighScoresAlpha; } }

    private string[] mDeathHints = {
                                       "Some enemies can't be killed by hooks",
                                       "Try a somersault!",
                                       "You can only carry a certain number of antacids, try to line up your shots",
                                       "Even if you hook a bird, it can still poop on you on the way down",
                                       ""
                                   };

    // Support for future high scores
    public float MeunHighScoresAlpha { get { return 0f; } }

    // Use this for initialization
    void Start()
    {
    }

    public void MenuTransition(MenuState state)
    {
        // Game jam level FSM :P
        switch (state)
        {
            case MenuState.Death:
                iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1,
                    "onupdate", "OnUpdateMenuDeathAlpha", "time", kMenuFadeDelay,
                    "easetype", iTween.EaseType.easeInQuad));
                mDeathHint = mDeathHints[Random.Range(0, mDeathHints.Length - 1)];
                break;
            case MenuState.Win:
                iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1,
                    "onupdate", "OnUpdateMenuWinAlpha", "time", kMenuFadeDelay,
                    "easetype", iTween.EaseType.easeInQuad));
                break;
        }
        mState = state;
    }

    public void ToggleHighScores()
    {
        if (mMenuHighScoresAlpha == 0.0f)
        {
            iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f,
                        "onupdate", "OnUpdateMenuHighScoresAlpha", "time", kMenuFadeDelay,
                        "easetype", iTween.EaseType.easeInQuad));
        }
        else if (mMenuHighScoresAlpha == 1.0f)
        {
            iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f,
                        "onupdate", "OnUpdateMenuHighScoresAlpha", "time", kMenuFadeDelay,
                        "easetype", iTween.EaseType.easeInQuad));
        }
    }

    void OnUpdateMenuHighScoresAlpha(float value)
    {
        mMenuHighScoresAlpha = value;
    }

    void OnUpdateMenuWinAlpha(float value)
    {
        mMenuWinAlpha = value;
    }

    void OnUpdateMenuDeathAlpha(float value)
    {
        mMenuDeathAlpha = value;
    }

    public void RestartGame()
    {
        Application.LoadLevel("sandbox");
    }

    // Update is called once per frame
    void Update()
    {
    }
}