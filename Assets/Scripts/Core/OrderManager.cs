using System;
using System.Collections.Generic;
using DeliveryRushExam.Data;
using UnityEngine;

namespace DeliveryRushExam.Core
{
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private float spawnInterval = 2.5f;
        [SerializeField] private int maxActiveOrders = 6;
        [SerializeField] private bool verboseLogs;

        [Header("Scene References")]
        [SerializeField] private ScoreManager scoreManager;

        private readonly List<OrderData> activeOrders = new List<OrderData>();
        private readonly string[] customerNames =
        {
            "Alex", "Taylor", "Sam", "Jordan", "Casey", "Morgan", "Riley", "Avery"
        };

        private float spawnTimer;
        private int nextOrderId;
        private bool isRunning;

        public IReadOnlyList<OrderData> ActiveOrders => activeOrders;
        public event Action OrdersChanged;

        private void Awake()
        {
            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
            }
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnOrder();
            }

            int expiredCount = 0;
            for (int i = activeOrders.Count - 1; i >= 0; i--)
            {
                activeOrders[i].remainingTime -= Time.deltaTime;

                if (activeOrders[i].remainingTime <= 0f)
                {
                    activeOrders.RemoveAt(i);
                    expiredCount++;
                }
            }

            if (expiredCount > 0)
            {
                OrdersChanged?.Invoke();
            }

            if (verboseLogs)
            {
                Debug.Log("Active orders: " + activeOrders.Count + " expired: " + expiredCount);
            }
        }

        public void StartOrders()
        {
            activeOrders.Clear();
            nextOrderId = 0;
            spawnTimer = spawnInterval;
            isRunning = true;
            OrdersChanged?.Invoke();
        }

        public void StopOrders()
        {
            isRunning = false;
            activeOrders.Clear();
            OrdersChanged?.Invoke();
        }

        public void CompleteOrder(string orderId)
        {
            int idx = -1;
            for (int i = 0; i < activeOrders.Count; i++)
            {
                if (activeOrders[i].id == orderId)
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0)
            {
                return;
            }

            OrderData order = activeOrders[idx];
            activeOrders.RemoveAt(idx);
            scoreManager.AddCompletedOrder(order);
            OrdersChanged?.Invoke();
        }

        private void TrySpawnOrder()
        {
            if (activeOrders.Count >= maxActiveOrders)
            {
                return;
            }

            string id = "ORDER_" + nextOrderId;
            string customer = customerNames[UnityEngine.Random.Range(0, customerNames.Length)];
            int points = UnityEngine.Random.Range(80, 151);
            int coins = UnityEngine.Random.Range(4, 12);
            float limit = UnityEngine.Random.Range(7f, 14f);

            activeOrders.Add(new OrderData(id, customer, points, coins, limit));
            nextOrderId++;

            OrdersChanged?.Invoke();
        }
    }
}