using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Users;
using SytsBackendGen2.Application.Extensions.DataBaseProvider;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Services.Authorization;

public record AuthorizeUserCommand : BaseRequest<AuthorizeUserResponse>
{
    public required string accessToken { get; set; }
}

public class AuthorizeUserResponse : BaseResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required UserPreviewDto UserData { get; set; }
}

public class AuthorizeUserCommandValidator : AbstractValidator<AuthorizeUserCommand>
{
    public AuthorizeUserCommandValidator()
    {
        RuleFor(x => x.accessToken).NotEmpty();
    }
}

public class AuthorizeUserCommandHandler : IRequestHandler<AuthorizeUserCommand, AuthorizeUserResponse>
{
    private readonly IAppDbContext _context;
    private readonly IJwtProvider _jwtProvider;
    private readonly IGoogleAuthProvider _googleAuthProvider;
    private readonly IMapper _mapper;

    public AuthorizeUserCommandHandler(
        IAppDbContext context,
        IJwtProvider jwtProvider,
        IGoogleAuthProvider googleAuthProvider,
        IMapper mapper)
    {
        _context = context;
        _jwtProvider = jwtProvider;
        _googleAuthProvider = googleAuthProvider;
        _mapper = mapper;
    }

    public async Task<AuthorizeUserResponse> Handle(AuthorizeUserCommand request, CancellationToken cancellationToken)
    {
        UserPreviewDto? userDto;
        try
        {
            userDto = await _googleAuthProvider.AuthorizeAsync(request.accessToken);
        }
        catch (HttpRequestException ex)
        {
            throw new Common.Exceptions.ValidationException(nameof(request.accessToken), [new ErrorItem(ex.Message, "AccessTokenNotValid")]);
        }
        catch (Exception)
        {
            throw;
        }

        User user = await GetOrCreateUserAsync(userDto, cancellationToken);

        string jwt = _jwtProvider.GenerateToken(user);
        string refreshToken = _jwtProvider.GenerateRefreshToken(user);
        await _context.SaveChangesAsync(cancellationToken);

        var previewUserDto = _mapper.Map<UserPreviewDto>(user);
        previewUserDto.Name = userDto.Name;
        previewUserDto.Picture = userDto.Picture;

        return new AuthorizeUserResponse { Token = jwt, RefreshToken = refreshToken, UserData = previewUserDto };
    }

    private async Task<User> GetOrCreateUserAsync(UserPreviewDto? userDto, CancellationToken cancellationToken)
    {
        User? user = await _context.Users
            .Include(u => u.RefreshTokens)
            .IgnoreQueryFilters()
            .WithRoleByEmailAsync(userDto.Email);
        if (user == null)
        {
            string youtubeId = await _googleAuthProvider.GetYoutubeIdByName(userDto.Name);

            user = new(youtubeId, userDto.Email);
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            user.Role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);
        }
        if (user.Deleted)
        {
            user.Deleted = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return user;
    }
}