using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

// Pomocna trida pro preklad
public class LocaleUtils : MonoBehaviour
{

    public static String GetLocalizetString(LocalizedString localizedString)
    {
        var stringOperation = localizedString.GetLocalizedStringAsync();
        if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
            return stringOperation.Result;
        return String.Empty;
    }
}
