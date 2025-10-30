using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationUtility
{
    /// <summary>
    /// 테이블에서 키 값에 대응하는 string 값을 반환합니다.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetLocalizedValueByKey(string key)
    {
        Locale currentLanguage = LocalizationSettings.SelectedLocale;
        return LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationStrings.TableName, key, currentLanguage);
    }
    
    /// <summary>
    /// 두 값 중 현재 언어에 맞는 값을 반환합니다.
    /// </summary>
    /// <param name="englishValue"></param>
    /// <param name="koreanValue"></param>
    /// <returns></returns>
    public static string ChooseLocalizedString(string englishValue, string koreanValue)
    {
        if (IsCurrentLanguage("en"))
        { 
            return englishValue;
        }
        else
        {
            return koreanValue;
        }
    }
    public static bool IsCurrentLanguage(string languageCode)
    {
        Locale currentLocale = LocalizationSettings.SelectedLocale;
        return currentLocale.Identifier.Code.Equals(languageCode, System.StringComparison.OrdinalIgnoreCase);
    }
    
    public static void SetLanguage(string languageCode)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code.Equals(languageCode, System.StringComparison.OrdinalIgnoreCase))
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }
    }
}
