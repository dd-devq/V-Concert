using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Text.RegularExpressions;
using EventData;

public class PlayFabAuthenticationController : PersistentManager<PlayFabAuthenticationController>
{
    private const string PlayFabRememberMeId = "PlayfabRememberMeId";
    private const string PlayFabRememberMe = "PlayfabRememberMe";
    public bool isLoggedIn;

    public override void Awake()
    {
        base.Awake();
        isLoggedIn = false;
    }

    #region Login

    public void Login(Component sender, object data)
    {
        var tmp = (LoginInfo)data;
        if (ValidateEmail(tmp.Username))
        {
            LoginWithEmail(tmp);
        }
        else
        {
            LoginWithUsername(tmp);
        }
    }

    public void AutoLogin(Component sender, object data)
    {
        var tmp = (AutoLoginInfo)data;
        var rememberMeId = PlayerPrefs.GetString(PlayFabRememberMeId);
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = rememberMeId,
            CreateAccount = true
        }, result =>
        {
            isLoggedIn = true;
            PlayFabPlayerDataController.Instance.PlayerId = result.PlayFabId;
            PlayFabGameDataController.Instance.GetAllData();
            PlayFabPlayerDataController.Instance.GetAllData();
            StartCoroutine(IsDataInit(tmp.AutoLoginSuccessCallback));
        }, error =>
        {
            PlayFabErrorHandler.Instance.HandleError(error);
            Logout(null, null);
        });
    }


    private void LoginWithEmail(LoginInfo data)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = data.Username,
            Password = data.Password,
            TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, result =>
            {
                isLoggedIn = true;
                PlayFabPlayerDataController.Instance.PlayerId = result.PlayFabId;
                PlayFabGameDataController.Instance.GetAllData();
                PlayFabPlayerDataController.Instance.GetAllData();
                StartCoroutine(IsDataInit(data.LoginSuccessCallback));
                RememberMe();
            },
            PlayFabErrorHandler.Instance.HandleError);
    }

    private void LoginWithUsername(LoginInfo data)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = data.Username,
            Password = data.Password,
            TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.LoginWithPlayFab(request, result =>
            {
                isLoggedIn = true;
                PlayFabPlayerDataController.Instance.PlayerId = result.PlayFabId;
                PlayFabGameDataController.Instance.GetAllData();
                PlayFabPlayerDataController.Instance.GetAllData();
                StartCoroutine(IsDataInit(data.LoginSuccessCallback));
                RememberMe();
            },
            PlayFabErrorHandler.Instance.HandleError);
    }

    #endregion

    #region Register

    public void Register(Component sender, object data)
    {
        var tmp = (RegisterInfo)data;
        var request = new RegisterPlayFabUserRequest
        {
            Email = tmp.Email,
            Username = tmp.Username,
            DisplayName = tmp.Username,
            Password = tmp.Password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, _ =>
            {
                tmp.RegisterSuccessCallback();
                var playerData = new Dictionary<string, string>
                {
                    { "Equip Item", "None" },
                    { "Character Path", "Models/SMPL/SMPL" }
                };
                PlayFabPlayerDataController.Instance.SetPlayerData(this, playerData);
            },
            error =>
            {
                tmp.RegisterFailCallback();
                PlayFabErrorHandler.Instance.HandleError(error);
            });
    }

    #endregion

    #region Logout

    public void Logout(Component sender, object data)
    {
        PlayFabClientAPI.ForgetAllCredentials();
        ClearRememberMe();
        PlayFabPlayerDataController.Instance.PlayerId = "";
    }

    #endregion

    #region ResetPassword

    public void ResetPassword(Component sender, object data)
    {
    }

    #endregion

    #region Utils

    private static bool ValidateEmail(string em)
    {
        const string emailPattern =
            @"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

        return Regex.IsMatch(em, emailPattern);
    }

    private static void ClearRememberMe()
    {
        PlayerPrefs.DeleteKey(PlayFabRememberMeId);
        PlayerPrefs.DeleteKey(PlayFabRememberMe);
    }

    private static void RememberMe()
    {
        var rememberMeId = Guid.NewGuid().ToString();
        PlayerPrefs.SetString(PlayFabRememberMeId, rememberMeId);
        PlayerPrefs.SetInt(PlayFabRememberMe, 1);

        PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
        {
            CustomId = rememberMeId,
            ForceLink = false
        }, null, null);
    }

    #endregion

    private static IEnumerator IsDataInit(Action callback)
    {
        while (!PlayFabFlags.Instance.IsInit())
        {
            yield return new WaitForSeconds(0.2f);
        }

        callback();
    }
}