using System.Collections;
using TMPro;
using UnityEngine;

public class LockPuzzle : MonoBehaviour
{
    [SerializeField]
    private TMP_Text[] digits;

    [SerializeField]
    Transform masterIndicator;

    [SerializeField]
    [Range(0.01f, 1f)]
    float rollAnimationTime;

    [SerializeField]
    [Range(0.01f, 1f)]
    float slideAnimationTime;

    [SerializeField]
    float rollDistance = 50f;

    [SerializeField]
    AnimationCurve animationCurve;

    private int selectedTable = 0;
    private string[] validDigits = new string[10]
    {
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
    };

    void Start()
    {
        for (int i = 0; i < digits.Length; i++)
            SetDigits(i, validDigits);

        Slide(selectedTable);
    }

    float lastRollInteraction = 0;
    float lastSlideInteraction = 0;

    void Update()
    {
        // ROLL INPUT
        if (lastRollInteraction >= rollAnimationTime)
        {
            if (Input.GetKey(KeyCode.W))
                Roll(selectedTable, true);
            else if (Input.GetKey(KeyCode.S))
                Roll(selectedTable, false);
            lastRollInteraction = 0;
        }
        else
            lastRollInteraction += Time.deltaTime;

        // SLIDE INPUT
        if (lastSlideInteraction >= slideAnimationTime)
        {
            if (Input.GetKey(KeyCode.A))
            {
                selectedTable = Mathf.Clamp(selectedTable - 1, 0, digits.Length - 1);
                Slide(selectedTable);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                selectedTable = Mathf.Clamp(selectedTable + 1, 0, digits.Length - 1);
                Slide(selectedTable);
            }
            lastSlideInteraction = 0;
        }
        else
            lastSlideInteraction += Time.deltaTime;
    }

    void SetDigits(int table, string[] digits)
    {
        string text = "\n";
        for (int i = 0; i < digits.Length; i++)
        {
            text += digits[i];
            if (i == digits.Length - 1)
                continue;
            text += "\n";
        }

        this.digits[table].text = text;
    }

    IEnumerator RollDigits(int table, bool up)
    { // four step system
        bool finished = false;

        TMP_Text digits = this.digits[table];

        float time = 0;
        Vector3 initialPos = digits.transform.localPosition;
        while (!finished)
        {
            float v = animationCurve.Evaluate(time / rollAnimationTime) * rollDistance; // 0 - 1
            int dir = up ? -1 : 1;
            digits.transform.localPosition = initialPos + new Vector3(0f, v * dir, 0f);
            time += Time.deltaTime * rollAnimationTime;
            if (time > rollAnimationTime)
                finished = true;
            yield return null;
        }

        digits.transform.localPosition = initialPos;
        string newText;
        if (up)
            newText =
                digits.text[0]
                + digits.text.Substring(digits.text.Length - 3, 3)
                + digits.text.Substring(1, digits.text.Length - 3);
        else
            newText =
                digits.text[0]
                + digits.text.Substring(digits.text.Length - 3, 3)
                + digits.text.Substring(1, digits.text.Length - 3);
        digits.text = newText;
        yield break;
    }

    /*
     \n1\n2\n3\n4\n5\n6\n7\n8\n9\n0
     \n0\n1\n2\n3\n4\n5\n6\n7\n8\n9
     */

    IEnumerator SlideIndicator(int table)
    {
        bool finished = false;
        float time = 0f;
        Vector3 initialPos = masterIndicator.localPosition;
        float startPosition = initialPos.x;
        float targetPosition = digits[table].transform.parent.localPosition.x;
        float amplitude = targetPosition - startPosition;
        while (!finished)
        {
            float v = animationCurve.Evaluate(time / rollAnimationTime) * amplitude; // 0 - 1

            masterIndicator.localPosition = initialPos + new Vector3(v, 0f, 0f);
            time += Time.deltaTime * slideAnimationTime;
            if (time > rollAnimationTime)
                finished = true;
            yield return null;
        }

        yield break;
    }

    void Slide(int table) => StartCoroutine(SlideIndicator(table));

    void Roll(int table, bool up) => StartCoroutine(RollDigits(table, up));

    void ShowIndicator() => masterIndicator.gameObject.SetActive(true);

    void HideIndicator() => masterIndicator.gameObject.SetActive(false);
}
