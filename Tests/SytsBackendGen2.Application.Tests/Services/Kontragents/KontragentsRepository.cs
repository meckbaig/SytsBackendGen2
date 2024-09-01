using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Entities;
using Moq;

namespace SytsBackendGen2.Application.Tests.Services.Kontragents;

public static class KontragentsRepository
{
    public static Mock<IAppDbContext> GetKontragentsRepository(this Mock<IAppDbContext> contextMock)
    {
        var kontragents = new List<Kontragent>()
        {
            new()
            {
                Id = 5,
                Address = new()
                {
                    City = new() { Name = "Алматы" },
                    Street = new() { Name = "Аль-фараби" },
                    HouseName = "101",
                    PorchNumber = 1,
                    Apartment = "4",
                    Region = new() { Name = "Бостандыкский район" }
                },
                PhoneNumber = "",
                KontragentAgreement = new()
                {
                    Balance = -620.00M,
                    DocumentNumber = "0041807",
                    PersonalAccount = "5379687",
                    ContractDate = DateOnly.FromDayNumber(733923)
                },
                Name = "АБДЕЛЬМАНОВА"
            }
        };

        var mockDbSet = Helper.CreateDbSetMock(kontragents.AsQueryable());
        contextMock.Setup(x => x.Kontragents).Returns(mockDbSet.Object);
        return contextMock;
    }
}