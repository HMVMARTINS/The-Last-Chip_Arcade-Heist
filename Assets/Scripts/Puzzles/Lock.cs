using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockPuzzle : InteractableGame
{
    [SerializeField]
    private TMP_Text[] digits;

    [SerializeField]
    Transform masterIndicator;

    [SerializeField]
    RectTransform[] outliners;

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

    [SerializeField]
    AnimationCurve finalAnimationCurve;

    [SerializeField]
    Vector2 finalOutlinerSize;

    [SerializeField]
    string correctCode;

    [SerializeField]
    InteractableObject interactableObject;

    [SerializeField]
    AudioSource[] audioSources;

    [SerializeField]
    AudioClip rollSound;

    [SerializeField]
    AudioClip correctSound;

    [SerializeField]
    AudioClip openingLockSound;

    private int selectedTable = 1;
    static int[] selectedNumbers = new int[3];
    private string[] validDigits = new string[10]
    {
        "6",
        "7",
        "8",
        "9",
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
    };

    bool interactable = true;

    void Start()
    {
        for (int i = 0; i < digits.Length; i++)
            SetDigits(i, string.Concat(validDigits));
    }

    float lastRollInteraction = 0;
    float lastSlideInteraction = 0;

    void Update()
    {
        if (!interactable)
            return;
        // ROLL INPUT
        if (lastRollInteraction >= rollAnimationTime + 0.1f)
        {
            if (Input.GetKey(KeyCode.W))
            {
                lastRollInteraction = 0;
                Roll(selectedTable, true);
                AutoRoll(selectedTable, true);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                lastRollInteraction = 0;
                Roll(selectedTable, false);
                AutoRoll(selectedTable, false);
            }
        }
        else
            lastRollInteraction += Time.deltaTime;

        // SLIDE INPUT
        if (lastSlideInteraction >= slideAnimationTime + 0.1f)
        {
            if (Input.GetKey(KeyCode.A))
            {
                selectedTable = Mathf.Clamp(selectedTable + 1, 0, digits.Length - 1);
                lastSlideInteraction = 0;
                Slide(selectedTable);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                selectedTable = Mathf.Clamp(selectedTable - 1, 0, digits.Length - 1);
                lastSlideInteraction = 0;
                Slide(selectedTable);
            }
        }
        else
            lastSlideInteraction += Time.deltaTime;
    }

    void CheckForCode()
    {
        string currentCode = string.Concat(selectedNumbers.Reverse());
        if (currentCode == correctCode)
            FinishPuzzle();
    }

    void SetDigits(int table, string digits)
    {
        string text = FormatData(digits);

        this.digits[table].text = text;
    }

    IEnumerator RollDigits(int table, bool up, int rotations, float initialDelay = 0f)
    {
        yield return new WaitForSeconds(initialDelay);
        for (int i = 0; i < rotations; i++)
        {
            bool finished = false;
            string newText;
            TMP_Text digits = this.digits[table];

            if (!up)
            {
                string numbers = CleanData(digits.text);
                string newNumbers = numbers.Substring(1) + numbers[0];
                newText = FormatData(newNumbers);

                if (selectedNumbers[table] + 1 > 9)
                    selectedNumbers[table] = 0;
                else
                    selectedNumbers[table] += 1;
            }
            else
            {
                string numbers = CleanData(digits.text);
                string newNumbers =
                    numbers[numbers.Length - 1] + numbers.Substring(0, numbers.Length - 1);
                newText = FormatData(newNumbers);

                if (selectedNumbers[table] - 1 < 0)
                    selectedNumbers[table] = 9;
                else
                    selectedNumbers[table] -= 1;
            }

            string currentCode = string.Concat(selectedNumbers);

            if (correctCode[table] == currentCode[table])
                audioSources[table].clip = correctSound;
            else
                audioSources[table].clip = rollSound;

            audioSources[table].Play();
            Debug.Log(
                "SLOT: " + table + " | CODE: " + currentCode[table] + " -> " + correctCode[table]
            );

            float time = 0;
            Vector3 initialPos = digits.transform.localPosition;
            while (!finished)
            {
                float v = animationCurve.Evaluate(time / rollAnimationTime) * rollDistance; // 0 - 1
                int dir = up ? -1 : 1;
                digits.transform.localPosition = initialPos + new Vector3(0f, v * dir, 0f);
                time += Time.deltaTime;
                if (time > rollAnimationTime)
                    finished = true;
                yield return null;
            }

            digits.transform.localPosition = initialPos;
            digits.text = newText;
            Debug.Log(
                "|"
                    + selectedNumbers[2]
                    + "|"
                    + selectedNumbers[1]
                    + "|"
                    + selectedNumbers[0]
                    + "| => |"
                    + correctCode[0]
                    + "|"
                    + correctCode[1]
                    + "|"
                    + correctCode[2]
                    + "|"
            );
        }
        CheckForCode();
        yield break;
    }

    /*
    \n0\n1\n2\n3\n4\n5\n6\n7\n8\n9
    \n9\n0\n1\n2\n3\n4\n5\n6\n7\n8
     */

    string FormatData(string data)
    {
        string cleanData = CleanData(data);

        string newData = "";
        for (int i = 0; i < data.Length; i++)
        {
            newData += "\n";
            if (i == data.Length / 2 - 1)
                newData += "<b>";
            newData += cleanData[i];
            if (i == data.Length / 2 - 1)
                newData += "</b>";
        }
        return newData;
    }

    string CleanData(string data) => data.Replace("\n", "").Replace("<b>", "").Replace("</b>", "");

    IEnumerator SlideIndicator(int table)
    {
        table = Mathf.Clamp(table, 0, digits.Length - 1);

        bool finished = false;
        float time = 0f;
        Vector3 initialPos = masterIndicator.localPosition;
        float startPosition = initialPos.x;
        float targetPosition = digits[table].transform.parent.localPosition.x;
        float amplitude = targetPosition - startPosition;

        while (!finished)
        {
            float v = animationCurve.Evaluate(time / slideAnimationTime) * amplitude; // 0 - 1

            masterIndicator.localPosition = initialPos + new Vector3(v, 0f, 0f);
            time += Time.deltaTime;
            if (time > slideAnimationTime)
            {
                float distance = Mathf.Abs(targetPosition - masterIndicator.localPosition.x);
                if (distance < 0.05f)
                    finished = true;
            }
            yield return null;
        }

        yield break;
    }

    void Slide(int table) => StartCoroutine(SlideIndicator(table));

    void Roll(int table, bool up, int rotations = 1, float initialDelay = 0f) =>
        StartCoroutine(RollDigits(table, up, rotations, initialDelay));

    void AutoRoll(int tableSelected, bool up)
    {
        switch (tableSelected)
        {
            case 0:
                Roll(2, !up, 1, 0.2f);
                break;
            case 1:
                Roll(0, up, 2, 0.2f);
                break;
            case 2:
                Roll(0, up, 1, 0.2f);
                break;
        }
    }

    void FinishPuzzle()
    {
        StartCoroutine(FinalAnimation());
    }

    IEnumerator FinalAnimation(float delay = 3f, float animationDuration = 0.75f)
    {
        interactable = false;

        for (int i = 0; i < outliners.Length; i++)
            outliners[i].gameObject.SetActive(true);

        masterIndicator.gameObject.SetActive(false);

        Vector2 defaultSize = outliners[0].rect.size;

        bool finished = false;

        Image[] images = new Image[outliners.Length];
        for (int i = 0; i < outliners.Length; i++)
        {
            images[i] = outliners[i].GetComponent<Image>();
            if (images[i] == null)
                Debug.Log("No image found");
        }

        Color defaultColor = images[0].color;

        Color newColor = defaultColor;
        float time = 0;
        while (!finished)
        {
            float v = time / animationDuration;

            if (time >= animationDuration)
            {
                time = animationDuration;
                finished = true;
            }
            time += Time.deltaTime;

            newColor.a = 1 - v;
            Vector2 newSize = Vector2.Lerp(defaultSize, finalOutlinerSize, v);
            for (int i = 0; i < outliners.Length; i++)
            {
                outliners[i].sizeDelta = new Vector2(newSize.x, newSize.y);

                images[i].color = newColor;
            }
            yield return null;
        }

        audioSources[0].clip = openingLockSound;
        audioSources[0].Play();

        yield return new WaitForSeconds(delay);

        interactableObject.SetInteractability(false);
        gameObject.SetActive(false); // hide UI
        yield break;
    }
}
