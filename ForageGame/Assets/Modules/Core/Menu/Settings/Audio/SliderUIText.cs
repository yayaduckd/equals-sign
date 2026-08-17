using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderUIText : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _text;
    [Header("Options")]
    [SerializeField] private string prefix = "";
    [SerializeField] private string affix = "";

    public void OnValueChanged()
    {
        _text.text = prefix + Mathf.RoundToInt(_slider.value).ToString() + affix;
    }

}
