using UnityEngine;

namespace Saving
{
    public class DataBase : MonoBehaviour
    {
        public static void Save<T>(string key, T data)
        {
            string jsonDataString = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(key, jsonDataString);
        }

        public static T Load<T>(string key) where T: new()
        {
            if (PlayerPrefs.HasKey(key))
            {
                string loadedString = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<T>(loadedString);
            }
            return new T();
        }
    }
}
