using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliveryRushExam.Data;
using UnityEngine;

#if DELIVERY_RUSH_UGS
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
#endif

namespace DeliveryRushExam.Save
{
    public class UgsCloudSaveService : ISaveService
    {
        // Una key por campo: los datos quedan legibles en el Dashboard de UGS
        // y coinciden con los nombres pedidos por la consigna.
        private const string KeyPlayerName = "playerName";
        private const string KeyBestScore = "bestScore";
        private const string KeyTotalCoins = "totalCoins";
        private const string KeyCompletedOrders = "completedOrders";
        private const string KeyUnlockedLevel = "unlockedLevel";
        private const string KeyLastSaveDate = "lastSaveDate";

#if DELIVERY_RUSH_UGS
        // Inicialización perezosa y cacheada: la primera llamada arranca UGS si
        // hace falta; las siguientes simplemente esperan al mismo Task (no-op
        // cuando ya está completado).
        private Task _initTask;

        private Task EnsureInitializedAsync()
        {
            if (_initTask == null)
            {
                _initTask = InitializeUgsAsync();
            }

            return _initTask;
        }

        private async Task InitializeUgsAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        public async Task<PlayerProgressData> LoadAsync()
        {
            var data = new PlayerProgressData();

            try
            {
                await EnsureInitializedAsync();

                var keys = new HashSet<string>
                {
                    KeyPlayerName,
                    KeyBestScore,
                    KeyTotalCoins,
                    KeyCompletedOrders,
                    KeyUnlockedLevel,
                    KeyLastSaveDate
                };

                Dictionary<string, Item> loaded =
                    await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

                // Si una key no existe (primer guardado del jugador), se mantiene
                // el valor por defecto del modelo. Por eso no se requiere "fallback".
                if (loaded.TryGetValue(KeyPlayerName, out Item nameItem))
                {
                    data.playerName = nameItem.Value.GetAs<string>();
                }

                if (loaded.TryGetValue(KeyBestScore, out Item bestItem))
                {
                    data.bestScore = bestItem.Value.GetAs<int>();
                }

                if (loaded.TryGetValue(KeyTotalCoins, out Item coinsItem))
                {
                    data.totalCoins = coinsItem.Value.GetAs<int>();
                }

                if (loaded.TryGetValue(KeyCompletedOrders, out Item ordersItem))
                {
                    data.completedOrders = ordersItem.Value.GetAs<int>();
                }

                if (loaded.TryGetValue(KeyUnlockedLevel, out Item levelItem))
                {
                    data.unlockedLevel = levelItem.Value.GetAs<int>();
                }

                if (loaded.TryGetValue(KeyLastSaveDate, out Item dateItem))
                {
                    data.lastSaveDate = dateItem.Value.GetAs<string>();
                }

                Debug.Log("[UgsCloudSave] Progreso cargado. PlayerId: " +
                          AuthenticationService.Instance.PlayerId);
            }
            catch (Exception ex)
            {
                Debug.LogError("[UgsCloudSave] Error en LoadAsync: " + ex.Message);
                // Fallback silencioso: devolvemos un PlayerProgressData con defaults
                // para no romper el flujo de partida.
            }

            return data;
        }

        public async Task SaveAsync(PlayerProgressData progressData)
        {
            try
            {
                await EnsureInitializedAsync();

                progressData.TouchSaveDate();

                var payload = new Dictionary<string, object>
                {
                    { KeyPlayerName, progressData.playerName },
                    { KeyBestScore, progressData.bestScore },
                    { KeyTotalCoins, progressData.totalCoins },
                    { KeyCompletedOrders, progressData.completedOrders },
                    { KeyUnlockedLevel, progressData.unlockedLevel },
                    { KeyLastSaveDate, progressData.lastSaveDate }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(payload);

                Debug.Log("[UgsCloudSave] Progreso guardado. PlayerId: " +
                          AuthenticationService.Instance.PlayerId);
            }
            catch (Exception ex)
            {
                Debug.LogError("[UgsCloudSave] Error en SaveAsync: " + ex.Message);
                throw;
            }
        }
#else
        // Stub: si los paquetes UGS no están instalados o el símbolo no está
        // definido, el servicio compila pero deja claro que no está activo.
        public async Task<PlayerProgressData> LoadAsync()
        {
            Debug.LogWarning("[UgsCloudSave] UGS no está habilitado. " +
                             "Instalar paquetes y definir DELIVERY_RUSH_UGS.");
            await Task.Yield();
            return new PlayerProgressData();
        }

        public async Task SaveAsync(PlayerProgressData progressData)
        {
            Debug.LogWarning("[UgsCloudSave] UGS no está habilitado. " +
                             "Instalar paquetes y definir DELIVERY_RUSH_UGS.");
            await Task.Yield();
        }
#endif
    }
}