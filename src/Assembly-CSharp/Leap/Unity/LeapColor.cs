using UnityEngine;

namespace Leap.Unity
{
	public static class LeapColor
	{
		public static Color black => Color.black;

		public static Color gray => new Color(0.5f, 0.5f, 0.5f);

		public static Color white => Color.white;

		public static Color pink => new Color(1f, 64f / 85f, 0.79607844f);

		public static Color magenta => Color.magenta;

		public static Color fuschia => lerp(Color.magenta, Color.blue, 0.1f);

		public static Color red => Color.red;

		public static Color brown => new Color(0.5882353f, 0.29411766f, 0f);

		public static Color beige => new Color(49f / 51f, 49f / 51f, 44f / 51f);

		public static Color coral => new Color(1f, 0.49803922f, 16f / 51f);

		public static Color orange => lerp(red, yellow, 0.5f);

		public static Color khaki => new Color(0.7647059f, 0.6901961f, 29f / 51f);

		public static Color amber => new Color(1f, 0.7490196f, 0f);

		public static Color yellow => Color.yellow;

		public static Color gold => new Color(0.83137256f, 35f / 51f, 11f / 51f);

		public static Color green => Color.green;

		public static Color forest => new Color(2f / 15f, 0.54509807f, 2f / 15f);

		public static Color lime => new Color(0.61960787f, 0.99215686f, 0.21960784f);

		public static Color mint => new Color(0.59607846f, 0.9843137f, 0.59607846f);

		public static Color olive => new Color(0.5019608f, 0.5019608f, 0f);

		public static Color jade => new Color(0f, 56f / 85f, 0.41960785f);

		public static Color teal => new Color(0f, 0.5019608f, 0.5019608f);

		public static Color veridian => new Color(0.2509804f, 26f / 51f, 0.42745098f);

		public static Color turquoise => new Color(0.2509804f, 0.8784314f, 0.8156863f);

		public static Color cyan => Color.cyan;

		public static Color cerulean => new Color(0f, 41f / 85f, 0.654902f);

		public static Color aqua => new Color(0.56078434f, 0.8784314f, 0.96862745f);

		public static Color electricBlue => new Color(25f / 51f, 83f / 85f, 1f);

		public static Color blue => Color.blue;

		public static Color navy => new Color(0f, 0f, 0.5019608f);

		public static Color periwinkle => new Color(0.8f, 0.8f, 1f);

		public static Color purple => lerp(magenta, blue, 0.3f);

		public static Color violet => new Color(0.49803922f, 0f, 1f);

		public static Color lavender => new Color(0.70980394f, 42f / 85f, 44f / 51f);

		private static Color lerp(Color a, Color b, float amount)
		{
			return Color.Lerp(a, b, amount);
		}
	}
}
