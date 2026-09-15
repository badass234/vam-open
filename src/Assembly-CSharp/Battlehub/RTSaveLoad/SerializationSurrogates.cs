using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Battlehub.RTSaveLoad.UnityEngineNS;
using Battlehub.RTSaveLoad.UnityEngineNS.AINS;
using Battlehub.RTSaveLoad.UnityEngineNS.ExperimentalNS.DirectorNS;
using Battlehub.RTSaveLoad.UnityEngineNS.SceneManagementNS;
using Battlehub.RTSaveLoad.UnityEngineNS.UINS;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad
{
	public static class SerializationSurrogates
	{
		private static Dictionary<Type, ISerializationSurrogate> m_surrogates;

		static SerializationSurrogates()
		{
			m_surrogates = new Dictionary<Type, ISerializationSurrogate>();
			m_surrogates.Add(typeof(GradientAlphaKey), new GradientAlphaKeySurrogate());
			m_surrogates.Add(typeof(GradientColorKey), new GradientColorKeySurrogate());
			m_surrogates.Add(typeof(LayerMask), new LayerMaskSurrogate());
			m_surrogates.Add(typeof(RectOffset), new RectOffsetSurrogate());
			m_surrogates.Add(typeof(AnimationTriggers), new AnimationTriggersSurrogate());
			m_surrogates.Add(typeof(ColorBlock), new ColorBlockSurrogate());
			m_surrogates.Add(typeof(NavMeshPath), new NavMeshPathSurrogate());
			m_surrogates.Add(typeof(ClothSkinningCoefficient), new ClothSkinningCoefficientSurrogate());
			m_surrogates.Add(typeof(BoneWeight), new BoneWeightSurrogate());
			m_surrogates.Add(typeof(TreeInstance), new TreeInstanceSurrogate());
			m_surrogates.Add(typeof(CharacterInfo), new CharacterInfoSurrogate());
			m_surrogates.Add(typeof(Vector3), new Vector3Surrogate());
			m_surrogates.Add(typeof(Color), new ColorSurrogate());
			m_surrogates.Add(typeof(Rect), new RectSurrogate());
			m_surrogates.Add(typeof(Matrix4x4), new Matrix4x4Surrogate());
			m_surrogates.Add(typeof(Scene), new SceneSurrogate());
			m_surrogates.Add(typeof(Bounds), new BoundsSurrogate());
			m_surrogates.Add(typeof(Vector4), new Vector4Surrogate());
			m_surrogates.Add(typeof(Vector2), new Vector2Surrogate());
			m_surrogates.Add(typeof(RenderBuffer), new RenderBufferSurrogate());
			m_surrogates.Add(typeof(Quaternion), new QuaternionSurrogate());
			m_surrogates.Add(typeof(JointMotor), new JointMotorSurrogate());
			m_surrogates.Add(typeof(JointLimits), new JointLimitsSurrogate());
			m_surrogates.Add(typeof(JointSpring), new JointSpringSurrogate());
			m_surrogates.Add(typeof(JointDrive), new JointDriveSurrogate());
			m_surrogates.Add(typeof(SoftJointLimitSpring), new SoftJointLimitSpringSurrogate());
			m_surrogates.Add(typeof(SoftJointLimit), new SoftJointLimitSurrogate());
			m_surrogates.Add(typeof(JointMotor2D), new JointMotor2DSurrogate());
			m_surrogates.Add(typeof(JointAngleLimits2D), new JointAngleLimits2DSurrogate());
			m_surrogates.Add(typeof(JointTranslationLimits2D), new JointTranslationLimits2DSurrogate());
			m_surrogates.Add(typeof(JointSuspension2D), new JointSuspension2DSurrogate());
			m_surrogates.Add(typeof(WheelFrictionCurve), new WheelFrictionCurveSurrogate());
			m_surrogates.Add(typeof(OffMeshLinkData), new OffMeshLinkDataSurrogate());
			m_surrogates.Add(typeof(PlayableGraph), new PlayableGraphSurrogate());
			m_surrogates.Add(typeof(Color32), new Color32Surrogate());
		}

		public static SurrogateSelector CreateSelector()
		{
			SurrogateSelector surrogateSelector = new SurrogateSelector();
			foreach (KeyValuePair<Type, ISerializationSurrogate> surrogate in m_surrogates)
			{
				surrogateSelector.AddSurrogate(surrogate.Key, new StreamingContext(StreamingContextStates.All), surrogate.Value);
			}
			return surrogateSelector;
		}

		public static Dictionary<Type, ISerializationSurrogate> GetSurrogates()
		{
			return m_surrogates;
		}
	}
}
