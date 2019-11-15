using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries.SupportFeatures;

namespace YandeSourcePlugin
{
    public class YandeAccountInfo : AccountInfo
    {
        public YandeAccountInfo(AccountInfo info)
        {
            Name = info.Name;
            Password = info.Password;
        }

        public string PasswordHash { get; set; }
    }
}
