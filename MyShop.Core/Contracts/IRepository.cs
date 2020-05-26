using System.Linq;
using MyShop.Core.Models;

namespace MyShop.Core.Contracts
{
    // BaseEnitity 형식의 객체에 대해 CRUD 함수를 수행하는 인터페이스
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> Collection();
        void Commit();
        void Delete(string Id);
        T Find(string Id);
        void Insert(T t);
        void Update(T t);
    }
}