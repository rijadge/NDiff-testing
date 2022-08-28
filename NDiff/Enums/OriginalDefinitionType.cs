using NDiff.CustomAttributes;

namespace NDiff.Enums
{
    /// <summary>
    /// Used to enumerate the C# Collections types (generic and non-generic).
    /// </summary>
    public enum OriginalDefinitionType
    {
        #region Arrays
        
        [EnumStringValue("System.Collections.Generic.List<T>")]
        GenericList,        
        
        [EnumStringValue("System.Collections.Generic.IList<T>")]
        GenericIList,   
        
        [EnumStringValue("System.Collections.Generic.Enumerable<T>")]
        GenericEnumerable,   
        
        [EnumStringValue("System.Collections.Generic.IEnumerable<T>")]
        GenericIEnumerable,

        [EnumStringValue("System.Collections.Generic.LinkedList<T>")]
        GenericLinkedList,
        
        [EnumStringValue("System.Collections.Generic.SortedSet<T>")]
        GenericSortedSet,
        
        [EnumStringValue("System.Collections.Generic.Stack<T>")]
        GenericStack,
        
        [EnumStringValue("System.Collections.Generic.HashSet<T>")]
        GenericHashSet,
        
        [EnumStringValue("System.Collections.Generic.Queue<T>")]
        GenericQueue,

        [EnumStringValue("System.Collections.ArrayList")]
        ArrayList,

        [EnumStringValue("System.Collections.LinkedList")]
        LinkedList,
        
        [EnumStringValue("System.Collections.BitArray")]
        BitArray,

        [EnumStringValue("System.Collections.Queue")]
        Queue,
        
        [EnumStringValue("System.Collections.SortedList")]
        SortedList,
        
        [EnumStringValue("System.Collections.Stack")]
        Stack,
        
        [EnumStringValue("System.Collections.Generic.ObservableCollection<T>")]
        GenericObservableCollection,
        
        #endregion

        
        #region Objects

        [EnumStringValue("System.Collections.Generic.Dictionary<TKey, TValue>")]
        GenericDictionary,

        [EnumStringValue("System.Collections.Generic.IDictionary<TKey, TValue>")]
        GenericIDictionary,
        
        [EnumStringValue("System.Collections.Generic.SortedDictionary<TKey, TValue>")]
        GenericSortedDictionary,
        
        [EnumStringValue("System.Collections.Generic.KeyValuePair<T>")]
        GenericKeyValuePair,
        
        [EnumStringValue("System.Collections.KeyValuePair")]
        KeyValuePair,

        [EnumStringValue("System.Collections.Hashtable")]
        Hashtable,
                
        [EnumStringValue("System.Collections.Generic.SortedList<TKey, TValue>")]
        GenericSortedList,
        
        #endregion
        
        
        [EnumStringValue("System.Nullable<T>")]
        GenericNullable,

    }
}