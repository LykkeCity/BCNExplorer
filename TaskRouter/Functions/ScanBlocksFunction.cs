﻿using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Microsoft.Azure.WebJobs;
using Providers.Providers.Ninja;

namespace TaskRouter.Functions
{
    public class ScanBlocksFunction
    {
        private readonly ILog _log;
        private readonly NinjaBlockProvider _blockProvider;

        public ScanBlocksFunction(ILog log, NinjaBlockProvider blockProvider)
        {
            _log = log;
            _blockProvider = blockProvider;
        }

        public async Task ScanBlocks([TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timer)
        {
            var lastBlock = await _blockProvider.GetLastBlockAsync();
            await _log.WriteInfo("ScanBlocksFunction", "ScanBlocks", $"last block {lastBlock.ToJson()}", "Scan blocks started");


        }
    }
}
