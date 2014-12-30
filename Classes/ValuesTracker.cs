using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace KCS.Common.Shared
{
    /// <summary>
	/// Facilitates saving any arbitray values to isolated storage.
    /// </summary>
	/// <example>
	///		1. Instantiate the class with the "storageKey" constructor.
	///		2. Use AddValue() as needed.
	///		3. Call Save() as needed. This persists the values to disk.
	///		4. To reload the values into memory, call Load().
	///		5. Call GetValue() as needed, to retrieve specific values by key (the same keys used in the calls to AddValue(...) ).
	/// </example>
	public class ValuesTracker
    {
        #region Members
		private bool _savingCancelled;
		private bool _loadingCancelled;
        #endregion

		#region Events
		/// <summary>
		/// Raised just before values are loaded from the datastore.
		/// </summary>
		public event EventHandler<CancelEventArgs> Loading;

		/// <summary>
		/// Raised just after values are loaded from the datastore.
		/// </summary>
		public event EventHandler<SuccessEventArgs> Loaded;

		/// <summary>
		/// Raised just before values are saved.
		/// </summary>
		public event EventHandler<CancelEventArgs> Saving;

		/// <summary>
		/// Raised just after values are saved.
		/// </summary>
		public event EventHandler<SuccessEventArgs> Saved;
		#endregion

		#region Properties
		/// <summary>
		/// Contains all Exceptions generated during the lifetime of the component.
		/// </summary>
		public List<Exception> Exceptions { get; private set; }

		/// <summary>
		/// Gets or sets the storage key, which is used to uniquely identify the associated data.
        /// data store. All instances of this component that share the same key will contain the same values at startup.
		/// </summary>
		protected string StorageKey { get; private set; }

        /// <summary>
        /// Contains the dictionary of values being tracked.
        /// </summary>
        protected Dictionary<string, object> Dictionary { get; private set; }
        #endregion		

        /// <summary>
		/// Default constructor.
		/// </summary>
        protected ValuesTracker()
        {
        }

		/// <summary>
		/// Main constructor.
		/// </summary>
        /// <param name="storageKey">Specific storage key. Cannot be null or empty.</param>
        public ValuesTracker(string storageKey) : this()
		{
			Dictionary = new Dictionary<string, object>();
			Exceptions = new List<Exception>();

            storageKey = Utility.GetStringValue(storageKey);

            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new NullReferenceException("StorageKey is required.");
            }

            if (Strings.IsAlphaNumericOnly(@storageKey) || storageKey.Length < 8 || storageKey.Length > 128)
            {
                throw new Exception("StorageKey must be between 8 and 128 alphanumeric characters.");
            }

            StorageKey = storageKey;
		}		

		/// <summary>
		/// Sets a byte array representation of the internal dictionary.
		/// </summary>
		/// <param name="data">Data to set.</param>
		public void SetDictionaryData(byte[] data)
		{
			MemoryStream stream;
			BinaryFormatter bf = new BinaryFormatter();

			// Save the dictionary as raw binary data. If it fails, the Dictionary is created, but it is empty.
			try
			{
				if (data == null)
				{
					Dictionary = new Dictionary<string, object>();
				}
				else
				{
					stream = new MemoryStream(data);
					Dictionary = (Dictionary<string, object>)bf.Deserialize(stream);
				}
			}
			catch (Exception ex)
			{
				Exceptions.Add(ex);
				Dictionary = new Dictionary<string, object>();
			}
		}

		/// <summary>
		/// Gets a byte array representation of the internal dictionary.
		/// </summary>
		/// <returns>Byte array.</returns>
		public byte[] GetDictionaryData()
		{
			var stream = new MemoryStream();
			var bf = new BinaryFormatter();

			// Save the dictionary as raw binary data
			bf.Serialize(stream, Dictionary);
			return stream.GetBuffer();
		}


        #region Save and Load methods
        /// <summary>
        /// Saves the values to the data store.
        /// </summary>
        public virtual void Save()
        {
			byte[] buffer;
			var ifs = IsolatedStorageFile.GetUserStoreForAssembly();
			IsolatedStorageFileStream isStream = null;

			// Persist the data to isolated storage
			try
			{
				// Raise the Saving event. If the event was not cancelled, proceed to save the data to
				// isoloated storage
				OnSaving();
				if (!_savingCancelled)
				{
					buffer = GetDictionaryData();
					isStream = new IsolatedStorageFileStream(StorageKey, FileMode.Create, FileAccess.Write, ifs);
					isStream.Write(buffer, 0, buffer.Length);
				}
				
				// Raise the Saved event, indicating success
				OnSaved(true);
			}
			catch (Exception ex)
			{
				Exceptions.Add(ex);

				// Raise the Saved event, indicating failure
				OnSaved(false);
			}
			finally
			{
				if (isStream != null)
				{
					isStream.Close();
					isStream = null;
				}
			}            
        }

        /// <summary>
        /// Loads all the previously-saved values from isolated storage.
        /// </summary>
        public void Load()
        {
			var bf = new BinaryFormatter();
			IsolatedStorageFileStream isStream = null;
			var ifs = IsolatedStorageFile.GetUserStoreForAssembly();

            // Load the data from isolated storage
			try
			{
				// Raise the Loading event. If it was not cancelled, proceed to get the data from isolated storage.
				OnLoading();
				if (!_loadingCancelled)
				{
					isStream = new IsolatedStorageFileStream(StorageKey, FileMode.OpenOrCreate, FileAccess.Read, ifs);
					if (isStream.Length > 0)
					{
						Dictionary = (Dictionary<string, object>)bf.Deserialize(isStream);
					}
				}

				// Raise the Loaded event, indicating success
				OnLoaded(true);
			}
			catch (Exception ex)
			{
				Exceptions.Add(ex);

				// Raise the Loaded event, indicating failure
				OnLoaded(false);
			}
			finally
			{
				if (isStream != null)
				{
					isStream.Close();
					isStream = null;
				}
			}

            // Make sure that the values dictionary exists and is populated with the minimum items
            if (Dictionary == null)
            {
                Dictionary = new Dictionary<string, object>();
            }
        }
        #endregion

        #region Methods for working with the dictionary (getting and setting values)
		/// <summary>
		/// Removes a value from the collection
		/// </summary>
		/// <param name="key">Key of the value to remove.</param>
		public void RemoveValue(string key)
		{
			if (Dictionary.ContainsKey(key))
			{
				Dictionary.Remove(key);
			}
		}

        /// <summary>
        /// Adds an object value. If the key already exists, the value is set instead.
        /// </summary>
        /// <param name="key">Value's key.</param>
        /// <param name="value">Value to set.</param>
        public void AddValue<T>(string key, T value)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary[key] = value;
            }
            else
            {
                Dictionary.Add(key, value);
            }
        }

		/// <summary>
		/// Adds multiple values to the internal dictionary.
		/// </summary>
		/// <param name="collection">Collection to be added.</param>
		public void AddValues<T>(Dictionary<string, T> collection)
		{
		    foreach(KeyValuePair<string, T> pair in collection)
		    {
				if (pair.Value != null)
				{
					AddValue(pair.Key, pair.Value);
				}
				else
				{
					RemoveValue(pair.Key);
				}
		    }
		}

		/// <summary> 
		/// Gets an object value. If the key does not exists, returns the default.
		/// </summary>
		/// <param name="key">Key of value to retrieve.</param>
		/// <param name="default">Default value.</param>
		/// <returns>Object value.</returns>
		public T GetValue<T>(string key, T @default)
		{
			if (Dictionary.ContainsKey(key))
			{
				return (T)Dictionary[key];
			}
			else
			{
				return @default;
			}
		}

		/// <summary> 
		/// Gets all values.
		/// </summary>
		/// <returns>Dictionary of all the values.</returns>
		public Dictionary<string, object> GetValues()
		{
			return new Dictionary<string, object>(this.Dictionary);
		}

        /// <summary> 
        /// Clears all the values from the dictionary.
        /// </summary>
        public void Clear()
        {
            Dictionary.Clear();
        }

		/// <summary> 
		/// Deletes the isolated storage file used to store values.
		/// </summary>
		public void DeleteDataStore()
		{
			try
			{
				var ifs = IsolatedStorageFile.GetUserStoreForAssembly();
				
				var files = ifs.GetFileNames("*.*");
				if (files.Contains(StorageKey))
				{
					ifs.DeleteFile(StorageKey);
				}
			}
			catch (Exception ex)
			{
				Exceptions.Add(ex);
			}
			finally
			{
			}
		}
        #endregion

		#region Methods that raise events.
		/// <summary>
		/// Raises the Loading event.
		/// </summary>
		protected void OnLoading()
		{
			CancelEventArgs args = new CancelEventArgs(false);
			if (Loading != null)
			{
				Loading(this, args);
				_loadingCancelled = args.Cancel;
			}
		}

		/// <summary>
		/// Raises the Loaded event.
		/// </summary>
		/// <param name="success">Success flag.</param>
		protected void OnLoaded(bool success)
		{
			if (Loaded != null)
			{
				Loaded(this, new SuccessEventArgs(success));
			}
		}

		/// <summary>
		/// Raises the Saving event.
		/// </summary>
		protected void OnSaving()
		{
			CancelEventArgs args = new CancelEventArgs(false);
			if (Saving != null)
			{
				Saving(this, args);
				_savingCancelled = args.Cancel;
			}
		}

		/// <summary>
		/// Raises the Saveded event.
		/// </summary>
		/// <param name="success">Success flag.</param>
		protected void OnSaved(bool success)
		{
			if (Saved != null)
			{
				Saved(this, new SuccessEventArgs(success));
			}
		}
		#endregion

        //public override bool Equals(object cmp)
        //{
        //    var cmpObj = (ValuesTracker)cmp;
        //    return this.StorageKey.Equals(cmpObj.StorageKey, StringComparison.CurrentCultureIgnoreCase));
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
	}

	
}
