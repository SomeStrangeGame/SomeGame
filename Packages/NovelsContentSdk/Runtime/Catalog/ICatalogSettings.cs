namespace Novels.Catalog
{
    // The application owns preferences; the bundled catalog only presents them.
    public interface ICatalogSettings
    {
        float Volume { get; }
        void SetVolume(float volume);
        void Save();
    }
}
