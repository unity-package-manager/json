namespace KLFramework.JSON
{
    public interface IJSONAPI
    {
        IJSONArray NewArray();
        
        IJSONObject NewObject();

        IJSONObject ParseObject(string json);
        
        IJSONArray ParseArray(string json);
    }
}