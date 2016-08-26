using NinjaProviders.TransportTypes;

namespace BCNExplorer.Web.Models
{
    public class LastBlockViewModel
    {
        public string BlockId { get; set;}
        public double Height { get; set; }

        public static LastBlockViewModel Create(NinjaBlockHeader header)
        {
            return new LastBlockViewModel
            {
                BlockId = header.Hash,
                Height = header.Height
            };
        }
    }
}