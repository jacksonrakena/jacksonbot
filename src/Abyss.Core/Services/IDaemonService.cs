using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Core.Services
{
    public interface IDaemonService
    {
        Task RestartApplicationAsync();
    }
}
