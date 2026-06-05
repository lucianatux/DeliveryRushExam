using System;
using TMPro;
using UnityEngine;

namespace DeliveryRushExam.UI
{
    public class ScorePopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float lifetime = 1.1f;
        [SerializeField] private float moveSpeed = 55f;

        private CanvasGroup _canvasGroup;
        private Transform _cachedTransform;
        private Action<ScorePopupView> _onReturn;
        private float age;
        private bool isReturned;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _cachedTransform = transform;
        }

        public void Setup(string message, Action<ScorePopupView> onReturn)
        {
            age = 0f;
            isReturned = false;
            messageText.text = message;
            _onReturn = onReturn;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        private void Update()
        {
            if (isReturned)
            {
                return;
            }

            age += Time.deltaTime;
            _cachedTransform.localPosition += Vector3.up * (moveSpeed * Time.deltaTime);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f - age / lifetime;
            }

            if (age >= lifetime)
            {
                isReturned = true;
                _onReturn?.Invoke(this);
            }
        }
    }
}