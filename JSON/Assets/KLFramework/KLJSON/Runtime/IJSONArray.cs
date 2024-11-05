using System.Collections;

namespace KLFramework.JSON
{
    public interface IJSONArray 
    {
        int Count { get; }
        
        int GetInt(int index);
    
        uint GetUint(int index);

        long GetLong(int index);
    
        ulong GetUlong(int index);
    
        bool GetBool(int index);
    
        string GetString(int index);
    
        char GetChar(int index);
    
        byte GetByte(int index);
        float GetFloat(int index);
        double GetDouble(int index);
    
        IJSONObject GetObject(int index);
    
        IJSONArray GetArray(int index);

        IJSONArray AddObject(object obj);
        
        string Serialize();
    }
}