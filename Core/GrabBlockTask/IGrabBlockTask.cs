using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.GrabBlockTask
{
    public interface IPendingGrabBlockCommand 
    {
        string BlockId { get; }
    }

    public interface IDoneGrabBlockResult 
    {
        string BlockId { get; }
    }

    public interface IGrabBlockFailedResult 
    {
        string FailDescription { get; }
        string BlockId { get; }
    }

    public abstract class BaseGrabBlockTask 
    {
        public string BlockId { get; set; }

    }

    public class PendingGrabBlockCommand : BaseGrabBlockTask, IPendingGrabBlockCommand
    {
        public static PendingGrabBlockCommand Create(string blockId)
        {
            return new PendingGrabBlockCommand
            {
                BlockId = blockId
            };
        }
    }

    public class DoneGrabBlockResult : BaseGrabBlockTask, IDoneGrabBlockResult
    {
        public static DoneGrabBlockResult Create(string blockId)
        {
            return new DoneGrabBlockResult
            {
                BlockId = blockId
            };
        }
    }

    public class GrabBlockFailedResult : BaseGrabBlockTask, IGrabBlockFailedResult
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

    public interface IGrabBlockCommandsRepository
    {
        Task CreateGrabCommand(IPendingGrabBlockCommand pendingCommand);
        Task SetGrabResultDone(IDoneGrabBlockResult doneResult);
        Task SetGrabResultFailed(IGrabBlockFailedResult grabBlockFailedResult);
        Task<IEnumerable<IPendingGrabBlockCommand>> GetAllPendingCommands();
    }
}
