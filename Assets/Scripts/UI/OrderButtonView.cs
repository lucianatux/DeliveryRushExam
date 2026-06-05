using System;
using DeliveryRushExam.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeliveryRushExam.UI
{
    public class OrderButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Button completeButton;

        private OrderData orderData;
        private Action<string> onCompleteClicked;
        private int _lastDisplayedSeconds = -1;

        public void Setup(OrderData order, Action<string> completeCallback)
        {
            orderData = order;
            onCompleteClicked = completeCallback;

            if (completeButton == null)
            {
                completeButton = GetComponent<Button>();
            }

            completeButton.onClick.RemoveAllListeners();
            completeButton.onClick.AddListener(HandleClick);

            // Datos fijos del pedido: se asignan una sola vez.
            titleText.text = "Deliver to " + order.customerName;
            rewardText.text = "+" + order.rewardPoints + " pts / +" + order.rewardCoins + " coins";

            _lastDisplayedSeconds = -1; // fuerza primer refresh del timer
            Refresh();
        }

        public void Refresh()
        {
            if (orderData == null)
            {
                return;
            }

            int seconds = Mathf.CeilToInt(orderData.remainingTime);
            if (seconds != _lastDisplayedSeconds)
            {
                timerText.text = "Time " + seconds;
                _lastDisplayedSeconds = seconds;
            }
        }

        private void HandleClick()
        {
            if (orderData != null)
            {
                onCompleteClicked?.Invoke(orderData.id);
            }
        }
    }
}