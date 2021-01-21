using CommonLib.Util;
using Facebook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.Logic.OAuth
{
    internal static class FacebookLogin
    {
        public static bool CheckLoginToken(string token, Action<bool> validationCallback)
        {
            Debug.Assert(!string.IsNullOrEmpty(token) && validationCallback != null);

            if (string.IsNullOrEmpty(token) || validationCallback == null)
                return false;

            var fb = new FacebookClient(token);
            fb.GetCompleted += (o, e) =>
            {
                if (e.Cancelled)
                    return;

                if (e.Error == null)
                {
                    try
                    {
                        //Success!!
                        var dictionary = (IDictionary<string, object>)e.GetResultData();
                        if (dictionary.TryGetValue("expires_in", out var expiresStr) && int.TryParse(expiresStr.ToString(), out var expires))
                        {
                            validationCallback(expires > 0);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        CLog.F("Failed to proccess Facebook access token validation response. Catching exception...");
                        CLog.Catch(ex);
                    }
                }
                else
                {
                    CLog.F("Failed to validate Facebook access token! Error: {0}.", e.Error);
                }

                //If we reach here, this means the token is invalid.
                validationCallback(false);
            };

            fb.GetTaskAsync("oauth/access_token_info");

            return true;
        }
    }
}
