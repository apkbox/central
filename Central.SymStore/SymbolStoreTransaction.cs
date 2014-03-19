namespace Central.SymStore
{
    public abstract class SymbolStoreTransaction
    {
        internal abstract void Commit(string symbolStoreDirectory);
    }
}