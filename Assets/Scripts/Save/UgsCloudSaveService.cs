using System.Threading.Tasks;
using DeliveryRushExam.Data;
using UnityEngine;

namespace DeliveryRushExam.Save
{
    public class UgsCloudSaveService : ISaveService
    {
        private const string ProgressKey = "delivery_rush_progress";

        public async Task<PlayerProgressData> LoadAsync()
        {
            Debug.LogWarning("UGS Cloud Save is not enabled. Add UGS packages and implement it.");
            await Task.Yield();
            return new PlayerProgressData();
        }

        public async Task SaveAsync(PlayerProgressData progressData)
        {
            Debug.LogWarning("UGS Cloud Save is not enabled. Add UGS packages and implement it.");
            await Task.Yield();
        }
    }
}
