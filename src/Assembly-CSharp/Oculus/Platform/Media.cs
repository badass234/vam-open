using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Media
	{
		public static Request<ShareMediaResult> ShareToFacebook(string postTextSuggestion, string filePath, MediaContentType contentType)
		{
			if (Core.IsInitialized())
			{
				return new Request<ShareMediaResult>(CAPI.ovr_Media_ShareToFacebook(postTextSuggestion, filePath, contentType));
			}
			return null;
		}
	}
}
