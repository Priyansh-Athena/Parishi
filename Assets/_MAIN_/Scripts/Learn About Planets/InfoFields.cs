using TMPro;
using UnityEngine;

public class InfoFields : MonoBehaviour
{
    public TMP_Text title, value;

    public void SetData(string _title, string _value)
    {
        if(title != null)
            title.text = _title;

        if(value != null)
            value.text = _value;
    }
}
