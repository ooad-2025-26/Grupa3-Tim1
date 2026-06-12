using BMDb.Models;

namespace BMDb.Services
{
    // Factory Method pattern: new media types can be added by extending this creation point.
    public class MediaFactory
    {
        public Entertainment CreateMedia(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "film" => new Film(),
                "serija" => new Serija(),
                _ => throw new ArgumentException("Nepodržan tip medija.", nameof(type))
            };
        }
    }
}
