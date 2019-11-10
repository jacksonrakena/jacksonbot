using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abyss
{
    public abstract class AbyssPack
    {
        public abstract string FriendlyName { get; }
        public abstract string Description { get; }
        public abstract Assembly Assembly { get; }

        public virtual Task OnLoadAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUnloadAsync()
        {
            return Task.CompletedTask;
        }
    }
}
