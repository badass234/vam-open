namespace Obi
{
	public interface IObiSolverClient
	{
		bool AddToSolver(object info);

		bool RemoveFromSolver(object info);

		void PushDataToSolver(ParticleData data = ParticleData.NONE);

		void PullDataFromSolver(ParticleData data = ParticleData.NONE);
	}
}
