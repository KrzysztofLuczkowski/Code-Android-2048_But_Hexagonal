using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using GoogleMobileAds.Sample;

namespace GoogleMobileAds.Samples
{
    /// <summary>
    /// Demonstrates how to use the Google Mobile Ads Unity plugin.
    /// </summary>
    [AddComponentMenu("GoogleMobileAds/Samples/GoogleMobileAdsController")]
    public class GoogleMobileAdsController : MonoBehaviour
    {
        /*
        // Always use test ads.
        // https://developers.google.com/admob/unity/test-ads
        internal static List<string> TestDeviceIds = new List<string>()
        {
            AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
            "702815ACFC14FF222DA1DC767672A573"
#endif
        };
        */

        // The Google Mobile Ads Unity plugin needs to be run only once.
        private static bool? _isInitialized;

        // Helper class that implements consent using the
        // Google User Messaging Platform (UMP) Unity plugin.
        [SerializeField, Tooltip("Controller for the Google User Messaging Platform (UMP) Unity plugin.")]
        private GoogleMobileAdsConsentController _consentController;

        /// <summary>
        /// Demonstrates how to configure Google Mobile Ads Unity plugin.
        /// </summary>
        private void Start()
        {
            // Ustawienia specyficzne dla iOS.
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            MobileAds.SetRequestConfiguration(new RequestConfiguration
            {
                //TestDeviceIds = TestDeviceIds
            });

            // Wywo�aj formularz zg�d na starcie.
            InitializeGoogleMobileAdsConsent();

            // Mo�esz te� ewentualnie sprawdzi�, czy zgoda jest ju� udzielona
            // i wtedy zainicjowa� reklamy i wy�wietli� baner:
            if (_consentController.CanRequestAds)
            {
                // Znajdujemy kontroler banera (upewnij si�, �e obiekt jest w scenie)
                BannerViewController bannerController = FindObjectOfType<BannerViewController>();
                if (bannerController != null)
                {
                    bannerController.LoadAd();
                }

                // Za�aduj reklam� pe�noekranow� (Interstitial)
                InterstitialAdController interstitialController = FindObjectOfType<InterstitialAdController>();
                if (interstitialController != null)
                {
                    interstitialController.LoadAd(); // �adujemy reklam�
                }
            }
        }



        /// <summary>
        /// Ensures that privacy and consent information is up to date.
        /// </summary>
        private void InitializeGoogleMobileAdsConsent()
        {
            Debug.Log("Google Mobile Ads gathering consent.");

            _consentController.GatherConsent((string error) =>
            {
                if (error != null)
                {
                    Debug.LogError("Failed to gather consent with error: " +
                        error);
                }
                else
                {
                    Debug.Log("Google Mobile Ads consent updated: "
                        + ConsentInformation.ConsentStatus);
                }

                if (_consentController.CanRequestAds)
                {
                    InitializeGoogleMobileAds();
                }
            });
        }

        /// <summary>
        /// Initializes the Google Mobile Ads Unity plugin.
        /// </summary>
        private void InitializeGoogleMobileAds()
        {
            // The Google Mobile Ads Unity plugin needs to be run only once and before loading any ads.
            if (_isInitialized.HasValue)
            {
                return;
            }

            _isInitialized = false;

            // Initialize the Google Mobile Ads Unity plugin.
            Debug.Log("Google Mobile Ads Initializing.");
            MobileAds.Initialize((InitializationStatus initstatus) =>
            {
                if (initstatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");
                    _isInitialized = null;
                    return;
                }

                // If you use mediation, you can check the status of each adapter.
                var adapterStatusMap = initstatus.getAdapterStatusMap();
                if (adapterStatusMap != null)
                {
                    foreach (var item in adapterStatusMap)
                    {
                        Debug.Log(string.Format("Adapter {0} is {1}",
                            item.Key,
                            item.Value.InitializationState));
                    }
                }

                Debug.Log("Google Mobile Ads initialization complete.");
                _isInitialized = true;
            });
        }

        /// <summary>
        /// Opens the AdInspector.
        /// </summary>
        public void OpenAdInspector()
        {
            Debug.Log("Opening ad Inspector.");
            MobileAds.OpenAdInspector((AdInspectorError error) =>
            {
                // If the operation failed, an error is returned.
                if (error != null)
                {
                    Debug.Log("Ad Inspector failed to open with error: " + error);
                    return;
                }

                Debug.Log("Ad Inspector opened successfully.");
            });
        }

        /// <summary>
        /// Opens the privacy options form for the user.
        /// </summary>
        /// <remarks>
        /// Your app needs to allow the user to change their consent status at any time.
        /// </remarks>
        private ConsentForm _consentForm;

        public void OpenPrivacyOptions()
        {
            if (_consentForm != null)
            {
                Debug.Log("Wy�wietlanie formularza zg�d...");

                _consentForm.Show((formError) =>
                {
                    if (formError != null)
                    {
                        Debug.LogError("B��d przy wy�wietlaniu formularza zg�d: " + formError.Message);
                    }
                    else
                    {
                        Debug.Log("Formularz zg�d zamkni�ty.");
                        _consentForm = null; // Resetujemy, poniewa� formularz po zamkni�ciu przestaje by� wa�ny
                    }
                });
            }
            else
            {
                Debug.Log("Formularz zg�d nie jest dost�pny. Pobieram nowy...");

                // Pobieramy formularz na nowo
                ConsentForm.Load((form, loadError) =>
                {
                    if (loadError != null)
                    {
                        Debug.LogError("B��d podczas �adowania formularza zg�d: " + loadError.Message);
                        return;
                    }

                    Debug.Log("Formularz zg�d za�adowany poprawnie.");
                    _consentForm = form;

                    // Teraz wy�wietlamy formularz
                    _consentForm.Show((showError) =>
                    {
                        if (showError != null)
                        {
                            Debug.LogError("B��d przy wy�wietlaniu formularza zg�d: " + showError.Message);
                        }
                        else
                        {
                            Debug.Log("Formularz zg�d zamkni�ty.");
                            _consentForm = null; // Resetujemy po zamkni�ciu
                        }
                    });
                });
            }
        }

    }
}