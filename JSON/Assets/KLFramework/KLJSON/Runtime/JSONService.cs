using System.Reflection;
using UnityEngine;

namespace KLFramework.JSON
{
    public class JSONService
    {
        private static IJSONAPI _JSONAPI;

        static JSONService()
        {
            
        }

        public static void Register(IJSONAPI jsonAPI)
        {
            _JSONAPI = jsonAPI;
        }

        public static IJSONArray ParseArray(string jsonArray)
        {
            #if UNITY_EDITOR
            _CheckInited();
            #endif
            return _JSONAPI.ParseArray(jsonArray);
        }

        public static IJSONObject ParseObject(string jsonObject)
        {
            #if UNITY_EDITOR
            _CheckInited();
            #endif
            return _JSONAPI.ParseObject(jsonObject);
        }
        
        public static IJSONArray NewArray()
        {
            #if UNITY_EDITOR
            _CheckInited();
            #endif
            return _JSONAPI.NewArray();
        }

        public static IJSONObject NewObject()
        {
            #if UNITY_EDITOR
            _CheckInited();
            #endif
            return _JSONAPI.NewObject();
        }

        private static void _CheckInited()
        {
            if (_JSONAPI == null)
            {
                Debug.LogError("You must Register a implementation of IJSONAPI first.");
            }
        }
    }
}