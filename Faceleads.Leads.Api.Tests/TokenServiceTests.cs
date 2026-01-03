using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Faceleads.Leads.Api.Services;
using Faceleads.Leads.Infrastructure;
using Faceleads.Leads.Domain;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Faceleads.Leads.Api.Tests;

public class TokenServiceTests
{
    [Fact]
    public async Task IssueTokensAsync_ReturnsTokens_OnSuccess()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:Key", "test-secret-key-which-is-long-enough" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var mockRepo = new Mock<IRefreshTokenRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);

        var service = new TokenService(configuration, mockRepo.Object);

        var result = await service.IssueTokensAsync("alice");

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrEmpty(result.Value!.accessToken));
        Assert.False(string.IsNullOrEmpty(result.Value.refreshToken));
    }

    [Fact]
    public async Task RefreshWithTokenAsync_ReturnsFail_OnInvalidToken()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:Key", "test-secret-key-which-is-long-enough" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var mockRepo = new Mock<IRefreshTokenRepository>();
        mockRepo.Setup(r => r.GetByTokenAsync("bad", default)).ReturnsAsync((RefreshToken?)null);

        var service = new TokenService(configuration, mockRepo.Object);

        var result = await service.RefreshWithTokenAsync("bad");

        Assert.False(result.Success);
        Assert.Equal("REFRESH_INVALID", result.ErrorCode);
    }
}
