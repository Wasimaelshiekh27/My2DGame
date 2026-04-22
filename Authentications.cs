using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;

public class Authentications : MonoBehaviour
{
    private FirebaseAuth auth;
    private bool firebaseReady = false;

    [Header("Login")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    [Header("Signup")]
    public TMP_InputField signupEmail;
    public TMP_InputField signupPassword;

    [Header("Optional Message Text")]
    public TMP_Text messageText;

    [Header("Scene After Login")]
    public string nextSceneName = "Level 1";

    private void Start()
    {
        Debug.Log("Checking Firebase dependencies...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus status = task.Result;

            if (status == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                firebaseReady = true;
                Debug.Log("Firebase is ready.");
                ShowMessage("Firebase ready.");
            }
            else
            {
                firebaseReady = false;
                Debug.LogError("Firebase dependency error: " + status);
                ShowMessage("Firebase not ready: " + status);
            }
        });
    }

    public void SignUp()
    {
        if (!firebaseReady)
        {
            Debug.LogError("Signup failed: Firebase is not ready.");
            ShowMessage("Firebase not ready.");
            return;
        }

        string email = signupEmail.text.Trim();
        string password = signupPassword.text.Trim();

        Debug.Log("Signup button pressed.");
        Debug.Log("Signup Email: " + email);
        Debug.Log("Signup Password Length: " + password.Length);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Please enter email and password.");
            Debug.LogWarning("Signup failed: empty fields.");
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage("Password must be at least 6 characters.");
            Debug.LogWarning("Signup failed: password too short.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Signup canceled.");
                ShowMessage("Signup canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Signup failed.");
                PrintFirebaseErrors(task.Exception, "Signup");
                return;
            }

            AuthResult result = task.Result;
            Debug.Log("Signup success: " + result.User.Email);
            ShowMessage("Signup successful.");
        });
    }

    public void Login()
    {
        if (!firebaseReady)
        {
            Debug.LogError("Login failed: Firebase is not ready.");
            ShowMessage("Firebase not ready.");
            return;
        }

        string email = loginEmail.text.Trim();
        string password = loginPassword.text.Trim();

        Debug.Log("Login button pressed.");
        Debug.Log("Login Email: " + email);
        Debug.Log("Login Password Length: " + password.Length);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Please enter email and password.");
            Debug.LogWarning("Login failed: empty fields.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Login canceled.");
                ShowMessage("Login canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Login failed.");
                PrintFirebaseErrors(task.Exception, "Login");
                return;
            }

            AuthResult result = task.Result;
            Debug.Log("Login success: " + result.User.Email);
            ShowMessage("Login successful.");

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        });
    }

    public void Logout()
    {
        if (!firebaseReady || auth == null)
        {
            Debug.LogError("Logout failed: Firebase not ready.");
            ShowMessage("Firebase not ready.");
            return;
        }

        auth.SignOut();
        Debug.Log("Logged out.");
        ShowMessage("Logged out.");
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        Debug.Log("UI Message: " + message);
    }

    private void PrintFirebaseErrors(System.AggregateException exception, string actionName)
    {
        if (exception == null)
        {
            ShowMessage(actionName + " failed.");
            return;
        }

        foreach (var e in exception.Flatten().InnerExceptions)
        {
            Debug.LogError(actionName + " full error: " + e);

            if (e is FirebaseException firebaseEx)
            {
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                Debug.LogError(actionName + " Firebase error code: " + errorCode);

                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        ShowMessage("Email is missing.");
                        break;

                    case AuthError.MissingPassword:
                        ShowMessage("Password is missing.");
                        break;

                    case AuthError.InvalidEmail:
                        ShowMessage("Invalid email format.");
                        break;

                    case AuthError.WrongPassword:
                        ShowMessage("Wrong password.");
                        break;

                    case AuthError.EmailAlreadyInUse:
                        ShowMessage("Email already in use.");
                        break;

                    case AuthError.WeakPassword:
                        ShowMessage("Weak password. Use at least 6 characters.");
                        break;

                    case AuthError.UserNotFound:
                        ShowMessage("User not found.");
                        break;

                    case AuthError.NetworkRequestFailed:
                        ShowMessage("Network error. Check internet.");
                        break;

                    default:
                        ShowMessage("Firebase Error: " + errorCode);
                        break;
                }

                return;
            }
        }

        ShowMessage(actionName + " failed.");
    }
}