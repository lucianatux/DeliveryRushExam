using System.Collections.Generic;
using DeliveryRushExam.Core;
using DeliveryRushExam.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeliveryRushExam.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private ScoreManager scoreManager;

        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text ordersCountText;

        [Header("Orders")]
        [SerializeField] private RectTransform ordersContainer;
        [SerializeField] private OrderButtonView orderButtonPrefab;

        [Header("Popups")]
        [SerializeField] private RectTransform popupsContainer;
        [SerializeField] private ScorePopupView scorePopupPrefab;

        [Header("Panels")]
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text resultsText;

        private readonly List<OrderButtonView> orderViews = new List<OrderButtonView>();
        private Canvas _canvas;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (orderManager == null)
            {
                orderManager = FindFirstObjectByType<OrderManager>();
            }

            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
            }

            _canvas = GetComponentInParent<Canvas>();
        }

        private void OnEnable()
        {
            orderManager.OrdersChanged += RefreshOrderList;
            scoreManager.OrderScored += ShowScorePopup;
        }

        private void OnDisable()
        {
            if (orderManager != null)
            {
                orderManager.OrdersChanged -= RefreshOrderList;
            }

            if (scoreManager != null)
            {
                scoreManager.OrderScored -= ShowScorePopup;
            }
        }

        private void Update()
        {
            if (scoreManager == null || gameManager == null)
            {
                return;
            }

            scoreText.text = "Score: " + scoreManager.Score;
            coinsText.text = "Coins: " + scoreManager.Coins;
            timerText.text = "Time: " + Mathf.CeilToInt(gameManager.RemainingTime);
            ordersCountText.text = "Orders: " + orderManager.ActiveOrders.Count;

            for (int i = 0; i < orderViews.Count; i++)
            {
                orderViews[i].Refresh();
            }
        }

        public void ShowGameplay()
        {
            gameplayPanel.SetActive(true);
            resultsPanel.SetActive(false);
            RefreshOrderList();
        }

        public void ShowResults(int score, int coins, int completedOrders, PlayerProgressData progressData)
        {
            gameplayPanel.SetActive(false);
            resultsPanel.SetActive(true);

            resultsText.text =
                "Delivery Rush Results\n" +
                "Score: " + score + "\n" +
                "Coins earned: " + coins + "\n" +
                "Completed orders: " + completedOrders + "\n" +
                "Best score: " + progressData.bestScore + "\n" +
                "Total coins: " + progressData.totalCoins;
        }

        private void RefreshOrderList()
        {
            OrderManager runtimeOrderManager = FindFirstObjectByType<OrderManager>();
            if (runtimeOrderManager != null)
            {
                orderManager = runtimeOrderManager;
            }

            for (int i = 0; i < orderViews.Count; i++)
            {
                Destroy(orderViews[i].gameObject);
            }

            orderViews.Clear();

            IReadOnlyList<OrderData> orders = orderManager.ActiveOrders;
            for (int i = 0; i < orders.Count; i++)
            {
                OrderButtonView view = Instantiate(orderButtonPrefab, ordersContainer);
                view.gameObject.SetActive(true);
                view.Setup(orders[i], orderManager.CompleteOrder);
                orderViews.Add(view);
            }
        }

        private void ShowScorePopup(OrderData order)
        {
            ScorePopupView popup = Instantiate(scorePopupPrefab, popupsContainer);
            popup.gameObject.SetActive(true);
            popup.transform.localPosition = new Vector3(Random.Range(-90f, 90f), Random.Range(-25f, 35f), 0f);
            popup.Setup("+" + order.rewardPoints + " points");
        }
    }
}