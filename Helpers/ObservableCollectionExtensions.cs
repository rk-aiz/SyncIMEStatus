using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SyncIMEStatus.Helpers
{

    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> source, IEnumerable<T> collection)
        {
            var pInfoItems = typeof(ObservableCollection<T>).GetProperty("Items", BindingFlags.NonPublic | BindingFlags.Instance);
            var mInfoCollectionReset = typeof(ObservableCollection<T>).GetMethod("OnCollectionReset", BindingFlags.NonPublic | BindingFlags.Instance);

            if (pInfoItems.GetValue(source) is List<T> list)
            {
                list.AddRange(collection);
                mInfoCollectionReset.Invoke(source, null);
            }
        }

        /*
        public static void ReplaceAll<T>(this ObservableCollection<T> source, IEnumerable<T> collection)
        {
            var itProperty = typeof(ObservableCollection<T>).GetProperty("Items", BindingFlags.NonPublic | BindingFlags.Instance);
            var colChangedMethod = typeof(ObservableCollection<T>).GetMethod("OnCollectionChanged", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(NotifyCollectionChangedEventArgs) }, null);

            if (itProperty.GetValue(source) is List<T> list)
            {
                System.Collections.IList oldItems = list.ToArray();
                list.Clear();
                colChangedMethod.Invoke(source, new[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems: oldItems) });
                list.AddRange(collection);
                colChangedMethod.Invoke(source, new[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems: (System.Collections.IList)collection) });
            }
        }
        */

        //使ってない
        public static void AddPropertyChanged<T>(this ObservableCollection<T> source, PropertyChangedEventHandler callback) where T : INotifyPropertyChanged 
        {
            if (callback == null) throw new ArgumentNullException();

            Debug.WriteLine($"AddPropertyChanged  count {source.Count}");

            if (source.Count > 0)
            {
                foreach (INotifyPropertyChanged item in source)
                    item.PropertyChanged += callback;
            }

            source.CollectionChanged += (s, e) =>
            {
                Debug.WriteLine($"CollectionChanged!! {e.Action} oldItems : {e.OldItems?.Count??0} newItems : {e.NewItems?.Count??0}");
                if (e.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.OldItems)
                        item.PropertyChanged -= callback;
                }
                if (e.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.NewItems)
                        item.PropertyChanged += callback;
                }
            };
        }
    }

    
    //使ってない
    public class NotifierCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public void AddRange(IEnumerable<T> collection)
        {
            ((List<T>)Items).AddRange(collection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        //INotifyPropertyChanged interited from ObservableCollection<T>
        #region INotifyPropertyChanged

        protected override event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        /// <summary> 
        /// Replaces all elements in existing collection with specified collection of the ObservableCollection(Of T). 
        /// </summary> 
        public void Replace(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            Items.Clear();
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public NotifierCollection()
            : base() { }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public NotifierCollection(IEnumerable<T> collection)
            : base(collection) { }
    }
}
