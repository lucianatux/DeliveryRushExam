using UnityEngine;

namespace DeliveryRushExam.Save
{
    public class SaveServicesInstaller : MonoBehaviour
    {
        public enum SaveMode
        {
            Local,
            Cloud
        }

        [SerializeField] private SaveMode saveMode = SaveMode.Local;

        private void Awake()
        {
            if (saveMode == SaveMode.Local)
            {
                ServiceLocator.Register<ISaveService>(new LocalSaveService());
                return;
            }

            ServiceLocator.Register<ISaveService>(new UgsCloudSaveService());
        }
    }
}
