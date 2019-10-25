namespace RabbitSyncService.Infrastructure
{
    public interface IDependency { }

    public interface ISingletonDependency : IDependency { }

    public interface IUnitOfWorkDependency : IDependency { }

    public interface ITransientDependency : IDependency { }

    public interface IDecorator<T> where T : IDependency  {  }
}
