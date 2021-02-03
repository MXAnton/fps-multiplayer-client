using UnityEngine;
using TMPro;

public class TMP_IPValidator : MonoBehaviour
{
    void Awake()
    {
        TMP_InputField input = GetComponent<TMP_InputField>();
        if (input)
        {
            input.onValidateInput = ValidateInput;
        }
    }

    static char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (addedChar >= '0' && addedChar <= '9')
        {
            text += addedChar;
            charIndex += 1;
            return addedChar;
        }
        else if (addedChar == '.')
        {
            text += addedChar;
            charIndex += 1;
            return addedChar;
        }

        return (char)0;
    }
}
