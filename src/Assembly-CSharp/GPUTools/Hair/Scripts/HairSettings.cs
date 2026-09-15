using GPUTools.Hair.Scripts.Runtime.Commands;
using GPUTools.Hair.Scripts.Runtime.Data;
using GPUTools.Hair.Scripts.Settings;
using UnityEngine;

namespace GPUTools.Hair.Scripts
{
	public class HairSettings : GPUCollidersConsumer
	{
		public HairStandsSettings StandsSettings = new HairStandsSettings();

		public HairPhysicsSettings PhysicsSettings = new HairPhysicsSettings();

		public HairRenderSettings RenderSettings = new HairRenderSettings();

		public HairLODSettings LODSettings = new HairLODSettings();

		public HairShadowSettings ShadowSettings = new HairShadowSettings();

		protected bool consumerRegistered;

		public RuntimeData RuntimeData { get; private set; }

		public BuildRuntimeHair HairBuidCommand { get; private set; }

		private void Awake()
		{
			if (ValidateImpl())
			{
				RuntimeData = new RuntimeData();
				HairBuidCommand = new BuildRuntimeHair(this);
				HairBuidCommand.Build();
			}
		}

		public void ReStart()
		{
			if (ValidateImpl())
			{
				if (HairBuidCommand != null)
				{
					HairBuidCommand.Dispose();
				}
				Awake();
			}
		}

		private void FixedUpdate()
		{
			if (HairBuidCommand != null)
			{
				SyncConsumer();
				HairBuidCommand.FixedDispatch();
			}
		}

		private void LateUpdate()
		{
			if (HairBuidCommand != null)
			{
				StandsSettings.Provider.Dispatch();
				HairBuidCommand.Dispatch();
			}
		}

		public void UpdateSettings()
		{
			if (HairBuidCommand != null && Application.isPlaying)
			{
				HairBuidCommand.UpdateSettings();
			}
		}

		public void OnDestroy()
		{
			if (HairBuidCommand != null)
			{
				HairBuidCommand.Dispose();
			}
		}

		private bool ValidateImpl()
		{
			return StandsSettings.Validate() && PhysicsSettings.Validate() && RenderSettings.Validate() && LODSettings.Validate() && ShadowSettings.Validate();
		}

		private void OnDrawGizmos()
		{
			StandsSettings.DrawGizmos();
			PhysicsSettings.DrawGizmos();
			RenderSettings.DrawGizmos();
			LODSettings.DrawGizmos();
			ShadowSettings.DrawGizmos();
		}

		protected void SyncConsumer()
		{
			if (PhysicsSettings.IsEnabled && !consumerRegistered)
			{
				GPUCollidersManager.RegisterConsumer(this);
				consumerRegistered = true;
			}
			else if (!PhysicsSettings.IsEnabled && consumerRegistered)
			{
				GPUCollidersManager.DeregisterConsumer(this);
				consumerRegistered = false;
			}
		}

		protected override void OnEnable()
		{
			if (Application.isPlaying)
			{
				SyncConsumer();
			}
		}

		protected override void OnDisable()
		{
			if (Application.isPlaying && consumerRegistered)
			{
				GPUCollidersManager.DeregisterConsumer(this);
				consumerRegistered = false;
			}
		}
	}
}
