using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TDK.RegionTitles
{
    public class RegionTitleHandler : MonoBehaviour
    {
        [SerializeField] private string _regionText;

        void OnTriggerEnter(Collider other)
        {
            RegionTitleManager.Instance?.TriggerRegionTitle(_regionText);
        }
    }
}