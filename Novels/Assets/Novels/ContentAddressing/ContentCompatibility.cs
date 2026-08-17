namespace Novels.ContentAddressing
{
    public static class ContentCompatibility
    {
        public const int MinimumSupportedSchemaVersion = 4;
        public const int MaximumSupportedSchemaVersion = 4;

        public static bool Supports(int schemaVersion) =>
            schemaVersion >= MinimumSupportedSchemaVersion
            && schemaVersion <= MaximumSupportedSchemaVersion;
    }
}
