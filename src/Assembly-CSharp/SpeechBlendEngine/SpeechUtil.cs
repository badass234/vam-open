using System;
using UnityEngine;

namespace SpeechBlendEngine
{
	public class SpeechUtil : MonoBehaviour
	{
		public enum Mode
		{
			jawOnly,
			jawAndVisemes
		}

		public enum Accuracy
		{
			Low,
			Medium,
			High
		}

		[Serializable]
		public class VisemeBlendshapeIndexes
		{
			public VoiceProfile.Template template;

			public int[] visemeIndex;

			public int mouthOpenIndex;

			public VisemeBlendshapeIndexes(VoiceProfile.Template tmplte)
			{
				template = tmplte;
				visemeIndex = new int[tmplte.Nvis];
				for (int i = 0; i < tmplte.Nvis; i++)
				{
					visemeIndex[i] = -1;
				}
				mouthOpenIndex = -1;
			}

			public int[] ReturnArray()
			{
				return visemeIndex;
			}

			public void LoadFromArray(int[] array)
			{
				visemeIndex = array;
			}

			public int GetByIndex(int index)
			{
				return visemeIndex[index];
			}

			public bool BlendshapeAssigned(int index)
			{
				return GetByIndex(index) > -1;
			}

			public bool MouthOpenBlendshapeAssigned()
			{
				return mouthOpenIndex > -1;
			}

			public bool AnyAssigned()
			{
				for (int i = 0; i < template.Nvis; i++)
				{
					if (BlendshapeAssigned(i))
					{
						return true;
					}
				}
				return MouthOpenBlendshapeAssigned();
			}

			public bool JawOnly()
			{
				for (int i = 0; i < template.Nvis && !BlendshapeAssigned(i); i++)
				{
				}
				return false;
			}
		}

		[Serializable]
		public class VisemeBlendshapeNames
		{
			public VoiceProfile.Template template;

			public string[] visemeNames;

			public string mouthOpenName;

			public VisemeBlendshapeNames(VoiceProfile.Template tmplte)
			{
				template = tmplte;
				visemeNames = new string[tmplte.Nvis];
				for (int i = 0; i < tmplte.Nvis; i++)
				{
					visemeNames[i] = null;
				}
				mouthOpenName = null;
			}

			public string[] ReturnArray()
			{
				return visemeNames;
			}

			public void LoadFromArray(string[] array)
			{
				visemeNames = array;
			}

			public string GetByIndex(int index)
			{
				return visemeNames[index];
			}

			public bool BlendshapeAssigned(int index)
			{
				return GetByIndex(index) != null;
			}

			public bool MouthOpenBlendshapeAssigned()
			{
				return mouthOpenName != null;
			}

			public bool AnyAssigned()
			{
				for (int i = 0; i < template.Nvis; i++)
				{
					if (BlendshapeAssigned(i))
					{
						return true;
					}
				}
				return MouthOpenBlendshapeAssigned();
			}

			public bool JawOnly()
			{
				for (int i = 0; i < template.Nvis && !BlendshapeAssigned(i); i++)
				{
				}
				return false;
			}
		}

		[Serializable]
		public class VisemeWeight
		{
			public float[] weights;

			public VoiceProfile.Template template;

			public VisemeWeight(VoiceProfile.Template tmplte)
			{
				template = tmplte;
				weights = new float[template.Nvis];
				for (int i = 0; i < template.Nvis; i++)
				{
					weights[i] = 1f;
				}
			}

			public void LoadFromArray(float[] array)
			{
				weights = array;
			}

			public float[] ReturnArray()
			{
				return weights;
			}

			public float GetByIndex(int index)
			{
				return weights[index];
			}
		}
	}
}
