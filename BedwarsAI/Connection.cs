namespace BedwarsAI;

public class Connection
{
    private int _MaxLength;
    private int _BridgeLength;

    public Connection(int maxLength)
    {
        this._MaxLength = maxLength;
        this._BridgeLength = 0;
    }

    public void Extend(int bridgingSkill)
    {
        _BridgeLength += bridgingSkill;
    }
    
    public bool IsComplete()
    {
        return _BridgeLength >= _MaxLength;
    }

}