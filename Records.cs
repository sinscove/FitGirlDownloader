using System.Collections.Generic;

namespace FitGirlDownloader
{
    public record GameInfo
    {
        public string PageUrl     { get; init; }
        public string Description { get; init; }
        public IEnumerable<Mirror> Mirrors { get; init; }
    }

    public record Mirror
    {
        public string MirrorName { get; init; }
        public string MirrorUrl  { get; init; }
    }
}
