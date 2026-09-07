using Retro.Core.Data;

namespace Retro.Infrastructure.Database;

public interface IMediaRepository
{
    public Task UpsertMediaAsync(Media media);
    public Task<Media> ReadMediaAsync(string id);
    public Task DeleteMediaAsync(string id);
}