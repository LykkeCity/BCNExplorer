using System.Threading.Tasks;

namespace Core.AssetBlockChanges
{
    public interface IBlock
    {
        int Height { get; set; }
        string Hash { get; set; }
    }

    public class Block: IBlock
    {
        public int Height { get; set; }
        public string Hash { get; set; }
    }

    public interface IBlockRepository
    {
        Task AddAsync(IBlock[] blocks);
    }
}
