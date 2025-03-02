using System;
using System.Threading.Tasks;

namespace ScreenTools.App;

public interface IStorageService<T>
{
    Task SaveData(T data);
    Task<T> LoadData();
}