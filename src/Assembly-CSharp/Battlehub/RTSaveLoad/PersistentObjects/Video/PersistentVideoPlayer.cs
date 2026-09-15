using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Video;

namespace Battlehub.RTSaveLoad.PersistentObjects.Video
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentVideoPlayer : PersistentBehaviour
	{
		public uint source;

		public string url;

		public long clip;

		public uint renderMode;

		public long targetCamera;

		public long targetTexture;

		public long targetMaterialRenderer;

		public string targetMaterialProperty;

		public uint aspectRatio;

		public float targetCameraAlpha;

		public bool waitForFirstFrame;

		public bool playOnAwake;

		public double time;

		public long frame;

		public float playbackSpeed;

		public bool isLooping;

		public uint timeSource;

		public bool skipOnDrop;

		public ushort controlledAudioTrackCount;

		public uint audioOutputMode;

		public bool sendFrameReadyEvents;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			VideoPlayer videoPlayer = (VideoPlayer)obj;
			videoPlayer.source = (VideoSource)source;
			videoPlayer.url = url;
			videoPlayer.clip = (VideoClip)objects.Get(clip);
			videoPlayer.renderMode = (VideoRenderMode)renderMode;
			videoPlayer.targetCamera = (Camera)objects.Get(targetCamera);
			videoPlayer.targetTexture = (RenderTexture)objects.Get(targetTexture);
			videoPlayer.targetMaterialRenderer = (Renderer)objects.Get(targetMaterialRenderer);
			videoPlayer.targetMaterialProperty = targetMaterialProperty;
			videoPlayer.aspectRatio = (VideoAspectRatio)aspectRatio;
			videoPlayer.targetCameraAlpha = targetCameraAlpha;
			videoPlayer.waitForFirstFrame = waitForFirstFrame;
			videoPlayer.playOnAwake = playOnAwake;
			videoPlayer.time = time;
			videoPlayer.frame = frame;
			videoPlayer.playbackSpeed = playbackSpeed;
			videoPlayer.isLooping = isLooping;
			videoPlayer.timeSource = (VideoTimeSource)timeSource;
			videoPlayer.skipOnDrop = skipOnDrop;
			videoPlayer.controlledAudioTrackCount = controlledAudioTrackCount;
			videoPlayer.audioOutputMode = (VideoAudioOutputMode)audioOutputMode;
			videoPlayer.sendFrameReadyEvents = sendFrameReadyEvents;
			return videoPlayer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				VideoPlayer videoPlayer = (VideoPlayer)obj;
				source = (uint)videoPlayer.source;
				url = videoPlayer.url;
				clip = videoPlayer.clip.GetMappedInstanceID();
				renderMode = (uint)videoPlayer.renderMode;
				targetCamera = videoPlayer.targetCamera.GetMappedInstanceID();
				targetTexture = videoPlayer.targetTexture.GetMappedInstanceID();
				targetMaterialRenderer = videoPlayer.targetMaterialRenderer.GetMappedInstanceID();
				targetMaterialProperty = videoPlayer.targetMaterialProperty;
				aspectRatio = (uint)videoPlayer.aspectRatio;
				targetCameraAlpha = videoPlayer.targetCameraAlpha;
				waitForFirstFrame = videoPlayer.waitForFirstFrame;
				playOnAwake = videoPlayer.playOnAwake;
				time = videoPlayer.time;
				frame = videoPlayer.frame;
				playbackSpeed = videoPlayer.playbackSpeed;
				isLooping = videoPlayer.isLooping;
				timeSource = (uint)videoPlayer.timeSource;
				skipOnDrop = videoPlayer.skipOnDrop;
				controlledAudioTrackCount = videoPlayer.controlledAudioTrackCount;
				audioOutputMode = (uint)videoPlayer.audioOutputMode;
				sendFrameReadyEvents = videoPlayer.sendFrameReadyEvents;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(clip, dependencies, objects, allowNulls);
			AddDependency(targetCamera, dependencies, objects, allowNulls);
			AddDependency(targetTexture, dependencies, objects, allowNulls);
			AddDependency(targetMaterialRenderer, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				VideoPlayer videoPlayer = (VideoPlayer)obj;
				AddDependency(videoPlayer.clip, dependencies);
				AddDependency(videoPlayer.targetCamera, dependencies);
				AddDependency(videoPlayer.targetTexture, dependencies);
				AddDependency(videoPlayer.targetMaterialRenderer, dependencies);
			}
		}
	}
}
