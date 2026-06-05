using System.Threading.Tasks;
using DeliveryRushExam.Data;

namespace DeliveryRushExam.Save
{
    public interface ISaveService
    {
        Task<PlayerProgressData> LoadAsync();
        Task SaveAsync(PlayerProgressData progressData);
    }
}