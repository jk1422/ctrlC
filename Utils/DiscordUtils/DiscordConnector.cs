using Colossal.IO.AssetDatabase;
using Colossal.Json;
using Colossal.Logging;
using ctrlC.Tools;
using Game.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Networking;
using static Colossal.PSI.Common.IModsUploadSupport;

namespace ctrlC.Utils.DiscordUtils
{
	public static class DiscordConnector
	{
		//public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(DiscordConnector)}").SetShowsErrorsInUI(false);
		//static string clientId = "1258903403686400060";
		//static string clientSecret = "PCU0r-ywLlCbTl_BMOJDaPGG9Kqps0eE"; //guild id: 1245495986583900272
		//static string redirectUri = "https://jk142.se/?openModal=true";
		//static string state = Guid.NewGuid().ToString("N");  // For CSRF protection
		//static string oauthUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=identify%20guilds";
		//public static void PostToUrl(string code, Action onSuccess, Action<string> onError)
		//{
		//	CoroutineRunner.Instance.RunCoroutine(ValidateCode(code, onSuccess, onError));
		//}
		//
		//public static void GetValidationCode()
		//{
		//	string state = Guid.NewGuid().ToString("N");  // För CSRF-skydd
		//
		//	
		//	Application.OpenURL(oauthUrl);
		//}
		//
		//private static IEnumerator ValidateCode(string code, Action onSuccess, Action<string> onError)
		//{
		//
		//
		//
		//
		//	
		//	string tokenUrl = "https://discord.com/api/v10/oauth2/token";
		//	WWWForm form = new WWWForm();
		//	form.AddField("client_id", clientId);
		//	form.AddField("client_secret", clientSecret);
		//	form.AddField("grant_type", "authorization_code");
		//	form.AddField("code", code);
		//	form.AddField("redirect_uri", redirectUri);
		//
		//	
		//
		//	using (UnityWebRequest w = UnityWebRequest.Post(tokenUrl, form))
		//	{
		//
		//		
		//
		//		
		//		w.downloadHandler = new DownloadHandlerBuffer();
		//		w.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		//
		//		// Skicka förfrågan och vänta på att den slutförs
		//		yield return w.SendWebRequest();
		//
		//		if (w.result != UnityWebRequest.Result.Success)
		//		{
		//			log.Error($"Error when requesting: {w.error}");
		//			onError?.Invoke(w.error);
		//		}
		//		else
		//		{
		//			var jsonResponse = w.downloadHandler.text;
		//			
		//			var accessToken = ExtractAccessToken(jsonResponse);
		//			var refreshToken = ExtractRefreshToken(jsonResponse);
		//			var expiresIn = ExtractExpireTime(jsonResponse);
		//
		//			TokenData tokenData = new TokenData
		//			{
		//				access_token = accessToken,
		//				token_type = "Bearer",
		//				expires_in = expiresIn,
		//				refresh_token = refreshToken,
		//				is_validated = true
		//			};
		//
		//
		//
		//
		//			
		//			
		//
		//			CoroutineRunner.Instance.RunCoroutine(GetUserID(accessToken, onSuccess, onError));
		//		}
		//	}
		//}
		//
		//private static string ExtractAccessToken(string jsonResponse)
		//{
		//	// Här bör du extrahera access token från JSON-svaret
		//	// Det kan se ut så här beroende på svarets format:
		//	//var tokenData = JsonUtility.FromJson<TokenResponse>(jsonResponse);
		//
		//	Variant x = Colossal.Json.JSON.Load(jsonResponse);
		//
		//	Colossal.Json.JSON.MakeInto<TokenResponse>(x, out TokenResponse data);
		//
		//	return data.access_token;
		//}
		//private static string ExtractRefreshToken(string jsonResponse)
		//{
		//	// Här bör du extrahera access token från JSON-svaret
		//	// Det kan se ut så här beroende på svarets format:
		//	Variant x = Colossal.Json.JSON.Load(jsonResponse);
		//
		//	Colossal.Json.JSON.MakeInto<TokenResponse>(x, out TokenResponse data);
		//	return data.refresh_token;
		//}
		//private static int ExtractExpireTime(string jsonResponse)
		//{
		//	// Här bör du extrahera access token från JSON-svaret
		//	// Det kan se ut så här beroende på svarets format:
		//	Variant x = Colossal.Json.JSON.Load(jsonResponse);
		//
		//	Colossal.Json.JSON.MakeInto<TokenResponse>(x, out TokenResponse data);
		//	return data.expires_in;
		//}
		//
		//private static IEnumerator GetUserID(string accessToken, Action onSuccess, Action<string> onError)
		//{
		//	string apiUrl = "https://discord.com/api/v10/users/@me";
		//	UnityWebRequest request = UnityWebRequest.Get(apiUrl);
		//	request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
		//
		//	yield return request.SendWebRequest();
		//
		//	if (request.result != UnityWebRequest.Result.Success)
		//	{
		//		log.Error($"Error: {request.error}");
		//		onError?.Invoke(request.error);
		//	}
		//	else
		//	{
		//		var jsonResponse = request.downloadHandler.text;
		//
		//		
		//
		//		// Deserialize the JSON response
		//		Variant variant = Colossal.Json.JSON.Load(jsonResponse);
		//		Colossal.Json.JSON.MakeInto<DiscordUser>(variant, out DiscordUser user);
		//
		//		// Extract the user ID
		//		string userId = user.id;
		//
		//		DataManager.SaveUserData(user);
		//		onSuccess?.Invoke();
		//		//Mod mod = g
		//		//
		//		//Action<DiscordMember> onSuccess = (discordMember) => { mod.OnRoleCheckSuccess(discordMember); };
		//		//
		//		//Action<string> onError = (error) =>
		//		//{
		//		//	mod.OnRoleCheckError(error);
		//		//};
		//		//CoroutineRunner.Instance.RunCoroutine(CheckUserRole(userId, onSuccess, onError));
		//	}
		//}
		//public static void callVersionCheck(Action<string> version)
		//{
		//	CoroutineRunner.Instance.RunCoroutine(CheckVersionCoroutine(version)); //
		//}
		//
		//private static IEnumerator CheckVersionCoroutine(Action<string> version)
		//{
		//	string apiUrl = $"https://discord.com/api/v10/channels/1259165685494317098/messages/1259165794084851774";
		//	UnityWebRequest request = UnityWebRequest.Get(apiUrl);
		//	request.SetRequestHeader("Authorization", $"Bot MTI1ODkwMzQwMzY4NjQwMDA2MA.GzQ7sM.WSLTQhktwMYHGY_mz3RbX3Hdy120VVXPaeHedU");
		//	request.SetRequestHeader("Content-Type", "application/json");
		//
		//	yield return request.SendWebRequest();
		//
		//	if (request.result != UnityWebRequest.Result.Success)
		//	{
		//		string error = request.error;
		//		log.Error($"Error: {error}");
		//	}
		//	else
		//	{
		//		var jsonResponse = request.downloadHandler.text;
		//		
		//
		//		// Deserialize the JSON response
		//		Variant variant = Colossal.Json.JSON.Load(jsonResponse);
		//		Colossal.Json.JSON.MakeInto<DiscordMessage>(variant, out DiscordMessage messages);
		//
		//		// Get the latest message content
		//		if (messages != null)
		//		{
		//			
		//			string latestVersion = messages.content;
		//
		//			version?.Invoke(latestVersion);
		//		}
		//	}
		//}
		//
		//public static void CheckRole(string userId, Action<DiscordMember> onSuccess, Action<string> onError)
		//{
		//	CoroutineRunner.Instance.RunCoroutine(CheckUserRole(userId, onSuccess, onError));
		//}
		//private static IEnumerator CheckUserRole(string userID, Action<DiscordMember> onSuccess, Action<string> onError)
		//{
		//	string apiUrl = "https://discord.com/api/v10/guilds/1245495986583900272/members/"+userID;
		//	UnityWebRequest request = UnityWebRequest.Get(apiUrl);
		//	request.SetRequestHeader("Authorization", $"Bot MTI1ODkwMzQwMzY4NjQwMDA2MA.GzQ7sM.WSLTQhktwMYHGY_mz3RbX3Hdy120VVXPaeHedU");
		//
		//	yield return request.SendWebRequest();
		//
		//	if (request.result != UnityWebRequest.Result.Success)
		//	{
		//		string error = request.error;
		//		log.Error($"Error: {error}");
		//		onError?.Invoke(error);
		//	}
		//	else
		//	{
		//		var jsonResponse = request.downloadHandler.text;
		//		
		//
		//		// Deserialize the JSON response
		//		Variant variant = Colossal.Json.JSON.Load(jsonResponse);
		//		Colossal.Json.JSON.MakeInto<DiscordMember>(variant, out DiscordMember member);
		//
		//		onSuccess?.Invoke(member);
		//	}
		//
		//}
		//
		//public class CoroutineRunner : MonoBehaviour
		//{
		//	private static CoroutineRunner _instance;
		//	public static CoroutineRunner Instance
		//	{
		//		get
		//		{
		//			if (_instance == null)
		//			{
		//				GameObject obj = new GameObject("CoroutineRunner");
		//				_instance = obj.AddComponent<CoroutineRunner>();
		//				DontDestroyOnLoad(obj);
		//			}
		//			return _instance;
		//		}
		//	}
		//
		//	public void RunCoroutine(IEnumerator coroutine)
		//	{
		//		StartCoroutine(coroutine);
		//	}
		//}


	}
}
