namespace ashley.Signals
{
    public interface IListener<T>
    {
        void Receive(Signal<T> signal, T value);
    }
}