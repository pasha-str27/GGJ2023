using System.Threading.Tasks;
using Google;
using System;
using System.Collections.Generic;

public class GoogleConnect
{
    private static GoogleConnect _instance;
        
    private GoogleSignInConfiguration _configuration;

    private Action<string> _onConnect;

    private string _webClientId = "550050470980-2lbnerli2d3gnvjjg0qvu1s3lp6ru7u5.apps.googleusercontent.com";

    public static GoogleConnect Instance
    {
        get
        {
            if(_instance == null)
                _instance = new GoogleConnect();

            return _instance;
        }
    }

    public GoogleConnect()
    {
        _configuration = new GoogleSignInConfiguration { WebClientId = _webClientId, RequestEmail = true, RequestIdToken = true };
    }

    public void Connect(Action<string> onConnect)
    {
        _onConnect = onConnect;

        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnConnectFinised);
    }

    internal void OnConnectFinised(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                }
            }
        }

        else if (task.IsCanceled)
        {
            
        }

        else
        {
            var email = task.Result.Email;

            _onConnect.Invoke(email);

            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
}