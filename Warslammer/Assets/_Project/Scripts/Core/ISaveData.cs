namespace Warslammer.Core
{
    /// <summary>
    /// Interface for objects that can be saved and loaded
    /// </summary>
    public interface ISaveData
    {
        /// <summary>
        /// Serialize this object to a saveable format
        /// </summary>
        /// <returns>Serialized string data</returns>
        string Serialize();

        /// <summary>
        /// Deserialize and restore this object from saved data
        /// </summary>
        /// <param name="data">Serialized data to load</param>
        void Deserialize(string data);

        /// <summary>
        /// Get a unique identifier for this saved object
        /// </summary>
        /// <returns>Unique ID string</returns>
        string GetSaveID();
    }
}