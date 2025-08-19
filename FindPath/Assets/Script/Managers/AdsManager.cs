using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class AdsManager : IBaseManager
    {
        // 배너 광고
        private string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private BannerView _bannerView;

        // 보상형 광고
        private string _rewardedAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        private RewardedAd _rewardedAd;
        
        // 전면 광고
        private string _interstitialAdUnitId;
        private InterstitialAd _interstitialAd;

        private UniTaskCompletionSource _taskCompletionSource;

        public void InitManager()
        {
            Init().Forget();
        }

        private async UniTask Init()
        {
            _taskCompletionSource = new UniTaskCompletionSource();
           
            // Google Mobile Ads 초기화
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                Debug.Log($"MobileAds 초기화 완료");
                
                // 배너 광고 로드
                //LoadBannerAd();

                // 보상형 광고 로드
                LoadRewardedAd();
                
                // 전면 광고 로드
                LoadInterstitialAd();
                
                _taskCompletionSource.TrySetResult();
            });
            
            await _taskCompletionSource.Task;
        }

#region BannerView
        private void LoadBannerAd()
        {
            if (_bannerView == null)
            {
                // 배너 뷰 생성
                CreateBannerView();
            }

            if (_bannerView == null) return;
            
            var adRequest = new AdRequest();
            _bannerView.LoadAd(adRequest);
        }
        
        /// <summary>
        /// 배너 뷰 생성
        /// </summary>
        private void CreateBannerView() 
        {
            if (_bannerView != null)
            {
                // 기존 광고 제거
                DestroyBannerView();
            }

            var adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            _bannerView = new BannerView(_bannerAdUnitId, adaptiveSize, AdPosition.Bottom);
        }
        
        /// <summary>
        /// 배너 뷰 광고 제거
        /// </summary>
        private void DestroyBannerView()
        {
            if (_bannerView != null)
            {
                Debug.Log("배너 광고 제거.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }
#endregion

#region RewardedAd
        public void ShowRewardedAd(UnityAction onsuccessCallback, UnityAction onFailedCallback)
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    if (reward == null)
                    {
                        Debug.LogError("[AdsManager] ShowAd Reward is null");
                        onFailedCallback?.Invoke();
                    }
                    else
                    {
                        Debug.Log($"[AdsManager] ShowAd Reward = {reward.Type} {reward.Amount}");
                        onsuccessCallback?.Invoke();
                    }
                    
                    LoadRewardedAd();
                });
            }
            else
            {
                onFailedCallback?.Invoke();
                LoadRewardedAd();
            }
        }

        private void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(_rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedAd = ad;
                });
        }
#endregion

#region InterstitialAd
        public void ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
            }
        }

        private void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("Loading the interstitial ad.");

            var adRequest = new AdRequest();
            InterstitialAd.Load(_interstitialAdUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " + "with error : " + error);
                        return;
                    }
                    Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                    _interstitialAd = ad;
                    RegisterEventHandlers(ad);
                });
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
            };
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            interstitialAd.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                LoadInterstitialAd();
            };
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " + "with error : " + error);
                LoadInterstitialAd();
            };
        }
#endregion
    }
}
