public interface IJsonSaveable
{
    string CaptureState();
    void RestoreState(string json);
}