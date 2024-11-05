using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace KLFramework.JSON
{
    public class JSON2Bean
    {
        private static Type _LIST_TYPE = typeof(List<>);
        
        private static Dictionary<Type, Func<object, object>> _Parsers = new Dictionary<Type, Func<object, object>>();

        public static T FromJSON<T>(string json)
        {
            object obj;
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof(string))
            {
                if (type == typeof(string))
                {
                    return (T) (object) json;
                }
                if (type == typeof(bool))
                {
                    return (T) (object) bool.Parse(json);
                }
                if (type == typeof(byte))
                {
                    return (T) (object) byte.Parse(json);
                }
                if (type == typeof(char))
                {
                    return (T) (object) char.Parse(json);
                }
                if (type == typeof(int))
                {
                    return (T) (object) int.Parse(json);
                }
                if (type == typeof(long))
                {
                    return (T) (object) long.Parse(json);
                }
                if (type == typeof(uint))
                {
                    return (T) (object) uint.Parse(json);
                }
                if (type == typeof(ulong))
                {
                    return (T) (object) ulong.Parse(json);
                }
                if (type == typeof(float))
                {
                    return (T) (object) float.Parse(json);
                }
                if (type == typeof(double))
                {
                    return (T) (object) double.Parse(json);
                }
            }

            if (type.IsArray)
            {
                obj = JSONService.ParseArray(json);
            }
            else
            {
                obj = JSONService.ParseObject(json);
            }
            return (T) _Create(type, obj);
        }

        private static object _Create(Type type, object obj)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var jsonArray = (IJSONArray)obj;
                var array = Array.CreateInstance(elementType, jsonArray.Count);

                for (int i = 0, count = jsonArray.Count; i < count; i++)
                {
                    array.SetValue(_Create(elementType, _Get(jsonArray, i, elementType)), i);
                }

                return array;
            }
            
            if (type.IsGenericType)
            {
                if (_LIST_TYPE == type.GetGenericTypeDefinition())
                {
                    var list = Activator.CreateInstance(type);
                    var genericType = type.GenericTypeArguments[0];
                    var jsonArray = (IJSONArray)obj;
                    var addMethod = type.GetMethod("Add");
                    for (int i = 0, count = jsonArray.Count; i < count; i++)
                    {
                        addMethod.Invoke(list, new[] { _Create(genericType, _Get(jsonArray, i, genericType)) });
                    }
                    return list;
                }
            }
            
            if (type == typeof(string))
            {
                return (string)obj;
            }
            if (type == typeof(bool))
            {
                return (bool)obj;
            }
            if (type == typeof(byte))
            {
                return (byte)obj;
            }
            if (type == typeof(char))
            {
                return (char)obj;
            }
            if (type == typeof(int))
            {
                return (int)obj;
            }
            if (type == typeof(long))
            {
                return (long)obj;
            }
            if (type == typeof(uint))
            {
                return (uint)obj;
            }
            if (type == typeof(ulong))
            {
                return (ulong)obj;
            }
            if (type == typeof(float))
            {
                return (float)obj;
            }
            if (type == typeof(double))
            {
                return (double)obj;
            }
            
            var isStruct = type.IsValueType && !type.IsEnum &&
                           !type.IsEquivalentTo(typeof(decimal)) && 
                           !type.IsPrimitive;
            
            if (type.IsClass || isStruct)
            {
                var jsonParser = type.GetCustomAttribute<JSONParser>();
                object t = null;
                if (jsonParser != null)
                {
                    t = _GetOrCreateParser(jsonParser.ParserType)(obj);
                }
                else
                {
                    t = Activator.CreateInstance(type);
                    var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var fieldInfo in fieldInfos)
                    {
                        var jsonProperty = fieldInfo.GetCustomAttribute<JSONProperty>();
                        if (jsonProperty == null)
                        {
                            continue;
                        }

                        if (_Get((IJSONObject)obj, fieldInfo, jsonProperty, out object o))
                        {
                            var fieldValue = _Create(fieldInfo.FieldType, o);
                            fieldInfo.SetValue(t, fieldValue);
                        }
                    }
                }
                return t;
            }

            throw new Exception("Unsupport type " + type);
        }

        private static Func<object, object> _GetOrCreateParser(Type jsonParserType)
        {
            if (_Parsers.ContainsKey(jsonParserType))
            {
                return _Parsers[jsonParserType];
            }

            if (!typeof(IJSONParserFromArray).IsAssignableFrom(jsonParserType) && !typeof(IJSONParserFromObject).IsAssignableFrom(jsonParserType))
            {
                Debug.LogError("A parser type must implement IJSONParserFromArray or IJSONParserFromObject, but " + jsonParserType + " does not!");
                return null;
            }

            if (typeof(IJSONParserFromArray).IsAssignableFrom(jsonParserType))
            {
                var parser = (IJSONParserFromArray)Activator.CreateInstance(jsonParserType);
                _Parsers.Add(jsonParserType, (obj)=> parser.Parse((IJSONArray)obj));
            }

            if (typeof(IJSONParserFromObject).IsAssignableFrom(jsonParserType))
            {
                var parser = (IJSONParserFromObject)Activator.CreateInstance(jsonParserType);
                _Parsers.Add(jsonParserType, (obj)=> parser.Parse((IJSONObject)obj));
            }
            
            return _Parsers[jsonParserType];
        }

        private static bool _Get(IJSONObject jsonObject, FieldInfo fieldInfo, JSONProperty jsonProperty, out object o)
        {
            var type = fieldInfo.FieldType;
            
            if (type.IsArray)
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetArray, out string jsonKey, out IJSONArray array))
                {
                    o = array;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            
            if (type.IsGenericType && _LIST_TYPE == type.GetGenericTypeDefinition())
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetArray, out string jsonKey, out IJSONArray array))
                {
                    o = array;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            
            if (type == typeof(string))
            {
                return _GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetString, out string jsonKey, out o);
            }
            if (type == typeof(bool))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetBool, out string jsonKey, out bool b))
                {
                    o = b;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(byte))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetByte, out string jsonKey, out byte b))
                {
                    o = b;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(char))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetChar, out string jsonKey, out char c))
                {
                    o = c;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(int))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetInt, out string jsonKey, out int i))
                {
                    o = i;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(long))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetLong, out string jsonKey, out long l))
                {
                    o = l;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(uint))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetUint, out string jsonKey, out uint ui))
                {
                    o = ui;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(ulong))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetUlong, out string jsonKey, out ulong ul))
                {
                    o = ul;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(float))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetFloat, out string jsonKey, out float uf))
                {
                    o = uf;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            if (type == typeof(double))
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetDouble, out string jsonKey, out double ud))
                {
                    o = ud;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }
            
            var isStruct = type.IsValueType && !type.IsEnum &&
                           !type.IsEquivalentTo(typeof(decimal)) && 
                           !type.IsPrimitive;
            if (type.IsClass || isStruct)
            {
                if (_GetValue(jsonObject, fieldInfo, jsonProperty, jsonObject.GetObject, out string jsonKey, out IJSONObject obj))
                {
                    if (jsonObject.GetType(jsonKey) == typeof(string))
                    {
                        obj = JSONService.ParseObject(jsonObject.GetString(jsonKey));
                    }
                    o = obj;
                    return true;
                }
                else
                {
                    o = null;
                    return false;
                }
            }

            throw new Exception("Unsupport type " + type);
        }

        private static object _Get(IJSONArray jsonArray, int index, Type type)
        {
            var jsonParser = type.GetCustomAttribute<JSONParser>();
            if (jsonParser != null)
            {
                var method = jsonParser.ParserType.GetMethod("Parse");
                var parameterType = method.GetParameters()[0].ParameterType;
                if (typeof(IJSONArray).IsAssignableFrom(parameterType))
                {
                    return jsonArray.GetArray(index);    
                }
                else
                {
                    return jsonArray.GetObject(index);
                }
            }

            if (type.IsArray)
            {
                return jsonArray.GetArray(index);
            }
            if (type == typeof(string))
            {
                return jsonArray.GetString(index);
            }
            if (type == typeof(bool))
            {
                return jsonArray.GetBool(index);
            }
            if (type == typeof(byte))
            {
                return jsonArray.GetByte(index);
            }
            if (type == typeof(char))
            {
                return jsonArray.GetChar(index);
            }
            if (type == typeof(int))
            {
                return jsonArray.GetInt(index);
            }
            if (type == typeof(long))
            {
                return jsonArray.GetLong(index);
            }
            if (type == typeof(uint))
            {
                return jsonArray.GetUint(index);
            }
            if (type == typeof(ulong))
            {
                return jsonArray.GetUlong(index);
            }
            if (type == typeof(float))
            {
                return jsonArray.GetFloat(index);
            }
            if (type == typeof(double))
            {
                return jsonArray.GetDouble(index);
            }
            
            var isStruct = type.IsValueType && !type.IsEnum &&
                           !type.IsEquivalentTo(typeof(decimal)) && 
                           !type.IsPrimitive;
            
            if (type.IsClass || isStruct)
            {
                return jsonArray.GetObject(index);
            }

            throw new Exception("Unsupport type " + type);
        }

        private static bool _GetValue<T>(IJSONObject jsonObject, FieldInfo fieldInfo, JSONProperty jsonProperty, Func<string, T> getValue, out string key, out T t)
        {
            if (jsonProperty.PropertyNames != null && jsonProperty.PropertyNames.Length > 0)
            {
                foreach (var propertyName in jsonProperty.PropertyNames)
                {
                    if (jsonObject.Has(propertyName))
                    {
                        key = propertyName;
                        t = getValue(propertyName);
                        return true;
                    }
                }
            }
            else
            {
                if (jsonObject.Has(fieldInfo.Name))
                {
                    key = fieldInfo.Name;
                    t =  getValue(fieldInfo.Name);
                    return true;
                }
            }

            if (jsonProperty.Required)
            {
                Debug.LogError("Can not found data for field " + fieldInfo.Name);
            }

            key = null;
            t = default(T);
            return false;
        }

        [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class JSONProperty : Attribute
        {
            public string[] PropertyNames;
            public bool Required = false;

            public JSONProperty(params string[] propertyNames)
            {
                PropertyNames = propertyNames;
            }
        }

        public class JSONParser : Attribute
        {
            public Type ParserType;
        }
        
        public interface IJSONParserFromArray
        {
            object Parse(IJSONArray jsonArray);
        }
        
        public interface IJSONParserFromObject
        {
            object Parse(IJSONObject jsonArray);
        }
    }
}