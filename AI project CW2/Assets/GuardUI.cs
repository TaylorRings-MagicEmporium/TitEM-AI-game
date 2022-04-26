using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI element of the guard like suspicion level and thought bubbles
/// </summary>
public class GuardUI : MonoBehaviour
{

    public Image SuspicionDisplay;
    public Image SuspicionMeter;

    public GameObject canvas;

    public Image ThoughtHolder;
    public List<Sprite> guard_thoughts = new List<Sprite>();

    //canvas offset
    Vector3 Canvas_diff;

    // Start is called before the first frame update
    void Start()
    {
        Canvas_diff = canvas.transform.position - transform.position;
    }

    public void ChangeSuspicionFillAmount(float newValue, float maxValue = 100)
    {
        SuspicionMeter.fillAmount = newValue / maxValue;
    }

    private void Update()
    {
        canvas.transform.position = transform.position + Canvas_diff;
    }

    public void ToggleThoughtImage(bool state)
    {
        ThoughtHolder.enabled = state;
    }

    public bool GetThoughtImageActiveState()
    {
        return ThoughtHolder.enabled;
    }

    public void ChangeThoughtImage(int index)
    {
        if (index >= guard_thoughts.Count)
        {
            Debug.LogWarning("WARNING: index not valid with number of guard thoughts. ignoring function", this);
            return;
        }

        ThoughtHolder.sprite = guard_thoughts[index];
    }
}
