using System.Threading.Tasks;
using Google;
using System;

public static class GoogleConnect
{
    private static GoogleSignInConfiguration _configuration;

    private static Action<GoogleSignInUser> _onSuccesfulConnect;

    private static string _webClientId = "550050470980-2lbnerli2d3gnvjjg0qvu1s3lp6ru7u5.apps.googleusercontent.com";

    public static void StartConnect(Action<GoogleSignInUser> onSuccesfulConnect)
    {
        _configuration = new GoogleSignInConfiguration { WebClientId = _webClientId, RequestEmail = true, RequestIdToken = true };
        _onSuccesfulConnect = onSuccesfulConnect;

        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnConnectFinised);
    }

    internal static void OnConnectFinised(Task<GoogleSignInUser> task)
    {
        if (!task.IsFaulted && !task.IsCanceled)
        {
            _onSuccesfulConnect?.Invoke(task.Result);

            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
}