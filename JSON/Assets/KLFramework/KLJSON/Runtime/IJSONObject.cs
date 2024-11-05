using System;

namespace KLFramework.JSON
{
    public interface IJSONObject
    {
        int GetInt(string key);
    
        uint GetUint(string key);

        long GetLong(string key);
    
        ulong GetUlong(string key);
    
        bool GetBool(string key);
    
        string GetString(string key);
    
        char GetChar(string key);
    
        byte GetByte(string key);
    
        float GetFloat(string key);
        
        double GetDouble(string key);

        bool Has(string key);
        
        object GetRawObject(string key);
        
        Type GetType(string key);
    
        IJSONObject GetObject(string key);
    
        IJSONArray GetArray(string key);

        IJSONObject SetObject(string key, Object val);
        
        string Serialize();
    }
}
