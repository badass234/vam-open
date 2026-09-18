using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.RTSaveLoad.PersistentObjects;
using Battlehub.RTSaveLoad.PersistentObjects.AI;
using Battlehub.RTSaveLoad.PersistentObjects.Audio;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using Battlehub.RTSaveLoad.PersistentObjects.Experimental.Rendering;
using Battlehub.RTSaveLoad.PersistentObjects.Networking.Match;
using Battlehub.RTSaveLoad.PersistentObjects.Networking.PlayerConnection;
using Battlehub.RTSaveLoad.PersistentObjects.Rendering;
using Battlehub.RTSaveLoad.PersistentObjects.UI;
using Battlehub.RTSaveLoad.PersistentObjects.Video;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(500, typeof(PersistentAnimationCurve))]
	[ProtoInclude(501, typeof(PersistentBurst))]
	[ProtoInclude(502, typeof(PersistentColorBySpeedModule))]
	[ProtoInclude(503, typeof(PersistentColorOverLifetimeModule))]
	[ProtoInclude(504, typeof(PersistentCustomDataModule))]
	[ProtoInclude(505, typeof(PersistentEmitParams))]
	[ProtoInclude(506, typeof(PersistentExternalForcesModule))]
	[ProtoInclude(507, typeof(PersistentForceOverLifetimeModule))]
	[ProtoInclude(508, typeof(PersistentGradient))]
	[ProtoInclude(509, typeof(PersistentInheritVelocityModule))]
	[ProtoInclude(510, typeof(PersistentKeyframe))]
	[ProtoInclude(511, typeof(PersistentLightsModule))]
	[ProtoInclude(512, typeof(PersistentLimitVelocityOverLifetimeModule))]
	[ProtoInclude(513, typeof(PersistentMainModule))]
	[ProtoInclude(514, typeof(PersistentMinMaxCurve))]
	[ProtoInclude(515, typeof(PersistentMinMaxGradient))]
	[ProtoInclude(516, typeof(PersistentNoiseModule))]
	[ProtoInclude(517, typeof(PersistentParticle))]
	[ProtoInclude(518, typeof(PersistentRotationBySpeedModule))]
	[ProtoInclude(519, typeof(PersistentRotationOverLifetimeModule))]
	[ProtoInclude(520, typeof(PersistentShapeModule))]
	[ProtoInclude(521, typeof(PersistentSizeBySpeedModule))]
	[ProtoInclude(522, typeof(PersistentSizeOverLifetimeModule))]
	[ProtoInclude(523, typeof(PersistentSubEmittersModule))]
	[ProtoInclude(524, typeof(PersistentTextureSheetAnimationModule))]
	[ProtoInclude(525, typeof(PersistentTrailModule))]
	[ProtoInclude(526, typeof(PersistentVelocityOverLifetimeModule))]
	[ProtoInclude(527, typeof(PersistentGUIStyle))]
	[ProtoInclude(528, typeof(PersistentGUIStyleState))]
	[ProtoInclude(529, typeof(PersistentCollisionModule))]
	[ProtoInclude(530, typeof(PersistentEmissionModule))]
	[ProtoInclude(531, typeof(PersistentTriggerModule))]
	[ProtoInclude(532, typeof(PersistentObject))]
	[ProtoInclude(533, typeof(PersistentNavigation))]
	[ProtoInclude(534, typeof(PersistentOptionData))]
	[ProtoInclude(535, typeof(PersistentSpriteState))]
	public abstract class PersistentData
	{
		public const int USER_DEFINED_FIELD_TAG = 100000;

		protected static readonly Dictionary<Type, Type> m_objToData;

		public bool ActiveSelf;

		public long InstanceId;

		public PersistentObject AsPersistentObject => this as PersistentObject;

		static PersistentData()
		{
			m_objToData = new Dictionary<Type, Type>();
			m_objToData.Add(typeof(AssetBundle), typeof(PersistentAssetBundle));
			m_objToData.Add(typeof(AssetBundleManifest), typeof(PersistentAssetBundleManifest));
			m_objToData.Add(typeof(ScriptableObject), typeof(PersistentScriptableObject));
			m_objToData.Add(typeof(Behaviour), typeof(PersistentBehaviour));
			m_objToData.Add(typeof(BillboardAsset), typeof(PersistentBillboardAsset));
			m_objToData.Add(typeof(BillboardRenderer), typeof(PersistentBillboardRenderer));
			m_objToData.Add(typeof(Camera), typeof(PersistentCamera));
			m_objToData.Add(typeof(Component), typeof(PersistentComponent));
			m_objToData.Add(typeof(ComputeShader), typeof(PersistentComputeShader));
			m_objToData.Add(typeof(FlareLayer), typeof(PersistentFlareLayer));
			m_objToData.Add(typeof(GameObject), typeof(PersistentGameObject));
			m_objToData.Add(typeof(OcclusionArea), typeof(PersistentOcclusionArea));
			m_objToData.Add(typeof(OcclusionPortal), typeof(PersistentOcclusionPortal));
			m_objToData.Add(typeof(RenderSettings), typeof(PersistentRenderSettings));
			m_objToData.Add(typeof(QualitySettings), typeof(PersistentQualitySettings));
			m_objToData.Add(typeof(MeshFilter), typeof(PersistentMeshFilter));
			m_objToData.Add(typeof(SkinnedMeshRenderer), typeof(PersistentSkinnedMeshRenderer));
			m_objToData.Add(typeof(Flare), typeof(PersistentFlare));
			m_objToData.Add(typeof(LensFlare), typeof(PersistentLensFlare));
			m_objToData.Add(typeof(Renderer), typeof(PersistentRenderer));
			m_objToData.Add(typeof(Projector), typeof(PersistentProjector));
			m_objToData.Add(typeof(Skybox), typeof(PersistentSkybox));
			m_objToData.Add(typeof(TrailRenderer), typeof(PersistentTrailRenderer));
			m_objToData.Add(typeof(LineRenderer), typeof(PersistentLineRenderer));
			m_objToData.Add(typeof(LightProbes), typeof(PersistentLightProbes));
			m_objToData.Add(typeof(LightmapSettings), typeof(PersistentLightmapSettings));
			m_objToData.Add(typeof(MeshRenderer), typeof(PersistentMeshRenderer));
			// GUIElement was removed from the engine in 2019.1, so it can no longer be registered here.
			// PersistentGUIElement stays: the class itself never names the type.
			m_objToData.Add(typeof(Light), typeof(PersistentLight));
			m_objToData.Add(typeof(LightProbeGroup), typeof(PersistentLightProbeGroup));
			m_objToData.Add(typeof(LightProbeProxyVolume), typeof(PersistentLightProbeProxyVolume));
			m_objToData.Add(typeof(LODGroup), typeof(PersistentLODGroup));
			m_objToData.Add(typeof(Mesh), typeof(PersistentMesh));
			m_objToData.Add(typeof(MonoBehaviour), typeof(PersistentMonoBehaviour));
			m_objToData.Add(typeof(ReflectionProbe), typeof(PersistentReflectionProbe));
			m_objToData.Add(typeof(GraphicsSettings), typeof(PersistentGraphicsSettings));
			m_objToData.Add(typeof(Shader), typeof(PersistentShader));
			m_objToData.Add(typeof(Material), typeof(PersistentMaterial));
			m_objToData.Add(typeof(ShaderVariantCollection), typeof(PersistentShaderVariantCollection));
			m_objToData.Add(typeof(Sprite), typeof(PersistentSprite));
			m_objToData.Add(typeof(SpriteRenderer), typeof(PersistentSpriteRenderer));
			m_objToData.Add(typeof(TextAsset), typeof(PersistentTextAsset));
			m_objToData.Add(typeof(Texture), typeof(PersistentTexture));
			m_objToData.Add(typeof(Texture2D), typeof(PersistentTexture2D));
			m_objToData.Add(typeof(Cubemap), typeof(PersistentCubemap));
			m_objToData.Add(typeof(Texture3D), typeof(PersistentTexture3D));
			m_objToData.Add(typeof(Texture2DArray), typeof(PersistentTexture2DArray));
			m_objToData.Add(typeof(CubemapArray), typeof(PersistentCubemapArray));
			m_objToData.Add(typeof(SparseTexture), typeof(PersistentSparseTexture));
			m_objToData.Add(typeof(RenderTexture), typeof(PersistentRenderTexture));
			m_objToData.Add(typeof(WindZone), typeof(PersistentWindZone));
			m_objToData.Add(typeof(Transform), typeof(PersistentTransform));
			m_objToData.Add(typeof(RectTransform), typeof(PersistentRectTransform));
			m_objToData.Add(typeof(SortingGroup), typeof(PersistentSortingGroup));
			m_objToData.Add(typeof(ParticleSystem), typeof(PersistentParticleSystem));
			m_objToData.Add(typeof(ParticleSystemRenderer), typeof(PersistentParticleSystemRenderer));
			m_objToData.Add(typeof(Rigidbody), typeof(PersistentRigidbody));
			m_objToData.Add(typeof(Joint), typeof(PersistentJoint));
			m_objToData.Add(typeof(HingeJoint), typeof(PersistentHingeJoint));
			m_objToData.Add(typeof(SpringJoint), typeof(PersistentSpringJoint));
			m_objToData.Add(typeof(FixedJoint), typeof(PersistentFixedJoint));
			m_objToData.Add(typeof(CharacterJoint), typeof(PersistentCharacterJoint));
			m_objToData.Add(typeof(ConfigurableJoint), typeof(PersistentConfigurableJoint));
			m_objToData.Add(typeof(ConstantForce), typeof(PersistentConstantForce));
			m_objToData.Add(typeof(Collider), typeof(PersistentCollider));
			m_objToData.Add(typeof(BoxCollider), typeof(PersistentBoxCollider));
			m_objToData.Add(typeof(SphereCollider), typeof(PersistentSphereCollider));
			m_objToData.Add(typeof(MeshCollider), typeof(PersistentMeshCollider));
			m_objToData.Add(typeof(CapsuleCollider), typeof(PersistentCapsuleCollider));
			m_objToData.Add(typeof(PhysicMaterial), typeof(PersistentPhysicMaterial));
			m_objToData.Add(typeof(CharacterController), typeof(PersistentCharacterController));
			m_objToData.Add(typeof(CircleCollider2D), typeof(PersistentCircleCollider2D));
			m_objToData.Add(typeof(BoxCollider2D), typeof(PersistentBoxCollider2D));
			m_objToData.Add(typeof(Joint2D), typeof(PersistentJoint2D));
			m_objToData.Add(typeof(AreaEffector2D), typeof(PersistentAreaEffector2D));
			m_objToData.Add(typeof(PlatformEffector2D), typeof(PersistentPlatformEffector2D));
			m_objToData.Add(typeof(Rigidbody2D), typeof(PersistentRigidbody2D));
			m_objToData.Add(typeof(Collider2D), typeof(PersistentCollider2D));
			m_objToData.Add(typeof(EdgeCollider2D), typeof(PersistentEdgeCollider2D));
			m_objToData.Add(typeof(CapsuleCollider2D), typeof(PersistentCapsuleCollider2D));
			m_objToData.Add(typeof(CompositeCollider2D), typeof(PersistentCompositeCollider2D));
			m_objToData.Add(typeof(PolygonCollider2D), typeof(PersistentPolygonCollider2D));
			m_objToData.Add(typeof(AnchoredJoint2D), typeof(PersistentAnchoredJoint2D));
			m_objToData.Add(typeof(SpringJoint2D), typeof(PersistentSpringJoint2D));
			m_objToData.Add(typeof(DistanceJoint2D), typeof(PersistentDistanceJoint2D));
			m_objToData.Add(typeof(FrictionJoint2D), typeof(PersistentFrictionJoint2D));
			m_objToData.Add(typeof(HingeJoint2D), typeof(PersistentHingeJoint2D));
			m_objToData.Add(typeof(RelativeJoint2D), typeof(PersistentRelativeJoint2D));
			m_objToData.Add(typeof(SliderJoint2D), typeof(PersistentSliderJoint2D));
			m_objToData.Add(typeof(TargetJoint2D), typeof(PersistentTargetJoint2D));
			m_objToData.Add(typeof(FixedJoint2D), typeof(PersistentFixedJoint2D));
			m_objToData.Add(typeof(WheelJoint2D), typeof(PersistentWheelJoint2D));
			m_objToData.Add(typeof(PhysicsMaterial2D), typeof(PersistentPhysicsMaterial2D));
			m_objToData.Add(typeof(PhysicsUpdateBehaviour2D), typeof(PersistentPhysicsUpdateBehaviour2D));
			m_objToData.Add(typeof(ConstantForce2D), typeof(PersistentConstantForce2D));
			m_objToData.Add(typeof(Effector2D), typeof(PersistentEffector2D));
			m_objToData.Add(typeof(BuoyancyEffector2D), typeof(PersistentBuoyancyEffector2D));
			m_objToData.Add(typeof(PointEffector2D), typeof(PersistentPointEffector2D));
			m_objToData.Add(typeof(SurfaceEffector2D), typeof(PersistentSurfaceEffector2D));
			m_objToData.Add(typeof(WheelCollider), typeof(PersistentWheelCollider));
			m_objToData.Add(typeof(Cloth), typeof(PersistentCloth));
			m_objToData.Add(typeof(NavMeshData), typeof(PersistentNavMeshData));
			m_objToData.Add(typeof(NavMeshAgent), typeof(PersistentNavMeshAgent));
			m_objToData.Add(typeof(NavMeshObstacle), typeof(PersistentNavMeshObstacle));
			m_objToData.Add(typeof(OffMeshLink), typeof(PersistentOffMeshLink));
			m_objToData.Add(typeof(AudioSource), typeof(PersistentAudioSource));
			m_objToData.Add(typeof(AudioLowPassFilter), typeof(PersistentAudioLowPassFilter));
			m_objToData.Add(typeof(AudioHighPassFilter), typeof(PersistentAudioHighPassFilter));
			m_objToData.Add(typeof(AudioReverbFilter), typeof(PersistentAudioReverbFilter));
			m_objToData.Add(typeof(AudioClip), typeof(PersistentAudioClip));
			m_objToData.Add(typeof(AudioBehaviour), typeof(PersistentAudioBehaviour));
			m_objToData.Add(typeof(AudioListener), typeof(PersistentAudioListener));
			m_objToData.Add(typeof(AudioReverbZone), typeof(PersistentAudioReverbZone));
			m_objToData.Add(typeof(AudioDistortionFilter), typeof(PersistentAudioDistortionFilter));
			m_objToData.Add(typeof(AudioEchoFilter), typeof(PersistentAudioEchoFilter));
			m_objToData.Add(typeof(AudioChorusFilter), typeof(PersistentAudioChorusFilter));
			m_objToData.Add(typeof(AudioMixer), typeof(PersistentAudioMixer));
			m_objToData.Add(typeof(AudioMixerSnapshot), typeof(PersistentAudioMixerSnapshot));
			m_objToData.Add(typeof(AudioMixerGroup), typeof(PersistentAudioMixerGroup));
			// MovieTexture was removed from the engine in 2019.1. PersistentMovieTexture stays registered
			// nowhere, but the class itself survives as the carrier of its serialised `loop` field.
			m_objToData.Add(typeof(WebCamTexture), typeof(PersistentWebCamTexture));
			m_objToData.Add(typeof(Animator), typeof(PersistentAnimator));
			m_objToData.Add(typeof(StateMachineBehaviour), typeof(PersistentStateMachineBehaviour));
			m_objToData.Add(typeof(AnimatorOverrideController), typeof(PersistentAnimatorOverrideController));
			m_objToData.Add(typeof(AnimationClip), typeof(PersistentAnimationClip));
			m_objToData.Add(typeof(Animation), typeof(PersistentAnimation));
			m_objToData.Add(typeof(RuntimeAnimatorController), typeof(PersistentRuntimeAnimatorController));
			m_objToData.Add(typeof(Avatar), typeof(PersistentAvatar));
			m_objToData.Add(typeof(AvatarMask), typeof(PersistentAvatarMask));
			m_objToData.Add(typeof(Motion), typeof(PersistentMotion));
			m_objToData.Add(typeof(TerrainData), typeof(PersistentTerrainData));
			m_objToData.Add(typeof(Terrain), typeof(PersistentTerrain));
			m_objToData.Add(typeof(Tree), typeof(PersistentTree));
			m_objToData.Add(typeof(TextMesh), typeof(PersistentTextMesh));
			m_objToData.Add(typeof(Font), typeof(PersistentFont));
			m_objToData.Add(typeof(Canvas), typeof(PersistentCanvas));
			m_objToData.Add(typeof(CanvasGroup), typeof(PersistentCanvasGroup));
			m_objToData.Add(typeof(CanvasRenderer), typeof(PersistentCanvasRenderer));
			m_objToData.Add(typeof(TerrainCollider), typeof(PersistentTerrainCollider));
			m_objToData.Add(typeof(GUISkin), typeof(PersistentGUISkin));
			m_objToData.Add(typeof(NetworkMatch), typeof(PersistentNetworkMatch));
			m_objToData.Add(typeof(VideoPlayer), typeof(PersistentVideoPlayer));
			m_objToData.Add(typeof(VideoClip), typeof(PersistentVideoClip));
			m_objToData.Add(typeof(PlayerConnection), typeof(PersistentPlayerConnection));
			m_objToData.Add(typeof(RenderPipelineAsset), typeof(PersistentRenderPipelineAsset));
			m_objToData.Add(typeof(EventSystem), typeof(PersistentEventSystem));
			m_objToData.Add(typeof(EventTrigger), typeof(PersistentEventTrigger));
			m_objToData.Add(typeof(UIBehaviour), typeof(PersistentUIBehaviour));
			m_objToData.Add(typeof(BaseInput), typeof(PersistentBaseInput));
			m_objToData.Add(typeof(BaseInputModule), typeof(PersistentBaseInputModule));
			m_objToData.Add(typeof(PointerInputModule), typeof(PersistentPointerInputModule));
			m_objToData.Add(typeof(StandaloneInputModule), typeof(PersistentStandaloneInputModule));
			m_objToData.Add(typeof(BaseRaycaster), typeof(PersistentBaseRaycaster));
			m_objToData.Add(typeof(Physics2DRaycaster), typeof(PersistentPhysics2DRaycaster));
			m_objToData.Add(typeof(PhysicsRaycaster), typeof(PersistentPhysicsRaycaster));
			m_objToData.Add(typeof(Button), typeof(PersistentButton));
			m_objToData.Add(typeof(Dropdown), typeof(PersistentDropdown));
			m_objToData.Add(typeof(Graphic), typeof(PersistentGraphic));
			m_objToData.Add(typeof(GraphicRaycaster), typeof(PersistentGraphicRaycaster));
			m_objToData.Add(typeof(Image), typeof(PersistentImage));
			m_objToData.Add(typeof(InputField), typeof(PersistentInputField));
			m_objToData.Add(typeof(Mask), typeof(PersistentMask));
			m_objToData.Add(typeof(MaskableGraphic), typeof(PersistentMaskableGraphic));
			m_objToData.Add(typeof(RawImage), typeof(PersistentRawImage));
			m_objToData.Add(typeof(RectMask2D), typeof(PersistentRectMask2D));
			m_objToData.Add(typeof(Scrollbar), typeof(PersistentScrollbar));
			m_objToData.Add(typeof(ScrollRect), typeof(PersistentScrollRect));
			m_objToData.Add(typeof(Selectable), typeof(PersistentSelectable));
			m_objToData.Add(typeof(Slider), typeof(PersistentSlider));
			m_objToData.Add(typeof(Text), typeof(PersistentText));
			m_objToData.Add(typeof(Toggle), typeof(PersistentToggle));
			m_objToData.Add(typeof(ToggleGroup), typeof(PersistentToggleGroup));
			m_objToData.Add(typeof(AspectRatioFitter), typeof(PersistentAspectRatioFitter));
			m_objToData.Add(typeof(CanvasScaler), typeof(PersistentCanvasScaler));
			m_objToData.Add(typeof(ContentSizeFitter), typeof(PersistentContentSizeFitter));
			m_objToData.Add(typeof(GridLayoutGroup), typeof(PersistentGridLayoutGroup));
			m_objToData.Add(typeof(HorizontalLayoutGroup), typeof(PersistentHorizontalLayoutGroup));
			m_objToData.Add(typeof(HorizontalOrVerticalLayoutGroup), typeof(PersistentHorizontalOrVerticalLayoutGroup));
			m_objToData.Add(typeof(LayoutElement), typeof(PersistentLayoutElement));
			m_objToData.Add(typeof(LayoutGroup), typeof(PersistentLayoutGroup));
			m_objToData.Add(typeof(VerticalLayoutGroup), typeof(PersistentVerticalLayoutGroup));
			m_objToData.Add(typeof(BaseMeshEffect), typeof(PersistentBaseMeshEffect));
			m_objToData.Add(typeof(Outline), typeof(PersistentOutline));
			m_objToData.Add(typeof(PositionAsUV1), typeof(PersistentPositionAsUV1));
			m_objToData.Add(typeof(Shadow), typeof(PersistentShadow));
			m_objToData.Add(typeof(UnityEngine.Object), typeof(PersistentObject));
			m_objToData.Add(typeof(GUIStyle), typeof(PersistentGUIStyle));
			m_objToData.Add(typeof(GUIStyleState), typeof(PersistentGUIStyleState));
			m_objToData.Add(typeof(DetailPrototype), typeof(PersistentDetailPrototype));
			m_objToData.Add(typeof(TreePrototype), typeof(PersistentTreePrototype));
			m_objToData.Add(typeof(SplatPrototype), typeof(PersistentSplatPrototype));
		}

		public virtual void ReadFrom(object obj)
		{
			UnityEngine.Object @object = obj as UnityEngine.Object;
			if (@object != null)
			{
				InstanceId = @object.GetMappedInstanceID();
			}
		}

		public void GetDependencies(object obj, Dictionary<long, UnityEngine.Object> dependencies)
		{
			GetDependencies(dependencies, obj);
		}

		protected virtual void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
		}

		protected void AddDependencies(UnityEngine.Object[] objs, Dictionary<long, UnityEngine.Object> dependencies)
		{
			foreach (UnityEngine.Object obj in objs)
			{
				AddDependency(obj, dependencies);
			}
		}

		protected void AddDependency(UnityEngine.Object obj, Dictionary<long, UnityEngine.Object> dependencies)
		{
			if (!(obj == null))
			{
				long mappedInstanceID = obj.GetMappedInstanceID();
				if (!dependencies.ContainsKey(mappedInstanceID))
				{
					dependencies.Add(mappedInstanceID, obj);
				}
			}
		}

		protected void GetDependencies<T, V>(T[] dst, V[] src, Dictionary<long, UnityEngine.Object> dependencies) where T : PersistentData, new()
		{
			if (src != null)
			{
				if (dst == null)
				{
					dst = new T[src.Length];
				}
				if (dst.Length != src.Length)
				{
					Array.Resize(ref dst, src.Length);
				}
				for (int i = 0; i < src.Length; i++)
				{
					GetDependencies(dst[i], src[i], dependencies);
				}
			}
		}

		protected void GetDependencies<T>(T dst, object obj, Dictionary<long, UnityEngine.Object> dependencies) where T : PersistentData, new()
		{
			if (obj != null)
			{
				if (dst == null)
				{
					dst = new T();
				}
				dst.GetDependencies(dependencies, obj);
			}
		}

		public virtual void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
		}

		protected void AddDependencies<T>(long[] ids, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			T[] array = Resolve<T, T>(ids, objects);
			for (int i = 0; i < ids.Length; i++)
			{
				T val = array[i];
				if (val != null || allowNulls)
				{
					long key = ids[i];
					if (!dependencies.ContainsKey(key))
					{
						dependencies.Add(key, val);
					}
				}
			}
		}

		protected void AddDependency<T>(long id, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			T val = objects.Get(id);
			if ((val != null || allowNulls) && !dependencies.ContainsKey(id))
			{
				dependencies.Add(id, val);
			}
		}

		protected void FindDependencies<T, V>(V data, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls) where V : PersistentData
		{
			data?.FindDependencies(dependencies, objects, allowNulls);
		}

		protected void FindDependencies<T, V>(V[] data, Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls) where V : PersistentData
		{
			if (data != null)
			{
				for (int i = 0; i < data.Length; i++)
				{
					FindDependencies(data[i], dependencies, objects, allowNulls);
				}
			}
		}

		public virtual object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			if (obj is UnityEngine.Object)
			{
				UnityEngine.Object @object = (UnityEngine.Object)obj;
				if (@object == null)
				{
					return null;
				}
			}
			return obj;
		}

		protected T[] Read<T, V>(T[] dst, V[] src) where T : PersistentData, new()
		{
			if (src == null)
			{
				return null;
			}
			if (dst == null)
			{
				dst = new T[src.Length];
			}
			if (dst.Length != src.Length)
			{
				Array.Resize(ref dst, src.Length);
			}
			for (int i = 0; i < dst.Length; i++)
			{
				dst[i] = Read(dst[i], src[i]);
			}
			return dst;
		}

		protected T Read<T>(T dst, object src) where T : PersistentData, new()
		{
			if (src == null)
			{
				return (T)null;
			}
			if (dst == null)
			{
				dst = new T();
			}
			dst.ReadFrom(src);
			return dst;
		}

		protected PersistentUnityEventBase Read(PersistentUnityEventBase dst, object src)
		{
			if (src == null)
			{
				return null;
			}
			if (dst == null)
			{
				dst = new PersistentUnityEventBase();
			}
			dst.ReadFrom((UnityEventBase)src);
			return dst;
		}

		protected T[] Write<T>(T[] dst, PersistentData[] src, Dictionary<long, UnityEngine.Object> objects)
		{
			if (src == null)
			{
				return null;
			}
			if (dst == null)
			{
				dst = new T[src.Length];
			}
			if (dst.Length != src.Length)
			{
				Array.Resize(ref dst, src.Length);
			}
			for (int i = 0; i < dst.Length; i++)
			{
				dst[i] = Write(dst[i], src[i], objects);
			}
			return dst;
		}

		protected T Write<T>(T dst, PersistentUnityEventBase src, Dictionary<long, UnityEngine.Object> objects) where T : UnityEventBase
		{
			if (src == null)
			{
				return (T)null;
			}
			if (dst == null)
			{
				try
				{
					dst = Activator.CreateInstance<T>();
				}
				catch (MissingMethodException)
				{
					Debug.LogWarningFormat("Unable to instantiate object. {0} default constructor missing", typeof(T).FullName);
				}
			}
			src.WriteTo(dst, objects);
			return dst;
		}

		protected T Write<T>(T dst, PersistentData src, Dictionary<long, UnityEngine.Object> objects)
		{
			if (src == null)
			{
				return default(T);
			}
			if (dst == null)
			{
				try
				{
					dst = Activator.CreateInstance<T>();
				}
				catch (MissingMethodException)
				{
					Debug.LogWarningFormat("Unable to instantiate object. {0} default constructor missing", typeof(T).FullName);
				}
			}
			return (T)src.WriteTo(dst, objects);
		}

		protected T2[] Resolve<T2, T1>(long[] ids, Dictionary<long, T1> objects) where T2 : T1
		{
			T2[] array = new T2[ids.Length];
			for (int i = 0; i < ids.Length; i++)
			{
				array[i] = (T2)(object)objects.Get(ids[i]);
			}
			return array;
		}

		public static bool CanCreate(object obj)
		{
			Type type = obj.GetType();
			if (!m_objToData.ContainsKey(type))
			{
				if (type.IsScript())
				{
					do
					{
						type = type.BaseType();
					}
					while (type != null && !m_objToData.ContainsKey(type));
					return true;
				}
				return false;
			}
			return m_objToData.ContainsKey(type);
		}

		public static PersistentData Create(object obj)
		{
			Type type = obj.GetType();
			if (!m_objToData.ContainsKey(type))
			{
				if (type.IsScript())
				{
					do
					{
						type = type.BaseType();
					}
					while (type != null && !m_objToData.ContainsKey(type));
					PersistentData baseObjData = null;
					if (type != null)
					{
						baseObjData = (PersistentData)Activator.CreateInstance(m_objToData[type]);
					}
					return new PersistentScript(baseObjData);
				}
				Debug.Log($"there is no persistent data object for {type}");
				return null;
			}
			return (PersistentData)Activator.CreateInstance(m_objToData[type]);
		}

		public static void RestoreDataAndResolveDependencies(PersistentData[] dataObjects, Dictionary<long, UnityEngine.Object> objects)
		{
			Dictionary<UnityEngine.Object, PersistentData> dictionary = new Dictionary<UnityEngine.Object, PersistentData>();
			foreach (PersistentData persistentData in dataObjects)
			{
				UnityEngine.Object value;
				if (objects.TryGetValue(persistentData.InstanceId, out value))
				{
					dictionary.Add(value, persistentData);
				}
			}
			foreach (KeyValuePair<UnityEngine.Object, PersistentData> item in dictionary)
			{
				PersistentIgnore persistentIgnore = item.Key as PersistentIgnore;
				if (!(persistentIgnore == null))
				{
					GameObject gameObject = persistentIgnore.gameObject;
					PersistentData persistentData2 = dictionary[gameObject];
					PersistentData value2 = item.Value;
					PersistentData persistentData3 = dictionary[gameObject.transform];
					persistentData2.WriteTo(gameObject, objects);
					value2.WriteTo(persistentIgnore, objects);
					persistentData3.WriteTo(gameObject.transform, objects);
				}
			}
			List<GameObject> list = new List<GameObject>();
			List<bool> list2 = new List<bool>();
			foreach (PersistentData persistentData4 in dataObjects)
			{
				if (!objects.ContainsKey(persistentData4.InstanceId))
				{
					Debug.LogWarningFormat("objects does not have object with instance id {0} however PersistentData of type {1} is present", persistentData4.InstanceId, persistentData4.GetType());
					continue;
				}
				UnityEngine.Object @object = objects[persistentData4.InstanceId];
				persistentData4.WriteTo(@object, objects);
				if (@object is GameObject)
				{
					list.Add((GameObject)@object);
					list2.Add(persistentData4.ActiveSelf);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				bool active = list2[k];
				GameObject gameObject2 = list[k];
				gameObject2.SetActive(active);
			}
		}

		public static void CreatePersistentDescriptorsAndData(GameObject[] gameObjects, out PersistentDescriptor[] descriptors, out PersistentData[] data)
		{
			List<PersistentData> list = new List<PersistentData>();
			List<PersistentDescriptor> list2 = new List<PersistentDescriptor>();
			foreach (GameObject go in gameObjects)
			{
				PersistentDescriptor persistentDescriptor = PersistentDescriptor.CreateDescriptor(go);
				if (persistentDescriptor != null)
				{
					list2.Add(persistentDescriptor);
				}
				CreatePersistentData(go, list);
			}
			descriptors = list2.ToArray();
			data = list.ToArray();
		}

		public static void CreatePersistentDescriptorsAndData(GameObject[] gameObjects, out PersistentDescriptor[] descriptors, out PersistentData[][] data)
		{
			List<PersistentData[]> list = new List<PersistentData[]>();
			List<PersistentDescriptor> list2 = new List<PersistentDescriptor>();
			foreach (GameObject go in gameObjects)
			{
				PersistentDescriptor persistentDescriptor = PersistentDescriptor.CreateDescriptor(go);
				if (persistentDescriptor != null)
				{
					list2.Add(persistentDescriptor);
				}
				List<PersistentData> list3 = new List<PersistentData>();
				CreatePersistentData(go, list3);
				list.Add(list3.ToArray());
			}
			descriptors = list2.ToArray();
			data = list.ToArray();
		}

		public static PersistentData[] CreatePersistentData(UnityEngine.Object[] objects)
		{
			List<PersistentData> list = new List<PersistentData>();
			foreach (UnityEngine.Object @object in objects)
			{
				if (!(@object == null))
				{
					PersistentData persistentData = Create(@object);
					if (persistentData != null)
					{
						persistentData.ReadFrom(@object);
						list.Add(persistentData);
					}
				}
			}
			return list.ToArray();
		}

		private static void CreatePersistentData(GameObject go, List<PersistentData> data)
		{
			PersistentIgnore component = go.GetComponent<PersistentIgnore>();
			if (component != null)
			{
				return;
			}
			PersistentData persistentData = Create(go);
			if (persistentData != null)
			{
				persistentData.ActiveSelf = go.activeSelf;
				persistentData.ReadFrom(go);
				data.Add(persistentData);
			}
			Component[] array;
			if (component == null)
			{
				array = (from c in go.GetComponents<Component>()
					where c != null && !PersistentDescriptor.IgnoreTypes.Contains(c.GetType())
					select c).ToArray();
			}
			else
			{
				array = go.GetComponents<Transform>();
				Array.Resize(ref array, array.Length + 1);
				array[array.Length - 1] = component;
			}
			foreach (Component obj in array)
			{
				PersistentData persistentData2 = Create(obj);
				if (persistentData2 != null)
				{
					persistentData2.ReadFrom(obj);
					data.Add(persistentData2);
				}
			}
			Transform transform = go.transform;
			foreach (Transform item in transform)
			{
				if (component == null)
				{
					CreatePersistentData(item.gameObject, data);
				}
			}
		}

		public static void RegisterPersistentType<T, TPersistent>() where TPersistent : PersistentData
		{
			m_objToData.Add(typeof(T), typeof(TPersistent));
		}
	}
}
