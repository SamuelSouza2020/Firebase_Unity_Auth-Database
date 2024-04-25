using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
                return;
            }

            // Initialize Firebase
            FirebaseApp app = FirebaseApp.DefaultInstance;
            //// Initialize App Check and use Play Integrity provider
            //PlayIntegrityAppCheckProviderFactory playIntegrityFactory = new PlayIntegrityAppCheckProviderFactory();
            //FirebaseAppCheck.Instance.InstallAppCheckProviderFactory(playIntegrityFactory);

            //Debug.Log("App Check with Play Integrity initialized.");
        });
    }
}
