using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenTools.App;

public static class CollectionExtensions
{
    public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> items)
    {
        return new ObservableCollection<T>(items);
    }
}