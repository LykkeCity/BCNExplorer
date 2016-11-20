namespace Core.AssetBlockChanges
{

    public interface IAssetChangesContext
    {
        int BlockHeight { get; }
        string AssetId { get; }
        string BlockHash { get; }
        string AddressId { get; }
        
    }

    public interface IAssetChangesContextRepository
    {
    }
}
