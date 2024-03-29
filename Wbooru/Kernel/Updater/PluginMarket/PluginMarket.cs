﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;

namespace Wbooru.Kernel.Updater.PluginMarket
{
    public abstract class PluginMarket: IMultiImplementProvidable
    {
        public abstract string MarketName { get; }
        public abstract IEnumerable<PluginMarketPost> GetPluginPosts();

        public override string ToString() => MarketName;
    }
}
