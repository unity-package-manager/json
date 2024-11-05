using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using IO.Unity3D.Source.Reflection;

namespace KLFramework.JSON
{

    //******************************************
    // Bean2JSON
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-08-08 15:29
    //******************************************
    public class Bean2JSON
    {
        private static Type _LIST_TYPE = typeof(List<>);
        
        public static string ToJSONString(object obj)
        {
            StringBuilder stringBuilder = new StringBuilder();

            _ToString(stringBuilder, obj, obj.GetType());

            return stringBuilder.ToString();
        }

        private static  void _ToString(StringBuilder stringBuilder, object o, Type type)
        { 
            if (type == typeof(int))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(float))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(uint))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(double))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(long))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(ulong))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(byte))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(sbyte))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(short))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(ushort))
            {
                stringBuilder.Append(o);
            }
            else if (type == typeof(string))
            {
                stringBuilder.Append("\"").Append(o ?? "").Append("\"");
            }
            else if (type == typeof(bool))
            {
                stringBuilder.Append("\"").Append(o).Append("\"");
            }
            else if (type.IsGenericType)
            {
                if (_LIST_TYPE == type.GetGenericTypeDefinition())
                {
                    stringBuilder.Append("[");
                    if (o != null)
                    {
                        int count = (int) Reflections.GetPropertyOrField(type, "Count").GetValue(o);
                        PropertyInfo indexer = type.GetProperty("Item");
                        var genericArgument = type.GetGenericArguments()[0];
                        for (int i = 0; i < count; i++)
                        {
                            object item = indexer.GetValue(o, new object[] { i });
                            _ToString(stringBuilder, item, genericArgument);
                            if (i < count - 1)
                            {
                                stringBuilder.Append(", ");
                            }
                        }
                    }
                    stringBuilder.Append("]");                    
                }
            }
            else if (type.IsArray)
            {
                stringBuilder.Append("[");

                Array arr = (Array)o;
                var length = arr.Length;
                var elementType = type.GetElementType();
                for (int i = 0; i < length; i++)
                {
                    object item = arr.GetValue(i);
                    _ToString(stringBuilder, item, elementType);
                    if (i < length - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Append("]");   
            }
            else
            {
                var propertiesAndFields = Reflections.GetPropertiesAndFields<JSON2Bean.JSONProperty>(o);
                stringBuilder.Append("{");
                for (int i = 0; i < propertiesAndFields.Count; i++)
                {
                    var propertiesAndField = propertiesAndFields[i];
                    var jsonProperty = propertiesAndField.GetCustomAttribute<JSON2Bean.JSONProperty>();
                    var propertyNames = jsonProperty.PropertyNames;
                    string propertyName = propertiesAndField.Name;
                    if (propertyNames != null && propertyNames.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(propertyNames[0]))
                        {
                            propertyName = propertyNames[0];
                        }
                    }
                    stringBuilder.Append('"').Append(propertyName).Append("\":");
                    var fieldVal = propertiesAndField.GetValue(o);
                    _ToString(stringBuilder, fieldVal, propertiesAndField.GetFieldOrPropertyType());
                    if (i < propertiesAndFields.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }
                stringBuilder.Append("}");
            }
        }
    }
}