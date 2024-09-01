using AutoMapper;
using AutoMapper.Internal;
using SytsBackendGen2.Application.DTOs.Roles;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;
using SytsBackendGen2.Domain.Entities.Authentification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace SytsBackendGen2.Application.Common.Dtos;

public interface IBaseDto
{
    static abstract Type GetOriginType();
}