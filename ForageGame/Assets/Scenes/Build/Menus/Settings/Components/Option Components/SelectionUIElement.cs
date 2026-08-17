using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SelectionUIElement : MonoBehaviour
{
    [SerializeField] private string[] _options;
    [SerializeField] private TMP_Text _textBox;
    public UnityEvent OnChange;
    public int _currentOption { get; private set; } = 0;

    public void SetOptions(string[] options) => _options = options;

    public void SetCurrentOption(int option)
    {
        Debug.Log(_options.Length);
        _currentOption = (option % _options.Length + _options.Length) % _options.Length;
        RefreshVisuals();
        OnChange.Invoke();
        Debug.Log("ok");
    }

    public void LeftButtonPressed() => SetCurrentOption(_currentOption - 1);
    public void RightButtonPressed() => SetCurrentOption(_currentOption + 1);

    public void RefreshVisuals()
    {
        if (0 <= _currentOption && _currentOption < _options.Length)
            _textBox.text = _options[_currentOption];
        else
            _textBox.text = "N/A";
    }
}
