using System;
using System.Collections.Generic;
using DeliveryRushExam.Core;
using DeliveryRushExam.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
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
        [SerializeField] private int orderViewPoolDefaultCapacity = 6;
        [SerializeField] private int orderViewPoolMaxSize = 12;

        [Header("Popups")]
        [SerializeField] private RectTransform popupsContainer;
        [SerializeField] private ScorePopupView scorePopupPrefab;
        [SerializeField] private int popupPoolDefaultCapacity = 5;
        [SerializeField] private int popupPoolMaxSize = 20;

        [Header("Panels")]
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text resultsText;

        private readonly List<OrderButtonView> orderViews = new List<OrderButtonView>();
        private Canvas _canvas;
        private ObjectPool<ScorePopupView> _popupPool;
        private ObjectPool<OrderButtonView> _orderViewPool;
        private int _lastTimerSeconds = -1;

        // Delegates cacheados: evita generar un Action nuevo en cada Setup.
        private Action<string> _completeOrderCallback;
        private Action<ScorePopupView> _returnPopupCallback;

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

            // Delegates cacheados una sola vez (se reutilizan en cada Setup de pedido/popup).
            _completeOrderCallback = orderManager.CompleteOrder;
            _returnPopupCallback = ReturnPopupToPool;

            _popupPool = new ObjectPool<ScorePopupView>(
                createFunc: CreatePopup,
                actionOnGet: OnPopupGet,
                actionOnRelease: OnPopupRelease,
                actionOnDestroy: OnPopupDestroy,
                collectionCheck: false,
                defaultCapacity: popupPoolDefaultCapacity,
                maxSize: popupPoolMaxSize);

            _orderViewPool = new ObjectPool<OrderButtonView>(
                createFunc: CreateOrderView,
                actionOnGet: OnOrderViewGet,
                actionOnRelease: OnOrderViewRelease,
                actionOnDestroy: OnOrderViewDestroy,
                collectionCheck: false,
                defaultCapacity: orderViewPoolDefaultCapacity,
                maxSize: orderViewPoolMaxSize);
        }

        private void OnEnable()
        {
            orderManager.OrdersChanged += RefreshOrderList;
            orderManager.OrdersChanged += UpdateOrdersCount;
            scoreManager.OrderScored += ShowScorePopup;
            scoreManager.ScoreChanged += UpdateScoreHud;
        }

        private void OnDisable()
        {
            if (orderManager != null)
            {
                orderManager.OrdersChanged -= RefreshOrderList;
                orderManager.OrdersChanged -= UpdateOrdersCount;
            }

            if (scoreManager != null)
            {
                scoreManager.OrderScored -= ShowScorePopup;
                scoreManager.ScoreChanged -= UpdateScoreHud;
            }
        }

        private void Update()
        {
            if (gameManager == null)
            {
                return;
            }

            int seconds = Mathf.CeilToInt(gameManager.RemainingTime);
            if (seconds != _lastTimerSeconds)
            {
                timerText.text = "Time: " + seconds;
                _lastTimerSeconds = seconds;
            }

            for (int i = 0; i < orderViews.Count; i++)
            {
                orderViews[i].Refresh();
            }
        }

        public void ShowGameplay()
        {
            gameplayPanel.SetActive(true);
            resultsPanel.SetActive(false);
            _lastTimerSeconds = -1; // fuerza primer refresh del timer al iniciar partida
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
            // En lugar de destruir/instanciar todas las views, las devolvemos a la pool
            // y pedimos las que haga falta. Mismo patrón que el pool de popups.
            for (int i = 0; i < orderViews.Count; i++)
            {
                _orderViewPool.Release(orderViews[i]);
            }
            orderViews.Clear();

            IReadOnlyList<OrderData> orders = orderManager.ActiveOrders;
            for (int i = 0; i < orders.Count; i++)
            {
                OrderButtonView view = _orderViewPool.Get();
                view.Setup(orders[i], _completeOrderCallback);
                orderViews.Add(view);
            }
        }

        private void UpdateScoreHud(int score, int coins, int completedOrders)
        {
            scoreText.text = "Score: " + score;
            coinsText.text = "Coins: " + coins;
        }

        private void UpdateOrdersCount()
        {
            ordersCountText.text = "Orders: " + orderManager.ActiveOrders.Count;
        }

        private void ShowScorePopup(OrderData order)
        {
            ScorePopupView popup = _popupPool.Get();
            popup.transform.localPosition = new Vector3(UnityEngine.Random.Range(-90f, 90f), UnityEngine.Random.Range(-25f, 35f), 0f);
            popup.Setup("+" + order.rewardPoints + " points", _returnPopupCallback);
        }

        // ---------- Popup pool callbacks ----------

        private ScorePopupView CreatePopup()
        {
            ScorePopupView popup = Instantiate(scorePopupPrefab, popupsContainer);
            popup.gameObject.SetActive(false);
            return popup;
        }

        private void OnPopupGet(ScorePopupView popup)
        {
            popup.gameObject.SetActive(true);
        }

        private void OnPopupRelease(ScorePopupView popup)
        {
            popup.gameObject.SetActive(false);
        }

        private void OnPopupDestroy(ScorePopupView popup)
        {
            if (popup != null)
            {
                Destroy(popup.gameObject);
            }
        }

        private void ReturnPopupToPool(ScorePopupView popup)
        {
            _popupPool.Release(popup);
        }

        // ---------- OrderButtonView pool callbacks ----------

        private OrderButtonView CreateOrderView()
        {
            OrderButtonView view = Instantiate(orderButtonPrefab, ordersContainer);
            view.gameObject.SetActive(false);
            return view;
        }

        private void OnOrderViewGet(OrderButtonView view)
        {
            view.gameObject.SetActive(true);
        }

        private void OnOrderViewRelease(OrderButtonView view)
        {
            view.gameObject.SetActive(false);
        }

        private void OnOrderViewDestroy(OrderButtonView view)
        {
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }
    }
}