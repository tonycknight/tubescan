using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using TubeScan.Io;
using TubeScan.Config;
using Xunit;

namespace TubeScan.Tests.Config
{
    public class EnvVarAppConfigurationProviderTests
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        [Fact]
        public void GetAppConfiguration_EmptyEnvVars_ReturnsDefaultConfig()
        {
            var envvars = new System.Collections.Hashtable();

            var io = Substitute.For<IIoProvider>();
            io.GetEnvironmentVariables().Returns(envvars);

            var cp = new EnvVarAppConfigurationProvider(io);

            var config = cp.GetAppConfiguration();

            config.Should().NotBeNull();
            config.Discord.ClientId.Should().BeNull();

            config.Discord.ClientToken.Should().BeNull();
            config.Mongo.Connection.Should().BeNull();
            config.Mongo.DatabaseName.Should().NotBeNullOrWhiteSpace();
            config.Tfl.AppKey.Should().BeNull();

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GetAppConfiguration_RandomEnvVars_ReturnsDefaultConfig(int evCount)
        {

            var vs = Enumerable.Range(1, evCount).Select(_ => Guid.NewGuid().ToString()).ToList();
            var envvars = new System.Collections.Hashtable(vs.ToDictionary(v => v, v => v));

            var io = Substitute.For<IIoProvider>();
            io.GetEnvironmentVariables().Returns(envvars);

            var cp = new EnvVarAppConfigurationProvider(io);

            var config = cp.GetAppConfiguration();

            config.Should().NotBeNull();
            config.Discord.ClientId.Should().BeNull();
            config.Discord.ClientToken.Should().BeNull();
            config.Mongo.Connection.Should().BeNull();
            config.Mongo.DatabaseName.Should().NotBeNullOrWhiteSpace();
            config.Tfl.AppKey.Should().BeNull();
        }

        [Fact]
        public void GetAppConfiguration_EnvVars_ReturnsConfig()
        {
            var envvarData =
                new Dictionary<string, string>()
                {
                    { "TubeScan_Discord_DiscordClientId", Guid.NewGuid().ToString() },
                    { "TubeScan_Discord_DiscordClientToken", Guid.NewGuid().ToString() },
                    { "TubeScan_Mongo_Connection", Guid.NewGuid().ToString() },
                    { "TubeScan_Mongo_DatabaseName", Guid.NewGuid().ToString() },
                    { "TubeScan_Tfl_AppKey", Guid.NewGuid().ToString() },
                };

            var envvarHs = new System.Collections.Hashtable(envvarData);

            var io = Substitute.For<IIoProvider>();
            io.GetEnvironmentVariables().Returns((System.Collections.IDictionary)envvarHs);

            var cp = new EnvVarAppConfigurationProvider(io);

            var config = cp.GetAppConfiguration();

            config.Should().NotBeNull();

            config.Discord.ClientId.Should().Be(envvarData["TubeScan_Discord_DiscordClientId"]);
            config.Discord.ClientToken.Should().Be(envvarData["TubeScan_Discord_DiscordClientToken"]);
            config.Mongo.Connection.Should().Be(envvarData["TubeScan_Mongo_Connection"]);
            config.Mongo.DatabaseName.Should().Be(envvarData["TubeScan_Mongo_DatabaseName"]);
            config.Tfl.AppKey.Should().Be(envvarData["TubeScan_Tfl_AppKey"]);
        }

#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
