using System;
using UnityEngine;

namespace GameUtilities
{
    public enum PlayerPrefsType
    {
        SkillJump,
        SkillRemoveObstacle,
        SkillShowMap,
        SkillSlow,
    }
    
    public class PlayerPrefsTool
    {
        public static T GetPlayerPrefs<T>(string key, T defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            var type = typeof(T);
            switch (type)
            {
                case { } t when t == typeof(int):
                    return (T)(object)PlayerPrefs.GetInt(key);
                case { } t when t == typeof(float):
                    return (T)(object)PlayerPrefs.GetFloat(key);
                case { } t when t == typeof(long):
                    return (T)(object)long.Parse(PlayerPrefs.GetString(key));
                case { } t when t == typeof(bool):
                    return PlayerPrefs.GetInt(key) == 1 ? (T)(object)true : (T)(object)false;
                case { IsEnum: true }:
                    var value = PlayerPrefs.GetString(key, defaultValue.ToString());
                    return (T)Enum.Parse(typeof(T), value, true);
                default:
                    return (T)(object)PlayerPrefs.GetString(key);
            }
        }


        public static void SetPlayerPrefs(string key, object setValue)
        {
            var type = setValue.GetType();
            switch (type)
            {
                case { } t when t == typeof(int):
                    PlayerPrefs.SetInt(key, (int)setValue);
                    break;
                case { } t when t == typeof(float):
                    PlayerPrefs.SetFloat(key, (float)setValue);
                    break;
                case { } t when t == typeof(long):
                    PlayerPrefs.SetString(key, setValue.ToString());
                    break;
                case { } t when t == typeof(bool):
                    PlayerPrefs.SetInt(key, (bool)setValue ? 1 : 0);
                    break;
                case { IsEnum: true }:
                    PlayerPrefs.SetString(key, setValue.ToString());
                    break;
                default:
                    PlayerPrefs.SetString(key, setValue.ToString());
                    break;
            }

            PlayerPrefs.Save();
        }
    }
}