using UnityEngine;
using System.Threading.Tasks;
using AudioIntegration;

[RequireComponent(typeof(Animator))]
public class TransitionScreenController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    void OnValidate()
    {
        _animator = GetComponent<Animator>();
    }

    [SerializeField] private float _enterDuration = 1f;
    [SerializeField] private float _exitDuration = 1f;

    public async Task FadeOutAsync()
    {
        AudioManager.Instance.StopAndReleaseAll();
        _animator.SetTrigger("Enter");
        await Task.Delay(Mathf.CeilToInt(_enterDuration * 1000));
    }

    public void FadeOut()
    {
        _animator.SetTrigger("Enter");
    }

    public async Task FadeInAsync()
    {
        _animator.SetTrigger("Exit");
        await Task.Delay(Mathf.CeilToInt(_exitDuration * 100));
    }

    public void FadeIn()
    {
        _animator.SetTrigger("Exit");
    }
}