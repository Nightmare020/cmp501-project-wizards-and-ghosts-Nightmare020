using System;
using UnityEngine;

namespace Utils
{
    public class ShowSomethingWhenNotRendered : MonoBehaviour
    {
        [SerializeField] private CanvasGroup hudElement;

        private void OnBecameInvisible()
        {
            hudElement.transform.SetSiblingIndex(1);
            hudElement.alpha = 1;
        }

        private void OnBecameVisible()
        {
            hudElement.alpha = 0;
            hudElement.transform.SetAsLastSibling();
        }
    }
}