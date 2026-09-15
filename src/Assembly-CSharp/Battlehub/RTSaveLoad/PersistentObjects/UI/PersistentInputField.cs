using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentInputField : PersistentSelectable
	{
		public bool shouldHideMobileInput;

		public string text;

		public float caretBlinkRate;

		public int caretWidth;

		public long textComponent;

		public long placeholder;

		public Color caretColor;

		public bool customCaretColor;

		public Color selectionColor;

		public PersistentUnityEventBase onEndEdit;

		public PersistentUnityEventBase onValueChanged;

		public InputField.OnValidateInput onValidateInput;

		public int characterLimit;

		public InputField.ContentType contentType;

		public InputField.LineType lineType;

		public InputField.InputType inputType;

		public TouchScreenKeyboardType keyboardType;

		public InputField.CharacterValidation characterValidation;

		public bool readOnly;

		public char asteriskChar;

		public int caretPosition;

		public int selectionAnchorPosition;

		public int selectionFocusPosition;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			InputField inputField = (InputField)obj;
			inputField.shouldHideMobileInput = shouldHideMobileInput;
			inputField.text = text;
			inputField.caretBlinkRate = caretBlinkRate;
			inputField.caretWidth = caretWidth;
			inputField.textComponent = (Text)objects.Get(textComponent);
			inputField.placeholder = (Graphic)objects.Get(placeholder);
			inputField.caretColor = caretColor;
			inputField.customCaretColor = customCaretColor;
			inputField.selectionColor = selectionColor;
			Write(inputField.onEndEdit, onEndEdit, objects);
			Write(inputField.onValueChanged, onValueChanged, objects);
			inputField.onValidateInput = onValidateInput;
			inputField.characterLimit = characterLimit;
			inputField.contentType = contentType;
			inputField.lineType = lineType;
			inputField.inputType = inputType;
			inputField.keyboardType = keyboardType;
			inputField.characterValidation = characterValidation;
			inputField.readOnly = readOnly;
			inputField.asteriskChar = asteriskChar;
			inputField.caretPosition = caretPosition;
			inputField.selectionAnchorPosition = selectionAnchorPosition;
			inputField.selectionFocusPosition = selectionFocusPosition;
			return inputField;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				InputField inputField = (InputField)obj;
				shouldHideMobileInput = inputField.shouldHideMobileInput;
				text = inputField.text;
				caretBlinkRate = inputField.caretBlinkRate;
				caretWidth = inputField.caretWidth;
				textComponent = inputField.textComponent.GetMappedInstanceID();
				placeholder = inputField.placeholder.GetMappedInstanceID();
				caretColor = inputField.caretColor;
				customCaretColor = inputField.customCaretColor;
				selectionColor = inputField.selectionColor;
				Read(onEndEdit, inputField.onEndEdit);
				Read(onValueChanged, inputField.onValueChanged);
				onValidateInput = inputField.onValidateInput;
				characterLimit = inputField.characterLimit;
				contentType = inputField.contentType;
				lineType = inputField.lineType;
				inputType = inputField.inputType;
				keyboardType = inputField.keyboardType;
				characterValidation = inputField.characterValidation;
				readOnly = inputField.readOnly;
				asteriskChar = inputField.asteriskChar;
				caretPosition = inputField.caretPosition;
				selectionAnchorPosition = inputField.selectionAnchorPosition;
				selectionFocusPosition = inputField.selectionFocusPosition;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(textComponent, dependencies, objects, allowNulls);
			AddDependency(placeholder, dependencies, objects, allowNulls);
			if (onEndEdit != null)
			{
				onEndEdit.FindDependencies(dependencies, objects, allowNulls);
			}
			if (onValueChanged != null)
			{
				onValueChanged.FindDependencies(dependencies, objects, allowNulls);
			}
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				InputField inputField = (InputField)obj;
				AddDependency(inputField.textComponent, dependencies);
				AddDependency(inputField.placeholder, dependencies);
				PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
				persistentUnityEventBase.GetDependencies(inputField.onEndEdit, dependencies);
				persistentUnityEventBase.GetDependencies(inputField.onValueChanged, dependencies);
			}
		}
	}
}
