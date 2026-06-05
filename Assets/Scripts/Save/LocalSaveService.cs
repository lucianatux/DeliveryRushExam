using System.Threading.Tasks;
using DeliveryRushExam.Data;
using UnityEngine;

namespace DeliveryRushExam.Save
{
    public class LocalSaveService : ISaveService
    {
        private const string ProgressKey = "delivery_rush_progress";

        public Task<PlayerProgressData> LoadAsync()
        {
            if (!PlayerPrefs.HasKey(ProgressKey))
            {
                return Task.FromResult(new PlayerProgressData());
            }

            string json = PlayerPrefs.GetString(ProgressKey);
            PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json);
            return Task.FromResult(data ?? new PlayerProgressData());
        }

        public Task SaveAsync(PlayerProgressData progressData)
        {
            progressData.TouchSaveDate();
            string json = JsonUtility.ToJson(progressData);

            PlayerPrefs.SetString(ProgressKey, json);
            PlayerPrefs.Save();

            return Task.CompletedTask;
        }
    }
}
