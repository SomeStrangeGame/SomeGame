namespace Novels.Content
{
    public static class ContentDeliveryGroupConvention
    {
        public static string Shared(string contentId) => $"{contentId}/shared";

        public static string Episode(string contentId, string episodeId) =>
            $"{contentId}/{episodeId}";
    }
}
