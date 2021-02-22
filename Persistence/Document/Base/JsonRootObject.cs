using System;
using System.Threading.Tasks;
using Lament.Persistence.Relational;

namespace Lament.Persistence.Document
{
    public abstract class JsonRootObject<T> where T: JsonRootObject<T>
    {
    }
}