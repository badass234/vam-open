using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class AudioClipManager : JSONStorable
{
	protected Dictionary<string, List<NamedAudioClip>> categoryToClipList;

	protected Dictionary<string, NamedAudioClip> uidToClip;

	protected List<NamedAudioClip> clips;

	public AudioSource testAudioSource;

	protected bool _isPlaying;

	protected bool isPlaying
	{
		get
		{
			return _isPlaying;
		}
		set
		{
			if (_isPlaying == value)
			{
				return;
			}
			_isPlaying = value;
			if (_isPlaying)
			{
				foreach (NamedAudioClip clip in clips)
				{
					if (clip.testButtonText != null)
					{
						clip.testButtonText.text = "Stop";
					}
				}
				return;
			}
			foreach (NamedAudioClip clip2 in clips)
			{
				if (clip2.testButtonText != null)
				{
					clip2.testButtonText.text = "Test";
				}
			}
		}
	}

	public NamedAudioClip GetClip(string uid)
	{
		NamedAudioClip value;
		if (uidToClip.TryGetValue(uid, out value))
		{
			return value;
		}
		uid = Regex.Replace(uid, "\\\\", "/");
		uid = Regex.Replace(uid, ".*/", string.Empty);
		if (uidToClip.TryGetValue(uid, out value))
		{
			return value;
		}
		return null;
	}

	public List<string> GetCategories()
	{
		List<string> list = new List<string>(categoryToClipList.Keys);
		list.Sort();
		return list;
	}

	public List<NamedAudioClip> GetCategoryClips(string category)
	{
		List<NamedAudioClip> value;
		if (categoryToClipList.TryGetValue(category, out value))
		{
			return value;
		}
		return null;
	}

	public virtual bool AddClip(NamedAudioClip nac)
	{
		NamedAudioClip value;
		if (uidToClip.TryGetValue(nac.uid, out value))
		{
			Debug.LogError("Found duplicate audio clip " + nac.uid);
			return false;
		}
		uidToClip.Add(nac.uid, nac);
		List<NamedAudioClip> value2;
		if (!categoryToClipList.TryGetValue(nac.category, out value2))
		{
			value2 = new List<NamedAudioClip>();
			categoryToClipList.Add(nac.category, value2);
		}
		value2.Add(nac);
		clips.Add(nac);
		nac.manager = this;
		return true;
	}

	public virtual bool RemoveClip(NamedAudioClip nac)
	{
		if (clips.Remove(nac))
		{
			nac.destroyed = true;
			if (uidToClip.Remove(nac.uid))
			{
				List<NamedAudioClip> value;
				if (categoryToClipList.TryGetValue(nac.category, out value))
				{
					if (!value.Remove(nac))
					{
						Debug.LogError("Could not find clip " + nac.uid + " in category list " + nac.category);
						return false;
					}
					if (value.Count == 0)
					{
						categoryToClipList.Remove(nac.category);
					}
					return true;
				}
				Debug.LogError("Could not find category list for " + nac.uid + " " + nac.category);
				return false;
			}
			Debug.LogError("Tried to remove clip that is not registered " + nac.uid);
			return false;
		}
		Debug.LogError("Tried to remove clip that is not registered " + nac.uid);
		return false;
	}

	public virtual void RemoveAllClips()
	{
		categoryToClipList.Clear();
		uidToClip.Clear();
		foreach (NamedAudioClip clip in clips)
		{
			clip.destroyed = true;
		}
		clips.Clear();
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ValidateAllAtoms();
		}
	}

	public void TestClip(NamedAudioClip ac)
	{
		if (testAudioSource != null)
		{
			if (testAudioSource.isPlaying)
			{
				testAudioSource.Stop();
				return;
			}
			testAudioSource.clip = ac.sourceClip;
			testAudioSource.Play();
		}
	}

	protected virtual void Init()
	{
		categoryToClipList = new Dictionary<string, List<NamedAudioClip>>();
		uidToClip = new Dictionary<string, NamedAudioClip>();
		clips = new List<NamedAudioClip>();
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}

	protected virtual void Update()
	{
		if (testAudioSource != null)
		{
			isPlaying = testAudioSource.isPlaying;
		}
	}
}
