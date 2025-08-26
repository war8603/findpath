using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FindPath;
using GameUtilities;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Managers
{
    public class IAPManager : IBaseManager
    {
        private StoreController _store;                       // v5의 중심 객체
        private const string RemoveAdsId = "remove_ads";     // 콘솔의 상품 ID와 동일해야 함

        public ReactiveProperty<bool> HasNoAds { get; private set; } = new(false);
        public void InitManager()
        {
            LoadCache();
            InitIAP().Forget();
        }

        private void LoadCache()
        {
            HasNoAds.Value = PlayerPrefsTool.GetPlayerPrefs(PlayerPrefsKeyNames.RemoveAdsId, false);
        }
        
        private async UniTask InitIAP()
        {
            _store = UnityIAPServices.StoreController();

            // 제품 정보 갱신
            _store.OnProductsFetched += OnProductsFetched;
            _store.OnProductsFetchFailed += err => Debug.LogError($"FetchProducts failed: {err.FailureReason}");
            
            // 상품 구매 정보 갱신
            _store.OnPurchasesFetched += OnPurchasesFetched;
            _store.OnPurchasesFetchFailed += err => Debug.LogError($"FetchPurchases failed: {err.FailureReason}");
            
            // 구매
            _store.OnPurchaseConfirmed += order => ApplyNoAds(true); // 구매 성공
            _store.OnPurchaseFailed += failed => Debug.LogWarning($"Purchase failed: {failed.Details}");
            _store.OnPurchasePending += pending =>
            {
                // 필요 시 보류 상태 UI 처리
            };
            
            // 1) 스토어 연결
            await _store.Connect();

            // 2) 상품 조회 (코드로 직접 정의하는 방식)
            var products = new List<ProductDefinition> {
                new ProductDefinition(RemoveAdsId, ProductType.NonConsumable)
            };
            _store.FetchProducts(products);
        }
        
        private void OnProductsFetched(List<Product> products)
        {
            // 3) 구매내역 조회 → 이 때 과거 구매(자격)을 확인
            _store.FetchPurchases();
        }

        private void OnPurchasesFetched(Orders orders)
        {
            // orders에 완료/보류된 모든 주문 정보가 들어옴 → 자격 확인
            var owned = false;
            
            foreach (var confirmed in orders.ConfirmedOrders)
            {
                var info = confirmed.Info;
                if (info.PurchasedProductInfo.Any(purchased => purchased.productId == RemoveAdsId))
                {
                    owned = true;
                }
                if (owned) break;
            }

            ApplyNoAds(owned);
        }

        public void BuyRemoveAds()
        {
            // 4) 구매 시작
            _store.PurchaseProduct(RemoveAdsId);
        }
        
        public void RestorePurchases()
        {
            _store.RestoreTransactions((success, message) =>
            {
                Debug.Log($"Restore finished. success={success}, msg={message}");
                // 성공/실패 여부는 여기서 알 수 있고,
                // 실제 소유 권한은 위의 OnPurchasesFetched 이벤트에서 판정.
            });
        }

        private void ApplyNoAds(bool enable)
        {
            HasNoAds.Value = enable;
            
            PlayerPrefsTool.SetPlayerPrefs(PlayerPrefsKeyNames.RemoveAdsId, HasNoAds.Value);
            PlayerPrefs.Save();
        }
    }
}