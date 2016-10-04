using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.GrabTransactionTask;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Grab
{
    public class GrabBlockCommandEntity : TableEntity, IGrabBlockCommand
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

        public static GrabBlockCommandEntity Create(string blockId)
        {
            return new GrabBlockCommandEntity
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

    public class GrabBlockDoneResultEntity : TableEntity, IGrabBlockDoneResult
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

    public abstract class GrabBlockDirectorBase: IGrabBlockDirectorBase
    {
        private readonly INoSQLTableStorage<GrabBlockCommandEntity> _commandRepository;
        private readonly INoSQLTableStorage<GrabBlockFailedResultEntity> _failedResultRepository;
        private readonly INoSQLTableStorage<GrabBlockDoneResultEntity> _doneResultRepository;
        private readonly int _attemptLimit;

        public GrabBlockDirectorBase(INoSQLTableStorage<GrabBlockCommandEntity> commandRepository, 
            INoSQLTableStorage<GrabBlockFailedResultEntity> failedResultRepository, 
            INoSQLTableStorage<GrabBlockDoneResultEntity> doneResultRepository, int attemptLimit)
        {
            _commandRepository = commandRepository;
            _failedResultRepository = failedResultRepository;
            _doneResultRepository = doneResultRepository;
            _attemptLimit = attemptLimit;
        }


        public async Task CreateGrabCommandAsync(IGrabBlockCommand command)
        {
            await _commandRepository.InsertOrMergeAsync(GrabBlockCommandEntity.Create(command.BlockId));
        }

        public async Task SetGrabResultDoneAsync(IGrabBlockDoneResult grabBlockDoneResult)
        {
            await Task.WhenAll(
                _commandRepository.DeleteAsync(GrabBlockCommandEntity.Create(grabBlockDoneResult.BlockId)),
                _doneResultRepository.InsertOrMergeAsync(GrabBlockDoneResultEntity.Create(grabBlockDoneResult.BlockId)));
        }

        public async Task SetGrabResultFailedAsync(IGrabBlockFailedResult grabBlockFailedResult)
        {
            var removeCmdTask = Task.Run(async () =>
            {
                if (await IsAttemptLimitReached(grabBlockFailedResult.BlockId))
                {
                    await _commandRepository.DeleteAsync(GrabBlockCommandEntity.Create(grabBlockFailedResult.BlockId));
                }
            });

            var addFailResultTask =
                _failedResultRepository.InsertAsync(GrabBlockFailedResultEntity.Create(grabBlockFailedResult.BlockId,
                    grabBlockFailedResult.FailDescription));

            await Task.WhenAll(removeCmdTask, addFailResultTask);
        }

        public async Task<IEnumerable<IGrabBlockCommand>> GetAllPendingCommandsAsync()
        {
            return await _commandRepository.GetDataAsync(GrabBlockCommandEntity.GeneratePartitionKey());
        }

        public bool NeedToAddToProccessing(string blockId)
        {
            return !_commandRepository.RecordExists(GrabBlockCommandEntity.Create(blockId)) 
                && !_doneResultRepository.RecordExists(GrabBlockDoneResultEntity.Create(blockId))
                && !IsAttemptLimitReached(blockId).Result;
        }

        private async Task<bool> IsAttemptLimitReached(string blockId)
        {
            var attempts = await _failedResultRepository.GetDataAsync(GrabBlockFailedResultEntity.GeneratePartitionKey(blockId));

            return attempts.Count() >= _attemptLimit;
        }
    }

    public class GrabNewAssets:GrabBlockDirectorBase
    {
        public GrabNewAssets(INoSQLTableStorage<GrabBlockCommandEntity> commandRepository, INoSQLTableStorage<GrabBlockFailedResultEntity> failedResultRepository, INoSQLTableStorage<GrabBlockDoneResultEntity> doneResultRepository, int attemptLimit) : base(commandRepository, failedResultRepository, doneResultRepository, attemptLimit)
        {
        }
    }
}
