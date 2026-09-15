using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MHLab.PATCH.Settings;
using UnityEngine;

public class Localizatron : Singleton<Localizatron>
{
	private string _languagePath;

	private string _currentLanguage;

	private Dictionary<string, string> languageTable;

	public Dictionary<string, string> GetLanguageTable()
	{
		return languageTable;
	}

	public bool SetLanguage(string language)
	{
		if (Regex.IsMatch(language, "^[a-z]{2}_[A-Z]{2}$"))
		{
			_currentLanguage = language;
			_languagePath = _currentLanguage;
			languageTable = loadLanguageTable(_languagePath);
			Debug.Log("[Localizatron] Locale loaded at: " + _languagePath);
			return true;
		}
		return false;
	}

	public string GetCurrentLanguage()
	{
		return _currentLanguage;
	}

	public string Translate(string key)
	{
		if (languageTable != null)
		{
			if (languageTable.ContainsKey(key))
			{
				return languageTable[key];
			}
			return key;
		}
		return key;
	}

	public Dictionary<string, string> loadLanguageTable(string fileName)
	{
		try
		{
			TextAsset textAsset = Resources.Load<TextAsset>("Localizatron/Locale/" + fileName);
			string text = textAsset.text;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Regex regex = new Regex("<key>(.*?)</key>");
			Regex regex2 = new Regex("<value>(.*?)</value>");
			MatchCollection matchCollection = regex.Matches(text);
			MatchCollection matchCollection2 = regex2.Matches(text);
			IEnumerator enumerator = matchCollection.GetEnumerator();
			IEnumerator enumerator2 = matchCollection2.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator2.MoveNext();
				dictionary.Add(enumerator.Current.ToString().Replace("<key>", string.Empty).Replace("</key>", string.Empty), enumerator2.Current.ToString().Replace("<value>", string.Empty).Replace("</value>", string.Empty));
			}
			return dictionary;
		}
		catch (FileNotFoundException ex)
		{
			Debug.Log(ex.Message);
			return null;
		}
	}

	private void Init()
	{
		SetLanguage(Settings.LANGUAGE_DEFAULT);
	}

	private void Awake()
	{
		Init();
	}
}
