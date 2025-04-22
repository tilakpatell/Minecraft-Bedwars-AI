using System.Data;
using BedwarsAI.Items;

namespace BedwarsAI;

public class Inventory(int initialDiamonds = 0, int initialEmeralds = 0, int initialGold = 0, int initialIron = 0)
{
    private List<Item> _items = new List<Item>();
    private Diamond _diamond = new Diamond(initialDiamonds);
    private Emerald _emerald = new Emerald(initialEmeralds);
    private Gold _gold = new Gold(initialGold);
    private Iron _iron = new Iron(initialIron);

    public bool hasEnoughMoney(Money money)
    {
        if (money is Diamond diamond)
            return _diamond.Count > diamond.Count;
        if (money is Emerald emerald)
            return _emerald.Count > emerald.Count;
        if (money is Gold gold)
            return _gold.Count > gold.Count;
        if (money is Iron iron)
            return _iron.Count > iron.Count;

        throw new NotSupportedException("Unknown money type");
    }

    public void SubtractMoney(Money money)
    {
        if(money is Diamond diamond) {
            _diamond.Count -= diamond.Count;
            return; 
        }
        if (money is Emerald emerald) {
            _emerald.Count -= emerald.Count;
            return;
        }
        if (money is Gold gold) {
            _gold.Count -= gold.Count;
            return;
        }
        if (money is Iron iron) {
            _iron.Count -= iron.Count;
            return; 
        }
        
        throw new NotSupportedException("Unknown money type");
    }

    public void AddMoney(Money money)
    {
        if (money is Diamond diamond) {
            _diamond.Count += diamond.Count;
            return; 
        }
        if (money is Emerald emerald) {
            _emerald.Count += emerald.Count;
            return; 
        }
        if (money is Gold gold) {
            _gold.Count += gold.Count;
            return; 
        }
        if (money is Iron iron) {
            _iron.Count += iron.Count;
            return;
        }
        throw new NotSupportedException("Unknown money type");
    }
    
    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public void Clear()
    {
        _items = new List<Item>();
        _diamond = new Diamond(0);
        _emerald = new Emerald(0);
        _gold = new Gold(0);
        _iron = new Iron(0);

    }
    
    public bool HasItems<T>(int count) where T : Item
    {
        return _items.OfType<T>().Count() >= count;
    }

    public bool RemoveItems<T>(int count) where T : Item
    {
        var itemsOfType = _items.OfType<T>().ToList();
    
        if (itemsOfType.Count >= count)
        {
            for (int i = 0; i < count; i++)
            {
                _items.Remove(itemsOfType[i]);
            }
            return true;
        }
        return false;
    }
}
    
    
    

