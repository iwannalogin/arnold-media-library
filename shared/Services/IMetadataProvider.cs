using arnold.Models;

namespace arnold.Services;

public interface IMetadataProvider {
    public (string name, string description)[] TargetAttributes { get; }

    public Task SetAttributesAsync( FileMetadata file );

    public Task AddTagsAsync( FileMetadata file );
}