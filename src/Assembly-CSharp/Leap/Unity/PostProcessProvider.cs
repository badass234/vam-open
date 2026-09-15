using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public abstract class PostProcessProvider : LeapProvider
	{
		public enum DataUpdateMode
		{
			UpdateOnly,
			FixedUpdateOnly,
			UpdateAndFixedUpdate
		}

		[Tooltip("The LeapProvider whose output hand data will be copied, modified, and output by this post-processing provider.")]
		[SerializeField]
		[OnEditorChange("inputLeapProvider")]
		protected LeapProvider _inputLeapProvider;

		[Tooltip("Whether this post-processing provider should process data received from Update frames, FixedUpdate frames, or both. Processing both kinds of frames is only recommended if your post-process is stateless.")]
		public DataUpdateMode dataUpdateMode;

		[Tooltip("When this setting is enabled, frame data is passed from this provider's input directly to its output without performing any post-processing.")]
		public bool passthroughOnly;

		private Frame _cachedUpdateFrame = new Frame();

		private Frame _cachedFixedFrame = new Frame();

		public LeapProvider inputLeapProvider
		{
			get
			{
				return _inputLeapProvider;
			}
			set
			{
				if (Application.isPlaying && _inputLeapProvider != null)
				{
					_inputLeapProvider.OnFixedFrame -= processFixedFrame;
					_inputLeapProvider.OnUpdateFrame -= processUpdateFrame;
				}
				_inputLeapProvider = value;
				validateInput();
				if (Application.isPlaying && _inputLeapProvider != null)
				{
					_inputLeapProvider.OnFixedFrame -= processFixedFrame;
					_inputLeapProvider.OnFixedFrame += processFixedFrame;
					_inputLeapProvider.OnUpdateFrame -= processUpdateFrame;
					_inputLeapProvider.OnUpdateFrame += processUpdateFrame;
				}
			}
		}

		public override Frame CurrentFrame => _cachedUpdateFrame;

		public override Frame CurrentFixedFrame => _cachedFixedFrame;

		protected virtual void OnEnable()
		{
			inputLeapProvider = _inputLeapProvider;
		}

		protected virtual void OnValidate()
		{
			validateInput();
		}

		public abstract void ProcessFrame(ref Frame inputFrame);

		private void validateInput()
		{
			if (detectCycle())
			{
				_inputLeapProvider = null;
				Debug.LogError("The input to the post-process provider on " + base.gameObject.name + " causes an infinite cycle, so its input has been set to null.");
			}
		}

		private bool detectCycle()
		{
			LeapProvider leapProvider = _inputLeapProvider;
			LeapProvider leapProvider2 = _inputLeapProvider;
			while (leapProvider is PostProcessProvider)
			{
				leapProvider2 = (leapProvider2 as PostProcessProvider).inputLeapProvider;
				if (leapProvider == leapProvider2)
				{
					return true;
				}
				if (!(leapProvider2 is PostProcessProvider))
				{
					return false;
				}
				leapProvider = (leapProvider as PostProcessProvider).inputLeapProvider;
				leapProvider2 = (leapProvider2 as PostProcessProvider).inputLeapProvider;
				if (!(leapProvider2 is PostProcessProvider))
				{
					return false;
				}
			}
			return false;
		}

		private void processUpdateFrame(Frame inputFrame)
		{
			if (dataUpdateMode != DataUpdateMode.FixedUpdateOnly)
			{
				_cachedUpdateFrame.CopyFrom(inputFrame);
				if (!passthroughOnly)
				{
					ProcessFrame(ref _cachedUpdateFrame);
				}
				DispatchUpdateFrameEvent(_cachedUpdateFrame);
			}
		}

		private void processFixedFrame(Frame inputFrame)
		{
			if (dataUpdateMode != 0)
			{
				_cachedFixedFrame.CopyFrom(inputFrame);
				if (!passthroughOnly)
				{
					ProcessFrame(ref _cachedFixedFrame);
				}
				DispatchFixedFrameEvent(_cachedFixedFrame);
			}
		}
	}
}
