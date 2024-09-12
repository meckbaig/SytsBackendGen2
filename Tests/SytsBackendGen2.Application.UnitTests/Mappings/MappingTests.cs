using AutoMapper;
using NUnit.Framework;
using System.Dynamic;
using System.Reflection;
using System.Runtime.Serialization;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.DTOs.Users;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace Application.UnitTests.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(config =>
            config.AddMaps(Assembly.GetAssembly(typeof(IAppDbContext))));

        _mapper = _configuration.CreateMapper();
    }

    //[Test]
    //public void ShouldHaveValidConfiguration()
    //{
    //    _configuration.AssertConfigurationIsValid();
    //}

    /// TODO: Add here more DTOs
    [Test]
    [TestCase(typeof(Access), typeof(AccessDto))]
    [TestCase(typeof(Folder), typeof(FolderDto))]
    [TestCase(typeof(FolderEditDto), typeof(Folder))]
    [TestCase(typeof(ExpandoObject), typeof(VideoDto))]
    [TestCase(typeof(ExpandoObject), typeof(SubChannelDto))]
    [TestCase(typeof(User), typeof(UserPreviewDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);

        _mapper.Map(instance, source, destination);
    }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        return FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050 // Type or member is obsolete
    }
}