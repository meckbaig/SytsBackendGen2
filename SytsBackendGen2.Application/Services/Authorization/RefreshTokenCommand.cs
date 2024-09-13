using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Extensions.DataBaseProvider;
using SytsBackendGen2.Domain.Entities.Authentification;
using System.Security.Claims;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;

namespace SytsBackendGen2.Application.Services.Authorization;

public record RefreshTokenCommand : BaseAuthentificatedRequest<RefreshTokenResponse>
{
    public required string refreshToken { get; set; }
    internal override int userId { get; set; }
}

public class RefreshTokenResponse : BaseResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IAppDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(IAppDbContext context, IJwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        request.refreshToken = request.refreshToken.Replace(' ', '+');

        User? user = await _context.Users
            .Include(u => u.RefreshTokens.Where(t => t.Token.Equals(request.refreshToken)))
            .WithRoleByIdAsync(request.userId, cancellationToken);

        await ValidateUserAndToken(user, request.userId, request, cancellationToken);

        string jwt = _jwtProvider.GenerateToken(user);
        string refreshToken = _jwtProvider.GenerateRefreshToken(user, request.refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse { Token = jwt, RefreshToken = refreshToken };
    }

    private async Task ValidateUserAndToken(
        User user,
        int userId,
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new Common.Exceptions.ValidationException(
                "jwtToken",
                [new ErrorItem($"Unable to find user with id {userId}.", ValidationErrorCode.EntityIdValidator)]);
        }
        RefreshToken? token = user.RefreshTokens.FirstOrDefault();
        if (token == null || token.Invalidated)
        {
            if (token?.Invalidated ?? false)
                await TryInvalidateAllUserRefreshTokensAsync(userId, cancellationToken);
            await _context.SaveChangesAsync();

            throw new Common.Exceptions.ValidationException(
                nameof(request.refreshToken),
                [new ErrorItem($"Refresh token is not valid.", ValidationErrorCode.RefreshTokenNotValid)]);
        }
        if (token.ExpirationDate < DateTimeOffset.UtcNow)
        {
            throw new Common.Exceptions.ValidationException(
                nameof(request.refreshToken),
                [new ErrorItem($"Refresh token has expired.", ValidationErrorCode.RefreshTokenExpired)]);
        }
    }

    private async Task<bool> TryInvalidateAllUserRefreshTokensAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ExecuteUpdateAsync(x => x.SetProperty(t => t.Invalidated, true), cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
