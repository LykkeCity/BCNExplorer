﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IocContainer;
using Common.Log;
using Core.GrabBlockTask;
using Core.Settings;

namespace AzureRepositories.Binders
{
    public static class AzureRepositoriesBinder
    {
        public static void BindAzureRepositories(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IGrabBlockCommandsRepository>(AzureRepoFactories.CreateGrabBlockTaskRepository(baseSettings, log));

        }
    }
}
