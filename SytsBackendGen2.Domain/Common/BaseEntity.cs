using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SytsBackendGen2.Domain.Common
{
    public abstract class BaseEntity : IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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
