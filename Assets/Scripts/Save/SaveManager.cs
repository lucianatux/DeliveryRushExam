using System;
using System.Threading.Tasks;
using DeliveryRushExam.Data;
using UnityEngine;

namespace DeliveryRushExam.Save
{
    public class SaveManager : MonoBehaviour
    {
        public PlayerProgressData CurrentProgress { get; private set; } = new PlayerProgressData();

        public event Action<PlayerProgressData> ProgressLoaded;

        private ISaveService saveService;

        private async void Awake()
        {
            saveService = ServiceLocator.Get<ISaveService>();
            await LoadProgressAsync();
        }

        public async Task LoadProgressAsync()
        {
            CurrentProgress = await saveService.LoadAsync();
            ProgressLoaded?.Invoke(CurrentProgress);
        }

        public async Task SaveMatchResultAsync(int score, int coins, int completedOrders)
        {
            CurrentProgress.bestScore = Mathf.Max(CurrentProgress.bestScore, score);
            CurrentProgress.totalCoins += coins;
            CurrentProgress.completedOrders += completedOrders;

            // Nivel simple para tener un dato extra persistido.
            CurrentProgress.unlockedLevel = Mathf.Max(CurrentProgress.unlockedLevel, 1 + CurrentProgress.completedOrders / 10);

            await saveService.SaveAsync(CurrentProgress);
        }
    }
}