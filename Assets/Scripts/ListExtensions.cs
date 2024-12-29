using System.Collections.Generic;

public static class ListExtensions
{
    /// <summary>
    /// Performs a Fisher–Yates shuffle on a list.
    /// </summary>
    /// <param name="list">The list to be shuffled.</param>
    /// <typeparam name="T">The type of the objects the list contains.</typeparam>
    /// <seealso href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Fisher–Yates shuffle on Wikipedia</seealso>
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int j = RNG.Range(i, n);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
