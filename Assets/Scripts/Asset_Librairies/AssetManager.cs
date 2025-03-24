using System;
using System.Collections.Generic;

public class Asset<T>
{
    public T asset;
    public string name;

    public Asset(T asset, string name = null)
    {
        this.asset = asset;
        this.name = name;
    }
}

public struct HandleId
{
    public Guid Id { get; }

    public HandleId(Guid id)
    {
        Id = id;
    }

    public static HandleId Random()
    {
        return new HandleId(Guid.NewGuid());
    }
}

public struct Handle<T>
{
    public HandleId Id { get; }

    public Handle(HandleId id)
    {
        Id = id;
    }
}

public class AssetManager<T>
{
    private Dictionary<HandleId, T> assets = new Dictionary<HandleId, T>();
    private Dictionary<string, HandleId> byName = new Dictionary<string, HandleId>();

    public Handle<T> Add(Asset<T> asset)
    {
        HandleId id = HandleId.Random();
        assets[id] = asset.asset;

        if (!string.IsNullOrEmpty(asset.name))
        {
            byName[asset.name] = id;
        }

        return new Handle<T>(id);
    }

    public T Get(Handle<T> handle)
    {
        return assets.TryGetValue(handle.Id, out T asset) ? asset : default;
    }

    public T GetByName(string name)
    {
        return byName.TryGetValue(name, out HandleId id) ? Get(new Handle<T>(id)) : default;
    }

    public Handle<T>? GetHandleByName(string name)
    {
        return byName.TryGetValue(name, out HandleId id) ? new Handle<T>(id) : (Handle<T>?)null;
    }
}
