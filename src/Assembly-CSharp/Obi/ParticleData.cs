using System;

namespace Obi
{
	[Flags]
	public enum ParticleData
	{
		NONE = 0,
		ACTIVE_STATUS = 1,
		ACTOR_ID = 2,
		POSITIONS = 4,
		VELOCITIES = 8,
		INV_MASSES = 0x10,
		VORTICITIES = 0x20,
		SOLID_RADII = 0x40,
		PHASES = 0x80,
		REST_POSITIONS = 0x100,
		COLLISION_MATERIAL = 0x200,
		ALL = -1
	}
}
