using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyWallet.Models.Setting
{
    public class NetworkItem : ReactiveObject
    {
        public NetworkItem(string name, string poolName)
        {
            Name = name;
            PoolName = poolName;
        }

        public string Name { get; set; }
        
        public string PoolName { get; set; }
    }
}
