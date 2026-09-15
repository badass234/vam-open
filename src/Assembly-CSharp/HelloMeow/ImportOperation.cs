using UnityEngine;

namespace HelloMeow
{
	public class ImportOperation : CustomYieldInstruction
	{
		public override bool keepWaiting => !importer.isLoaded && !importer.isError;

		public AudioImporter importer { get; private set; }

		public ImportOperation(AudioImporter importer)
		{
			this.importer = importer;
		}
	}
}
