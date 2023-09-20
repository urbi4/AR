using System.Collections.Generic;
using UnityEngine.Networking;

// Trida pro ulehceni prace s URL parametry
public class RequestParameters
{
	private Dictionary<string, string> parameters = new Dictionary<string, string>();


	public bool HasKey(string key)
	{
		return parameters.ContainsKey(key);
	}

	// This can be called from Start(), but not earlier
	public string GetValue(string key)
	{
		return parameters[key];
	}

	public void SetRequestParameters(string parametersString)
	{
		char[] parameterDelimiters = new char[] { '?', '&' };
		string[] splittedString = parametersString.Split(parameterDelimiters, System.StringSplitOptions.RemoveEmptyEntries);


		char[] keyValueDelimiters = new char[] { '=' };
		for (int i = 0; i < splittedString.Length; ++i)
		{
			string[] keyValue = splittedString[i].Split(keyValueDelimiters, System.StringSplitOptions.None);

			if (keyValue.Length >= 2)
			{
				// TODO ONLY TO FIX ://scan/
				char[] parameterScanDelimiters = new char[] { ':','?', '&' };
				string[] splittedScanString = UnityWebRequest.UnEscapeURL(keyValue[1]).Split(parameterScanDelimiters, System.StringSplitOptions.RemoveEmptyEntries);
				parameters.Add(UnityWebRequest.UnEscapeURL(keyValue[0]), splittedScanString[0]);
				// ORIG parameters.Add(UnityWebRequest.UnEscapeURL(keyValue[0]), UnityWebRequest.UnEscapeURL(keyValue[1]));
			}
			else if (keyValue.Length == 1)
			{
				parameters.Add(UnityWebRequest.UnEscapeURL(keyValue[0]), "");
			}
		}
	}
}
