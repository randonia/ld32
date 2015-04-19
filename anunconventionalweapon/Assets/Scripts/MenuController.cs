using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public enum MenuState
    {
        None,
        Death
    }

    private MenuState mState = MenuState.None;
    private const float kMenuFadeDelay = 1.0f;

    private float mMenuDeathAlpha = 0.0f;

    public float MenuDeathAlpha { get { return mMenuDeathAlpha; } }

    private string mDeathHint;

    public string DeathHint { get { return mDeathHint; } }

    private string[] mDeathHints = {
                                       "Some enemies can't be killed by hooks",
                                       "Try a somersault!",
                                       "You can only carry a certain number of debris, try to line up your shots",
                                       ""
                                   };

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
        }
        mState = state;
    }

    public void RestartGame()
    {
    }

    void OnUpdateMenuDeathAlpha(float value)
    {
        mMenuDeathAlpha = value;
    }

    // Update is called once per frame
    void Update()
    {
    }
}