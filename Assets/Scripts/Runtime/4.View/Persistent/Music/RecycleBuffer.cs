using System;

public class RecycleBuffer<T> where T : class, IRecyclable
{
    private readonly T[] _buffer;
    private readonly bool[] _used;
    private int _head;
    private readonly int _capacity;

    public int Capacity => _capacity;

    public RecycleBuffer(T[] buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _capacity = buffer.Length;
        _used = new bool[_capacity];
        _head = 0;

        // 初期化時に各要素へIDを割り振る
        for (int i = 0; i < _capacity; i++)
        {
            if (_buffer[i] != null)
            {
                _buffer[i].RecycleId = i;
            }
        }
    }

    /// <summary>
    /// オブジェクトを1つ取得します。
    /// 空きがなければ、最も古いオブジェクトを強制的に再利用します。
    /// </summary>
    public T Get()
    {
        // 現在のヘッド位置にあるものを候補とする
        T item = _buffer[_head];
        int index = _head;

        // もし既に使用中なら、強制的にリサイクル処理を走らせる
        if (_used[index])
        {
            item.OnRecycle();
        }

        _used[index] = true;

        // 次回のためにヘッドを進めるリングバッファ的奈
        _head = (_head + 1) % _capacity;

        return item;
    }

    /// <summary>
    /// IDを利用して返却する
    /// </summary>
    public void Release(T item)
    {
        if (item == null) return;

        int id = item.RecycleId;

        // 範囲チェックと、このバッファの持ち物かどうかの簡易チェック
        if (id < 0 || id >= _capacity || !ReferenceEquals(_buffer[id], item))
        {
            throw new ArgumentException("指定されたアイテムはこのバッファの管理下ではありません。");
        }

        // 使用中フラグを下ろす
        _used[id] = false;
    }

    /// <summary>
    /// すべてのオブジェクトをリサイクルする
    /// </summary>
    public void RecycleAll()
    {
        for (int i = 0; i < _capacity; i++)
        {
            if (_used[i])
            {
                _buffer[i].OnRecycle();
                _used[i] = false;
            }
        }

        _head = 0;
    }
}