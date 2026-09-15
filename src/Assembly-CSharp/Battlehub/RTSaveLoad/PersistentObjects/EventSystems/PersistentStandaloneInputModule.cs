using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentStandaloneInputModule : PersistentPointerInputModule
	{
		public bool forceModuleActive;

		public float inputActionsPerSecond;

		public float repeatDelay;

		public string horizontalAxis;

		public string verticalAxis;

		public string submitButton;

		public string cancelButton;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			StandaloneInputModule standaloneInputModule = (StandaloneInputModule)obj;
			standaloneInputModule.forceModuleActive = forceModuleActive;
			standaloneInputModule.inputActionsPerSecond = inputActionsPerSecond;
			standaloneInputModule.repeatDelay = repeatDelay;
			standaloneInputModule.horizontalAxis = horizontalAxis;
			standaloneInputModule.verticalAxis = verticalAxis;
			standaloneInputModule.submitButton = submitButton;
			standaloneInputModule.cancelButton = cancelButton;
			return standaloneInputModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				StandaloneInputModule standaloneInputModule = (StandaloneInputModule)obj;
				forceModuleActive = standaloneInputModule.forceModuleActive;
				inputActionsPerSecond = standaloneInputModule.inputActionsPerSecond;
				repeatDelay = standaloneInputModule.repeatDelay;
				horizontalAxis = standaloneInputModule.horizontalAxis;
				verticalAxis = standaloneInputModule.verticalAxis;
				submitButton = standaloneInputModule.submitButton;
				cancelButton = standaloneInputModule.cancelButton;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
