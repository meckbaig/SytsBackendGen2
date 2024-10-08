namespace SytsBackendGen2.Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastModified { get; set; }

        private readonly List<BaseEvent> _domainEvents = new();

        //[NotMapped]
        public IReadOnlyCollection<BaseEvent> GetDomainEvents()
            => _domainEvents.AsReadOnly();

        public void AddDomainEvent(BaseEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(BaseEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
