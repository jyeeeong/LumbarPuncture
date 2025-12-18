using System.Collections;
using UnityEngine;
using TMPro;

public class WarningUI : MonoBehaviour
{
    public TextMeshProUGUI warningText;
    public float hideDelay = 0.2f;

    Coroutine hideCo;

    void Awake()
    {
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public void Show(string msg)
    {
        if (warningText == null) return;

        if (hideCo != null) StopCoroutine(hideCo);

        warningText.text = msg;
        warningText.gameObject.SetActive(true);
    }

    public void HideSoon()
    {
        if (warningText == null) return;

        if (hideCo != null) StopCoroutine(hideCo);
        hideCo = StartCoroutine(HideAfter(hideDelay));
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (warningText != null) warningText.gameObject.SetActive(false);
        hideCo = null;
    }
}
