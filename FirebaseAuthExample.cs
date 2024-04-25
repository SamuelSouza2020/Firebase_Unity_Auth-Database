using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseAuthExample : MonoBehaviour
{
    /// <summary>
    /// Create a gameobject and add this script to it, just bind the login button on it.
    /// </summary>
    [SerializeField]
    Button ButtonLoginGoogle, ButtonLoginGuest;

    [SerializeField]
    TextMeshProUGUI StatusFirebase;

    private string GoogleWebAPI = "12345678-abcd1efgh2ijlm3.apps.googleusercontent.com";//your web client ID
    private string uidUserLogin;

    private GoogleSignInConfiguration configuration;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference reference;

    private void Start()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = GoogleWebAPI,
            RequestIdToken = true
        };
        StartCoroutine(InitFirebase());

        ShowLoginButton();
    }
    void ShowLoginButton()
    {
        ButtonLoginGoogle.onClick.AddListener(GoogleSignInClick);
        ButtonLoginGuest.onClick.AddListener(SignInAnonymously);
    }
    
    public void GoogleSignInClick() //Method via Google
    {
        ButtonLoginGoogle.interactable = false;
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);
    }

    void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.Log("Fault");
        }
        else if (task.IsCanceled)
        {
            Debug.LogError("Login Cancel");
        }
        else
        {
            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                user = auth.CurrentUser;

                uidUserLogin = user.UserId;

                StatusFirebase.text = "Auth Google Completed";

                //Here you can call methods to continue the operation after successful login
                UpdateLastLoginDate(uidUserLogin);
            });
        }
    }
    public void SignInAnonymously() //Method via Anonymous
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            //Successful anonymous authentication
            user = auth.CurrentUser;
            uidUserLogin = user.UserId; //Anonymous user Uid

            StatusFirebase.text = "Auth Anonymous Completed";

            //Here you can call methods to continue the operation after successful login
            UpdateLastLoginDate(uidUserLogin);
        });
    }
    private void UpdateLastLoginDate(string userId)
    {
        string lastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        reference.Child("users").Child(userId).Child("lastLogin").SetValueAsync(lastLogin).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.Log("\nError updating last login date");
            }
            else if (task.IsCompleted)
            {
                StatusFirebase.text += "\ndata save";
            }
        });
    }
    IEnumerator InitFirebase()
    {
        var checkAndFixTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkAndFixTask.IsCompleted);
        var dependencyStatus = checkAndFixTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            //Get the DB instance with the URL provided
            reference = FirebaseDatabase.GetInstance("https://linktoyourdatabase.firebaseio.com/").RootReference;

            auth = FirebaseAuth.DefaultInstance;
            ButtonLoginGoogle.interactable = true;
        }
        else
        {
            Debug.Log($"\nCould not resolve all Firebase dependencies: {dependencyStatus}");
            yield break; //Stop the coroutine here
        }
    }
}
