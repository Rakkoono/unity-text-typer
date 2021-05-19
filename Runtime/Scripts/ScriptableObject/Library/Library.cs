namespace RedBlueGames.Tools.TextTyper
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface IPreset
    {
        public string Name { get; }
    }

    public abstract class Library<T> : ScriptableObject where T : IPreset
    {
        public List<T> Presets;

        /// <summary>
        /// Get the preset from this library with the provided key/name
        /// </summary>
        /// <param name="key">Key/name identifying the desired preset</param>
        /// <returns>Matching preset</returns>
        public T this[string key]
        {
            get
            {
                var preset = this.FindPresetOrNull(key);
                if (preset == null)
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    return preset;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return this.FindPresetOrNull(key) != null;
        }

        private T FindPresetOrNull(string key)
        {
            foreach (var preset in this.Presets)
            {
                if (preset.Name.ToUpper() == key.ToUpper())
                {
                    return preset;
                }
            }

            return default(T);
        }

        public string[] ToStringArray()
        {
            var array = new string[this.Presets.Count];

            for (int i = 0; i < this.Presets.Count; i++)
            {
                array[i] = this.Presets[i].Name;
            }

            return array;
        }
    }
}