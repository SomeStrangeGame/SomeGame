namespace Novels.ContentAddressing
{
    public static class ContentCompatibility
    {
        public const int MinimumSupportedSchemaVersion = 5;
        public const int MaximumSupportedSchemaVersion = 5;

        public static bool Supports(int schemaVersion) =>
            schemaVersion >= MinimumSupportedSchemaVersion
            && schemaVersion <= MaximumSupportedSchemaVersion;
    }
}
