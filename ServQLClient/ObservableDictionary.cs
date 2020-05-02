using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ServQLClient
{
    public class ObservableDictionary<T1,T2> : IDictionary
    {
        public class DictionaryChangedEventArgs : EventArgs
        {
            int Change { get; set; }
            Collection<T1> NewKeys { get; set; }
            Collection<T1> OldKeys { get; set; }
            Collection<T2> NewValues { get; set; }
            Collection<T2> OldValues { get; set; }

            public DictionaryChangedEventArgs(int Change, Collection<T1> NewKeys, Collection<T1> OldKeys, Collection<T2> NewValues, Collection<T2> OldValues)
            {
                this.Change = Change;
                this.NewKeys = NewKeys;
                this.OldKeys = OldKeys;
                this.NewValues = NewValues;
                this.OldValues = OldValues;
            }


            public DictionaryChangedEventArgs(int Change)
            {
                this.Change = Change;
                this.NewKeys = new Collection<T1>();
                this.OldKeys = new Collection<T1>();
                this.NewValues = new Collection<T2>();
                this.OldValues = new Collection<T2>();
            }

        }          
        
        public static class Change                        
        {
            public static int Add = 0;
            public static int Remove = 1;
            public static int Edit = 2;
            public static int Clear = 3;

        }

        public delegate void DictionaryChanged(object sender, DictionaryChangedEventArgs e);
        public event DictionaryChanged DictionaryChange;

        public object this[object key] { 
            
            get  {

                int index = Keys.IndexOf((T1)key);
                return Values[index];

            }

            set {
                DictionaryChangedEventArgs changedEventArgs = new DictionaryChangedEventArgs(Change.Edit);

                DictionaryChange?.Invoke(this,changedEventArgs);
                int index = Keys.IndexOf((T1)key);
                Values[index] = (T2)value;

            }
        }


        public Collection<T1> Keys = new Collection<T1>();

        public Collection<T2> Values => new Collection<T2>();

        public int Count => Keys.Count;

        public bool IsFixedSize => throw new NotImplementedException(); //TODO

        public bool IsReadOnly => throw new NotImplementedException(); //TODO

        ICollection IDictionary.Keys => Keys;

        ICollection IDictionary.Values => Values;

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public void Add(object key, object value)
        {
            DictionaryChangedEventArgs changedEventArgs = new DictionaryChangedEventArgs(Change.Edit);
            Keys.Add((T1)key);
            Values.Add((T2)value);
            DictionaryChange?.Invoke(this,changedEventArgs);
        }

        public void Clear()
        {
            Keys.Clear();
            Values.Clear();
            //DictionaryChange?.Invoke(this);

        }

        public bool Contains(object key)
        {
            return Keys.Contains((T1)key);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException(); // TODO
        }


        public void Remove(object key)
        {
            int index = Keys.IndexOf((T1)key);
            Values.RemoveAt(index);
            Keys.RemoveAt(index);
            //DictionaryChange?.Invoke(Change.Remove);

        }

        public IDictionaryEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
