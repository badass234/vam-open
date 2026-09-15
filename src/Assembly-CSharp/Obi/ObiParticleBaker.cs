using System;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ObiSolver))]
	public class ObiParticleBaker : MonoBehaviour
	{
		public ObiParticleCache cache;

		public float playhead;

		public int frameSkip = 8;

		public int fixedBakeFramerate = 60;

		public bool interpolate = true;

		public bool loopPlayback = true;

		public bool bakeOnAwake;

		public bool playOnAwake;

		private bool baking;

		private bool playing;

		private bool paused;

		private int framesToSkip;

		private ObiSolver solver;

		private ObiParticleCache.Frame frame = new ObiParticleCache.Frame();

		public bool Baking
		{
			get
			{
				return baking;
			}
			set
			{
				baking = value;
				if (baking)
				{
					Time.captureFramerate = Mathf.Max(0, fixedBakeFramerate);
					playing = false;
					solver.simulate = true;
					solver.RequireRenderablePositions();
				}
				else
				{
					framesToSkip = 0;
					Time.captureFramerate = 0;
					solver.RelinquishRenderablePositions();
				}
			}
		}

		public bool Playing
		{
			get
			{
				return playing;
			}
			set
			{
				playing = value;
				solver.simulate = !playing;
				if (playing)
				{
					baking = false;
				}
			}
		}

		public bool Paused
		{
			get
			{
				return paused;
			}
			set
			{
				paused = value;
			}
		}

		private void Awake()
		{
			solver = GetComponent<ObiSolver>();
			if (Application.isPlaying)
			{
				if (bakeOnAwake)
				{
					playhead = 0f;
					Baking = true;
				}
				else if (playOnAwake)
				{
					playhead = 0f;
					Playing = true;
				}
			}
		}

		private void OnEnable()
		{
			solver.OnFrameEnd += Solver_OnFrameEnd;
			solver.OnBeforeActorsFrameEnd += Solver_OnBeforeActorsFrameEnd;
		}

		private void OnDisable()
		{
			Baking = false;
			solver.OnFrameEnd -= Solver_OnFrameEnd;
			solver.OnBeforeActorsFrameEnd -= Solver_OnBeforeActorsFrameEnd;
		}

		private void Solver_OnFrameEnd(object sender, EventArgs e)
		{
			if (cache != null && !playing && baking)
			{
				playhead += Time.deltaTime;
				if (framesToSkip <= 0)
				{
					BakeFrame(playhead);
					framesToSkip = frameSkip;
				}
				else
				{
					framesToSkip--;
				}
			}
		}

		private void Solver_OnBeforeActorsFrameEnd(object sender, EventArgs e)
		{
			if (!(cache != null) || !playing)
			{
				return;
			}
			if (!paused)
			{
				playhead += Time.deltaTime;
				if (loopPlayback)
				{
					playhead = ((cache.Duration != 0f) ? (playhead % cache.Duration) : 0f);
				}
				else if (playhead > cache.Duration)
				{
					playhead = cache.Duration;
				}
			}
			PlaybackFrame(playhead);
		}

		public void BakeFrame(float time)
		{
			if (cache == null)
			{
				return;
			}
			ObiParticleCache.Frame frame = new ObiParticleCache.Frame();
			frame.time = time;
			for (int i = 0; i < solver.renderablePositions.Length; i++)
			{
				ObiSolver.ParticleInActor particleInActor = solver.particleToActor[i];
				if (particleInActor != null && particleInActor.actor.active[particleInActor.indexInActor])
				{
					frame.indices.Add(i);
					if (cache.localSpace)
					{
						frame.positions.Add(solver.transform.InverseTransformPoint(solver.renderablePositions[i]));
					}
					else
					{
						frame.positions.Add(solver.renderablePositions[i]);
					}
				}
			}
			cache.AddFrame(frame);
		}

		private void PlaybackFrame(float time)
		{
			if (cache == null || cache.Duration == 0f)
			{
				return;
			}
			cache.GetFrame(time, interpolate, ref frame);
			if (solver.AllocParticleCount < frame.indices.Count)
			{
				Debug.LogError("The ObiSolver doesn't have enough allocated particles to playback this cache.");
				Playing = false;
				return;
			}
			Matrix4x4 matrix4x = ((!cache.localSpace) ? Matrix4x4.identity : solver.transform.localToWorldMatrix);
			for (int i = 0; i < frame.indices.Count; i++)
			{
				if (frame.indices[i] >= 0 && frame.indices[i] < solver.renderablePositions.Length)
				{
					solver.renderablePositions[frame.indices[i]] = matrix4x.MultiplyPoint3x4(frame.positions[i]);
				}
			}
			Oni.SetParticlePositions(solver.OniSolver, solver.renderablePositions, solver.renderablePositions.Length, 0);
			solver.UpdateActiveParticles();
		}
	}
}
