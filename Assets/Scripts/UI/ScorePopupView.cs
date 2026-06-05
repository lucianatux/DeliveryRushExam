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
        private float age;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Setup(string message)
        {
            age = 0f;
            messageText.text = message;
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.localPosition += Vector3.up * moveSpeed * Time.deltaTime;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f - age / lifetime;
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}