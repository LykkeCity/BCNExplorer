using Core.Block;

namespace BCNExplorer.Web.Models
{
    public class LastBlockViewModel
    {
        public string BlockId { get; set;}
        public int Height { get; set; }

        public static LastBlockViewModel Create(IBlockHeader header)
        {
            return new LastBlockViewModel
            {
                BlockId = header.Hash,
                Height = header.Height
            };
        }
    }
}