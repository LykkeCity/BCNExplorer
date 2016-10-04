using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.GrabTransactionTask
{
    public interface IGrabBlockCommand 
    {
        string BlockId { get; }
    }

    public interface IGrabBlockDoneResult 
    {
        string BlockId { get; }
    }

    public interface IGrabBlockFailedResult 
    {
        string FailDescription { get; }
        string BlockId { get; }
    }

    public abstract class BaseGrabTransactionTask 
    {
        public string BlockId { get; set; }

    }

    public class GrabBlockCommand : BaseGrabTransactionTask, IGrabBlockCommand
    {
        public static GrabBlockCommand Create(string blockId)
        {
            return new GrabBlockCommand
            {
                BlockId = blockId
            };
        }
    }

    public class GrabBlockDoneResult : BaseGrabTransactionTask, IGrabBlockDoneResult
    {
        public static GrabBlockDoneResult Create(string blockId)
        {
            return new GrabBlockDoneResult
            {
                BlockId = blockId
            };
        }
    }

    public class GrabBlockFailedResult : BaseGrabTransactionTask, IGrabBlockFailedResult
    {

        public static GrabBlockFailedResult Create(string blockId, string failDescr)
        {
            return new GrabBlockFailedResult
            {
                BlockId = blockId,
                FailDescription = failDescr
            };
        }

        public string FailDescription { get; set; }
    }

    public interface IGrabBlockDirectorBase
    {
        Task CreateGrabCommandAsync(IGrabBlockCommand command);
        Task SetGrabResultDoneAsync(IGrabBlockDoneResult grabBlockDoneResult);
        Task SetGrabResultFailedAsync(IGrabBlockFailedResult grabBlockFailedResult);
        Task<IEnumerable<IGrabBlockCommand>> GetAllPendingCommandsAsync();
        bool NeedToAddToProccessing(string blockId);
    }

    public interface IGrabNewAssetsDirector: IGrabBlockDirectorBase
    {
        
    }
}
