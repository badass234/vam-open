using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAnimator : PersistentBehaviour
	{
		public Vector3 rootPosition;

		public Quaternion rootRotation;

		public bool applyRootMotion;

		public bool linearVelocityBlending;

		public uint updateMode;

		public bool stabilizeFeet;

		public float feetPivotActive;

		public float speed;

		public uint cullingMode;

		public float recorderStartTime;

		public float recorderStopTime;

		public long runtimeAnimatorController;

		public long avatar;

		public bool layersAffectMassCenter;

		public bool logWarnings;

		public bool fireEvents;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Animator animator = (Animator)obj;
			animator.rootPosition = rootPosition;
			animator.rootRotation = rootRotation;
			animator.applyRootMotion = applyRootMotion;
			animator.linearVelocityBlending = linearVelocityBlending;
			animator.updateMode = (AnimatorUpdateMode)updateMode;
			animator.stabilizeFeet = stabilizeFeet;
			animator.feetPivotActive = feetPivotActive;
			animator.speed = speed;
			animator.cullingMode = (AnimatorCullingMode)cullingMode;
			animator.recorderStartTime = recorderStartTime;
			animator.recorderStopTime = recorderStopTime;
			animator.runtimeAnimatorController = (RuntimeAnimatorController)objects.Get(runtimeAnimatorController);
			animator.avatar = (Avatar)objects.Get(avatar);
			animator.layersAffectMassCenter = layersAffectMassCenter;
			animator.logWarnings = logWarnings;
			animator.fireEvents = fireEvents;
			return animator;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Animator animator = (Animator)obj;
				rootPosition = animator.rootPosition;
				rootRotation = animator.rootRotation;
				applyRootMotion = animator.applyRootMotion;
				linearVelocityBlending = animator.linearVelocityBlending;
				updateMode = (uint)animator.updateMode;
				stabilizeFeet = animator.stabilizeFeet;
				feetPivotActive = animator.feetPivotActive;
				speed = animator.speed;
				cullingMode = (uint)animator.cullingMode;
				recorderStartTime = animator.recorderStartTime;
				recorderStopTime = animator.recorderStopTime;
				runtimeAnimatorController = animator.runtimeAnimatorController.GetMappedInstanceID();
				avatar = animator.avatar.GetMappedInstanceID();
				layersAffectMassCenter = animator.layersAffectMassCenter;
				logWarnings = animator.logWarnings;
				fireEvents = animator.fireEvents;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(runtimeAnimatorController, dependencies, objects, allowNulls);
			AddDependency(avatar, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Animator animator = (Animator)obj;
				AddDependency(animator.runtimeAnimatorController, dependencies);
				AddDependency(animator.avatar, dependencies);
			}
		}
	}
}
