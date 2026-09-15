using System.ComponentModel;

namespace Oculus.Platform
{
	public enum CloudStorageDataStatus
	{
		[Description("UNKNOWN")]
		Unknown,
		[Description("IN_SYNC")]
		InSync,
		[Description("NEEDS_DOWNLOAD")]
		NeedsDownload,
		[Description("REMOTE_DOWNLOADING")]
		RemoteDownloading,
		[Description("NEEDS_UPLOAD")]
		NeedsUpload,
		[Description("LOCAL_UPLOADING")]
		LocalUploading,
		[Description("IN_CONFLICT")]
		InConflict
	}
}
