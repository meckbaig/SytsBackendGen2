using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Kontragents;
using SytsBackendGen2.Application.Services.Kontragents;
using Moq;
using NSubstitute;
using System.Reflection;

namespace SytsBackendGen2.Application.Tests.Services.Kontragents;

public class GetKontragentsQueryTests //: KontragentsApiFactory
{
    //IAppDbContext _inMemoryContext;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public GetKontragentsQueryTests()
    {
        //_container.StartAsync().Wait();
        //var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
        //    .UseNpgsql(//_container.GetConnectionString())
        //        "Server=localhost;Port=5433;Database=SytsBackendGen2;User ID=postgres;Password=testtest;")
        //    .Options;
        //_inMemoryContext = new AppDbContext(dbContextOptions, Substitute.For<ILogger<TransactionLoggingInterceptor>>());

        var config = new MapperConfiguration(c =>
            c.AddMaps(Assembly.GetAssembly(typeof(IAppDbContext))));
        _mapper = config.CreateMapper();
        _cache = Substitute.For<IDistributedCache>();
    }

    //[Fact]
    //public async Task GetKontragentsQuery_ReturnSingle_WhenTake1_DtoCheck()
    //{
    //    // Arrange
    //    KonragentPreviewDto previewDto = new()
    //    {
    //        AddressString = "�. ������, ��. ���-������, �. 101, ���. 1, ��. 4",
    //        Balance = -620.00M,
    //        ContractDate = DateOnly.FromDayNumber(733923),
    //        DocumentNumber = "0041807",
    //        Id = 5,
    //        Name = "������������",
    //        PersonalAccount = "5379687",
    //        PhoneNumber = "",
    //        RegionString = "������������� �����"
    //    };


    //    Mock<IAppDbContext> contextMock = new Mock<IAppDbContext>().GetKontragentsRepository();

    //    var handler = new GetKontragentsQueryHandler(contextMock.Object, _mapper, _cache);
    //    var query = new GetKontragentsQuery();

    //    // Act
    //    var result = await handler.Handle(query, default);

    //    // Assert
    //    Assert.NotNull(result?.Items);
    //    Assert.Equal(result?.Items.Count, 1);

    //    foreach (var prop in typeof(KonragentPreviewDto).GetProperties())
    //    {
    //        object v1 = prop.GetValue(result.Items[0]);
    //        object v2 = prop.GetValue(previewDto);
    //        Assert.Equal(v1, v2);
    //    }
    //}


}