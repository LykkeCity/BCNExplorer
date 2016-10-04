using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.GrabBlockTask;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.GrabBlockTask
{
    public class PendingGrabBlockCommandEntity : TableEntity, IPendingGrabBlockCommand
    {
        public static string GenerateRowKey(string blockId)
        {
            return blockId;
        }

        public static string GeneratePartitionKey()
        {
            return "PGBT";
        }

        public string BlockId { get; set; }

        public static PendingGrabBlockCommandEntity Create(string blockId)
        {
            return new PendingGrabBlockCommandEntity
            {
                PartitionKey = GenerateRowKey(blockId),
                RowKey = GeneratePartitionKey(),
                BlockId = blockId
            };
        }
    }

    public class GrabBlockFailedResultEntity : TableEntity, IGrabBlockFailedResult
    {
        public static string GeneratePartitionKey(string blockId)
        {
            return blockId;
        }

        public static string GenerateRowKey()
        {
            return IdGenerator.GenerateDateTimeId(DateTime.Now);
        }

        public string BlockId { get; set; }

        public static GrabBlockFailedResultEntity Create(string blockId, string failDescr)
        {
            return new GrabBlockFailedResultEntity
            {
                PartitionKey = GeneratePartitionKey(blockId),
                RowKey = GenerateRowKey(),
                BlockId = blockId,
                FailDescription = failDescr
            };
        }
        
        public string FailDescription { get; set; }
    }

    public class GrabBlockDoneResultEntity : TableEntity, IDoneGrabBlockResult
    {
        public static string GenerateRowKey(string blockId)
        {
            return blockId;
        }

        public static string GeneratePartitionKey()
        {
            return "DGBT";
        }

        public string BlockId { get; set; }

        public static GrabBlockDoneResultEntity Create(string blockId)
        {
            return new GrabBlockDoneResultEntity
            {
                PartitionKey = GenerateRowKey(blockId),
                RowKey = GeneratePartitionKey(),
                BlockId = blockId
            };
        }
    }

    public class GrabBlockCommandsRepository: IGrabBlockCommandsRepository
    {
        private readonly INoSQLTableStorage<PendingGrabBlockCommandEntity> _commandRepository;

        private readonly INoSQLTableStorage<GrabBlockFailedResultEntity> _failedResultRepository;

        private readonly INoSQLTableStorage<GrabBlockDoneResultEntity> _doneResultRepository;

        private readonly BaseSettings _baseSettings;

        public GrabBlockCommandsRepository(INoSQLTableStorage<PendingGrabBlockCommandEntity> commandRepository, 
            INoSQLTableStorage<GrabBlockFailedResultEntity> failedResultRepository, 
            INoSQLTableStorage<GrabBlockDoneResultEntity> doneResultRepository, 
            BaseSettings baseSettings)
        {
            _commandRepository = commandRepository;
            _failedResultRepository = failedResultRepository;
            _doneResultRepository = doneResultRepository;
            _baseSettings = baseSettings;
        }


        public async Task CreateGrabCommand(IPendingGrabBlockCommand pendingCommand)
        {
            await _commandRepository.InsertOrMergeAsync(PendingGrabBlockCommandEntity.Create(pendingCommand.BlockId));
        }

        public async Task SetGrabResultDone(IDoneGrabBlockResult doneResult)
        {
            await Task.WhenAll(
                _commandRepository.DeleteAsync(PendingGrabBlockCommandEntity.Create(doneResult.BlockId)),
                _doneResultRepository.InsertOrMergeAsync(GrabBlockDoneResultEntity.Create(doneResult.BlockId)));
        }

        public async Task SetGrabResultFailed(IGrabBlockFailedResult grabBlockFailedResult)
        {
            var removeCmdTask = Task.Run(async () =>
            {
                var attempts = await _failedResultRepository.GetDataAsync(GrabBlockFailedResultEntity.GeneratePartitionKey(grabBlockFailedResult.BlockId));
                if (attempts.Count() >= _baseSettings.Jobs.MaxGrabTransactionAttemptCount)
                {
                    await _commandRepository.DeleteAsync(PendingGrabBlockCommandEntity.Create(grabBlockFailedResult.BlockId));
                }
            });

            var addFailResultTask =
                _failedResultRepository.InsertAsync(GrabBlockFailedResultEntity.Create(grabBlockFailedResult.BlockId,
                    grabBlockFailedResult.FailDescription));

            await Task.WhenAll(removeCmdTask, addFailResultTask);
        }

        public async Task<IEnumerable<IPendingGrabBlockCommand>> GetAllPendingCommands()
        {
            return await _commandRepository.GetDataAsync(PendingGrabBlockCommandEntity.GeneratePartitionKey());
        }
    }
}
