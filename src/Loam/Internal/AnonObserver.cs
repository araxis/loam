namespace Loam.Internal;

/// <summary>A minimal <see cref="IObserver{T}"/> that forwards <c>OnNext</c> to a delegate, for subscribing to resource/property observables without Rx.</summary>
internal sealed class AnonObserver<T> : IObserver<T>
{
    private readonly Action<T> _onNext;

    public AnonObserver(Action<T> onNext) => _onNext = onNext;

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(T value) => _onNext(value);
}
